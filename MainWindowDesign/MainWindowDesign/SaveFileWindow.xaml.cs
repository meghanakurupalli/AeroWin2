using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for SaveFileWindow.xaml
    /// </summary>
    public partial class SaveFileWindow : Window
    {
        
        //SaveFileDialog save = new SaveFileDialog();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        MainWindow mwin = new MainWindow();
        public Button startButtonSFW;
        string _dataFileName;
        string _protocolFileName;

        public event EventHandler OkButtonClicked;
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public string Data_File_Name
        {
            get { return _dataFileName; }
            set { _dataFileName = value; }
        }

        public string Protocol_File_Name
        {
            get { return _protocolFileName; }
            set { _protocolFileName = value; }
        }
        public SaveFileWindow()
        {
            InitializeComponent();

            //This code is to add an autocomplete box for selecting protocol files if necessary.

            string[] filenames = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"]);
            List<string> fileNameList = new List<string>();

            for (int i = 0; i < filenames.Length;i++)
            {
                fileNameList.Add(filenames[i]);
            }
            
           
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

            //save.Filter = filter;
            //StreamWriter writer = null;
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

            if (openFileDialog.ShowDialog()==true)
            {
                string ProtocolFileName_ext;
                ProtocolFileName_ext = openFileDialog.ToString();
                _protocolFileName = System.IO.Path.GetFileNameWithoutExtension(ProtocolFileName_ext);
                Debug.Print("protocol File name : " + _protocolFileName);
                ProtocolFileName.Text = _protocolFileName;
                
            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(OkButtonClicked != null)
            {
                _dataFileName = FileName.Text;
                _protocolFileName = ProtocolFileName.Text;
                OkButtonClicked(this, new EventArgs());
            }

            //this.DialogResult = true;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
