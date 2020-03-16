using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for TokenListWindow.xaml
    /// </summary>
    public partial class TokenListWindow
    {
        //string _protocolFileNameTWin;
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public ObservableCollection<Protocol> Protocols { get; set; }

        string path;
        string noOfDoneRepCount = "1";
        public event EventHandler TokenListWindowCloseEvent;
        public event EventHandler<SelectedTokenArguments> TokenIsSelectedEvent;
        private bool tempChar = false;
        static int i;
        static int saveme = 1;
        public int CurrentRepetitionCount { get; set; } = 1;

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
            string temp = str;
            path = Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            var reader = new StreamReader(File.OpenRead(path));
            Protocols = new ObservableCollection<Protocol>();
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                var temp1 = string.Concat(noOfDoneRepCount, " of ", splits[4]);
                Protocols.Add(new Protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = temp1 });

            }
            Protocols.RemoveAt(0);
            //TokenListGrid.ItemsSource = Protocols;
            TokenListGrid.DataContext = this;
            TokenListGrid.SelectedIndex = 0;
            reader.Close();
           
        }

        public void ChangeIndexSelection()
        {
          
            var countOfProtocols = Protocols.Count;
            var currentProtocol = Protocols[TokenListGrid.SelectedIndex];
            var temp = currentProtocol.TotalRepetitionCount.Split(' ');
            var currentProtocolRepetitionCount = int.Parse(temp[2]);
            //TokenListGrid.ItemsSource = Protocols;
            if (saveme < currentProtocolRepetitionCount)
            {
                
                saveme++;
                CurrentRepetitionCount = saveme; 
                var temp2 = string.Concat(saveme, " of ", currentProtocolRepetitionCount);
                currentProtocol.TotalRepetitionCount = temp2;
                Debug.Print("temp2 : " + temp2);
                Protocols[TokenListGrid.SelectedIndex].TotalRepetitionCount = temp2;
            }
            else
            {
                int nextIndex = TokenListGrid.SelectedIndex + 1;
                
                if (nextIndex <= countOfProtocols - 1)
                {
                    TokenListGrid.SelectedIndex = nextIndex;
                    
                    saveme = 1;
                    CurrentRepetitionCount = saveme;
                }
            }
        }

        private void TokenListWindowClosed_Event(object sender, EventArgs e)
        {
            TokenListWindowCloseEvent?.Invoke(sender, e);
        }

        private void dg_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            var args = new SelectedTokenArguments();
            args.Protocol = (Protocol)e.AddedItems[0];
            TokenIsSelectedEvent?.Invoke(sender, args);
        }
    }

    public class SelectedTokenArguments : EventArgs
    {
        public Protocol Protocol { get; set; }
    }


}
