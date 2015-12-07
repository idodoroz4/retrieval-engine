using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class TermInDoc
    {
        public string _doc_id;
        public int _ocurrences_in_doc;
        public bool _is_in_headline;
        public List<int> _positions;
        public string _batch_id;

        public TermInDoc(string doc_id, string batch_id)
        {
            _doc_id = doc_id;
            _ocurrences_in_doc = 0;
            _is_in_headline = false;
            _positions = new List<int>();
            _batch_id = batch_id;
        }

    }
}
