using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetEng
{
    // Represent the link to the posting file, saved on the inverted files
    public class Posting
    {
        public int df;
        public bool is_popular;
        public List<string> posting_locations;
        // Reminder to add a max heap to gain access to the x-best terms 
        public Posting()
        {
            posting_locations = new List<string>();
            df = 1;
            is_popular = false;
        }
        public void increament()
        {
            df++;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("locations: ");
            foreach (string str in posting_locations)
                sb.Append(str + ",");
            sb.Append("\nDF: " + df);
            return sb.ToString();
        }
    }
}
