using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Document
    {
        public string id { get; private set; }
        public string date { get; private set; }
        public string title { get; private set; }
        public string text { get; private set; }

        public Document(string doc)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            int id_idx_start = doc.IndexOf("<DOCNO>", 0);
            int id_idx_end = doc.IndexOf("</DOCNO>");
            id = doc.Substring(id_idx_start + 8, id_idx_end - (id_idx_start + 8));

            int date_idx_start = doc.IndexOf("<DATE1>", 0);
            int date_idx_end = doc.IndexOf("</DATE1>");
            date = doc.Substring(date_idx_start + 8, date_idx_end - (date_idx_start + 8));

            int title_idx_start = doc.IndexOf("<TI>", 0);
            int title_idx_end = doc.IndexOf("</TI>");
            title = doc.Substring(title_idx_start + 5, title_idx_end - (title_idx_start + 5));

            int text_idx_start = doc.IndexOf("<TEXT>", 0);
            int text_idx_end = doc.IndexOf("</TEXT>");
            text = doc.Substring(text_idx_start + 7, text_idx_end - (text_idx_start + 7));
        }

        

        public override string ToString()
        {

            return "id: " + id + " \r\nDate: " + date.ToString() +
                "\r\nTitle: " + title + "\r\nText: " + text; 
        }
        public void empty_text()
        {
            this.text = "";
        }

    }
}
