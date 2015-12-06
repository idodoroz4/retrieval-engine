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
        private ConcurrentDictionary<Document, int> num_of_terms_in_doc;
        private int num_of_threads_readFile;
        private int num_of_threads_Parser;
        private int num_of_threads_indexer;
        public bool readFileProcessFinished { get; private set; }
        public bool parserProcessFinished { get; private set; }
        public bool indexerProcessFinished { get; private set; }

    public Controller()
        {
            batch_files = new ConcurrentQueue<string>();
            docs_to_parse = new ConcurrentQueue<Document>();
            termAfterParse = new ConcurrentQueue<Dictionary<string, TermInDoc>>();
            num_of_terms_in_doc = new ConcurrentDictionary<Document, int>();
            
            num_of_threads_readFile = 10;
            num_of_threads_Parser = 10;
            num_of_threads_indexer = 1;

            readFileProcessFinished = false;
            parserProcessFinished = false;
            indexerProcessFinished = false;

        }

        public void initiate()
        {
            // insert the batch files into the "batch_files" queue
            foreach (string file in Directory.GetFiles(@"D:\corpus"))
                insertBatch(file);

            Task[] readFileTasks = new Task[num_of_threads_readFile];
            for (int i=0; i< num_of_threads_readFile; i++)
            {
                Task readfile = Task.Run(() => { startReadFile(); });
                readFileTasks[i] = readfile;
            }

            Task[] parserTasks = new Task[num_of_threads_Parser];
            for (int i = 0; i < num_of_threads_Parser; i++)
            {
                Task parser = Task.Run(() => { startParser(); });
                readFileTasks[i] = parser;
            }

            //Task indexer = 

            Task waitReadFile = Task.Run(() => { waitforReadFileProcess(readFileTasks); });
            Task waitParser = Task.Run(() => { waitforparserProcess(parserTasks); });


        }
        

        private void startReadFile()
        {
            string btch;
            List<Document> doc_list;
            while (!batch_files.IsEmpty)
            {
                if (batch_files.TryDequeue(out btch)){
                    ReadFile rf = new ReadFile();
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
            while (!readFileProcessFinished && !docs_to_parse.IsEmpty)
            {
                if (docs_to_parse.TryDequeue(out d))
                {
                    Parser prs = new Parser();
                    TermDic = prs.parse_doc(d);
                    termAfterParse.Enqueue(TermDic);
                }
            }
        }
        private void startindexer(int cache_size, int heap_size)
        {
            while (!parserProcessFinished && termAfterParse.Count > 0)
            {
                Indexer idxr = new Indexer(cache_size, heap_size);
                //idxr.insert();
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

        public void insert_num_of_terms_in_doc(Document doc,int num_of_terms)
        {
            num_of_terms_in_doc[doc] = num_of_terms;
        }

        public int get_num_of_terms_in_doc(Document doc)
        {
            return num_of_terms_in_doc[doc];
        }

    }
}
