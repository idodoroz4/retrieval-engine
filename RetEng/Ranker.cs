using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetEng
{
    public class Ranker
    {
        string _query;
        Controller _ctrl;
        Dictionary<string, Dictionary<string, int>> _tf_all_docs;
        Dictionary<string, double> ranked_docs;
        List<string> _ExistingTerms;
        int _num_terms_in_query;
        int _N;
        Dictionary<string, Tuple<int, int, string, int>> _num_of_terms_in_doc;
        Dictionary<string, List<TermInDoc>> _myTerms;

        public Ranker(Controller ctrl, Dictionary<string, Dictionary<string, int>> tf_all_doc,
            List<string> ExistingTerm, int num_terms_in_query, 
            Dictionary<string, Tuple<int, int, string, int>> num_of_terms_in_doc,
            Dictionary<string, List<TermInDoc>> myTerms)
        {
            _ctrl = ctrl;
            _tf_all_docs = tf_all_doc;
            ranked_docs = new Dictionary<string, double>();
            _ExistingTerms = ExistingTerm;
            _num_terms_in_query = num_terms_in_query;
            _num_of_terms_in_doc = num_of_terms_in_doc;
            _N = _tf_all_docs.Count;
            _myTerms = myTerms;
        }

        public List<string> rank_relevent_docs(List<TermInDoc> relevent_docs)
        {
            foreach (TermInDoc item in relevent_docs)
            {
                ranked_docs.Add(item._doc_id, cossim_numerator(item) / cossim_denumerator(item));
            }

            List<string> best_docs = new List<string>();
            int i = 1;
            foreach (KeyValuePair<string, double> ranked in ranked_docs.OrderByDescending(key => key.Value))
            {
                best_docs.Add(ranked.Key);
                if (i == 50)
                    break;
                i++;
            }

            return best_docs;
        }
        private bool is_in_headline  (string doc_id,string term)
        {

            foreach (TermInDoc t in _myTerms[term])
                if (t._doc_id == doc_id)
                    return t._is_in_headline;

            return false;
        }
        private double cossim_numerator(TermInDoc item)
        {
            double sum = 0;
            
            foreach (string term in _ExistingTerms)
            {
                sum += tf_idf(item._doc_id, term);
                if (is_in_headline(item._doc_id, term))
                    sum *= 2;
            }

            if (sum == 0)
                Console.WriteLine();
            
            return sum;
        }

        private double cossim_denumerator(TermInDoc item)
        {
            double sum = 0;
            
            foreach (var item2 in _tf_all_docs[item._doc_id])
            {
                sum += Math.Pow(tf_idf(item._doc_id,  item2.Key),2);
            }
            
            return Math.Sqrt(sum * _num_terms_in_query);
        }
        private double tf_idf (string doc_id,string term)
        {
            if (doc_id == "FBIS4-68220 ")
                Console.WriteLine();
            if (!_tf_all_docs[doc_id].ContainsKey(term))
                return 0;

            double tf = (double)_tf_all_docs[doc_id][term] / (double)_num_of_terms_in_doc[doc_id].Item2;
            if (_tf_all_docs[doc_id][term] > _num_of_terms_in_doc[doc_id].Item2)
                Console.WriteLine();

            
            double df = _ctrl.idxr.main_dic[term].df;
            return (tf * (Math.Log((_N / df),2)));
        }
       
    }
}
