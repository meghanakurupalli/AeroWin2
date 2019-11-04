using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class TokenHistoryWindow : Window
    {
        public TokenHistoryWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<protocol> THWprotocols = new ObservableCollection<protocol>();
        bool _isPrevButtonClicked = false;
        bool _isNextButtonClicked = false;

        public TokenHistoryWindow(string str)
        {
            //string temp = str;
            // path = System.IO.Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            InitializeComponent();
            var reader = new StreamReader(File.OpenRead(str));
            //List<string> protocol = new List<string>();
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                THWprotocols.Add(new protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = splits[4] });

                // Didn't yet edit in this window. Just gave something arbitrary.

            }
            //protocols.RemoveAt(0);
            TokenHistoryGrid.ItemsSource = THWprotocols;
            TokenHistoryGrid.DataContext = this;
            TokenHistoryGrid.SelectedIndex = 0;
            reader.Close();

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
