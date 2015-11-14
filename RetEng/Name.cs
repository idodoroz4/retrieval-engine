using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Name : Term
    {
        private string _name;

        public Name(string name)
        {
            _name = name;
            _counts = 1;
        }
        public override string get_value()
        {
            return _name;
        }
    }
}
