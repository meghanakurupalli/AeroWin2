using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for TokenHistoryWIndow.xaml
    /// </summary>
    public class GetterSetter
    {
        public string dataFileName { get; set; }
    }
    public partial class TokenHistoryWindow : Window
    {
        public TokenHistoryWindow()
        {
            InitializeComponent();
        }
        private MainWindow mWin;
        string data_File_Name;
        string path_For_Wave_Files;
        string token_History_File;

        public string THaudio { get; set; }
        public string THPrAf { get; set; }
        List<string> indices = new List<string>();

       


        public ObservableCollection<protocol> THWprotocols = new ObservableCollection<protocol>();
        bool _isPrevButtonClicked = false;
        bool _isNextButtonClicked = false;

        public TokenHistoryWindow(MainWindow mainWindow)
        {
            this.mWin = mainWindow;
            data_File_Name = mainWindow.DataFileName;
            path_For_Wave_Files = mainWindow.pathForwavFiles;
            token_History_File = mainWindow.openExtFile_THFName;
            tokenHistoryWindow(token_History_File, data_File_Name, path_For_Wave_Files);
        }

        public void tokenHistoryWindow(string TokenHistoryFile, string DataFileName, string PathForWaveFiles)
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
                    THWprotocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = splits[4] });
                    indices.Add(splits[5]);

                    // Didn't yet edit in this window. Just gave something arbitrary.

                }
                //protocols.RemoveAt(0);
                TokenHistoryGrid.ItemsSource = THWprotocols;
                TokenHistoryGrid.DataContext = this;
                TokenHistoryGrid.SelectedIndex = 0;
                TokenHistoryGrid.SelectionChanged += TokenHistoryGrid_SelectionChanged; //Trying to eliminate the play button.
                reader.Close();
                string first_file_rep_count = THWprotocols[0].TotalRepetitionCount;
                string[] splits2 = first_file_rep_count.Split(' ');
                var splits0 = Int32.Parse(splits2[0]);

                //string splits0 = indices[0];
                string rep_count = "_0" + "_" + splits0;
                string prAfrep_count = "pr_af_0" + "_" + splits0;
                string audioFileToBePlayed = System.IO.Path.Combine(PathForWaveFiles, DataFileName + rep_count + ".wav");
                string pressureAirflowFileToBeDisplayed = System.IO.Path.Combine(PathForWaveFiles, DataFileName + prAfrep_count + ".csv");

                //create a condition in such a way that whenever something is updated in the tokenhistory window, mainwindow is notified and audio and the other file are displayed.

                THaudio = audioFileToBePlayed;
                THPrAf = pressureAirflowFileToBeDisplayed;

                mWin.playAudio(audioFileToBePlayed);
                mWin.displayPressureandAirflow(pressureAirflowFileToBeDisplayed);
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

            
            string tot_rep_count = THWprotocols[TokenHistoryGrid.SelectedIndex].TotalRepetitionCount; //Gets repetition count and split it for audio file path.
            string[] splits = tot_rep_count.Split(' ');
            var splits0 = Int32.Parse(splits[0]);
           
            rep_count = "_" + indices[selectedIndex] + "_" + splits0;
            prAfrep_count = "pr_af_" + indices[selectedIndex] + "_" + splits0;
            string audioFileToBePlayed = System.IO.Path.Combine(path_For_Wave_Files, data_File_Name + rep_count + ".wav");
            string pressureAirflowFileToBeDisplayed = System.IO.Path.Combine(path_For_Wave_Files, data_File_Name + prAfrep_count + ".csv");

            Debug.Print("audioFileToBePlayed : "+ audioFileToBePlayed+ " pressureAirflowFileToBeDisplayed : "+ pressureAirflowFileToBeDisplayed);

            mWin.playAudio(audioFileToBePlayed);
            mWin.displayPressureandAirflow(pressureAirflowFileToBeDisplayed);

            

            //MessageBox.Show("Hooray");

        }

        private void THWPreviousButton_Click(object sender, RoutedEventArgs e)
        {
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
            if(nextBtnClicked!=null)
            {
                _isNextButtonClicked = true;
                nextBtnClicked(this, new EventArgs());
            }
            
            int now = TokenHistoryGrid.SelectedIndex;
            if (now == TokenHistoryGrid.Items.Count-1)
            {
                TokenHistoryGrid.SelectedIndex = TokenHistoryGrid.Items.Count - 1;
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
}
