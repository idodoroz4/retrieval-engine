using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace RetEng
{
    class Controller
    {
        private ConcurrentQueue<string> batch_files;
        private ConcurrentQueue<Document> docs_to_parse;
        private ConcurrentQueue<Dictionary<string, TermInDoc>> termAfterParse;
        private ConcurrentDictionary<string, int> num_of_terms_in_doc;
        private int num_of_threads_readFile;
        private int num_of_threads_Parser;
        private int num_of_threads_indexer;
        public bool readFileProcessFinished { get; private set; }
        public bool parserProcessFinished { get; private set; }
        public bool indexerProcessFinished { get; private set; }
        private int cacheSize;
        private int heapSize;

        private Task[] readFileTasks;
        private Task[] parserTasks;

        public Controller(int cache_size, int heap_size)
        {
            batch_files = new ConcurrentQueue<string>();
            docs_to_parse = new ConcurrentQueue<Document>();
            termAfterParse = new ConcurrentQueue<Dictionary<string, TermInDoc>>();
            num_of_terms_in_doc = new ConcurrentDictionary<string, int>();
            
            num_of_threads_readFile = 4;
            num_of_threads_Parser = 8;
            num_of_threads_indexer = 1;

            readFileProcessFinished = false;
            parserProcessFinished = false;
            indexerProcessFinished = false;

            cacheSize = cache_size;
            heapSize = heap_size;

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
            foreach (string file in Directory.GetFiles(@"D:\small_corpus"))
                insertBatch(file);


            Task status = Task.Run(() => { read_data(); });
            for (int i = 0; i < 2; i++)
                readFileAsync();
            parserAsync();

           /* readFileTasks = new Task[num_of_threads_readFile];
            for (int i=0; i< num_of_threads_readFile; i++)
            {
                
                Task readfile = Task.Run(() => { startReadFile(); });
                readFileTasks[i] = readfile;
            }

            parserTasks = new Task[num_of_threads_Parser];
            for (int i = 0; i < num_of_threads_Parser; i++)
            {
                /*Thread newThread = new Thread(startParser);
                newThread.Start();
                Task parser = Task.Run(() => { startParser(); });
                
                parserTasks[i] = parser;
            }*/

            
            Task indexer = Task.Run(() => { startindexer(cacheSize, heapSize); });
            //Task waitReadFile = Task.Run(() => { waitforReadFileProcess(readFileTasks); });    
            //Task waitParser = Task.Run(() => { waitforparserProcess(parserTasks); });

            Console.WriteLine("done!");
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
            Thread.Sleep(5000);
            while (true)
            {

                /*int readFilethreads = 0;
                int parserthreads = 0;
                foreach (Task t in readFileTasks)
                    if (!t.IsCompleted)
                        readFilethreads++;

                foreach (Task t in parserTasks)
                    if (!t.IsCompleted)
                        parserthreads++;
                */
                string str = "#Batch: " + batch_files.Count + "\nDocToParse: " + docs_to_parse.Count +
                   "\nTermAfterParse: " + termAfterParse.Count + "\nReadFileThreads: ";
                   /*+ readFilethreads + " from " + num_of_threads_readFile +
                   "\nParserThreads: " + parserthreads + "from" + num_of_threads_Parser;*/
                Console.WriteLine(str);
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
            Parser prs = new Parser();

            while (!readFileProcessFinished || !docs_to_parse.IsEmpty)
            {
                if (docs_to_parse.TryDequeue(out d))
                {  
                    TermDic = prs.parse_doc(d);
                    termAfterParse.Enqueue(TermDic);
                }
            }
        }

        private void startindexer(int cache_size, int heap_size)
        {
            Indexer idxr = new Indexer(cache_size, heap_size);
            Dictionary<string, TermInDoc> dicTerm;
            while (!parserProcessFinished || !termAfterParse.IsEmpty)
            {
                if (termAfterParse.TryDequeue(out dicTerm))
                {
                    List<TermInDoc> tempList = dicTerm.Values.ToList();
                   // num_of_terms_in_doc[tempList[0]._doc_id] = tempList.Count;
                    idxr.insert(dicTerm);
                }
            }
            // After Indexer finished, save memory and cache
            idxr.save_memory();
            idxr.save_cache();
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

        public void insert_num_of_terms_in_doc(string doc,int num_of_terms)
        {
            num_of_terms_in_doc[doc] = num_of_terms;
        }

        public int get_num_of_terms_in_doc(string doc)
        {
            return num_of_terms_in_doc[doc];
        }

    }
}
