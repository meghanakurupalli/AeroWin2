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
        }

        private List<RecordedProtocolHistory> recordedNCTokens;
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
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedNCToken = RecordedNCTokens[SelectNCTokenGrid.SelectedIndex];
            SelectedNCTokenArgs = new SelectedNCTokenArgs();
            SelectedNCTokenArgs.RecordedNCToken = selectedNCToken;
            NCTokenForVPCalculationIsSelected?.Invoke(sender, SelectedNCTokenArgs);
        }
    }

    public class SelectedNCTokenArgs : EventArgs
    {
        public RecordedProtocolHistory RecordedNCToken { get; set; }
    }
}
