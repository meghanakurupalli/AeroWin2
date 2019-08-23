using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for SaveFileWindow.xaml
    /// </summary>
    public partial class SaveFileWindow : Window
    {
        string DataFileName;
        SaveFileDialog save = new SaveFileDialog();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        MainWindow mwin = new MainWindow();
        public Button startButtonSFW;
        public SaveFileWindow()
        {
            InitializeComponent();
        }

        private void DataFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //private void portocolfilebrowse_click(object sender, routedeventargs e)
        //{

        //}

        private void ProtocolFileBrowse_Click(object sender, RoutedEventArgs e)
        {


            //save.FileName = "Protocol.csv";
            string filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            //save.Filter = filter;
            //StreamWriter writer = null;
            openFileDialog.Filter = filter;
            //if (save.ShowDialog() == true)

            //{

            //    //filter = save.FileName;
            //    //writer = new StreamWriter(filter);
            //    //const string header = "Token_Type,Utterance,Rate,Intensity,Repetition_Count";
            //    //writer.WriteLine(header);
            //    ////var csv = new StringBuilder();

            //    //writer.Close();
            //    string DataFileName_ext;
            //    DataFileName_ext = save.ToString();
            //    DataFileName = System.IO.Path.GetFileNameWithoutExtension(DataFileName_ext);
            //    Debug.Print("File name : " + DataFileName);
            //    ProtocolFileName.Text = DataFileName;
            //    mwin.FileNameFromSFW = DataFileName;

            //}


            
            if(openFileDialog.ShowDialog()==true)
            {
                string DataFileName_ext;
                DataFileName_ext = openFileDialog.ToString();
                DataFileName = System.IO.Path.GetFileNameWithoutExtension(DataFileName_ext);
                Debug.Print("File name : " + DataFileName);
                ProtocolFileName.Text = DataFileName;
                mwin.FileNameFromSFW = DataFileName;
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //mwin.StartButton = this.SaveButton;
            //mwin.StartButton.IsEnabled = true;
            // StartButton.IsEnabled = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
