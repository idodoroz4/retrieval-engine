using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Concurrent;

namespace RetEng
{
    public class Compare : IEqualityComparer<TermInDoc>
    {
        public bool Equals (TermInDoc t1, TermInDoc t2)
        {
            return t1._doc_id == t2._doc_id;
        }
        public int GetHashCode(TermInDoc t)
        {
            return t._doc_id.GetHashCode();
        }

    }
    public class Searcher
    {
        public List<string> terms;
        public List<string> Existing_terms;
        public Controller _ctrl;
        public Dictionary<string, List<string>> doctoTerms;
        Dictionary<string, List<TermInDoc>> myTerms;
        Dictionary<string, Dictionary<string, List<TermInDoc>>> posting_cache;
        public Dictionary<string, Tuple<int, int, string, int>> num_of_terms_in_doc;
        public Dictionary<string, Dictionary<string, int>> tf_all_docs;
        string _posting_path;
        short _fromMonth;
        short _toMonth;
        public Searcher()
        {
            doctoTerms = new Dictionary<string, List<string>>();
            myTerms = new Dictionary<string, List<TermInDoc>>();
            Existing_terms = new List<string>();
            posting_cache = new Dictionary<string, Dictionary<string, List<TermInDoc>>>();
            num_of_terms_in_doc = null;


        }
        public void load_tf_all_docs(string posting_path)
        {
            tf_all_docs = new Dictionary<string, Dictionary<string, int>>();
            num_of_terms_in_doc = new Dictionary<string, Tuple<int, int, string, int>>();
            _posting_path = posting_path;
            //tf_all_docs = get_tf_all_docs2();
            get_tf_all_docs2();
        }
        public List<string> start_search(string query, string path_sw, bool stems, Controller ctrl, string posting_path, string FromMonth, string toMonth)
        {
            _fromMonth = FromMonth == "" ? (short)1 : short.Parse(FromMonth);
            _toMonth = toMonth == "" ? (short)12 : short.Parse(toMonth);
            
            // implementing %doll% 
            StringBuilder sb = new StringBuilder();
            if (query[0] == '%' && query[query.Length - 1] == '%'){
                query = query.Substring(1, query.Length - 2);
                foreach (string key in ctrl.idxr.main_dic.Keys)
                {
                    if (key.Contains(query))
                        sb.Append(key + " ");
                }
                query = sb.ToString();
            }


            Parser prs = new Parser(query, path_sw, stems);
            terms = prs.parse_query();
          
            _ctrl = ctrl;
            _posting_path = posting_path;
            myTerms.Clear();
            Existing_terms.Clear();
            doctoTerms.Clear();

            foreach (string trm in terms)
            {
                if (!_ctrl.idxr.main_dic.ContainsKey(trm))
                    continue;
                Existing_terms.Add(trm);
                Posting tmp = _ctrl.idxr.main_dic[trm];
                foreach (string path in tmp.posting_locations)
                {
                    Dictionary<string, List<TermInDoc>> tmpdic;
                    if (!posting_cache.ContainsKey(path))
                    {
                        tmpdic = new Dictionary<string, List<TermInDoc>>();
                        tmpdic = read_file2(_posting_path + "\\" + path);
                        posting_cache.Add(path, tmpdic);
                    }
                    else
                    {
                        tmpdic = posting_cache[path];
                    }
                    if (!(FromMonth == "" || toMonth == ""))
                    {
                        if (!myTerms.ContainsKey(trm))
                            myTerms.Add(trm, filter_docs(tmpdic[trm]));
                        else
                            myTerms[trm].AddRange(filter_docs(tmpdic[trm]));
                    }
                    else
                    { 

                        if (!myTerms.ContainsKey(trm))
                            myTerms.Add(trm, tmpdic[trm]);
                        else
                            myTerms[trm].AddRange(tmpdic[trm]);
                    }
                }
                
            }

            List<TermInDoc> relevent_docs = get_docs();
            Ranker r = new Ranker(_ctrl, tf_all_docs, Existing_terms, terms.Count,num_of_terms_in_doc,myTerms);
            List<string> best_docs = r.rank_relevent_docs(relevent_docs);
            return best_docs;
            //and_docs();
            //return null;
        }
        private List<TermInDoc> filter_docs (List<TermInDoc> tid)
        {
            List<TermInDoc> filtered = new List<TermInDoc>();
            foreach (TermInDoc t in tid)
            {
                int mymonth = num_of_terms_in_doc[t._doc_id].Item4;
                if (mymonth <= _toMonth && mymonth >= _fromMonth)
                    filtered.Add(t);
            }
            return filtered;
        }
        public void start_search_file (string file_path, string posting_sw, bool stem, Controller ctrl, string posting_path, string to, string from)
        {
            var lines = File.ReadLines(file_path);
            foreach (var line in lines)
            {
                string[] split = line.Split('\t');
                List<string> top_50 = start_search(split[1], posting_sw, stem, ctrl, posting_path, from, to);
                foreach (string doc in top_50)
                {
                    File.AppendAllText(posting_path + "\\qury_res", split[0] + " 0 " + doc + " 1 " + Environment.NewLine);
                }
            }
        }
        private List<TermInDoc> get_docs()
        {
            
            List<TermInDoc> relevent_docs = new List<TermInDoc>();
            List<TermInDoc> intersect_all = and_docs(Existing_terms);
            relevent_docs = relevent_docs.Union(intersect_all).ToList();
            if (long_terms().Count > 0) {
                relevent_docs = relevent_docs.Union(and_docs(long_terms()),new Compare()).ToList();
                relevent_docs = relevent_docs.Union(and_docs(short_terms()), new Compare()).ToList();
            }

            return relevent_docs;


        }

        private List<string> long_terms()
        {
            return new List<string>(Existing_terms.Where(x => x.Contains(' ')));
        }
        private List<string> short_terms()
        {
            return new List<string>(Existing_terms.Where(x => !x.Contains(' ')));
        }




        private List<TermInDoc> and_docs(List<string> Terms)
        {

            List<TermInDoc> interDocs = new List<TermInDoc>(myTerms[Terms[0]]);
            
            foreach (string key in Terms)
            {
                interDocs = interDocs.Intersect(myTerms[key],new Compare()).ToList();
                Console.WriteLine();
            }
            foreach (TermInDoc t in interDocs) {

                if (doctoTerms.ContainsKey(t._doc_id))
                    doctoTerms[t._doc_id] = doctoTerms[t._doc_id].Union(Terms).ToList();
                
                else
                    doctoTerms.Add(t._doc_id, Terms);
            }
            return interDocs;
        }

        private Dictionary<string, List<TermInDoc>> read_file2(string path)
        {
            Console.WriteLine(path);
            string input_json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, List<TermInDoc>>>(input_json);
        }
        private Dictionary<string, List<TermInDoc>> read_file (string path)
        {
            Dictionary<string, List<TermInDoc>> tmpdic;
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            //StringBuilder sb = new StringBuilder();
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer ser = new JsonSerializer();
                tmpdic = ser.Deserialize<Dictionary<string, List<TermInDoc>>>(reader);
            }

            return tmpdic;
        }

        private Dictionary<string, Tuple<int, int, string, int>> get_number_of_terms_in_doc(string path)
        {
            Dictionary<string, Tuple<int, int, string, int>> tmpdic = new Dictionary<string, Tuple<int, int, string, int>>();
            FileStream fs = File.Open(path + "\\number_of_terms_in_doc.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            //StringBuilder sb = new StringBuilder();
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer ser = new JsonSerializer();
                tmpdic = ser.Deserialize<Dictionary<string, Tuple<int, int, string, int>>>(reader);
            }

            return tmpdic;
        }
        private void get_tf_all_docs2()
        {
            string input_json = System.IO.File.ReadAllText(_posting_path + "\\tf_all_docs.txt");
            tf_all_docs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(input_json);

            string input_json2 = System.IO.File.ReadAllText(_posting_path + "\\number_of_terms_in_doc.txt");
            num_of_terms_in_doc = JsonConvert.DeserializeObject<Dictionary<string, Tuple<int, int, string, int>>>(input_json2);
        }
        private void get_tf_all_docs()
        {
            //Dictionary<string, Dictionary<string, int>> tmpdic = new Dictionary<string, Dictionary<string, int>>();
            FileStream fs = File.Open(_posting_path + "\\tf_all_docs.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            //StringBuilder sb = new StringBuilder();
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer ser = new JsonSerializer();
                tf_all_docs = ser.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader);
                //tmpdic = ser.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader);
            }

            //return tmpdic;
        }

        /*private void load_tf_in_doc()
        {
            string[] fileEntries = Directory.GetFiles(_posting_path);
            foreach (string fileName in fileEntries)
            {
                if (fileName.Contains("tf"))
                {
                    string input_json = System.IO.File.ReadAllText(_posting_path + "\\" + fileName);
                    main_dic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Posting>>(input_json);
                }
            }

               
        }*/
    }
}
