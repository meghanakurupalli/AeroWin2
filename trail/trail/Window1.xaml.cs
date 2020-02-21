using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace trail
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    
    public partial class Window1 : Window
    {
        private MainWindow mainWindow;

        public Window1(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            int thatVal = mainWindow.some_val;
            Debug.Print("{0}",thatVal);
            
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.textbox.Text = "changed";
            mainWindow.printVal(40);
        }
    }
}
