﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    // Represnt the posting data. this class get seriliaze to json and save on disk. collection of it is a posting file
    public class TermInDoc
    {
        public string _term;
        public string _doc_id;
        public int _tf;
        public bool _is_in_headline;
        public List<int> _positions;
        public string _batch_id;

        public TermInDoc(string doc_id, string batch_id)
        {
            _term = "";
            _doc_id = doc_id;
            _tf = 0;
            _is_in_headline = false;
            _positions = new List<int>();
            //_batch_id = batch_id;
        }

        /*public override bool Equals(object obj)
        {
            var item = obj as TermInDoc;
            if (item == null)
                return false;

            if (this._doc_id == item._doc_id)
                return true;

            return false;
        }*/

    }
}
