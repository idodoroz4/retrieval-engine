using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    // Abstract class which represent a term. a term can be a number, a date, a name and so on
    public abstract class Term
    {
        public int _counts { get; set; }
        public abstract string get_value();
    }
    
}
