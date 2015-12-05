using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Range : Term
    {
        public Number _first_num { get; private set; }
        public Number _last_num { get; private set; }
        public Range(Number first, Number last)
        {
            _first_num = first;
            _last_num = last;
            _counts = 1;
        }
        public override string get_value()
        {
            return _first_num.ToString() + " - " + _last_num.ToString();
        }

        public override string ToString()
        {
            return _first_num.ToString() + " - " + _last_num.ToString();
        }

    }
  


}
