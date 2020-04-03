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

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for ChannelRangesWindow.xaml
    /// </summary>
    
    public partial class ChannelRangesWindow : Window
    {
        public List<double?> ChannelRanges { get; set; }
        private List<string> Texts;
        public event EventHandler OkButtonClicked;
        public ChannelRangesWindow()
        {
            InitializeComponent();
            // ReSharper disable once AssignNullToNotNullAttribute
            AudioMin.Text = null;
            AudioMax.Text = null;
            AirflowMin.Text = null;
            AirflowMax.Text = null;
            PressureMin.Text = null;
            PressureMax.Text = null;
            ResistanceMin.Text = null;
            ResistanceMax.Text = null;
        }


        private void ChannelRangesOK_OnClick(object sender, RoutedEventArgs e)
        {
            ChannelRanges = new List<double?>();
            Texts = new List<string>
            {
                AudioMin.Text,
                AudioMax.Text,
                AirflowMin.Text,
                AirflowMax.Text,
                PressureMin.Text,
                PressureMax.Text,
                ResistanceMin.Text,
                ResistanceMax.Text
            };

            
            try
            {

                foreach (var VARIABLE in Texts)
                {
                    if (VARIABLE == "")
                    {
                        ChannelRanges.Add(null);
                    }
                    else
                    {
                        var temp = double.Parse(VARIABLE);
                        ChannelRanges.Add(temp);
                    }
                }

                OkButtonClicked?.Invoke(this, new EventArgs());
                Close();
                
            }
            catch (Exception exception) 
            {
                MessageBox.Show("Invalid Input Values. Try again!", "Invalid Input!", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }

            //return ChannelRanges;
        }

        

        private void AudioChannelRange_OnUnchecked(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            AudioMin.IsEnabled = true;
            AudioMax.IsEnabled = true;
        }

        private void AirflowChannelRange_OnUnchecked(object sender, RoutedEventArgs e)
        {
            AirflowMin.IsEnabled = true;
            AirflowMax.IsEnabled = true;
            //throw new NotImplementedException();
        }

        private void PressureChannelRange_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PressureMin.IsEnabled = true;
            PressureMax.IsEnabled = true;
            //throw new NotImplementedException();
        }

        private void ResistanceChannelRange_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ResistanceMin.IsEnabled = true;
            ResistanceMax.IsEnabled = true;
            //throw new NotImplementedException();
        }

        private void AudioChannelRange_OnChecked(object sender, RoutedEventArgs e)
        {
            AudioMin.IsEnabled = false;
            AudioMax.IsEnabled = false;
        }

        private void AirflowChannelRange_OnChecked(object sender, RoutedEventArgs e)
        {
            AirflowMin.IsEnabled = false;
            AirflowMax.IsEnabled = false;
        }

        private void PressureChannelRange_OnChecked(object sender, RoutedEventArgs e)
        {
            PressureMin.IsEnabled = false;
            PressureMax.IsEnabled = false;
        }

        private void ResistanceChannelRange_OnChecked(object sender, RoutedEventArgs e)
        {
            ResistanceMin.IsEnabled = false;
            ResistanceMax.IsEnabled = false;
        }
    }
}
