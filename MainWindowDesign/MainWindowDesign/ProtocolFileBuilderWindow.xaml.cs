using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Intensity.Items.Add(null);
        }

        String TokenTypeVal = null; 
        String UtteranceVal, RateVal, IntensityVal, RepetitionCountVal;
        String protocolItem;
        
        
        

        private void tokenList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //((ListBoxItem)tokenList.SelectedValue).Background = Brushes.Transparent;
            //tokenList.Background = Brushes.White;
            

            //((ListBoxItem)tokenList.SelectedValue).Background = Brushes.LightBlue;
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
                utteranceList.Items.Add("NCItem1");
                utteranceList.Items.Add("NCItem2");
                utteranceList.Items.Add("NCItem3");
                utteranceList.Items.Add("NCItem4");
                //((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }
            else if(TokenTypeVal == "VP")
            {
                //tokenList.Items.Background = Brushes.White;
                
                utteranceList.Items.Clear();
                utteranceList.Items.Add("VPItem1");
                utteranceList.Items.Add("VPItem2");
                utteranceList.Items.Add("VPItem3");
                utteranceList.Items.Add("VPItem4");
                //((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }
            else if (TokenTypeVal == "LR")
            {
                tokenList.Background = Brushes.White;
                utteranceList.Items.Clear();
                utteranceList.Items.Add("LRItem1");
                utteranceList.Items.Add("LRItem2");
                utteranceList.Items.Add("LRItem3");
                utteranceList.Items.Add("LRItem4");
            //    ((ListBoxItem)tokenList.SelectedItem).Background = Brushes.LightBlue;
            }

            
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (tokenList.SelectedIndex == -1 || Intensity.SelectedIndex == -1)
                MessageBox.Show("Please select an Item first!");
            String protocol = TokenTypeVal + " " + UtteranceVal + " " + RateVal + " " + IntensityVal + " " + RepetitionCountVal;
            ProtocolList.Items.Add(protocol);
            tokenList.UnselectAll();
            utteranceList.UnselectAll();
            Rate.UnselectAll();
            Intensity.UnselectAll();
            RepetitionCount.UnselectAll();
        }

        private void utteranceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                ProtocolList.Items.Remove(protocolItem);
            }
            else
            {
                MessageBox.Show("Select an item to delete!");
            }
        }

        private void ProtocolList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProtocolList.SelectedItem == null)
            {
                return;
            }
            protocolItem = ProtocolList.SelectedItem.ToString();
        }

        private void Rate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Rate.SelectedItem == null)
            {
                return;
            }
            //RateVal = Rate.SelectedItem.ToString();
            RateVal = ((ListBoxItem)Rate.SelectedValue).Content.ToString();
        }

        private void Intensity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Intensity.SelectedItem == null)
            {
                return;
            }
            //IntensityVal = Intensity.SelectedItem.ToString();
            IntensityVal = ((ListBoxItem)Intensity.SelectedValue).Content.ToString();
        }

        private void RepetitionCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RepetitionCount.SelectedItem == null)
            {
                return;
            }
            //RepetitionCountVal = RepetitionCount.SelectedItem.ToString();
            RepetitionCountVal = ((ListBoxItem)RepetitionCount.SelectedValue).Content.ToString();
        }
    }
}

//When tokens in list boxes are added like utterance list values, change them like in utterance list values then.
//Enable and disable tyhe list boxes as required.  For checking the selection, check only if the list box is enabled.
