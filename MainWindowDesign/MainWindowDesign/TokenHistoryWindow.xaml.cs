using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MainWindowDesign
{
    public partial class TokenHistoryWindow
    {
        public TokenHistoryWindow()
        {
            InitializeComponent();
        }

        public TokenHistoryWindow(MainWindow mainWindow)
        {
            mWin = mainWindow;
            data_File_Name = mainWindow.DataFileName;
            path_For_Wave_Files = mainWindow.pathForwavFiles;
            token_History_File = mainWindow.openExtFile_THFName;
            InitializeTokenHistoryWindow(token_History_File, data_File_Name, path_For_Wave_Files);
        }

        private MainWindow mWin;
        string data_File_Name;
        string path_For_Wave_Files;
        string token_History_File;
        public string THaudio { get; set; }
        public string THPrAf { get; set; }
        List<string> indices = new List<string>();
        int flag = 0;
        public ObservableCollection<Protocol> THWprotocols = new ObservableCollection<Protocol>();
        bool _isPrevButtonClicked = false;
        bool _isNextButtonClicked = false;

        public void InitializeTokenHistoryWindow(string TokenHistoryFile, string DataFileName, string PathForWaveFiles)
        {
            //string temp = str;
            // path = System.IO.Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            InitializeComponent();
            try
            {
                var reader = new StreamReader(File.OpenRead(TokenHistoryFile));
                //List<string> protocol = new List<string>();
                while (!reader.EndOfStream)
                {
                    var splits = reader.ReadLine().Split(',');
                    THWprotocols.Add(new Protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = splits[4] });
                    indices.Add(splits[5]);

                    // Didn't yet edit in this window. Just gave something arbitrary.

                }
                //protocols.RemoveAt(0);
                TokenHistoryGrid.ItemsSource = THWprotocols;
                TokenHistoryGrid.DataContext = this;
                TokenHistoryGrid.SelectedIndex = 0;
                TokenHistoryGrid.SelectionChanged += TokenHistoryGrid_SelectionChanged; //Trying to eliminate the play button.
                reader.Close();

                //CHange here


                string first_file_rep_count = THWprotocols[0].TotalRepetitionCount;
                string tokenType = THWprotocols[0].TokenType;
                string[] splits2 = first_file_rep_count.Split(' ');
                var splits0 = Int32.Parse(splits2[0]);

                //string splits0 = indices[0];
                string rep_count = "_"+ indices[0] + "_" + splits0;
                string prAfrep_count = "pr_af_" + indices[0] + "_" + splits0;
                string resistancerepCount = "res_" + indices[0] + "_" + splits0;

                string audioFileToBePlayed = System.IO.Path.Combine(PathForWaveFiles, DataFileName + rep_count + ".wav");
                string pressureAirflowFileToBeDisplayed = System.IO.Path.Combine(PathForWaveFiles, DataFileName + prAfrep_count + ".csv");
                //string resistanceFileToBeDisplayed = System.IO.Path.Combine(PathForWaveFiles, DataFileName + resistancerepCount + ".csv");

                //create a condition in such a way that whenever something is updated in the tokenhistory window, mainwindow is notified and audio and the other file are displayed.

                THaudio = audioFileToBePlayed;
                THPrAf = pressureAirflowFileToBeDisplayed;

                mWin.pathForPlayingAudioFile = audioFileToBePlayed;
                mWin.playAudio(audioFileToBePlayed);
                mWin.DisplayPressureAirflowResistance(pressureAirflowFileToBeDisplayed, tokenType);
                //data_File_Name = DataFileName;
                //path_For_Wave_Files = PathForWaveFiles;

                //code to open the file relating to first protocol in the token history window. 
            }
            catch (System.IO.FileNotFoundException e)
            {
                MessageBox.Show("Token history file not found!", "File not found");
            }
        }

        private void TokenHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Yayy");

            //throw new NotImplementedException();

            displayMethod();
        }

        private void displayMethod()
        {
            mWin.PressureLineSeriesValues.Clear();
            mWin.AirFlowLineSeriesValues.Clear();
            string rep_count;
            string prAfrep_count;

            int selectedIndex = TokenHistoryGrid.SelectedIndex;
            if( TokenHistoryGrid.SelectedIndex>=TokenHistoryGrid.Items.Count)
            {
                TokenHistoryGrid.SelectedIndex = TokenHistoryGrid.Items.Count - 1;
            }
            if (TokenHistoryGrid.SelectedIndex < 0)
            {
                TokenHistoryGrid.SelectedIndex = 0;
            }

            //Getting an error here.

            string tot_rep_count = THWprotocols[TokenHistoryGrid.SelectedIndex].TotalRepetitionCount; //Gets repetition count and split it for audio file path.
            string tokenType = THWprotocols[TokenHistoryGrid.SelectedIndex].TokenType;
            string[] splits = tot_rep_count.Split(' ');
            var splits0 = Int32.Parse(splits[0]);
           
            rep_count = "_" + indices[selectedIndex] + "_" + splits0;
            prAfrep_count = "pr_af_" + indices[selectedIndex] + "_" + splits0;
            string audioFileToBePlayed = System.IO.Path.Combine(path_For_Wave_Files, data_File_Name + rep_count + ".wav");
            string pressureAirflowFileToBeDisplayed = System.IO.Path.Combine(path_For_Wave_Files, data_File_Name + prAfrep_count + ".csv");

            Debug.Print("audioFileToBePlayed : "+ audioFileToBePlayed+ " pressureAirflowFileToBeDisplayed : "+ pressureAirflowFileToBeDisplayed);

            mWin.playAudio(audioFileToBePlayed);
            mWin.DisplayPressureAirflowResistance(pressureAirflowFileToBeDisplayed, tokenType);

            

            //MessageBox.Show("Hooray");

        }

        private void THWPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            flag = 1; 
            if(prevBtnClicked!=null)
            {
                _isPrevButtonClicked = true;
                prevBtnClicked(this, new EventArgs());
            }
            //_isPrevButtonClicked = true;
            int now = TokenHistoryGrid.SelectedIndex;
            if(now==0)
            {
                TokenHistoryGrid.SelectedIndex = 0;
            }
            else
            {
                TokenHistoryGrid.SelectedIndex = now - 1;
            }
            displayMethod();
            
        }

        private void THWNextButton_Click(object sender, RoutedEventArgs e)
        {
            flag = 1;
            if(nextBtnClicked!=null)
            {
                _isNextButtonClicked = true;
                nextBtnClicked(this, new EventArgs());
            }
            
            int now = TokenHistoryGrid.SelectedIndex;
            if (now == TokenHistoryGrid.Items.Count-2)
            {
                TokenHistoryGrid.SelectedIndex = TokenHistoryGrid.Items.Count - 2;
            }
            else
            {
                TokenHistoryGrid.SelectedIndex = now + 1;
            }
            displayMethod();
        }

        public EventHandler prevBtnClicked;
        public EventHandler nextBtnClicked;
        public bool isPrevButtonClicked
        {
            get { return _isPrevButtonClicked; }
            set { _isPrevButtonClicked = value; }
        }
    }

    public class RecordedProtocolHistory
    {

        public int SelectedIndex { get; set; }
        public string TokenType { get; set; }
        public string Utterance { get; set; }
        public string Rate { get; set; }
        public string Intensity { get; set; }
        public string TotalRepetitionCount { get; set; }
        
    }
}
