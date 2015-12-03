using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RetEng
{
    public class ReadFile
    {
        public static void get_docs(string path)
        {
            if (! Directory.Exists(path) ){ // checks if the given path not valid
                
            }
            string[] files = Directory.GetFiles(path);
            foreach (string file in files){
                Batch single_batch = new Batch();

                string text = System.IO.File.ReadAllText(file);
                while (text.IndexOf("<DOC>", 0) != -1)
                {
                    
                    int doc_idx_start = text.IndexOf("<DOC>", 0);
                    int doc_idx_end = text.IndexOf("</DOC>");
                    string single_doc = text.Substring(doc_idx_start + 6, doc_idx_end - (doc_idx_start + 6));
                    Document doc = new Document(single_doc);
                    /* for testing only */
                    Parser.parse_doc(doc);
                    /* end of testing */
                    single_batch.AddDoc(doc);
                    text = text.Substring(doc_idx_end + 6, text.Length - (doc_idx_end + 6));
                }
               
            }
            
           
        }

    }
}
