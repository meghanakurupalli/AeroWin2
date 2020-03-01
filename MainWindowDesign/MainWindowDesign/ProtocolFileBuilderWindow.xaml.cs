using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for ProtocolFileBuilderWindow.xaml
    /// </summary>
    public partial class ProtocolFileBuilderWindow : Window
    {
        public ProtocolFileBuilderWindow()
        {
            InitializeComponent();
            //IntensityList.Items.Add(null);
            //protocols = new ObservableCollection<protocol>(protocols.OrderBy(TokenTypeVal=>TokenTypeVal));
        }

        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        String TokenTypeVal = null;
        String UtteranceVal, RateVal, IntensityVal, RepetitionCountVal;
        String protocolItem;
        int protocolIndex;
        //List<protocol> protocols = new List<protocol>();
        ObservableCollection<Protocol> protocols = new ObservableCollection<Protocol>();
        ObservableCollection<Protocol> newCollection = new ObservableCollection<Protocol>();

        public int CountOfProtocolFile
        {
            get { return protocols.Count(); }
        }
        

        private void tokenList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //((ListBoxItem)tokenList.SelectedValue).Background = Brushes.Transparent;
            //tokenList.Background = Brushes.White;


            //((ListBoxItem)tokenList.SelectedValue).Background = Brushes.LightBlue;
            utteranceList.SelectedIndex = 0;
            if (tokenList.SelectedItem == null)
            {
                return;
            }
            TokenTypeVal = ((ListBoxItem)tokenList.SelectedValue).Content.ToString();
            if (TokenTypeVal == "NC")
            {
                //((ListBoxItem)tokenList.SelectedValue).Background = Brushes.Transparent;
                //  tokenList.Background = Brushes.White;
                utteranceList.Items.Clear();
                IntensityList.Items.Clear();
                RateList.Items.Clear();
                utteranceList.Items.Add("Rest Breath");
                utteranceList.Items.Add("Deep Breath");
                utteranceList.Items.Add("ma-ma-ma");
                utteranceList.Items.Add("/M/ Sustained");
                utteranceList.SelectedIndex = 0;
                //((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }
            else if (TokenTypeVal == "VP")
            {
                //tokenList.Items.Background = Brushes.White;

                utteranceList.Items.Clear();
                utteranceList.Items.Add("Pa-Pa-Pa");
                utteranceList.Items.Add("Puppy 2x");
                utteranceList.Items.Add("Puffy 2X");
                utteranceList.Items.Add("Pamper 2x");
                utteranceList.Items.Add("Buy Bobby a puppy");
                IntensityList.Items.Clear();
                IntensityList.Items.Add("HAB");
                IntensityList.Items.Add("1");
                IntensityList.Items.Add("2");
                IntensityList.Items.Add("3");
                RateList.Items.Clear();
                RateList.Items.Add("HAB");
                RateList.Items.Add("3");
                RateList.Items.Add("7");
                RateList.Items.Add("10");
                RateList.Items.Add("15");
                utteranceList.SelectedIndex = 0;
                RateList.SelectedIndex = 0;
                IntensityList.SelectedIndex = 0;
                //((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }
            else if (TokenTypeVal == "LR")
            {
                tokenList.Background = Brushes.White;
                utteranceList.Items.Clear();
                utteranceList.Items.Add("Pi");
                utteranceList.Items.Add("Pa");
                IntensityList.Items.Clear();
                //IntensityList.Items.Add("HAB");
                IntensityList.Items.Add("1");
                IntensityList.Items.Add("2");
                IntensityList.Items.Add("3");
                RateList.Items.Clear();
                RateList.Items.Add("HAB");
                RateList.Items.Add("3");
                RateList.Items.Add("7");
                RateList.Items.Add("10");
                RateList.Items.Add("15");
                utteranceList.SelectedIndex = 0;
                RateList.SelectedIndex = 0;
                IntensityList.SelectedIndex = 0;
                //    ((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }


        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            int flag = 0;
            int i = 0;
            Protocol idk = new Protocol();
            
            try
            {
                
                switch(TokenTypeVal)
                {
                    case "NC":
                        RateVal = "-";
                        IntensityVal = "-";
                        
                        // ProtocolList.Items.Add(protocol);
                                                
                        
                        protocols.Add(new Protocol() { TokenType = TokenTypeVal, Utterance = UtteranceVal, Rate = RateVal, Intensity = IntensityVal, TotalRepetitionCount = RepetitionCountVal });

                        break;

                    case "VP":
                        while (i < ProtocolGrid.Items.Count)
                        {
                            string temp = protocols[i].TokenType;
                            if (temp == "NC")
                                flag = 1;
                            i++;
                        }
                        if (flag == 0)
                        {
                            MessageBox.Show("Cannot add VP items without atleast one NC item","Cannot add Protocol!",MessageBoxButton.OK,MessageBoxImage.Asterisk);
                        }
                        else
                        {
                          
                            protocols.Add(new Protocol() { TokenType = TokenTypeVal, Utterance = UtteranceVal, Rate = RateVal, Intensity = IntensityVal, TotalRepetitionCount = RepetitionCountVal });

                        }
                        break;

                    case "LR":
                        protocols.Add(new Protocol() { TokenType = TokenTypeVal, Utterance = UtteranceVal, Rate = RateVal, Intensity = IntensityVal, TotalRepetitionCount = RepetitionCountVal });
                        break;

                    default:
                        MessageBox.Show("Cannot add VP items without atleast one NC item", "Cannot add Protocol!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;

                }

                //protocols.OrderBy(t=>TokenTypeVal);
                var newCollection = protocols.OrderBy(x => x.TokenType);
                ProtocolGrid.ItemsSource = newCollection;
                ProtocolGrid.DataContext = this;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Cannot add protocol");
            }


        }

        private void utteranceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //utteranceList.SelectedIndex = 0;
            if (utteranceList.SelectedItem == null)
            {
                return;
            }
            UtteranceVal = utteranceList.SelectedItem.ToString();
            //UtteranceVal=((ListBoxItem)utteranceList.SelectedValue).Content.ToString();


        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(protocolItem!=null)
            {
                protocols.RemoveAt(protocolIndex);
                var newCollection = protocols.OrderBy(x => x.TokenType);
                ProtocolGrid.ItemsSource = newCollection;
                ProtocolGrid.DataContext = this;
            }
            else
            {
                MessageBox.Show("Select an item to delete!", "Warning");
            }
        }

        private void ProtocolList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ProtocolList.SelectedItem == null)
            //{
            //    return;
            //}
            //protocolItem = ProtocolList.SelectedItem.ToString();
        }

        private void tokenList_Loaded(object sender, RoutedEventArgs e)
        {
            tokenList.SelectedIndex = 0;
            
        }

        private void utteranceList_Loaded(object sender, RoutedEventArgs e)
        {
            utteranceList.SelectedIndex = 0;
        }

        private void RateList_Loaded(object sender, RoutedEventArgs e)
        {
            RateList.SelectedIndex = 0;
        }

        private void IntensityList_Loaded(object sender, RoutedEventArgs e)
        {
            IntensityList.SelectedIndex = 0;
        }

        private void RepetitionCountList_Loaded(object sender, RoutedEventArgs e)
        {
            RepetitionCountList.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog save = new SaveFileDialog();
            //save.FileName = "Protocol.csv";
            string filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            save.Filter = filter;
            save.InitialDirectory = generatedProtocolFilesPath;
            StreamWriter writer = null;
            if (save.ShowDialog() == true)

            {
                
                filter = save.FileName;
                writer = new StreamWriter(filter);
                const string header = "Token_Type,Utterance,Rate,Intensity,Repetition_Count";
                writer.WriteLine(header);
                var csv = new StringBuilder();

                

                for(int i = 0; i < protocols.Count; i++)
                {
                    string protocol = protocols[i].TokenType + "," + protocols[i].Utterance + "," + protocols[i].Rate + "," + protocols[i].Intensity + "," +  protocols[i].TotalRepetitionCount;
                    writer.WriteLine(protocol);
                }

               
                writer.Close();
                
            }
            Close();
            
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            string ProtocolFileName;
            string ProtocolFileName_ext = "temp";
            ObservableCollection<Protocol> tempcollection = new ObservableCollection<Protocol>();
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*",
                InitialDirectory = generatedProtocolFilesPath
                
            };
            if(openFileDialog.ShowDialog()==true)
            {
                ProtocolFileName_ext = openFileDialog.ToString();
            }
            ProtocolFileName = System.IO.Path.GetFileNameWithoutExtension(ProtocolFileName_ext);

            string fileName = System.IO.Path.Combine(generatedProtocolFilesPath, ProtocolFileName + ".csv");
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(File.OpenRead(fileName));
            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("File not found");
            }


            
            var count = File.ReadLines(fileName).Count(); // This gives the no of protocols in a given protocol file,+1, for header.
            count = count - 1;//Header
            int i = 0;
            
            while (i!=count)
            {
                int j = 0;
                var splitline = streamReader.ReadLine().Split(',');
                while(j!=splitline.Count())
                {
                   // Debug.Print("Splitline : " + splitline[j]);
                    j++;
                }
                //openExisting[i].TokenType = splitline[0];
                //string temp = splitline[0];
                //Debug.Print("temp : " + temp);
               // var RepititionCountString = splitline[4];
                //var splitRepititionCountString = RepititionCountString.Split(' ');
                //var temp1 = splitRepititionCountString[0]+" of ";
                //var temp2 = splitRepititionCountString[2];

                protocols.Add(new Protocol { TokenType = splitline[0], Utterance = splitline[1], Rate = splitline[2], Intensity = splitline[3], TotalRepetitionCount = splitline[4] });
                i++;
            }

            ProtocolGrid.ItemsSource = protocols;
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void ProtocolGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (ProtocolGrid.SelectedItem == null)
            {
                return;
            }
            protocolItem = ProtocolGrid.SelectedItem.ToString();
            protocolIndex = ProtocolGrid.SelectedIndex;
            //Debug.Print("Protoocl item : " + protocolItem + "Index  : "+protocolIndex);
        }

        

        //private void utteranceList_Loaded(object sender, RoutedEventArgs e)
        //{
        //    utteranceList.SelectedIndex = 0;
        //}

        private void RateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //RateList.SelectedIndex = 0;
            if (RateList.SelectedItem == null)
            {
                return;
            }
            RateVal = RateList.SelectedItem.ToString();
            //RateVal = ((ListBoxItem)RateList.SelectedValue).Content.ToString();
        }

        private void IntensityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //IntensityList.SelectedIndex = 0;
            if (IntensityList.SelectedItem == null)
            {
                return;
            }
            IntensityVal = IntensityList.SelectedItem.ToString();
            //IntensityVal = ((ListBoxItem)IntensityList.SelectedValue).Content.ToString();
        }

        private void RepetitionCountList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          //  RepetitionCountList.SelectedIndex = 0;
            if (RepetitionCountList.SelectedItem == null)
            {
                return;
            }
            //RepetitionCountVal = RepetitionCountList.SelectedItem.ToString();
            RepetitionCountVal = ((ListBoxItem)RepetitionCountList.SelectedValue).Content.ToString();
        }
        
    }

    public class Protocol : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _TokenType = "";
        public string TokenType
        {
            get
            {
                return _TokenType;
            }

            set
            {
                _TokenType = value;
                OnPropertyChanged("TokenType");
            }
        }

        private string _Utterance = "";
        public string Utterance
        {
            get
            {
                return _Utterance;
            }

            set
            {
                _Utterance = value;
                OnPropertyChanged("Utterance");
            }
        }

        private string _Rate = "";
        public string Rate
        {
            get
            {
                return _Rate;
            }

            set
            {
                _Rate = value;
                OnPropertyChanged("Rate");
            }
        }

        private string _Intensity = "";
        public string Intensity
        {
            get
            {
                return _Intensity;
            }

            set
            {
                _Intensity = value;
                OnPropertyChanged("Intensity");
            }
        }

        private string _TotalRepetitionCount = "";
        public string TotalRepetitionCount
        {
            get
            {
                return _TotalRepetitionCount;
            }

            set
            {
                _TotalRepetitionCount = value;
                OnPropertyChanged("TotalRepetitionCount");
            }
        }

        //public string CurrentRepetitionCount { get; set; }
    }

    


}

//When tokens in list boxes are added like utterance list values, change them like in utterance list values then.
//Enable and disable tyhe list boxes as required.  For checking the selection, check only if the list box is enabled.
