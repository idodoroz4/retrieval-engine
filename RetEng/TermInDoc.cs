using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class TermInDoc
    {
        public Term t;
        public string doc_id;
        public int ocurrences_in_doc;
        public bool is_in_headline;
        public string batch_name;
        public int offset;
        public List<int> positions;

        public TermInDoc(string docId,string bName, int nDoc)
        {
            doc_id = docId;
            ocurrences_in_doc = 0;
            is_in_headline = false;
            batch_name = bName;
            offset = nDoc;
            positions = new List<int>();
        }

    }
}
