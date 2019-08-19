using Microsoft.Win32;
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
    /// Interaction logic for ProtocolFileBuilderWindow.xaml
    /// </summary>
    public partial class ProtocolFileBuilderWindow : Window
    {
        public ProtocolFileBuilderWindow()
        {
            InitializeComponent();
            //IntensityList.Items.Add(null);
        }

        String TokenTypeVal = null;
        String UtteranceVal, RateVal, IntensityVal, RepetitionCountVal;
        String protocolItem;
        int protocolIndex;
        //List<protocol> protocols = new List<protocol>();
        ObservableCollection<protocol> protocols = new ObservableCollection<protocol>();


        ListBox SaveList = new ListBox();
        

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
            try
            {
                if (TokenTypeVal == "NC")
                {
                    RateVal = "-";
                    IntensityVal = "-";
                }
                String SaveProtocol = TokenTypeVal + "," + UtteranceVal + "," + RateVal + "," + IntensityVal + "," + RepetitionCountVal;
                // ProtocolList.Items.Add(protocol);
                SaveList.Items.Add(SaveProtocol);
                
                protocols.Add(new protocol() { TokenType = TokenTypeVal, Utterance = UtteranceVal, Rate = RateVal, Intensity = IntensityVal, RepetitionCount = RepetitionCountVal });

                ProtocolGrid.ItemsSource = protocols;
                ProtocolGrid.DataContext = this;
                
            }

            catch(Exception ex)
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

                // ProtocolList.Items.Remove(protocolItem);
                // ProtocolGrid.Items.Remove(protocolItem);
                //ProtocolGrid.Items.RemoveAt(ProtocolGrid.SelectedIndex);
                //ProtocolGrid.Items.RemoveAt(protocolIndex);
                protocols.RemoveAt(protocolIndex);
                
                //SaveList.Items.Remove(protocolItem);
                SaveList.Items.RemoveAt(protocolIndex);
                
                
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
            StreamWriter writer = null;
            if (save.ShowDialog() == true)

            {
                
                filter = save.FileName;
                writer = new StreamWriter(filter);
                const string header = "Token_Type,Utterance,Rate,Intensity,Repetition_Count";
                writer.WriteLine(header);
                //var csv = new StringBuilder();
                for (int i = 0; i < SaveList.Items.Count; i++)
                {
                    string protocol = SaveList.Items[i].ToString();

                    //string[] SplitProtocol = protocol.Split(' ');
                    //csv.Append(protocol);
                    //Debug.Print(protocol + "\n");
                    writer.WriteLine(protocol);
                }
                writer.Close();
                
            }
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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
            Debug.Print("Protoocl item : " + protocolItem + "Index  : "+protocolIndex);
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

    public class protocol
    {
        public string TokenType { get; set; }
        public string Utterance { get; set; }
        public string Rate { get; set; }
        public string Intensity { get; set; }
        public string RepetitionCount { get; set; }
    }
}

//When tokens in list boxes are added like utterance list values, change them like in utterance list values then.
//Enable and disable tyhe list boxes as required.  For checking the selection, check only if the list box is enabled.
