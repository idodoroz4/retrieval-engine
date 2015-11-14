using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Number : Term
    {
        public bool _is_precent { get; private set; }
        public bool _is_price { get; private set; }
        public double _value  { get; private set;}
        public string _size { get; private set; }

        public Number(bool is_precent, bool is_price, double value, string size )
        {
            _is_precent = is_precent;
            _is_price = is_price;
            _value = value;
            _size = 
        }
        public string get_value()
        {
            return _value.ToString();
        }
    }
}
