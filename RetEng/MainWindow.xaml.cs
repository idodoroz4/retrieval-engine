using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace RetEng
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReadFile.get_docs(@"D:\corpus");
            





        }

        private void reg (string pattern)
        {
            //string sText = " $323,000 dollars 2,444 $4.1312312334 million $33bn dollars 3/4565m $2,000 billion 45455/3 56 and dollars 900";
            string sText = "16th may 1990 23 apr 2015 22 dec 91 31 June, 1898, February 1899, dec 28, 1777 - dec 21";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(sText);

            Console.WriteLine();
        }
       
    }
}
