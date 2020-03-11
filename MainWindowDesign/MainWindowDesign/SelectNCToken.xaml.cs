using System;
using MainWindowDesign;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace MainWIndowDesign
{
    /// <summary>
    /// Interaction logic for SelectNCToken.xaml
    /// </summary>
    public partial class SelectNCToken: INotifyPropertyChanged
    {

        public SelectNCToken()
        {
            InitializeComponent();

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
        }

        private List<RecordedProtocolHistory> recordedNCTokens;
        public bool isNCTokenSelectedForSubtractionTable { get; set; }
        public List<RecordedProtocolHistory> RecordedNCTokens
        {
            get { return recordedNCTokens;}
            set
            {
                recordedNCTokens = value;
                RaisePropertyChanged("RecordedNCTokens");
            }
        }

        private SelectedNCTokenArgs SelectedNCTokenArgs { get; set; }
       

        public event EventHandler<SelectedNCTokenArgs> NCTokenForVPCalculationIsSelected;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string propertyName)
        {
            var handlers = PropertyChanged;

            handlers(this, new PropertyChangedEventArgs(propertyName));
        }

        public SelectNCToken(List<RecordedProtocolHistory> recordedNCTokens)
        {
            InitializeComponent();
            RecordedNCTokens = recordedNCTokens;
            SelectNCTokenGrid.DataContext = this;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width-20;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectNCTokenGrid.SelectedIndex < RecordedNCTokens.Count)
            {
                try
                {
                    var selectedNCToken = RecordedNCTokens[SelectNCTokenGrid.SelectedIndex];
                    SelectedNCTokenArgs = new SelectedNCTokenArgs();
                    SelectedNCTokenArgs.RecordedNCToken = selectedNCToken;
                    NCTokenForVPCalculationIsSelected?.Invoke(sender, SelectedNCTokenArgs);
                    isNCTokenSelectedForSubtractionTable = true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        "You cannot proceed to record a VP token without selecting an NC token for subtraction table", "Cannot record VP token!",MessageBoxButton.OK);
                }
                
            }
            
        }
    }

    public class SelectedNCTokenArgs : EventArgs
    {
        public RecordedProtocolHistory RecordedNCToken { get; set; }
    }
}
