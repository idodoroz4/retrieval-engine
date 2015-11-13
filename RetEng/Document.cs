using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Document
    {
        private string id;
        private DateTime date;
        private string title;
        private string text;
        public Document(string doc)
        {
            int id_idx_start = doc.IndexOf("<DOCNO>", 0);
            int id_idx_end = doc.IndexOf("</DOCNO>");
            id = doc.Substring(id_idx_start, id_idx_end - id_idx_start);

            int date_idx_start = doc.IndexOf("<DATE1>", 0);
            int date_idx_end = doc.IndexOf("</DATE1>");
            date = DateTime.Parse(doc.Substring(date_idx_start, date_idx_end - date_idx_start));

            int title_idx_start = doc.IndexOf("<TI>", 0);
            int title_idx_end = doc.IndexOf("</TI>");
            title = doc.Substring(title_idx_start, title_idx_end - title_idx_start);

            int text_idx_start = doc.IndexOf("<TEXT>", 0);
            int text_idx_end = doc.IndexOf("</TEXT>");
            text = doc.Substring(text_idx_start, text_idx_end - text_idx_start);

        }

        public string get_id()
        {
            return this.id;       
        }

        public DateTime get_time()
        {
            return this.date;
        }

        public string get_title()
        {
            return this.title;
        }

        public string get_text()
        {
            return this.text;
        }
    }
}
