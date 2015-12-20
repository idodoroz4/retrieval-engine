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
using System.Windows.Shapes;

namespace RetEng
{
    /// <summary>
    /// Interaction logic for show_memory.xaml
    /// </summary>
    public partial class show_memory : Window
    {
        Controller _ctrl;
        public show_memory(Controller ctrl)
        {
            _ctrl = ctrl;
            InitializeComponent();
        }

   
    }
}
