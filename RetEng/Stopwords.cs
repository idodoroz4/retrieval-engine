using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    public sealed class Stopwords
    {
        private string stopword_file_path = @"Resources\stop_words.txt";
        private static volatile Stopwords instance;
        private static object syncRoot = new Object();
        public static Dictionary<string, bool> swDic;

        private Stopwords()
        {
            swDic = new Dictionary<string, bool>();
            foreach (string line in File.ReadLines(stopword_file_path))
                if (!(swDic.ContainsKey(line)))
                    swDic.Add(line, true);
        }

        public static Stopwords Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Stopwords();
                    }
                }

                return instance;
            }
        }
    }
}
