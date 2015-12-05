using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetEng
{
    class Parser
    {
        Dictionary<string, TermInDoc> termDic;
        
        static string doc_text;
        static string doc_title;
        static string doc_date;
        static string doc_id;
        static string batch_id;
        static int doc_offset;
        Stemmer stem;
        

        public void parse_doc (Document doc) 
        {
            termDic = new Dictionary<string, TermInDoc>();
            doc_text = doc.text;
            doc_title = doc.title;
            doc_date = doc.date;
            doc_id = doc.id;
            string original = doc.text;
            batch_id = doc.batch_id;
            doc_offset = doc.doc_idx;
            stem = new Stemmer();
            

            Stopwords stopword = Stopwords.Instance;

            dates_parse(doc_text);
            names_parse();
            numbers_parse();

            remove_stopwords_text();
            remove_stopwords_title();
            regular_words_parse(doc_text);
            regular_words_parse(doc_title);


            Console.WriteLine("finish!");

        }


        private void remove_stopwords_text()
        {
            Regex rgx_words = new Regex(@"[a-z']+", RegexOptions.IgnoreCase);
            MatchCollection matches = rgx_words.Matches(doc_text);
            
            foreach (Match m in matches)
            {
               if (Stopwords.is_stopword(m.Value))
                {
                    string before_match = doc_text.Substring(0, m.Index);
                    string after_match = doc_text.Substring(m.Index + m.Length);
                    doc_text = before_match + create_nulls(m.Length) + after_match;
                }
            }
            
        }
        private void remove_stopwords_title()
        {
            Regex rgx_words = new Regex(@"[a-z']+", RegexOptions.IgnoreCase);
            MatchCollection matches = rgx_words.Matches(doc_title);

            foreach (Match m in matches)
            {
                if (Stopwords.is_stopword(m.Value))
                {

                    string before_match = doc_title.Substring(0, m.Index);
                    string after_match = doc_title.Substring(m.Index + m.Length);
                    doc_title = before_match + create_nulls(m.Length) + after_match;
                }
            }
            
        }

        private short month_str_to_short(string month) // convert month name to short variable
        {
            if ((String.Compare(month, "January", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "jan", StringComparison.OrdinalIgnoreCase) == 0))
                return 1;

            if ((String.Compare(month, "February", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "feb", StringComparison.OrdinalIgnoreCase) == 0))
                return 2;

            if ((String.Compare(month, "March", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "mar", StringComparison.OrdinalIgnoreCase) == 0))
                return 3;

            if ((String.Compare(month, "April", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "apr", StringComparison.OrdinalIgnoreCase) == 0))
                return 4;

            if (String.Compare(month, "May", StringComparison.OrdinalIgnoreCase) == 0) 
                return 5;

            if ((String.Compare(month, "June", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "jun", StringComparison.OrdinalIgnoreCase) == 0))
                return 6;

            if ((String.Compare(month, "July", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "jul", StringComparison.OrdinalIgnoreCase) == 0))
                return 7;

            if ((String.Compare(month, "August", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "aug", StringComparison.OrdinalIgnoreCase) == 0))
                return 8;

            if ((String.Compare(month, "September", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "sep", StringComparison.OrdinalIgnoreCase) == 0))
                return 9;

            if ((String.Compare(month, "October", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "oct", StringComparison.OrdinalIgnoreCase) == 0))
                return 10;

            if ((String.Compare(month, "November", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "nov", StringComparison.OrdinalIgnoreCase) == 0))
                return 11;

            if ((String.Compare(month, "December", StringComparison.OrdinalIgnoreCase) == 0) ||
                (String.Compare(month, "dec", StringComparison.OrdinalIgnoreCase) == 0))
                return 12;

            return 0;

        }

        private void add_to_dic(string str,int pos,bool is_in_head)
        {
            if (termDic.ContainsKey(str))
            {
                termDic[str].ocurrences_in_doc++;
                termDic[str].positions.Add(pos);
                termDic[str].is_in_headline = is_in_head;
            }
            else
            {
                termDic.Add(str, new TermInDoc(doc_id));
                termDic[str].positions.Add(pos);
                termDic[str].doc_id = doc_id;
                termDic[str].ocurrences_in_doc++;
                termDic[str].is_in_headline = is_in_head;
            }
        }

        private bool is_matched(string pattern,string input) 
            // checks the pattern of the string, return true if it is the pattern and else otherwise
            // the function is is needed to deremine how the data is presented,so we can know how to store it in the Term object
            // for example: date- "31 May 1978" or "May 31, 1978"
        {
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            return rgx.IsMatch(input);   
        }
        private void mark_as_read(MatchCollection matches) 
            // change the text that matched to a regex to '#'s , so it won't match to another regex.
        {
            foreach (Match m in matches)
            {
                string before_match = doc_text.Substring(0, m.Index);
                string after_match = doc_text.Substring(m.Index + m.Length);
                doc_text = before_match + create_nulls(m.Length) + after_match;
            }
        }

        private string create_nulls(int len)
        // return string of '#' to the function mark_as_read
        {
            string nulls = "";
            for (int i = 0; i < len; i++)
                nulls += "#";
            return nulls;
        }


        private double complex_number_format_to_double (string num)
            // the function convert a complex number format like 234,234,234 or 34 5/7 to double
        {
            string regular_num = @"\d+";
            string non_rational_num = @"\d+\.\d+";
            string rational_num = @"(\d+\s+)?\d+/\d+";
            string num_with_commas = @"\d{1,3}(,\d{3})+";
            string all_num_formats = @"(" + num_with_commas + "|" + rational_num + "|" + non_rational_num + "|" + regular_num + ")";

            Regex rgx_numbers = new Regex(num_with_commas, RegexOptions.IgnoreCase);
            Regex rgx_numbers2 = new Regex(rational_num, RegexOptions.IgnoreCase);

            if (rgx_numbers.IsMatch(num))
            {
                string no_commas = num.Replace(",", "");
                return double.Parse(new Regex(all_num_formats, RegexOptions.IgnoreCase).Match(no_commas).Value);
            }


            else if (rgx_numbers2.IsMatch(num))
            {
                if (num.Contains(' '))
                {
                    string[] str = num.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    double coefficient = double.Parse(str[0]);
                    string[] str2 = str[1].Split('/');
                    double numinator = double.Parse(str2[0]);
                    double decriminator = double.Parse(str2[1]);
                    return ((coefficient * decriminator + numinator) / decriminator);
                }

                string[] str3 = num.Split('/');
                double numinator2 = double.Parse(str3[0]);
                double decriminator2 = double.Parse(str3[1]);
                return numinator2 / decriminator2;
            }

            else
                return double.Parse(new Regex(all_num_formats, RegexOptions.IgnoreCase).Match(num).Value);


        }

        private char big_number_identifier(string num)
        {
            string regular_num = @"\d+";
            string non_rational_num = @"\d+\.\d+";
            string rational_num = @"(\d+\s+)?\d+/\d+";
            string num_with_commas = @"\d{1,3}(,\d{3})+";
            string all_num_formats = @"(" + num_with_commas + "|" + rational_num + "|" + non_rational_num + "|" + regular_num + ")";

            Regex rgx_million = new Regex(all_num_formats + @"(\s+million)|m", RegexOptions.IgnoreCase);
            Regex rgx_billion = new Regex(all_num_formats + @"(\s+billion)|bn", RegexOptions.IgnoreCase);
            Regex rgx_trillion = new Regex(all_num_formats + @"(\s+trillion)", RegexOptions.IgnoreCase);
            Regex rgx_hundreds = new Regex(all_num_formats + @"(\s+hundreds)", RegexOptions.IgnoreCase);

            string hun = rgx_hundreds.Match(num).Value;
            string mil = rgx_million.Match(num).Value;
            string bil = rgx_billion.Match(num).Value;
            string tril = rgx_trillion.Match(num).Value;
            if (hun != null)
                return 'h';
            if (mil != null)
                return 'm';
            if (bil != null)
                return 'b';
            if (tril != null)
                return 't';

            return 'n';

        }

        private void regular_words_parse(string text)
        {
            Stemmer stm = new Stemmer();
            string pattern = @"[a-z]+([-'][a-z]+)?";
            Regex rgx_anyWord = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx_anyWord.Matches(text);

            foreach (Match m in matches)
            {
                string word = stm.stemTerm(m.Value);
                if (text.Equals(doc_text))
                    add_to_dic(word, m.Index,false);
                else if (text.Equals(doc_title))
                    add_to_dic(word, m.Index, true);
            }
            
        }
        private void numbers_parse()
        {
            // numbers
            string regular_num = @"\d+";
            string non_rational_num = @"\d+\.\d+";
            string rational_num = @"(\d+\s+)?\d+/\d+";
            string num_with_commas = @"\d{1,3}(,\d{3})+";
            string all_num_formats = @"(" + num_with_commas + "|" + rational_num + "|" + non_rational_num + "|" + regular_num + ")";
            string numbers = all_num_formats + @"(\s+(million|billion|trillion|hundreds))?";

            //ranges
            string range_format_1 = all_num_formats + @"\s*-\s*" + all_num_formats;
            string range_format_2 = @"between\s+" + all_num_formats + @"\s+and\s+" + all_num_formats;
            string all_ranges = "(" + range_format_1 + "|" + range_format_2 + ")";

            //percents
            string percent = all_num_formats + @"(%|\s+percentage|\s+percents|\s+percent)";

            //prices
            string price_before_num = @"(Dollars\s+|\$)";
            string price_after_num = @"(\s+million|\s+billion|m|bn)?";
            string prices = price_before_num + all_num_formats + price_after_num;

            // price
            Regex rgx_prices = new Regex(prices, RegexOptions.IgnoreCase);
            MatchCollection pr_matches = rgx_prices.Matches(doc_text);
            mark_as_read(pr_matches);

            foreach (Match m in pr_matches)
            {
                Regex rgx = new Regex(all_num_formats, RegexOptions.IgnoreCase);
                Match m2 = rgx.Match(m.Value);
                Number num = new Number(false, true, complex_number_format_to_double(m2.Value),m2.Length.ToString(),big_number_identifier(m.Value));
                add_to_dic(num.ToString(), m.Index,false);
            }

            // percent
            Regex rgx_percent = new Regex(percent, RegexOptions.IgnoreCase);
            MatchCollection per_matches = rgx_percent.Matches(doc_text);
            mark_as_read(per_matches);

            foreach (Match m in per_matches)
            {
                Regex rgx = new Regex(all_num_formats, RegexOptions.IgnoreCase);
                Match m2 = rgx.Match(m.Value);
                Number num = new Number(true, false, complex_number_format_to_double(m2.Value), m2.Length.ToString(), big_number_identifier(m.Value));
                add_to_dic(num.ToString(), m.Index, false);
            }

            // range
            Regex rgx_range = new Regex(all_ranges, RegexOptions.IgnoreCase);
            MatchCollection rng_matches = rgx_range.Matches(doc_text);
            mark_as_read(rng_matches);

            foreach (Match m in rng_matches)
            {
                Regex rgx = new Regex(all_num_formats, RegexOptions.IgnoreCase);
                MatchCollection nums_in_ranges = rgx.Matches(m.Value);
                Number num1 = new Number(false, false, complex_number_format_to_double(nums_in_ranges[0].Value), nums_in_ranges[0].Length.ToString(), big_number_identifier(m.Value));
                Number num2 = new Number(false, false, complex_number_format_to_double(nums_in_ranges[1].Value), nums_in_ranges[1].Length.ToString(), big_number_identifier(m.Value));
                Range rng = new Range(num1, num2);
                add_to_dic(rng.ToString(), m.Index, false);
            }

            // numbers
            Regex rgx_numbers = new Regex(numbers, RegexOptions.IgnoreCase);
            MatchCollection num_matches = rgx_numbers.Matches(doc_text);
            mark_as_read(num_matches);

            foreach (Match m in num_matches)
            {
                Number num = new Number(false, false, complex_number_format_to_double(m.Value), m.Length.ToString(), big_number_identifier(m.Value));
                add_to_dic(num.ToString(), m.Index, false);
            }

        }

        private void names_parse() // parse the names 
            //checked
        {
            //names
            string names = @"[A-Z][a-z]+( [A-Z][a-z]+)+";

            Regex rgx_names = new Regex(names);
            MatchCollection names_matches = rgx_names.Matches(doc_text);
            mark_as_read(names_matches);

            foreach (Match m in names_matches)
            {
                Name name = new Name(m.Value);
                add_to_dic(name.ToString(), m.Index, false);
            }
        }



        public void dates_parse(string text) // parse the dates on the text
        {

            //dates
            string months = @"(January|February|March|April|May|June|July|August|September|October|November|December|" + 
                              "jan|feb|mar|apr|jun|jul|aug|sep|oct|nov|dec)";

            string days = @"((([0-2])?\d)|30|31)";
            string d4_years = @"([1-2]\d{3})";
            string d2_years = @"\d\d";

            string date_format_1_2 = days + "(th)?" + @"\s+" + months + @"\s+" + d4_years; 
            string date_format_3 = days + @"\s+" + months + @"\s+" + d2_years;
            string date_format_4 = days + @"\s+" + months;
            string date_format_7 = months + @"\s+" + days + @",\s+" + d4_years;
            string date_format_6 = months + @"\s+" + d4_years;
            string date_format_8 = months + @"\s+" + days;      
            string date_format_5 = @"\s" + d4_years + "|" + d4_years + @"\s";
            string all_dates = date_format_1_2 + "|" + date_format_3 + "|" + date_format_4 + "|" + date_format_7 +
                            "|" + date_format_6 + "|" + date_format_8 + "|" + date_format_5;

            Regex rgx_dates = new Regex(all_dates, RegexOptions.IgnoreCase);
            MatchCollection dates_matches = rgx_dates.Matches(text);
            mark_as_read(dates_matches);
            
            // creating for each match a Term and put it to the dictionary
            string date_format_1 = days + @"\s+" + months + @"\s+" + d4_years;
            string date_format_2 = days + "(th)" + @"\s+" + months + @"\s+" + d4_years;
            foreach (Match m in dates_matches)
            {
                if (is_matched(date_format_1, m.Value))
                {
                    string[] str = m.Value.Split(' ');
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    Date d = new Date(short.Parse(str[0]),month_str_to_short((str[1])),short.Parse(str[2]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_2, m.Value))
                {
                    string[] str = m.Value.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string day = str[0].Replace("th", "");
                    Date d = new Date(short.Parse(day), month_str_to_short(str[1]), short.Parse(str[2]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_3, m.Value)){
                    string[] str = m.Value.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string year = "19" + str[2];
                    Date d = new Date(short.Parse(str[0]), month_str_to_short(str[1]), short.Parse(year));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_4, m.Value))
                {
                    string[] str = m.Value.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    Date d = new Date(short.Parse(str[0]), month_str_to_short(str[1]), 0);
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_7, m.Value))
                {
                    string[] str1 = m.Value.Split(',');
                    string[] str2 = str1[0].Split(null);
                    str2 = str2.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string year = str1[1].Trim();
                    Date d = new Date(short.Parse(str2[1]), month_str_to_short(str2[0]), short.Parse(year));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_6, m.Value))
                {
                    string[] str = m.Value.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    Date d = new Date(1, month_str_to_short(str[0]), short.Parse(str[1]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_8, m.Value))
                {
                    string[] str = m.Value.Split(null);
                    str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    Date d = new Date(short.Parse(str[1]), month_str_to_short(str[0]), 0);
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_5, m.Value))
                {
                    string year = m.Value.Trim();
                    Date d = new Date(1, 1, short.Parse(year));
                    add_to_dic(d.ToString(), m.Index, false);
                }

            }

        }
    }
}
