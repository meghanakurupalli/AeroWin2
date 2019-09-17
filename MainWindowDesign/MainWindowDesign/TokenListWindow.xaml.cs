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

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for TokenListWindow.xaml
    /// </summary>
    public partial class TokenListWindow : Window
    {
        //string _protocolFileNameTWin;
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        ObservableCollection<protocol> protocols = new ObservableCollection<protocol>();

        public bool isStartButtonClicked = false;
        
        string path;

        public TokenListWindow()
        {

            InitializeComponent();
            //mwin = new MainWindow();
            

        }

        public TokenListWindow(string str)
        {
            
            InitializeComponent();
            string temp = str;
            path = System.IO.Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            var reader = new StreamReader(File.OpenRead(path));
            List<string> protocol = new List<string>();
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                protocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], RepetitionCount = splits[4] });

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
                protocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], RepetitionCount = splits[4] });

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

        public void ChangeIndexSelection() {
            var countOfProtocols = TokenListGrid.Items.Count;
            int nextIndex = TokenListGrid.SelectedIndex + 1;
            if (nextIndex <= countOfProtocols-2)
            {
                TokenListGrid.SelectedIndex = nextIndex;
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
        
        
    }

    
}
