using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class TermInDoc
    {
        public string doc_id;
        public int ocurrences_in_doc;
        public bool is_in_headline;
        public List<int> positions;

        public TermInDoc(string docId)
        {
            doc_id = docId;
            ocurrences_in_doc = 0;
            is_in_headline = false;
            positions = new List<int>();
        }

    }
}
