﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for TokenListWindow.xaml
    /// </summary>
    public partial class TokenListWindow
    {
        //string _protocolFileNameTWin;
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public ObservableCollection<Protocol> protocols = new ObservableCollection<Protocol>();

        string path;
        string noOfDoneRepCount = "1";

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
            if (TokenListGrid.SelectedIndex == -1)
            {
                PreviousButton.IsEnabled = false;
            }
            string temp = str;
            path = Path.Combine(generatedProtocolFilesPath, temp + ".csv");
            var reader = new StreamReader(File.OpenRead(path));
            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                var temp1 = string.Concat(noOfDoneRepCount, " of ", splits[4]);
                protocols.Add(new Protocol() { TokenType = splits[0], Utterance = splits[1], Rate = splits[2], Intensity = splits[3], TotalRepetitionCount = temp1 });

            }
            protocols.RemoveAt(0);
            TokenListGrid.ItemsSource = protocols;
            TokenListGrid.DataContext = this;
            TokenListGrid.SelectedIndex = 0;
            reader.Close();
           
        }

        public event EventHandler TokenListWindowCloseEvent;
        private bool tempChar = false;
        static int i;
        static int saveme = 1;
        public int CurrentRepetitionCount { get; set; } = 1;
        public int CurrentTotalRepetitionCount { get; set ; }
       
        public void ChangeIndexSelection()
        {
            
            var countOfProtocols = TokenListGrid.Items.Count;
            var currentProtocol = protocols[TokenListGrid.SelectedIndex];
            var temp = currentProtocol.TotalRepetitionCount.Split(' ');
            var currentProtocolRepetitionCount = Int32.Parse(temp[2]);
            CurrentTotalRepetitionCount = currentProtocolRepetitionCount; // Repetition count of current protocol. eg: VP, pa-pa-pa, 2  ==> gives 2. 
            TokenListGrid.ItemsSource = protocols;
            if (saveme < currentProtocolRepetitionCount)
            {
                
                saveme++;
                CurrentRepetitionCount = saveme; 
                var temp2 = string.Concat(saveme, " of ", currentProtocolRepetitionCount);
                currentProtocol.TotalRepetitionCount = temp2;
                Debug.Print("temp2 : " + temp2);
                protocols[TokenListGrid.SelectedIndex].TotalRepetitionCount = temp2;

            }
            else
            {
                int nextIndex = TokenListGrid.SelectedIndex + 1;
                if (nextIndex <= countOfProtocols - 2)
                {
                    TokenListGrid.SelectedIndex = nextIndex;
                    saveme = 1;
                    CurrentRepetitionCount = saveme;
                }
                //TokenListGrid.ItemsSource = protocols;
            }

            //var currentTokenType = protocols[TokenListGrid.SelectedIndex].TokenType;
           // var nextTokenType = protocols[TokenListGrid.SelectedIndex + 1].TokenType;
            

            //_givesCurrentRepCount = 0;    
            if (TokenListGrid.SelectedIndex >= TokenListGrid.Items.Count - 2)
            {
                TokenListGrid.SelectedIndex = TokenListGrid.Items.Count - 2;
            }


            CheckForVPTokenType(protocols[TokenListGrid.SelectedIndex].TokenType);
        }

        private void CheckForVPTokenType(string tokenType)
        {
            
            if (tokenType == "VP")
            {
                //SHow the window with NC tokens
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            saveme = 1;
            CurrentRepetitionCount = 1;
            int currentIndex = TokenListGrid.SelectedIndex;
            
            if(currentIndex == 0)
            {
                //PreviousButton.IsEnabled = false;
                TokenListGrid.SelectedIndex = 0;
            }
            else
            {
                int changedIndex = currentIndex - 1;
                TokenListGrid.SelectedIndex = changedIndex;
                
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            saveme = 1;
            CurrentRepetitionCount = 1;
            int currentIndex = TokenListGrid.SelectedIndex;

            if (currentIndex == protocols.Count - 1)
            {
                //NextButton.IsEnabled = false;
                TokenListGrid.SelectedIndex = protocols.Count - 1;
            }
            else
            {
                int changedIndex = currentIndex + 1;
                TokenListGrid.SelectedIndex = changedIndex;
                
            }
        }

        private void TokenListWindowClosed_Event(object sender, EventArgs e)
        {
            TokenListWindowCloseEvent?.Invoke(sender, e);
        }
    }

    
}
