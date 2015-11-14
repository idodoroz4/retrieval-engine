using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    public abstract class Term
    {
        public int _counts { get; set; }
        public abstract string get_value();
    }
    
}
