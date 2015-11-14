using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Range : Term
    {
        private Number _first_num;
        private Number _last_num;

        public string get_value()
        {
            return _first_num.ToString() + " - " + _last_num.ToString();
        }

    }
  


}
