using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (openFileDialog.ShowDialog() == true)
            {
                string ProtocolFileName_ext;
                ProtocolFileName_ext = openFileDialog.ToString();
                var fileExtension = Path.GetExtension(ProtocolFileName_ext);
                if (fileExtension?.ToLower() == ".csv")
                {
                    _protocolFileName = Path.GetFileNameWithoutExtension(ProtocolFileName_ext);
                    ProtocolFileName.Text = _protocolFileName;
                }
                else
                {
                    MessageBox.Show("Please select a valid protocol file of csv type.");
                }
            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            if (OkButtonClicked != null)
            {
                _dataFileName = FileName.Text;
                Match match = Regex.Match(_dataFileName, "^[a-zA-Z0-9_]+$", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    MessageBox.Show("Invalid characters detected in file name. Please rename the file");
                    return;
                }

                _protocolFileName = ProtocolFileName.Text;
                if (string.IsNullOrEmpty(_dataFileName))
                {
                    MessageBox.Show("Please enter a data file name");
                    return;
                    
                }

                if (string.IsNullOrEmpty(_protocolFileName))
                {
                    MessageBox.Show("Please select a protocol file.");
                    return;
                }
                OkButtonClicked(this, new EventArgs());
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
