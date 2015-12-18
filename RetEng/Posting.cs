using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetEng
{
    class Posting
    {
        public int ocurencess { get; private set; }
        public bool is_popular;
        // Reminder to add a max heap to gain access to the x-best terms 
        public Posting()
        {
            ocurencess = 1;
            is_popular = false;
        }
        public void increament()
        {
            ocurencess++;
        }
    }
}
