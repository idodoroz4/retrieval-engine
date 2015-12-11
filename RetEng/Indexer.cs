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
        ConcurrentDictionary<string, ConcurrentBag<TermInDoc>> cache;
        ConcurrentQueue<string> queue;
        ConcurrentDictionary<string, Posting> main_dic;
        int _cache_size;
        int _heap_size;

        public Indexer(int cache_size, int heap)
        {
            locker = new object();
            cache = new ConcurrentDictionary<string, ConcurrentBag<TermInDoc>>();
            queue = new ConcurrentQueue<string>();
            main_dic = new ConcurrentDictionary<string,Posting>();
            _cache_size = cache_size;
            _heap_size = heap;
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
            System.IO.File.WriteAllText("main_memory.txt", output);
        }

        private Task write_task(string s, ConcurrentBag<TermInDoc> term)
        {
            return Task.Run(() => { write(s, term); });
        }

        private async void write_async(string s, ConcurrentBag<TermInDoc> term)
        {
            await write_task(s, term);
        }

        public void save_cache()
        {
            foreach (var pair in cache)
            {
                write_async(pair.Key, pair.Value);
            }
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
                string removal_key;
                if (queue.TryPeek(out removal_key))
                {
                    //Thread io = new Thread(() => write(removal_key, cache[removal_key]));
                    //io.Start(); NEED TO CHECK WHETHER THREAD OR TASK IS BETTER!
                    
                    ConcurrentBag<TermInDoc> outt;
                    cache.TryRemove(removal_key, out outt);
                    queue.TryDequeue(out removal_key);
                    write_async(removal_key, outt);
                    //write_async(removal_key, templist);
                    queue.Enqueue(key);
                }  
            }
            cache.TryAdd(key, list);   
        }
        
        private void write(string key, ConcurrentBag<TermInDoc> terms)
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



            
        }

        
    }

}
