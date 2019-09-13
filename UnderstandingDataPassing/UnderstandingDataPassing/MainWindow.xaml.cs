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

namespace UnderstandingDataPassing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime newTime;
        public MainWindow()
        {
            InitializeComponent();
            // Window1 win1 = new Window1();

            //win1.Show();

            DateTime oldTime = DateTime.UtcNow;
            newTime = oldTime.AddSeconds(5);
            
        }

        private void Mainwinbtn_Click(object sender, RoutedEventArgs e)
        {
            //Window2 win2 = new Window2(MainWinTxtbox.Text.ToString());
            //win2.Show();
            int flag = 0;
            while (flag != 1)
            {
                if (DateTime.UtcNow == newTime)
                {
                    MessageBox.Show("It's a match!");
                    flag = 1;
                }
            }

        }
        public MainWindow(string str)
        {
            InitializeComponent();
            mainwinlabel.Content = str;
        }
    }
}
