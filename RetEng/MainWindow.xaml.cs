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
using System.IO;
using System.Threading;

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

            int cache_size = 20000;
            int heap_size = 50;
            delete_all_textFiles();
            Console.WriteLine();
            Controller ctrl = new Controller(cache_size,heap_size);
            //Thread t = new Thread(() => read_data(ctrl));
            
            ctrl.initiate();
            //t.Start();



        }
        private void read_data(Controller c)
        {
            while (true)
            {
                textBox.Text = c.data();
                Thread.Sleep(5000);
            }
            
        }
        private void delete_all_textFiles()
        {
            //string directoryPath = @"C:\test\";
            string extension = "*.txt";
            /*if (!Directory.Exists(directoryPath))
                return;*/

            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = di.GetFiles(extension).Where(p => p.Extension.ToLowerInvariant() == ".txt").ToArray();
            foreach (FileInfo file in files)
            {
                file.Attributes = FileAttributes.Normal;
                File.Delete(file.FullName);
            }
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
