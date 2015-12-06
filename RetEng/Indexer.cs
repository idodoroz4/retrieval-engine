using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.IO;

namespace RetEng
{
    class Indexer
    {
        ConcurrentDictionary<string, List<TermInDoc>> cache;
        List<string> queue;
        ConcurrentDictionary<string, Posting> main_dic;
        int _cache_size;
        int _heap_size;

        public Indexer(int cache_size, int heap)
        {
            cache = new ConcurrentDictionary<string, List<TermInDoc>>();
            queue = new List<string>();
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
                queue.Remove(key);
                queue.Add(key);
                cache[key].Add(term);
        }
        private void add_to_cache(string key, TermInDoc term)
        {
            List<TermInDoc> list = new List<TermInDoc>();
            list.Add(term);
            if (queue.Count < _cache_size)
            {
                queue.Add(key);
            }
            else
            {
                string removal_key = queue[0];
                write(removal_key, cache[removal_key]);
                queue.RemoveAt(0);
                queue.Add(key);
                
            }
            cache.TryAdd(key, list);
              
        }
        
            private void write(string key, List<TermInDoc> terms)
            {
                string output;
                if (File.Exists(key + ".txt"))
                {
                    string input_json = System.IO.File.ReadAllText(key + ".txt");
                    List<TermInDoc> list = JsonConvert.DeserializeObject<List<TermInDoc>>(input_json);
                    list.AddRange(terms);
                    output = JsonConvert.SerializeObject(list, Formatting.Indented);
                 
                }
                else
                {
                    output = JsonConvert.SerializeObject(terms, Formatting.Indented);
                }
                System.IO.File.WriteAllText(key + ".txt", output);
                List<TermInDoc> outt;
                cache.TryRemove(key,out outt);
            }
          
    }

}
