using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    public class Stopwords
    {
        private string stopword_file_path = @"Resources\stop_words.txt";
        public Dictionary<string, bool> swDic;

        public Stopwords()
        {
            swDic = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string line in File.ReadLines(stopword_file_path))
                if (!(swDic.ContainsKey(line)))
                    swDic.Add(line, true);
        }


        public bool is_stopword (string value)
        {
            return swDic.ContainsKey(value);
        }
    }
}
