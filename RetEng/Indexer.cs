using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace RetEng
{
    // Build the posting files, and handle with the inverted files. 
    //uses a cahce and implements pausdo LRU for terms that are mostly in use.
    public class Indexer
    {
        public ConcurrentDictionary<string, ConcurrentBag<TermInDoc>> cache;
        public ConcurrentQueue<string> queue;
        ConcurrentDictionary<string, Posting> main_dic;
        int _cache_size;
        int _heap_size;
        int _numOfTermsInPosting;
        int _file_number;
        string _posting_path;
        object thislock;

        public Indexer(int cache_size, int heap, int numOfTermsInPosting,string posting_path)
        {
            cache = new ConcurrentDictionary<string, ConcurrentBag<TermInDoc>>();
            queue = new ConcurrentQueue<string>();
            main_dic = new ConcurrentDictionary<string,Posting>();
            _cache_size = cache_size;
            _heap_size = heap;
            _numOfTermsInPosting = numOfTermsInPosting;
            _file_number = 0;
            thislock = new object();
            _posting_path = posting_path;
            List<string> keys_lst = new List<string>();
        }

        // Inserts a term to the inverted file.
        public void insert(Dictionary<string, TermInDoc> local)
        {
            foreach (var item in local)
            {
                if (in_dic(item.Key))
                {
                    if (in_cache(item.Key))
                        promote_queue(item.Key, item.Value); //psaudo LRU
                    else
                        add_to_cache(item.Key, item.Value);
                    main_dic[item.Key].increament();
                }
                else
                {
                    add_to_dic(item.Key, item.Value);
                    add_to_cache(item.Key, item.Value);
                }
            }
        }
        // save inverted file to disk
        public void save_memory()
        {
            string output = JsonConvert.SerializeObject(main_dic, Formatting.Indented);
            System.IO.File.WriteAllText(_posting_path + "\\Inverted_file.txt", output);
        }

        private Task write_task(string output, int fileNumber)
        {
            return Task.Run(() => { write_posting(output,fileNumber); });
        }
        // making the writing task parallel
        private async void write_async(string output, int fileNumber)
        {
            await write_task(output , fileNumber);
        }
        // adding to the inverted files dictionary
        private void add_to_dic(string p, TermInDoc termInDoc)
        {
            Posting post = new Posting();
            main_dic.TryAdd(p, post);
        }
        // is the term key inside the inverted files?
        private bool in_dic(string key)
        {
            return main_dic.ContainsKey(key);
        }
        //  is the term key inside the cache?
        private bool in_cache(string key)
        {
            return cache.ContainsKey(key);
        }
        // using the psaudo LRU idea, when a term is already in cache, promote it by a flag is_popular
        private void promote_queue(string key, TermInDoc term)
        {
             // needs to promote
            cache[key].Add(term);
            main_dic[key].is_popular = true;
        }
        // add term to the cache
        private void add_to_cache(string key, TermInDoc term)
        {
            ConcurrentBag<TermInDoc> list = new ConcurrentBag<TermInDoc>();
            list.Add(term);
            if (queue.Count < _cache_size)
            {
                queue.Enqueue(key);
            }
            else
            {
                write2(_numOfTermsInPosting);
                queue.Enqueue(key);
            }
            cache.TryAdd(key, list);
        }
        // create posting file and write it
        private void write2(int num_of_terms_in_file)
        {

            ConcurrentDictionary<string, ConcurrentBag<TermInDoc>> tempDic;
            tempDic = new ConcurrentDictionary<string, ConcurrentBag<TermInDoc>>();
            for (int i = 0; i < num_of_terms_in_file; i++)
            {
                if (cache.Count == 0)
                    break;
                string removal_key = "";
                bool undone = true;
                while (undone)
                {
                    if (queue.TryDequeue(out removal_key))
                    {
                        if (main_dic[removal_key].is_popular)
                        {
                            queue.Enqueue(removal_key);
                            main_dic[removal_key].is_popular = false;
                            //continue;
                        }
                        else
                            undone = false;

                    }
                }
                ConcurrentBag<TermInDoc> termsInDocs;
                cache.TryRemove(removal_key, out termsInDocs);
                tempDic.TryAdd(removal_key, termsInDocs);
                main_dic[removal_key].posting_locations.Add("p" + _file_number + ".txt"); //must be with locks if threaded

            }
            string output = JsonConvert.SerializeObject(tempDic, Formatting.Indented);
            //System.IO.File.WriteAllText("posting" + _file_number + ".txt", output);
            write_async(output,_file_number);
           _file_number++;
            
           
        }

        private void write_not_threaded(int num_of_terms_in_file)
        {

            ConcurrentDictionary<string, ConcurrentBag<TermInDoc>> tempDic;
            tempDic = new ConcurrentDictionary<string, ConcurrentBag<TermInDoc>>();
            for (int i = 0; i < num_of_terms_in_file; i++)
            {
                if (cache.Count == 0)
                    break;

                string removal_key = "";
                queue.TryDequeue(out removal_key);

                ConcurrentBag<TermInDoc> termsInDocs;
                if (cache.TryRemove(removal_key, out termsInDocs))
                tempDic.TryAdd(removal_key, termsInDocs);
                main_dic[removal_key].posting_locations.Add("p" + _file_number + ".txt"); //must be with locks if threaded

            }
            string output = JsonConvert.SerializeObject(tempDic, Formatting.Indented);
            //System.IO.File.WriteAllText("posting" + _file_number + ".txt", output);
            System.IO.File.WriteAllText(_posting_path + "\\p" + _file_number + ".txt", output);
            _file_number++;

        }
        private void write_posting(string text,int file_number)
        {
            System.IO.File.WriteAllText(_posting_path + "\\p" + file_number + ".txt", text);
        }
        // load inverted file to the main memory
        public void load_memory()
        {
            
            string input_json = System.IO.File.ReadAllText(_posting_path + "\\Inverted_file.txt");
            main_dic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Posting>>(input_json);
        }

       // show the inverted files sorted 
        public void show_memory()
        {
            var sortedDict = (from entry in main_dic orderby entry.Key ascending select entry)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
           string sorted_inveted = JsonConvert.SerializeObject(sortedDict, Formatting.Indented);
           File.WriteAllText("sorted_inveted.txt", sorted_inveted);
            Process.Start("sorted_inveted.txt");
        }

       
        // at the end, the cache needs to writen to disk
        public void writeCache()
        {
            ConcurrentBag<TermInDoc> doc;
            cache.TryRemove("\u0000", out doc);
           while (cache.Count > 0)
            write_not_threaded(_numOfTermsInPosting / 90);
        }


    }

}
