using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetEng
{
    class Posting
    {
        public int ocurencess { get; private set; }
        // Reminder to add a max heap to gain access to the x-best terms 
        public Posting()
        {
            ocurencess = 1;
        }
        public void increament()
        {
            ocurencess++;
        }
    }
}
