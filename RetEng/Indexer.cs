using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace RetEng
{
    class Indexer
    {
        private static object locker;
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

        }


        public void insert(Dictionary<string, TermInDoc> local)
        {
            foreach (var item in local)
            {
                if (in_dic(item.Key))
                {
                    if (in_cache(item.Key))
                        promote_queue(item.Key, item.Value);
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
        public void save_memory()
        {
            string output = JsonConvert.SerializeObject(main_dic, Formatting.Indented);
            System.IO.File.WriteAllText(_posting_path + "\\Inverted_file.txt", output);
        }

        private Task write_task(string output, int fileNumber)
        {
            return Task.Run(() => { write_posting(output,fileNumber); });
        }

        private async void write_async(string output, int fileNumber)
        {
            await write_task(output , fileNumber);
        }

        private void add_to_dic(string p, TermInDoc termInDoc)
        {
            Posting post = new Posting();
            main_dic.TryAdd(p, post);
        }
        private bool in_dic(string key)
        {
            return main_dic.ContainsKey(key);
        }
        private bool in_cache(string key)
        {
            return cache.ContainsKey(key);
        }
        private void promote_queue(string key, TermInDoc term)
        {
             // needs to promote
            cache[key].Add(term);
            main_dic[key].is_popular = true;
        }
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
                            continue;
                        }
                        else
                            undone = false;

                    }
                }
                ConcurrentBag<TermInDoc> termsInDocs;
                cache.TryRemove(removal_key, out termsInDocs);
                tempDic.TryAdd(removal_key, termsInDocs);
                main_dic[removal_key].posting_locations.Add("posting" + _file_number + ".txt"); //must be with locks if threaded

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
                cache.TryRemove(removal_key, out termsInDocs);
                tempDic.TryAdd(removal_key, termsInDocs);
                main_dic[removal_key].posting_locations.Add("posting" + _file_number + ".txt"); //must be with locks if threaded

            }
            string output = JsonConvert.SerializeObject(tempDic, Formatting.Indented);
            //System.IO.File.WriteAllText("posting" + _file_number + ".txt", output);
            System.IO.File.WriteAllText(_posting_path + "\\posting" + _file_number + ".txt", output);

            _file_number++;
            

        }
        private void write_posting(string text,int file_number)
        {
            System.IO.File.WriteAllText(_posting_path + "\\posting" + file_number + ".txt", text);
        }

        public void load_memory()
        {
            
            string input_json = System.IO.File.ReadAllText(_posting_path + "\\Inverted_file.txt");
            main_dic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Posting>>(input_json);
        }

        public string show_memory()
        {
            StringBuilder sb = new StringBuilder();
            List<string> keys_lst = new List<string>();
            keys_lst = main_dic.Keys.ToList();
            keys_lst.Sort();
            foreach (string key in keys_lst)
            {
                sb.Append(key + " : " + main_dic[key].ToString() + "\n");
            }
            return sb.ToString();

        }
        /*private void write(string key, ConcurrentBag<TermInDoc> terms)
        {
            
            string output;

            if (File.Exists(key + ".txt"))
            {
                string input_json = System.IO.File.ReadAllText(key + ".txt");
                ConcurrentBag<TermInDoc> list = JsonConvert.DeserializeObject<ConcurrentBag<TermInDoc>>(input_json);
                foreach(TermInDoc tid in terms)
                    list.Add(tid);
             
                output = JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            else
            {
                output = JsonConvert.SerializeObject(terms, Formatting.Indented);
            }

            lock (locker)
            {

                System.IO.File.WriteAllText("_" + key + ".txt", output);
            }
            
            
        }*/

        public void writeCache()
        {
            while (cache.Count > 0)
                write_not_threaded(_numOfTermsInPosting / 20);
            /*
            ConcurrentBag<TermInDoc> terms = cache[key];
            string output;

            if (File.Exists(key + ".txt"))
            {
                string input_json = System.IO.File.ReadAllText(key + ".txt");
                ConcurrentBag<TermInDoc> list = JsonConvert.DeserializeObject<ConcurrentBag<TermInDoc>>(input_json);
                foreach (TermInDoc tid in terms)
                    list.Add(tid);

                output = JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            else
            {
                output = JsonConvert.SerializeObject(terms, Formatting.Indented);
            }

            System.IO.File.WriteAllText("_" + key + ".txt", output);
            

            ConcurrentBag<TermInDoc> outt;
            cache.TryRemove(key, out outt);
            */

        }


    }

}
