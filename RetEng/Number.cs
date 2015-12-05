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

        // h - hundreds, m - million, b - billion, t - trillion, n - none
        public char _multi { get; private set; }


        public Number(bool is_percent, bool is_price, double value, string size ,char multi)
        {
            _is_percent = is_percent;
            _is_price = is_price;
            _value = value;
            _size = size;
            _multi = multi;
            _counts = 1;
        }
        public override string get_value()
        {
            return _value.ToString();
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
