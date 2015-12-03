using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Number : Term
    {
        public bool _is_percent { get; private set; }
        public bool _is_price { get; private set; }
        public double _value  { get; private set;}
        public string _size { get; private set; }

        public Number(bool is_percent, bool is_price, double value, string size )
        {
            _is_percent = is_percent;
            _is_price = is_price;
            _value = value;
            _size = size;
            _counts = 1;
        }
        public override string get_value()
        {
            return _value.ToString();
        }
    }
}
