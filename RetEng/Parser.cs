﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetEng
{
    // class that presents a Parser to the Docs files, using rules.
   public class Parser
    {
        Dictionary<string, TermInDoc> termDic;
        
        StringBuilder doc_text;
        StringBuilder doc_title;
        string doc_date;
        string doc_id;
        string batch_id;
        int doc_offset;
        Stemmer stem;
        Stopwords stopword;
        bool _is_stemming;

        string _query;

        public Parser(string stopwords_path,bool stems)
        {
            
            stopword = new Stopwords(stopwords_path);
            stem = new Stemmer();
            _is_stemming = stems;
        }

        public Parser(string query, string stopwords_path, bool stems)
        {
            stopword = new Stopwords(stopwords_path);
            stem = new Stemmer();
            _is_stemming = stems;
            doc_id = "000";
            batch_id = "000";
            doc_text = new StringBuilder(query);
            doc_title = new StringBuilder(" ");
        }
        public List<string> parse_query()
        {
            StringBuilder temp_sb = new StringBuilder(doc_text.ToString());
            termDic = new Dictionary<string, TermInDoc>();

            dates_parse(doc_text.ToString());
            numbers_parse();
            replace_chars();
            names_parse();
            doc_text = temp_sb;
            remove_stopwords_text();
            regular_words_parse_text();

            return termDic.Keys.ToList();
        }

        public Dictionary<string,TermInDoc> parse_doc (Document doc) 
        {

            termDic = new Dictionary<string, TermInDoc>();
            doc_text = new StringBuilder(doc.text);
            doc_title = new StringBuilder(doc.title);
            doc_date = doc.date;
            doc_id = doc.id;
            batch_id = doc.batch_id;
            doc_offset = doc.doc_idx;
            dates_parse(doc_text.ToString());
            numbers_parse();
            replace_chars();
            names_parse();
            remove_stopwords_text();
            remove_stopwords_title();
            regular_words_parse_text();
            regular_words_parse_title();
      
            return termDic;

        }

         
        // Replcaing non alpha numeric and un importent characters;
        private void replace_chars()
        {
            for (int i=0; i< doc_text.Length; i++)
            {

                switch (doc_text[i])
                {
                    case '/':
                        doc_text[i] = ' ';
                        break;
                    case ':':
                        doc_text[i] = ' ';
                        break;
                    case '"':
                        doc_text[i] = ' ';
                        break;
                    case '*':
                        doc_text[i] = ' ';
                        break;
                    case '?':
                        doc_text[i] = ' ';
                        break;
                    case '>':
                        doc_text[i] = ' ';
                        break;
                    case '<':
                        doc_text[i] = ' ';
                        break;
                    case '|':
                        doc_text[i] = ' ';
                        break;
                    case '(':
                        doc_text[i] = ' ';
                        break;
                    case ')':
                        doc_text[i] = ' ';
                        break;
                    case ']':
                        doc_text[i] = ' ';
                        break;
                    case '[':
                        doc_text[i] = ' ';
                        break;
                    case '.':
                        doc_text[i] = ' ';
                        break;
                    case ',':
                        doc_text[i] = ' ';
                        break;
                    case '\'':
                        doc_text[i] = ' ';
                        break;
                    case '`':
                        doc_text[i] = ' ';
                        break;
                    case ';':
                        doc_text[i] = ' ';
                        break;
                    case '-':
                        doc_text[i] = ' ';
                        break;
                    case '!':
                        doc_text[i] = ' ';
                        break;
                    case '$':
                        doc_text[i] = ' ';
                        break;        
                    case '�':
                        doc_text[i] = ' ';
                        break;

                }
            }

            for (int i = 0; i < doc_title.Length; i++)
            {

                switch (doc_title[i])
                {
                    case '/':
                        doc_title[i] = ' ';
                        break;
                    case ':':
                        doc_title[i] = ' ';
                        break;
                    case '"':
                        doc_title[i] = ' ';
                        break;
                    case '*':
                        doc_title[i] = ' ';
                        break;
                    case '?':
                        doc_title[i] = ' ';
                        break;
                    case '>':
                        doc_title[i] = ' ';
                        break;
                    case '<':
                        doc_title[i] = ' ';
                        break;
                    case '|':
                        doc_title[i] = ' ';
                        break;
                    case '(':
                        doc_title[i] = ' ';
                        break;
                    case ')':
                        doc_title[i] = ' ';
                        break;
                    case ']':
                        doc_title[i] = ' ';
                        break;
                    case '[':
                        doc_title[i] = ' ';
                        break;
                    case '.':
                        doc_text[i] = ' ';
                        break;
                    case ',':
                        doc_text[i] = ' ';
                        break;
                    case '\'':
                        doc_text[i] = ' ';
                        break;
                    case '`':
                        doc_text[i] = ' ';
                        break;
                    case ';':
                        doc_text[i] = ' ';
                        break;
                    case '-':
                        doc_text[i] = ' ';
                        break;
                    case '!':
                        doc_text[i] = ' ';
                        break;
                    case '$':
                        doc_text[i] = ' ';
                        break;
                    case '�':
                        doc_text[i] = ' ';
                        break;

                        
                }
            }
      
        }
       // Remove stop words from the text
        private void remove_stopwords_text()
        {
            StringBuilder sb = new StringBuilder();
            string[] words = doc_text.ToString().Split(new char[0] , StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (!stopword.is_stopword(words[i]))
                {
                    sb.Append(" " + words[i]);

                }
            }
           

            doc_text = sb;
           
       }
        // Remove stop words from the title
        private void remove_stopwords_title()
        {
            StringBuilder sb = new StringBuilder();
            string[] words = doc_title.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (!stopword.is_stopword(words[i]))
                    sb.Append(" " + words[i]);
            }

            doc_title = sb;

        }
        // convert month name to short variable
        public short month_str_to_short(string month) 
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
       // Adding term to the Dictionary
        private void add_to_dic(string str,int pos,bool is_in_head)
        {
            if (termDic.ContainsKey(str))
            {
                termDic[str]._tf++;
                termDic[str]._positions.Add(pos);
                termDic[str]._is_in_headline = is_in_head;
            }
            else
            {
                termDic.Add(str, new TermInDoc(doc_id,batch_id));
                termDic[str]._term = str;
                termDic[str]._positions.Add(pos);
                termDic[str]._doc_id = doc_id;
                termDic[str]._tf++;
                termDic[str]._is_in_headline = is_in_head;
            }
           
        }
        // checks the pattern of the string, return true if it is the pattern and else otherwise
        // the function is is needed to deremine how the data is presented,so we can know how to store it in the Term object
        // for example: date- "31 May 1978" or "May 31, 1978"
        private bool is_matched(string pattern,string input) 

        {
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            return rgx.IsMatch(input);   
        }

        private void Remove_string(MatchCollection matches)
        {
            foreach (Match m in matches)
            {
                doc_text.Replace(m.Value, "");
            }
        }
        // change the text that matched to a regex to '#'s , so it won't match to another regex.
        private void mark_as_read(MatchCollection matches) 
  
        {
            foreach (Match m in matches)
            {
                if (m.Value[0] == ' ')
                    doc_text.Replace(m.Value.Trim(), create_nulls(m.Length - 1), m.Index + 1, m.Length - 1);
                else
                    doc_text.Replace(m.Value, create_nulls(m.Length), m.Index, m.Length);
            }
        }

        // return string of '#' to the function mark_as_read
        private string create_nulls(int len)
    
        {
            string nulls = "";
            for (int i = 0; i < len; i++)
                nulls += "#";
            return nulls;
        }

        // the function convert a complex number format like 234,234,234 or 34 5/7 to double
        private double complex_number_format_to_double (string num)

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
                num = rgx_numbers2.Match(num).ToString();
                if (num.Contains(' '))
                {
                    string[] str = num.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
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
            
            if (num.IndexOf("m") != -1 || num.IndexOf("M") != -1)
                return 'm';
            if (num.IndexOf("h") != -1 || num.IndexOf("H") != -1)
                return 'h';
            if (num.IndexOf("t") != -1 || num.IndexOf("T") != -1)
                return 't';
            if (num.IndexOf("b") != -1 || num.IndexOf("B") != -1)
                return 'b';

            return 'n'; // normal
           

        }
       // Parsing unruled words
        private void regular_words_parse_text()
        {
            string x = doc_text.ToString();
            string[] words = doc_text.ToString().Split(new char[0] ,StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i< words.Length; i++)
            {
                if (words[i].IndexOf('#') == -1)
                    if (_is_stemming)
                        add_to_dic(stem.stemTerm(words[i].ToLower()), 5 * i, false);
                    else
                        add_to_dic(words[i].ToLower(), 5 * i, false);
            }
            
        }
        private void regular_words_parse_title()
        {

            string pattern = @"[a-z]+([-'][a-z]+)?";
            Regex rgx_anyWord = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx_anyWord.Matches(doc_title.ToString());

            foreach (Match m in matches)
                if (_is_stemming)
                    add_to_dic(stem.stemTerm(m.Value.ToLower()), m.Index, true);
                else
                    add_to_dic(m.Value.ToLower(), m.Index, true);


        }
       // Parsing numbers by regex
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
            MatchCollection pr_matches = rgx_prices.Matches(doc_text.ToString());
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
            MatchCollection per_matches = rgx_percent.Matches(doc_text.ToString());
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
            MatchCollection rng_matches = rgx_range.Matches(doc_text.ToString());
            mark_as_read(rng_matches);

            foreach (Match m in rng_matches) // may change
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
            MatchCollection num_matches = rgx_numbers.Matches(doc_text.ToString());
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
            //StringBuilder sb = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            string[] words = doc_text.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            //Array.Sort(words);
            //List<string> lii = words.ToList<string>();
            //lii.Sort();
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i][0] == '"' && words[i][words[i].Length - 1] == '"')
                {
                   add_to_dic(words[i], i * 5, false);
                   continue;
                }
                int count = 0;
                while (Char.IsUpper(words[i][0] ))
                {
                    if (words[i].Length > 1)
                        if (Char.IsUpper(words[i][1]))
                            break;
                    count++;
                    
                    temp.Append(words[i] + " " );
                    i++;
                    
                    if (words.Length == i)
                        break;
                }
                if (count >= 2 )
                {
                    add_to_dic(temp.ToString(), i * 5, false);
                }

                temp.Clear();


            }

            // doc_text = sb;
        }


        // parse the dates on the text
        public void dates_parse(string text) 
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
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    Date d = new Date(short.Parse(str[0]),month_str_to_short((str[1])),short.Parse(str[2]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_2, m.Value))
                {
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    string day = str[0].Replace("th", "");
                    Date d = new Date(short.Parse(day), month_str_to_short(str[1]), short.Parse(str[2]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_3, m.Value)){
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    string year = "19" + str[2];
                    Date d = new Date(short.Parse(str[0]), month_str_to_short(str[1]), short.Parse(year));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_4, m.Value))
                {
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    Date d = new Date(short.Parse(str[0]), month_str_to_short(str[1]), 0);
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_7, m.Value))
                {
                    string[] str1 = m.Value.Split(',');
                    string[] str2 = str1[0].Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    string year = str1[1].Trim();
                    Date d = new Date(short.Parse(str2[1]), month_str_to_short(str2[0]), short.Parse(year));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_6, m.Value))
                {
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    Date d = new Date(1, month_str_to_short(str[0]), short.Parse(str[1]));
                    add_to_dic(d.ToString(), m.Index, false);
                }
                else if (is_matched(date_format_8, m.Value))
                {
                    string[] str = m.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
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
