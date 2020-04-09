using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CursorWindow.xaml
    /// </summary>
    public partial class CursorWindow : Window
    {
        private MainWindow mainWin;
        public CursorWindow(MainWindow mWin)
        {
            InitializeComponent();
            mainWin = mWin;
            //this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            //this.Top = SystemParameters.PrimaryScreenWidth - this.Height;
        }

        private void CursorWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            mainWin.AudioCursor1.Visibility = Visibility.Hidden;
            mainWin.AudioCursor2.Visibility = Visibility.Hidden;
            mainWin.PressureCursor1.Visibility = Visibility.Hidden;
            mainWin.PressureCursor2.Visibility = Visibility.Hidden;
            mainWin.AirFlowCursor1.Visibility = Visibility.Hidden;
            mainWin.AirFlowCursor2.Visibility = Visibility.Hidden;
            mainWin.ResistanceCursor1.Visibility = Visibility.Hidden;
            mainWin.ResistanceCursor2.Visibility = Visibility.Hidden;
            mainWin.showCursor.IsChecked = false;
        }

       
    }
}
