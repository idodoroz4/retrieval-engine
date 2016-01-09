using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace RetEng
{
    // Controll the ReadFile , Parser and indexer, by multi thread programming
   public class Controller
    {
        private ConcurrentQueue<string> batch_files;
        private ConcurrentQueue<Document> docs_to_parse;
        private ConcurrentQueue<Dictionary<string, TermInDoc>> termAfterParse;
        public ConcurrentBag<ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>>> tf_all_docs;
        private ConcurrentDictionary<string, Tuple<int,int,string,int>> num_of_terms_in_doc;
        private int num_of_threads_readFile;
        private int num_of_threads_Parser;
        public bool readFileProcessFinished { get; private set; }
        public bool parserProcessFinished { get; private set; }
        public bool indexerProcessFinished { get; private set; }
        private int cacheSize;
        private int heapSize;

        private string _batch_path;
        private string _posting_path;
        private bool _stemming;

        private Task[] readFileTasks;
        private Task[] parserTasks;

        public Indexer idxr;

        public Controller(int cache_size, int heap_size)
        {
            batch_files = new ConcurrentQueue<string>();
            docs_to_parse = new ConcurrentQueue<Document>();
            termAfterParse = new ConcurrentQueue<Dictionary<string, TermInDoc>>();
            num_of_terms_in_doc = new ConcurrentDictionary<string, Tuple<int, int, string, int>>();
            tf_all_docs = new ConcurrentBag<ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>>>();
            cacheSize = cache_size;
            heapSize = heap_size;
            num_of_threads_readFile = 6;
            num_of_threads_Parser = 8;
            readFileProcessFinished = false;
            parserProcessFinished = false;
            indexerProcessFinished = false;

            cacheSize = cache_size;
            heapSize = heap_size;

        }
        // Used for debug and info
        public string data()
        {
            return "#Batch: " + batch_files.Count +"\nDocToParse: " + docs_to_parse.Count + 
                   "\nTermAfterParse: " + termAfterParse.Count;
        }
       //  a multi setter for few fields
        public void change_settings(string postig_path, string batch_path, bool stemming)
        {
            _posting_path = postig_path;
            _batch_path = batch_path;
            _stemming = stemming;

        }
       // Start the process, and create and manage threads
        public void initiate()
        {
            int numOfTermsInPosting = 50 * 1000;
            idxr = new Indexer(cacheSize, heapSize, numOfTermsInPosting, _posting_path);
            foreach (string file in Directory.GetFiles(_batch_path))
                if (!file.Equals("stop_words.txt"))
                    insertBatch(file);



        Task status = Task.Run(() => { read_data(); });
            /*for (int i = 0; i < 2; i++)
                readFileAsync();
            parserAsync();*/

             readFileTasks = new Task[num_of_threads_readFile];
             for (int i=0; i< num_of_threads_readFile; i++)
             {

                 Task readfile = Task.Run(() => { startReadFile(); });
                 readFileTasks[i] = readfile;
             }

             parserTasks = new Task[num_of_threads_Parser];
             for (int i = 0; i < num_of_threads_Parser; i++)
             {
                 
                 Task parser = Task.Run(() => { startParser(); });
                 parserTasks[i] = parser;
             }


            Task indexer = Task.Run(() => { startindexer(cacheSize, heapSize); });
            Task waitReadFile = Task.Run(() => { waitforReadFileProcess(readFileTasks); });    
            Task waitParser = Task.Run(() => { waitforparserProcess(parserTasks); });
            Task waitIndexer = Task.Run(() => { waitforindexerProcess(indexer); });
        }

       // The readFile Task
        private Task readFileTask()
        {
            return Task.Run(() => { startReadFile(); });
        }
       // The parser Task
        private Task ParserTask()
        {
            return Task.Run(() => { startParser(); });
        }
       // Using the async functions feture 
        private async void readFileAsync()
        {
            await readFileTask();
            readFileProcessFinished = true;
        }
        // Using the async functions feture 
        private async void parserAsync()
        {
            await ParserTask();
            parserProcessFinished = true;
        }
       // Used for debug - write threads info
        private void read_data()
        {
            DateTime d1 = DateTime.Now;
            Thread.Sleep(5000);
            while (true)
            {

                int readFilethreads = 0;
                int parserthreads = 0;
                foreach (Task t in readFileTasks)
                    if (!t.IsCompleted)
                        readFilethreads++;

                foreach (Task t in parserTasks)
                    if (!t.IsCompleted)
                        parserthreads++;

                if (!readFileProcessFinished || !parserProcessFinished || !indexerProcessFinished || idxr.cache.Count > 0)
                {
                    DateTime d2 = DateTime.Now;
                    TimeSpan d3 = d2.Subtract(d1);
                    string str = d3.ToString() + "\n#Batch: " + batch_files.Count + "\nDocToParse: " + docs_to_parse.Count +
                       "\nTermAfterParse: " + termAfterParse.Count + "\nReadFileThreads: "
                       + readFilethreads + " from " + num_of_threads_readFile +
                       "\nParserThreads: " + parserthreads + " from " + num_of_threads_Parser +
                       "\ncache Size: " + idxr.cache.Count +
                       "\nqueue Size:" + idxr.queue.Count;

                    Console.WriteLine(str);
                }
                else
                {
                    break;
                }
                Thread.Sleep(9000);
            }

        }
// Start the read file process 
        public void startReadFile()
        {
            string btch;
            List<Document> doc_list;
            ReadFile rf = new ReadFile();

            while (!batch_files.IsEmpty)
            {
                if (batch_files.TryDequeue(out btch))
                {
                    doc_list = rf.get_docs(btch);
                    foreach (Document d in doc_list)
                        docs_to_parse.Enqueue(d);
                }
            }
        }
// Start the Parser process
        private void startParser()
        {
            Document d;
            Dictionary<string, TermInDoc> TermDic;
            Parser prs = new Parser(_batch_path,_stemming);
            ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>> tf_doc = new ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>>();
            while (!readFileProcessFinished || !docs_to_parse.IsEmpty)
            {
                if (docs_to_parse.TryDequeue(out d))
                {
                    int int_date = prs.month_str_to_short(RemoveSpecialCharacters(d.date));

                    TermDic = prs.parse_doc(d);
                    Tuple<int,int,string> t = get_max_tf(TermDic,tf_doc);
                    if (tf_doc.Count >= 330)
                    {
                        tf_all_docs.Add(tf_doc);
                        tf_doc.Clear();
                    }
                    Tuple<int, int, string, int> t2 = new Tuple<int, int, string, int>(t.Item1, t.Item2, t.Item3, int_date);
                    num_of_terms_in_doc.TryAdd(d.id, t2);

                    termAfterParse.Enqueue(TermDic);
                }
            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        private string[] remove_nulls (string[] elems)
        {
            string[] new_elems = new string[3];
            int i = 0;
            foreach (string cell in elems)
            {
                if (cell != "") {
                    new_elems[i] = cell;
                    i++;
                }
            }
            return new_elems;
        }
        // Gettin the max tf from the parser
        private Tuple<int,int,string> get_max_tf(Dictionary<string, TermInDoc> termDic, ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>> tf_doc)
        {
            int max = 0;
            string term = "";
            string doc_id = termDic[termDic.Keys.ToList()[0]]._doc_id;
            foreach (var item in termDic)
            {
                if (tf_doc.ContainsKey(item.Key))
                    tf_doc[doc_id].Add(new Tuple<string, int>(item.Key, termDic[item.Key]._tf));
                else
                {
                    tf_doc.TryAdd(doc_id, new ConcurrentBag<Tuple<string, int>>());
                    tf_doc[doc_id].Add(new Tuple<string, int>(item.Key, termDic[item.Key]._tf));
                }


                if (item.Value._tf > max)
                {
                    max = item.Value._tf;
                    term = item.Key;
                }
                
            }
            return Tuple.Create<int,int, string>(termDic.Count,max, term); 
        }
       //  Start the indexer process
        private void startindexer(int cache_size, int heap_size)
        {
            
            Dictionary<string, TermInDoc> dicTerm;
            while (!parserProcessFinished || !termAfterParse.IsEmpty)
            {
                if (termAfterParse.TryDequeue(out dicTerm))
                {
                    idxr.insert(dicTerm);
                }
            }
            // After Indexer finished, save memory and cache
            idxr.writeCache();
            save_numTermsInDoc();
            
            idxr.save_memory();
            System.GC.Collect();
            save_tf_in_docs();

        }
        public void load_memory(string path)
        {
            if (idxr == null)
                idxr = new Indexer(cacheSize, heapSize, 20000, path);

            idxr.load_memory();
        }
     
        public void show_memory()
        {
            idxr.show_memory();
        }
       // Save docs dataset, each doc and it # terms
        private void save_numTermsInDoc()
        {
            string output = JsonConvert.SerializeObject(num_of_terms_in_doc, Formatting.Indented);
            System.IO.File.WriteAllText(_posting_path + "\\number_of_terms_in_doc.txt", output);
        }

        private void save_tf_in_docs()
        {
            int i = 0;
            foreach (ConcurrentDictionary<string, ConcurrentBag<Tuple<string, int>>> tf_doc in tf_all_docs)
            {
                string output = JsonConvert.SerializeObject(tf_doc, Formatting.Indented);
                System.IO.File.WriteAllText(_posting_path + "\\tf_in_doc" + i + ".txt", output);
                i++;
            }
        }

        private void waitforReadFileProcess(Task[] tasks)
        {
            Task.WaitAll(tasks);
            readFileProcessFinished = true;
        }
        private void waitforparserProcess(Task[] tasks)
        {
            Task.WaitAll(tasks);
            parserProcessFinished = true;
        }

        private void waitforindexerProcess(Task tasks)
        {
            Task.WaitAll(tasks);
            indexerProcessFinished = true;
        }


        public void insertBatch (string path_to_batch)
        {
            batch_files.Enqueue(path_to_batch);
        }
        public string getBatch()
        {
            string btch;
            batch_files.TryDequeue(out btch);
            return btch;
        }

        public void insertDoc (Document doc)
        {
            docs_to_parse.Enqueue(doc);
        }

        public Document getDoc()
        {
            Document d;
            docs_to_parse.TryDequeue(out d);
            return d;
        }

        public void insertTermDic (Dictionary<string,TermInDoc> dic)
        {
            termAfterParse.Enqueue(dic);
        }

        public Dictionary<string,TermInDoc> getTermDic()
        {
            Dictionary<string, TermInDoc> dic;
            termAfterParse.TryDequeue(out dic);
            return dic;
        }

    }
}
