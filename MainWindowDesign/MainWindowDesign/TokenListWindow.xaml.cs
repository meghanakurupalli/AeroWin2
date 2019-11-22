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
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;
using System.ComponentModel;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for TokenListWindow.xaml
    /// </summary>
    public partial class TokenListWindow : Window
    {
        //string _protocolFileNameTWin;
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public ObservableCollection<protocol> protocols = new ObservableCollection<protocol>();

        public bool isStartButtonClicked = false;
        
        string path;
        string noOfDoneRepCount = "1";

        public TokenListWindow()
        {

            InitializeComponent();
            //mwin = new MainWindow();
        }

        public TokenListWindow(string str)
        {

            
            InitializeComponent();
            Debug.Print("Selected Index here : " + TokenListGrid.SelectedIndex);
            //TokenListGrid.SelectedIndex = 0;
            if (TokenListGrid.SelectedIndex == -1)
            {
                PreviousButton.IsEnabled = false;
            }
            string temp = str;
            path = System.IO.Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            var reader = new StreamReader(File.OpenRead(path));
            List<string> protocol = new List<string>();
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                var temp1 = string.Concat(noOfDoneRepCount, " of ", splits[4]);
                protocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = temp1 });

            }
            protocols.RemoveAt(0);
            TokenListGrid.ItemsSource = protocols;
            TokenListGrid.DataContext = this;
            TokenListGrid.SelectedIndex = 0;
            reader.Close();
           
        }

        

        public TokenListWindow(string str, bool val)
        {
            System.Timers.Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            myTimer.Interval = 5000;
            myTimer.Enabled = true;
            InitializeComponent();
            string temp = str;
            
            path = System.IO.Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            var reader = new StreamReader(File.OpenRead(path));
            List<string> protocol = new List<string>();
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                var temp1 = string.Concat(noOfDoneRepCount, " of ", splits[4]);
                protocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = temp1 });

            }
            protocols.RemoveAt(0);
            TokenListGrid.ItemsSource = protocols;
            TokenListGrid.DataContext = this;
            TokenListGrid.SelectedIndex = 0;
            if (val == true)
            {
                tempChar = true;                
            }
        }

        private bool tempChar = false;
        static int i;
        static int saveme = 1;
    
        int _givesCurrentRepCount = 1;
        int _givesCurrent_TotalRepCount = 0;


        public int givesCurrentRepCount
        {
            get { return _givesCurrentRepCount; }
            set { _givesCurrentRepCount = value; }
        }

        public int givesCurrent_TotalRepCount
        {
            get { return _givesCurrent_TotalRepCount; }
            set { _givesCurrent_TotalRepCount = value; }
        }

        public void ChangeIndexSelection()
        {
            
            var countOfProtocols = TokenListGrid.Items.Count;
            var currentProtocol = protocols[TokenListGrid.SelectedIndex];
            var temp = currentProtocol.TotalRepetitionCount.Split(' ');
            var currentProtocolRepetitionCount = Int32.Parse(temp[2]);
            _givesCurrent_TotalRepCount = currentProtocolRepetitionCount; // Repetition count of current protocol. eg: VP, pa-pa-pa, 2  ==> gives 2. 
            TokenListGrid.ItemsSource = protocols;
            if (saveme < currentProtocolRepetitionCount)
            {
                
                saveme++;
                _givesCurrentRepCount = saveme; 
                var temp2 = string.Concat(saveme, " of ", currentProtocolRepetitionCount);
                currentProtocol.TotalRepetitionCount = temp2;
                Debug.Print("temp2 : " + temp2);
                protocols[TokenListGrid.SelectedIndex].TotalRepetitionCount = temp2;

            }
            else
            {
                int nextIndex = TokenListGrid.SelectedIndex + 1;
                if (nextIndex <= countOfProtocols - 2)
                {
                    TokenListGrid.SelectedIndex = nextIndex;
                    saveme = 1;
                    _givesCurrentRepCount = saveme;
                }
                //TokenListGrid.ItemsSource = protocols;
            }

            var currentTokenType = protocols[TokenListGrid.SelectedIndex].TokenType;
            var nextTokenType = protocols[TokenListGrid.SelectedIndex + 1].TokenType;
            i

            //_givesCurrentRepCount = 0;    
            if (TokenListGrid.SelectedIndex >= TokenListGrid.Items.Count - 2)
            {
                TokenListGrid.SelectedIndex = TokenListGrid.Items.Count - 2;
            }

        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if(tempChar==true && i<TokenListGrid.Items.Count-1)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    TokenListGrid.SelectedIndex = i;
                    i++;
                }));
            }
            
            
            
            //throw new NotImplementedException();
        }

        public void sayThisHappens()
        {
            if (i < TokenListGrid.Items.Count - 1)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    TokenListGrid.SelectedIndex = i;
                    i++;
                }));
            }
            else
            {
                i = 0;
            }
                
        }

        public void SelectionUpdater(int num)
        {
            TokenListGrid.SelectedIndex = num;
        }

        public string Protocol_File_Name_TWin;

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            saveme = 1;
            _givesCurrentRepCount = 1;
            int currentIndex = TokenListGrid.SelectedIndex;
            
            if(currentIndex == 0)
            {
                PreviousButton.IsEnabled = false;
            }
            else
            {
                int changedIndex = currentIndex - 1;
                TokenListGrid.SelectedIndex = changedIndex;
                
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            saveme = 1;
            _givesCurrentRepCount = 1;
            int currentIndex = TokenListGrid.SelectedIndex;

            if (currentIndex == protocols.Count)
            {
                NextButton.IsEnabled = false;
            }
            else
            {
                int changedIndex = currentIndex + 1;
                TokenListGrid.SelectedIndex = changedIndex;
                
            }
        }
    }

    
}
