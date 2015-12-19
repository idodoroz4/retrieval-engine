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
using System.Windows.Forms;
using System.Threading;

namespace RetEng
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller ctrl;
        public MainWindow()
        {
            InitializeComponent();
            




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

        private void choose_batch_path_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            batch_path.Text = fbd.SelectedPath;
        }

        private void choose_posting_path_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            posting_path.Text = fbd.SelectedPath;
        }

        private void start_btn_Click(object sender, RoutedEventArgs e)
        {
            int cache_size = 20 * 1000;
            int heap_size = 50;
            int numOfTermsInPosting = 10 * 1000;
            delete_all_textFiles();
            Console.WriteLine();
            ctrl = new Controller(cache_size, heap_size, numOfTermsInPosting, batch_path.Text, posting_path.Text,stem_cbx.IsChecked.Value);

            ctrl.initiate();
   
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            // ctrl.reset_dics();
            //string directoryPath = @"C:\test\";
            string extension = "*.txt";
            /*if (!Directory.Exists(directoryPath))
                return;*/

            DirectoryInfo di = new DirectoryInfo(posting_path.Text);
            FileInfo[] files = di.GetFiles(extension).Where(p => p.Extension.ToLowerInvariant() == ".txt").ToArray();
            foreach (FileInfo file in files)
            {
                file.Attributes = FileAttributes.Normal;
                File.Delete(file.FullName);
            }
        }

        private void load_if_btn_Click(object sender, RoutedEventArgs e)
        {
            ctrl = new Controller(0, 0, 0, batch_path.Text, posting_path.Text, stem_cbx.IsChecked.Value);
            ctrl.load_memory();
        }

        private void show_if_btn_Click(object sender, RoutedEventArgs e)
        {
            
            show_memory win = new show_memory();
            win.Show();
            win.memory_txt.Text = ctrl.show_memory();

        }
    }
}
