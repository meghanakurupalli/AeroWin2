using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            if (OkButtonClicked != null)
            {
                _dataFileName = FileName.Text;
                for (int i = 0; i < invalid.Length; i++)
                {
                    if (_dataFileName.Contains(invalid[i]))
                    {
                        MessageBox.Show("Invalid characters detected in file name. Please rename the file",
                            "Invalid file name", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                
                _protocolFileName = ProtocolFileName.Text;
                if (_dataFileName == null)
                {
                    MessageBox.Show("Field cannot be empty!", "Enter a data file name", MessageBoxButton.OK);
                    return;
                    
                }
                else if (_protocolFileName == null)
                {
                    MessageBox.Show("Protocol file not selected", "Please select a protocol file.", MessageBoxButton.OK);
                    return;
                }
                else if (_protocolFileName == null && _dataFileName == null)
                {
                    MessageBox.Show("Fields cannot be empty", "Please enter a file name and select a protocol file to proceed.", MessageBoxButton.OK);
                    return;
                }
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
