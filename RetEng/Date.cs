using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Date : Term
    {
        private short _year;
        private short _month;
        private short _day;
        const short error = -1;
        public Date(short day, short month, short year) {
            _counts = 1;
            _day = day < 32 && day > 0 ? day : error;
            _month = month < 13 && month > 0 ? month : error;
            _year = year > -1 && year < 10000 ? year : error;
            if (_day == error && _month == error && _year == error) throw new ArgumentException ("Date not exists");
    }
        public override string get_value()
        {
            return _day.ToString() +"-"+ _month.ToString() + "-" + _year.ToString();
        }
    }
   
}
