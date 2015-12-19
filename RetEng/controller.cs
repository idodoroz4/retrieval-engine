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
    class Controller
    {
        private ConcurrentQueue<string> batch_files;
        private ConcurrentQueue<Document> docs_to_parse;
        private ConcurrentQueue<Dictionary<string, TermInDoc>> termAfterParse;
        //private ConcurrentDictionary<string, int> num_of_terms_in_doc;
        private ConcurrentDictionary<string, Tuple<int,int,string>> num_of_terms_in_doc;
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

        private Indexer idxr;

        public Controller(int cache_size, int heap_size, int numOfTermsInPosting,string batch_path,string posting_path,bool stemming)
        {
            batch_files = new ConcurrentQueue<string>();
            docs_to_parse = new ConcurrentQueue<Document>();
            termAfterParse = new ConcurrentQueue<Dictionary<string, TermInDoc>>();
            //num_of_terms_in_doc = new ConcurrentDictionary<string, int>();
            num_of_terms_in_doc = new ConcurrentDictionary<string, Tuple<int, int, string>>();

            idxr = new Indexer(cache_size, heap_size, numOfTermsInPosting,posting_path);

            num_of_threads_readFile = 6;
            num_of_threads_Parser = 8;
            

            readFileProcessFinished = false;
            parserProcessFinished = false;
            indexerProcessFinished = false;

            cacheSize = cache_size;
            heapSize = heap_size;

            _batch_path = batch_path;
            _posting_path = posting_path;
            _stemming = stemming;

        }
        public void reset_dics()
        {
            num_of_terms_in_doc.Clear();
            batch_files = new ConcurrentQueue<string>();
            docs_to_parse = new ConcurrentQueue<Document>();
            termAfterParse = new ConcurrentQueue<Dictionary<string, TermInDoc>>();
        }
        
        public string data()
        {
            return "#Batch: " + batch_files.Count +"\nDocToParse: " + docs_to_parse.Count + 
                   "\nTermAfterParse: " + termAfterParse.Count;
        }
        public void initiate()
        {
            // insert the batch files into the "batch_files" queue
            //foreach (string file in Directory.GetFiles(@"D:\corpus"))
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


        private Task readFileTask()
        {
            return Task.Run(() => { startReadFile(); });
        }

        private Task ParserTask()
        {
            return Task.Run(() => { startParser(); });
        }

        private async void readFileAsync()
        {
            await readFileTask();
            readFileProcessFinished = true;
        }

        private async void parserAsync()
        {
            await ParserTask();
            parserProcessFinished = true;
        }

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

                //if (!readFileProcessFinished || !parserProcessFinished || !indexerProcessFinished || idxr.cache.Count > 0)
                //{
                    DateTime d2 = DateTime.Now;
                    TimeSpan d3 = d2.Subtract(d1);
                    string str = d3.ToString() + "\n#Batch: " + batch_files.Count + "\nDocToParse: " + docs_to_parse.Count +
                       "\nTermAfterParse: " + termAfterParse.Count + "\nReadFileThreads: "
                       + readFilethreads + " from " + num_of_threads_readFile +
                       "\nParserThreads: " + parserthreads + " from " + num_of_threads_Parser +
                       "\ncache Size: " + idxr.cache.Count +
                       "\nqueue Size:" + idxr.queue.Count;

                    Console.WriteLine(str);
                //}
                /*else
                {
                    break;
                }*/
                Thread.Sleep(5000);
            }

        }
        
        
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

        private void startParser()
        {
            Document d;
           Dictionary<string, TermInDoc> TermDic;
            Parser prs = new Parser(_batch_path,_stemming);

            while (!readFileProcessFinished || !docs_to_parse.IsEmpty)
            {
                if (docs_to_parse.TryDequeue(out d))
                {  
                    TermDic = prs.parse_doc(d);
                    Tuple<int,int,string> t = get_max_tf(TermDic);
                    num_of_terms_in_doc.TryAdd(d.id, t);
                    termAfterParse.Enqueue(TermDic);
                }
            }
        }

        private Tuple<int,int,string> get_max_tf(Dictionary<string, TermInDoc> termDic)
        {
            int max = 0;
            string term = "";
            foreach (var item in termDic)
            {
                if (item.Value._tf > max)
                {
                    max = item.Value._tf;
                    term = item.Key;
                }
                
            }
            return Tuple.Create<int,int, string>(termDic.Count,max, term); 
        }

        private void startindexer(int cache_size, int heap_size)
        {
            
            Dictionary<string, TermInDoc> dicTerm;
            while (!parserProcessFinished || !termAfterParse.IsEmpty)
            {
                if (termAfterParse.TryDequeue(out dicTerm))
                {
                    //List<TermInDoc> tempList = dicTerm.Values.ToList();
                   // num_of_terms_in_doc[tempList[0]._doc_id] = tempList.Count;
                    idxr.insert(dicTerm);
                }
            }
            // After Indexer finished, save memory and cache
            idxr.writeCache();
            idxr.save_memory();
            save_numTermsInDoc();
            
            /*Task[] saveCacheTasks = new Task[6];
            for (int i = 0; i < 6; i++)
            {
                Task saveCache = Task.Run(() => { startSavingCache(); });
                saveCacheTasks[i] = saveCache;
            }
            Console.WriteLine("finished!!!!");*/
        }
        public void load_memory()
        {
            idxr.load_memory();
        }
        public string show_memory()
        {
            return idxr.show_memory();
        }

        private void save_numTermsInDoc()
        {
            string output = JsonConvert.SerializeObject(num_of_terms_in_doc, Formatting.Indented);
            System.IO.File.WriteAllText("number_of_terms_in_doc.txt", output);
        }

        /*private void startSavingCache()
        {
            while (idxr.queue.Count > 0)
            {
                string key;
                idxr.queue.TryDequeue(out key);
                idxr.writeCache(key);

            }
        }*/
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
