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
// The main window for the GUI
namespace RetEng
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller ctrl;
        public MainWindow()
        { // init the controller before the gui statrs
            int cache_size = 100 * 1000;
            int heap_size = 50;
            ctrl = new Controller(cache_size, heap_size);
            InitializeComponent();
            
        }
        // delete all text file in directory path
        private void delete_all_textFiles()
        {
            string extension = "*.txt";

            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = di.GetFiles(extension).Where(p => p.Extension.ToLowerInvariant() == ".txt").ToArray();
            foreach (FileInfo file in files)
            {
                file.Attributes = FileAttributes.Normal;
                File.Delete(file.FullName);
            }
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
            try
            {
            ctrl.change_settings(posting_path.Text, batch_path.Text, stem_cbx.IsChecked.Value);
            ctrl.initiate();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Start faild \n, " + ex);
            }
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string extension = "*.txt";
                DirectoryInfo di = new DirectoryInfo(posting_path.Text);
                FileInfo[] files = di.GetFiles(extension).Where(p => p.Extension.ToLowerInvariant() == ".txt").ToArray();
                foreach (FileInfo file in files)
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Reset faild \n, " + ex);
            }
        }

        private void load_if_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ctrl.change_settings(posting_path.Text, batch_path.Text, stem_cbx.IsChecked.Value);
                ctrl.load_memory(posting_path.Text);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Load Inverted Files faild \n, " + ex);
            }
        }

        private void show_if_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ctrl.change_settings(posting_path.Text, batch_path.Text, stem_cbx.IsChecked.Value);
                ctrl.show_memory();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Show Inverted Files faild \n, " + ex);
            }

        }
    }
}
