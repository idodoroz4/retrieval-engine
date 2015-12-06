using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RetEng
{
    class ReadFile
    {
        public List<Document> get_docs(string file_path)
        {

            List <Document> my_batch = new List<Document>();
           
            int offset = 0;
            string text = System.IO.File.ReadAllText(file_path);
            while (text.IndexOf("<DOC>", 0) != -1)
            {
                    
                int doc_idx_start = text.IndexOf("<DOC>", 0);
                int doc_idx_end = text.IndexOf("</DOC>");
                string single_doc = text.Substring(doc_idx_start + 6, doc_idx_end - (doc_idx_start + 6));
                Document doc = new Document(single_doc,offset, file_path);
                offset = doc_idx_end + 2;
                my_batch.Add(doc);
                text = text.Substring(doc_idx_end + 6, text.Length - (doc_idx_end + 6));
            }

            return my_batch;
               
      }
            
           
        

    }
}
