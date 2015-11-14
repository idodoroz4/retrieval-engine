using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    class Batch
    {
        private List<Document> my_batch;
        public Batch()
        {
            my_batch = new List<Document>();
        }

        public void AddDoc (Document doc)
        {
            my_batch.Add(doc);
        }

        public Document GetDoc(int i)
        {
            return my_batch[i];
        }

        public void SetDoc(int i,Document doc)
        {
            my_batch[i] = doc;
        }


    }
}
