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

                    this.Close();

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Invalid Input Values. Try again!", "Invalid Input!", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }

                
            
        }

        
    }
}
