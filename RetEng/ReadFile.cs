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
           // string text = System.IO.File.ReadAllText(file_path);
            StringBuilder str = new StringBuilder(System.IO.File.ReadAllText(file_path));
            while (str.Length > 0)
            {
                int doc_idx_start2 = indexOf(str, "<DOC>");
                int doc_idx_end2 = indexOf(str, "</DOC>");
                string single_doc2 = str.ToString(doc_idx_start2 + 6, doc_idx_end2 - (doc_idx_start2 + 6));
                Document doc = new Document(single_doc2, offset, file_path);
                offset = doc_idx_end2 + 2;
                my_batch.Add(doc);
                str.Remove(doc_idx_start2, doc_idx_end2 + 6);
                /*
                int doc_idx_start = text.IndexOf("<DOC>", 0);
                int doc_idx_end = text.IndexOf("</DOC>");
                string single_doc = text.Substring(doc_idx_start + 6, doc_idx_end - (doc_idx_start + 6));
                Document doc = new Document(single_doc,offset, file_path);
                offset = doc_idx_end + 2;
                my_batch.Add(doc);
                text = text.Substring(doc_idx_end + 6, text.Length - (doc_idx_end + 6));*/
            }

            return my_batch;
               
      }


        private int indexOf(StringBuilder theStringBuilder, string value)
        {
            const int NOT_FOUND = -1;
          
            int count = theStringBuilder.Length;
            int len = value.Length;
            if (count < len)
            {
                return NOT_FOUND;
            }
            int loopEnd = count - len + 1;
           
            for (int loop = 0; loop < loopEnd; loop++)
            {
                bool found = true;
                for (int innerLoop = 0; innerLoop < len; innerLoop++)
                {
                    if (theStringBuilder[loop + innerLoop] != value[innerLoop])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return loop;
                }
            }
            return NOT_FOUND;
        }

    }
}
