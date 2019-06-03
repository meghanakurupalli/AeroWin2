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

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            if(listBox.SelectedItem.ToString()=="VP")
            {
                MessageBox.Show("VP selected");
            }
        }

        private void NC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void VP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        
    }
}
