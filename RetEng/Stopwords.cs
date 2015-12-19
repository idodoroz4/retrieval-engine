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
        public Dictionary<string, bool> swDic;

        public Stopwords(string path)
        {
            swDic = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string line in File.ReadLines(path + "\\stop_words.txt"))
                if (!(swDic.ContainsKey(line)))
                    swDic.Add(line, true);
        }


        public bool is_stopword (string value)
        {
            return swDic.ContainsKey(value);
        }
    }
}
