﻿using CenterSpace.NMath.Core;
using LiveCharts;
using NAudio.Wave;
using Nito.KitchenSink.CRC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LiveCharts.Defaults;
using MainWIndowDesign;
using FormsMessageBox = System.Windows.Forms.MessageBox;
using MessageBox = System.Windows.MessageBox;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        //public GearedValues<double> audioPoints { get; set; }//= new ChartValues<double>();
        public ChartValues<float> AudioPoints { get; set; }//= new ChartValues<double>();
                                                           // public SeriesCollection seriesCollection { get; set; }

        //string generatedWaveFilesPath = @"D:\GIT\AeroWin2\GeneratedWaveFiles";
        public string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        public string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        SaveFileWindow sWin;

        WaveIn wi;
        static WaveFileWriter wfw;

        public string DataFileName;
        string ProtocolFileName;

        string ProtFileTWin;

        double seconds = 0;

        Queue<float> displaypoint;

        public string pathForwavFiles;

        private bool _isTokenListWindowClosed;
        private bool isSelectNCTokenShown;
        private DoubleVector peterSavitzkyCoefficients = new DoubleVector();
        private int filterWidthLeft;
        private int filterWidthRight;

        public bool IsTokenListWindowClosed
        {
            get => _isTokenListWindowClosed;
            set
            {
                _isTokenListWindowClosed = value;
                RaisePropertyChanged("IsTokenListWindowClosed");
            }
        }

        private bool isAudioTokenRecorded;
        private RecordedProtocolHistory SelectedNCTokenForVPCalculation { get; set; }

        public ChartValues<float> PressureLineSeriesValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> AirFlowLineSeriesValues { get; set; } = new ChartValues<float>();
        public ChartValues<double> ResistanceLineSeriesValues { get; set; } = new ChartValues<double>();
        
        public double? _PressureXAxisMax;
        public double? _AirflowXAxisMax;
        public double? PressureXAxisMax
        {
            get => _PressureXAxisMax;
            set
            {
                _PressureXAxisMax = value;
                OnPropertyChanged("PressureXAxisMax");
            }
        }

        public double? AirflowXAxisMax
        {
            get => _AirflowXAxisMax;
            set
            {
                _AirflowXAxisMax = value;
                OnPropertyChanged("AirflowXAxisMax");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private readonly int QUEUE_THRESHOLD = 8;
        SerialPort port = null;
        Queue<byte> receivedData = new Queue<byte>();
        public static float[] temparray2 = new float[19];

        private CRC16.Definition definition;
        private CRC16 hashFunction;

        Queue<RawValuePacket> RawValuePackets = new Queue<RawValuePacket>();
        double time = 0;
        //ObservableCollection<protocol> protocols;
        TokenHistoryWindow THWin;
        private SelectNCToken SelectNCTokenWindow { get; set; }
        public List<RecordedProtocolHistory> TokenHistory { get; set; }

        List<float> pressures2 = new List<float>(); //Intermediately adds pressure values so that first value of this list can be added to livecharts once a limit is reached.
        List<float> airflows = new List<float>(); //Intermediately adds airflows values so that first value of this list can be added to livecharts once a limit is reached.
        int checkButtonFlag1, checkButtonFlag2;
        DateTime after5sec;
        public string SaveFileLocationOfTokenHistoryFile { get; set; }

        //private LR_SummaryStatistics lrsum =  new LR_SummaryStatistics();
        private VPSummaryStatisticsWindow vpsum = new VPSummaryStatisticsWindow();

        private SummaryStatisticsWindow _summaryStatisticsWindow = new SummaryStatisticsWindow();
        private TokenListWindow _tokenListWindow;
        List<byte> AudioData = new List<byte>() { 0x00 };
        public string FilePath { get; private set; }
        public string openExtFile_THFName;
        string audioFileToBePlayed;
        public string FilePathForPrandAf { get; private set; }
        public bool IsNCTokenForVPResistanceSelected { get; set; }

        public MainWindow()
        {

            InitializeComponent();
            DataContext = this;
            IsTokenListWindowClosed = true;
            AudioPoints = new ChartValues<float>();

            definition = new CRC16.Definition() { TruncatedPolynomial = 0x8005 };
            hashFunction = new CRC16(definition);
            hashFunction.Initialize();
            PressureXAxisMax = 210;
            AirflowXAxisMax = 210;

        }

        private void SaveFileWindow_SaveClicked(object sender, EventArgs e)
        {
            port = new SerialPort("COM3", 117000, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpen == false)
                {
                    port.Open();
                    port.Write("sp");

                }

                Thread backgroundThread = new Thread(dataCollectionThread);
                backgroundThread.IsBackground = true;
                backgroundThread.Start();

                port.DataReceived += SerialDataReceived;
                DataFileName = sWin.Data_File_Name;
                ProtocolFileName = sWin.Protocol_File_Name;
                ProtFileTWin = ProtocolFileName;

                pathForwavFiles = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName);// pathforwavFiles is also the path for saving pressure, airflow and resistance files.
                Directory.CreateDirectory(pathForwavFiles);

                var path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(ProtocolFileName);
                    }
                }

                //TokenListWindow TWin = new TokenListWindow();
                //TWin.Protocol_File_Name_TWin = ProtocolFileName;
                //Debug.Print("abba jabba : "+TWin.Protocol_File_Name_TWin.ToString());
                //TWin.Show();
                _tokenListWindow = new TokenListWindow(ProtFileTWin);
                _tokenListWindow.TokenListWindowCloseEvent += HandleTokenListWindowCloseEvent;
                _tokenListWindow.TokenIsSelectedEvent += HandleTokenSelectionChangeEvent;
                _tokenListWindow.Owner = this;
                IsTokenListWindowClosed = false;
                _tokenListWindow.Show();
                InitializeTokenHistory();
                //Debug.Print("Data File name from SFW : " + DataFileName + " Prot : " + ProtocolFileName);
            }
            catch (Exception)
            {
                FormsMessageBox.Show(@"Equipment not connected!", @"Please make sure that the equipment is connected and you have given the right port number.", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //FormsMessageBox.Show(Exception);
            }
        }

        private void HandleTokenListWindowCloseEvent(object sender, EventArgs e)
        {
            IsTokenListWindowClosed = true;
            if (TokenHistory != null && TokenHistory.Any())
            {
                SaveTokenHistoryToAFile(TokenHistory, SaveFileLocationOfTokenHistoryFile);
            }
        }

        private void HandleTokenSelectionChangeEvent(object sender, SelectedTokenArguments e)
        {
            var currentProtocol = e.Protocol;
            if (currentProtocol.TokenType == "VP")
            {
                checkButtonFlag1 = 1;
                if (IsNCTokenForVPResistanceSelected == false)
                {
                    ManageVPTokenRecording(currentProtocol, TokenHistory);
                }
                else
                {
                    checkButtonFlag1 = 0;
                }
            }
            else
            {
                checkButtonFlag1 = 0;
            }
           
        }

        private void InitializeTokenHistory()
        {
            TokenHistory = new List<RecordedProtocolHistory>();
        }

        public float[] GetCoefficients()
        {
            string[] lines = File.ReadAllLines(@"C:\Users\megha\Scripts\AeroWin2\MainWindowDesign\MainWindowDesign\Resources\coefficients.txt");

            string[] coefficients = new string[10];
            float[] coefficients1 = new float[10];
            foreach (string line in lines)
            {
                coefficients = line.Split(',');

            }

            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients1[i] = float.Parse(coefficients[i]);
            }

            return (coefficients1);
        }

        private bool StartAudioRecordingAndCheckIfWeCanProceedRecordingFurther(double time)
        {
            //wi = new WaveIn();
            wi.DataAvailable -= wi_DataAvailable;
            wi.DataAvailable += wi_DataAvailable;
            //wi.RecordingStopped -= wi_RecordingStopped;
            //wi.RecordingStopped += wi_RecordingStopped;
            wi.WaveFormat = new WaveFormat(4000, 32, 1); //Downsampled audio from 44KHz to 4kHz 

            //AudioData;
            //pathForwavFiles = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName);
            //System.IO.Directory.CreateDirectory(pathForwavFiles);

            int TWinSelectedIndex = _tokenListWindow.TokenListGrid.SelectedIndex;
            int TWinCurrentRepCount = _tokenListWindow.CurrentRepetitionCount;
            Debug.Print("na_moham : " + TWinCurrentRepCount);
            Debug.Print("File Name created : " + DataFileName + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount);
            FilePath = System.IO.Path.Combine(pathForwavFiles, DataFileName + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount + ".wav");
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            //wfw = new WaveFileWriter(path, wi.WaveFormat);
            //Debug.Print("DataFileName : " + DataFileName);
            displaypoint = new Queue<float>();

            wi.StartRecording();

            this.time = time;
            return true;
        }
        StringBuilder csv = new StringBuilder();

        void StopAudioRecording()
        {
            isAudioTokenRecorded = true;
            wi.StopRecording();
            wi.Dispose();

            // stop recording

            using (var wfw2 = new WaveFileWriter(FilePath, wi.WaveFormat))
            {
                wfw2.Write(AudioData.ToArray(), 0, AudioData.Count);
            }

            //AudioData = null;
            AudioData.Clear();
            SaveFileLocationOfTokenHistoryFile = System.IO.Path.Combine(pathForwavFiles, DataFileName + "TH" + ".csv");
            
            var indexOfMatchedToken = TokenHistory.FindIndex(
                x =>
                    x.TokenType == _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].TokenType
                    && x.Utterance == _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Utterance
                    && x.Rate == _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Rate
                    && x.Intensity == _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Intensity
                    && x.TotalRepetitionCount == _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].TotalRepetitionCount
                    && x.SelectedIndex == _tokenListWindow.TokenListGrid.SelectedIndex
            );

            var recordedProtocolHistory = new RecordedProtocolHistory
            {
                TokenType = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].TokenType,
                Utterance = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Utterance,
                Rate = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Rate,
                Intensity = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].Intensity,
                TotalRepetitionCount = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].TotalRepetitionCount,
                SelectedIndex = _tokenListWindow.TokenListGrid.SelectedIndex
            };

            if (indexOfMatchedToken < 0)
            {
                TokenHistory.Add(recordedProtocolHistory);
            }
            else
            {
                TokenHistory[indexOfMatchedToken] = recordedProtocolHistory;
            }
            seconds = 0;
            //AudioPoints.Clear();
            _tokenListWindow.ChangeIndexSelection();
        }

        private void ManageVPTokenRecording(Protocol currentProtocol, List<RecordedProtocolHistory> tokenHistoryAsOfNow)
        {
            if (currentProtocol == null || currentProtocol.TokenType != "VP") return;
            if (tokenHistoryAsOfNow == null || !tokenHistoryAsOfNow.Any())
            {
                MessageBox.Show("No Tokens are recorded. Cannot record resistance for VP.");
                return;
            }
            var ncRecordedTokenHistory = tokenHistoryAsOfNow?.Where(x => x.TokenType == "NC").ToList();
            if (ncRecordedTokenHistory != null && ncRecordedTokenHistory.Any())
            {
                ShowTokenHistoryForVPRecording(ncRecordedTokenHistory);
            }
            else
            {
                MessageBox.Show("No NC tokens are recorded. Cannot record resistance for VP.");
            }
        }

        private void ShowTokenHistoryForVPRecording(List<RecordedProtocolHistory> ncTokenHistory)
        {
            InitializeSelectNCTokenWindow(ncTokenHistory);
        }

        private void InitializeSelectNCTokenWindow(List<RecordedProtocolHistory> tokenHistory)
        {
            if (tokenHistory.Any())
            {
                SelectNCTokenWindow = new SelectNCToken(tokenHistory);
                SelectNCTokenWindow.NCTokenForVPCalculationIsSelected += HandleSelectedNCTokenFoVPCalculation;
                SelectNCTokenWindow.Topmost = true;
                SelectNCTokenWindow.Owner = this;
                SelectNCTokenWindow.ShowDialog();
            }
        }

        private void HandleSelectedNCTokenFoVPCalculation(object sender, SelectedNCTokenArgs args)
        {
            SelectedNCTokenForVPCalculation = args.RecordedNCToken;
            SelectNCTokenWindow.Close();
            IsNCTokenForVPResistanceSelected = true;
            checkButtonFlag1 = 0;
        }

        private void SaveTokenHistoryToAFile(List<RecordedProtocolHistory> tokenHistory, string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
            foreach (var token in tokenHistory)
            {
                var historyValue = token.TokenType + "," + token.Utterance + "," + token.Rate + "," + token.Intensity + "," + token.TotalRepetitionCount + "," + token.SelectedIndex;
                using (StreamWriter stream = File.AppendText(fileLocation))
                {
                    stream.WriteLine(historyValue);
                }
            }
        }

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            seconds += 1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0;

            if (isAudioTokenRecorded == false)
            {
                try
                {

                    for (int i = 0; i < e.BytesRecorded; i++)
                    {
                        AudioData.Add(e.Buffer[i]);
                    }

                    //wfw.Write(e.Buffer,0, e.BytesRecorded);


                    //double secondsRecorded = (double)(1.0 * wfw.Length / wfw.WaveFormat.AverageBytesPerSecond * 1.0);

                    byte[] points = new byte[4];


                    for (int i = 0; i < e.BytesRecorded - 4; i += 100)

                    {
                        points[0] = e.Buffer[i];
                        points[1] = e.Buffer[i + 1];
                        points[2] = e.Buffer[i + 2];
                        points[3] = e.Buffer[i + 3];
                        displaypoint.Enqueue(BitConverter.ToInt32(points, 0));
                    }


                    AudioPoints.Clear();
                    float[] points2 = displaypoint.ToArray();
                    float[] points3 = displaypoint.ToArray();

                    coefficients = GetCoefficients();
                    for (int i = 0; i < 5; i++)
                    {
                        a[i] = coefficients[i];

                    }
                    for (int i = 5; i < coefficients.Length; i++)
                    {
                        b[i - 5] = coefficients[i];

                    }

                    for (var x = 4; x < points2.Length; x++)
                    {
                        //coefficients from file generated by MATLAB
                        points3[x] = ((b[0] * x) + (b[1] * (x - 1)) + (b[2] * (x - 2)) + (b[3] * (x - 3)) + (b[4] * (x - 4)) + (a[1] * points2[x - 1]) + (a[2] * points2[x - 2]) + (a[3] * points2[x - 3]) + (a[4] * points2[x - 4]));

                    }

                    for (int x = 0; x < points3.Length; ++x)
                    {
                        //pl.Points.Add(Normalize(x, points3[x]));
                        Point p = Normalize2(x, points3[x]);
                        // Debug.Print("p.Y : " + p.Y);
                        AudioPoints.Add((float)p.Y);
                    }

                    if (seconds - time > 0)
                    {
                        //Debug.Print("inside if : " + time + ", Seconds : " + seconds);                
                        //wi.StopRecording();
                        //wi_RecordingStopped();
                        wi.GetPosition();
                        StopAudioRecording();
                        // May try flushing here
                        //wfw.Flush();
                        // audioTimer.Stop();
                        //TWin.Close();
                        //Debug.Print("stop recording");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }


        }

        Point Normalize2(Int32 x, float y)
        {

            Point p = new Point
            {

                // X = 1.99 * x / 1800 * plW,
                X = x,
                // Y = plH / 2.0 - y / (Math.Pow(2, 28) * 1.0) * (plH)
                Y = y / (Math.Pow(2, 25))


            };

            // double k = p.Y;
            //File.AppendAllText(@"D:\GIT\aerowinrt\audio_use\textfile1.txt", "(" + h.ToString("#.###") + "," + k.ToString(" #.##### ") + "),");
            return p;
        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            sWin = new SaveFileWindow();
            sWin.Owner = this;
            sWin.Activate();
            sWin.Topmost = true;
            sWin.Show();
            //StartButton.IsEnabled = true;
            sWin.OkButtonClicked -= SaveFileWindow_SaveClicked;
            sWin.OkButtonClicked += SaveFileWindow_SaveClicked;

        }

        private void OpenExistingFileButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPoints.Clear();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            string filter = "Txt file (*.txt)|*.txt| All Files (*.*)|*.*";
            //When integrated with the pressure sensor, show csv files instead of wav files.
            openFileDialog.Filter = filter;
            openFileDialog.InitialDirectory = generatedWaveFilesPath;
            bool? isFileSelected = openFileDialog.ShowDialog();
            if (isFileSelected.HasValue && isFileSelected.Value)
            {
                string DataFileName_ext;
                DataFileName_ext = openFileDialog.ToString();
                DataFileName = System.IO.Path.GetFileNameWithoutExtension(DataFileName_ext);
                Debug.Print("Data File name : " + DataFileName);


                //send datafilename to THWin.startRecordingDataAndCheckToProceedRecording
                //ProtocolFileName.Text = _protocolFileName;
            }
            else
            {
                return;
            }
            
            pathForwavFiles = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName);
            openExtFile_THFName = System.IO.Path.Combine(pathForwavFiles, DataFileName + "TH" + ".csv");

            THWin = new TokenHistoryWindow(this);
            THWin.Topmost = true;
            THWin.Owner = this;
            THWin.Show();
        }


        private void ProtocolBuilder_Click(object sender, RoutedEventArgs e)
        {
            ProtocolFileBuilderWindow pWin = new ProtocolFileBuilderWindow();
            pWin.Show();
        }

        private bool startRecordingDataAndCheckToProceedRecording()
        {
            after5sec = DateTime.UtcNow.AddSeconds(5);
            try
            {
                string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
                string[] txtFileContentGivesProtocolFileName_ext = File.ReadAllLines(path);
                string txtFileContentGivesProtocolFileName = System.IO.Path.GetFileNameWithoutExtension(txtFileContentGivesProtocolFileName_ext[0]);
                string pathToProtocolFileCount = System.IO.Path.Combine(generatedProtocolFilesPath, txtFileContentGivesProtocolFileName + ".csv");
                File.ReadAllLines(pathToProtocolFileCount);
                time = 5;
                //checkButtonFlag1 = 0;
                //checkButtonFlag2 = 0;
                return StartAudioRecordingAndCheckIfWeCanProceedRecordingFurther(time);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
                //MessageBox.Show("Exception : " + e);
                //return false;
            }

        }

        public void DisplayPressureAirflowResistance(string pressureAirflowFileToBeDisplayed)
        {
            //throw new NotImplementedException();
            PressureLineSeriesValues.Clear();
            AirFlowLineSeriesValues.Clear();
            ResistanceLineSeriesValues.Clear();
            AudioPoints.Clear();
            List<float> pressure = new List<float>();
            List<float> airflow = new List<float>();
            List<float> resistance = new List<float>();
            using (var reader = new StreamReader(pressureAirflowFileToBeDisplayed))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    pressure.Add(float.Parse(values[0]));
                    airflow.Add(float.Parse(values[1]));
                    resistance.Add(float.Parse(values[2]));

                }

                for (int i = 0; i < pressure.Count; i += 6)
                {
                    PressureLineSeriesValues.Add(pressure[i]);
                    AirFlowLineSeriesValues.Add(airflow[i]);
                    ResistanceLineSeriesValues.Add(resistance[i]);
                }
            }
            //calculateLRResistance(pressure, airflow);
        }

        private (List<float>, List<float>) DemeanSignal(List<float> pressure, List<float> airflow)
        {
            float pressureOffset = 0;
            float airflowOffset = 0;
            for (int i = 0; i < 100; i++)
            {
                pressureOffset = pressureOffset + pressure[i];
                airflowOffset = airflowOffset + airflow[i];
            }

            pressureOffset = pressureOffset / 100;
            airflowOffset = airflowOffset / 100;

            //pressures don't cross +/- 0.02

            pressure = pressure.Select(x => x - pressureOffset).ToList();
            airflow = airflow.Select(x => x - airflowOffset).ToList();
            return (pressure, airflow);
        }

        public PeakFinderSavitzkyGolay CalculatePeaks(List<float> demeanedPressure, List<float> demeanedAirflow)
        {
            double[] arr = new double[demeanedPressure.Count];
            for (int i = 0; i < demeanedPressure.Count; i++)
            {
                arr[i] = Convert.ToDouble(demeanedPressure[i]);
            }

            var v = new DoubleVector(arr);

            PeakFinderSavitzkyGolay pfa = new PeakFinderSavitzkyGolay(v, 8, 4);
            //pfa.LocatePeaks();
            List<float> list = new List<float>();
            //pfa.GetAllPeaks();
            return pfa;
        }


        //better get pressures directly from the files and then calculate the resisitance, put it in a csv file and just display it along with pressure and airflow when an existing file is opened.


        public (List<double>, List<string> ) calculateVPResistance(List<float>demeanedPressures, List<float> demeanedAirflows, List<float> NCPressures, List<float> NCAirflows, List<float> NCResistances, string filepath)
        {
            
            var pfa = CalculatePeaks(demeanedPressures,demeanedAirflows);
            
            pfa.LocatePeaks();
            // List<double> list = new List<float>();
            pfa.GetAllPeaks();
            List<double> pressureMaximas = new List<double>();
            List<int> pressureMaximaIndices = new List<int>();
            int flag1 = 0;
            int flag2 = 0;
            List<double> initialPressurePeaks = new List<double>();
            List<int> initialPressurePeakIndices = new List<int>();
            List<double> finalResistances = new List<double>();
            List<string> summaryStats = new List<string>();


            try
            {
                for (int p = 0; p < pfa.NumberPeaks; p++)
                {
                    Extrema peak = pfa[p];
                    if (peak.Y > 0.2)
                    {
                        Debug.Print("Found peak at = ({0},{1})", peak.X, peak.Y);
                        //pressureMaximas.Add(peak.Y);
                        //pressureMaximaIndices.Add(Convert.ToInt32(peak.X));
                        initialPressurePeaks.Add(peak.Y);
                        initialPressurePeakIndices.Add(Convert.ToInt32(peak.X));
                    }

                }

                for (int i = 0; i < initialPressurePeaks.Count - 1; i++)
                {
                    if (initialPressurePeakIndices[i + 1] - initialPressurePeakIndices[i] < 100)
                    {
                        initialPressurePeakIndices.RemoveAt(i);
                        initialPressurePeaks.RemoveAt(i);
                    }
                }


                List<int> indicesofStartofPeaks = new List<int>();
                List<int> indicesofEndofPeaks = new List<int>();
                List<int> midPressurePeaksIndices = new List<int>();

                //for (int i = 0; i < pressureMaximas.Count - 1; i++)
                //{
                //    int midPressurePeaksIndex = Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2);
                //    int k = 0;
                //    for (int j = midPressurePeaksIndex; j < pressureMaximaIndices[k + 1]; j++)
                //    {
                //        if (pressure[j] >= 0.1 && flag1 == 0)// look into start of pressure peak after the new sensor has come. 0.1 is just a place holder.
                //        {
                //            indicesofStartofPeaks.Add(j);
                //            flag1 = 1;
                //        }
                //    }
                //    flag1 = 0;
                //}
                for (int i = 0; i < pressureMaximas.Count - 1; i++)
                {
                    midPressurePeaksIndices.Add(Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2)); //From 2nd peak to last but one peak

                }


                int r = 0;
                while (r < midPressurePeaksIndices.Count - 1)
                {
                    for (int i = midPressurePeaksIndices[r]; i < pressureMaximaIndices[r + 1]; i++)
                    {
                        if (demeanedPressures[i] >= 0.1 && flag1 == 0)
                        {
                            indicesofStartofPeaks.Add(i);
                            flag1 = 1;
                        }
                    }

                    flag1 = 0;
                }

                int q = 1;
                while (q < midPressurePeaksIndices.Count - 1)
                {
                    for (int i = pressureMaximaIndices[q]; i < midPressurePeaksIndices[q]; i++)
                    {
                        if (demeanedPressures[i] <= 0.1 && flag2 == 0)
                        {
                            indicesofEndofPeaks.Add(i);
                            flag2 = 1;
                        }
                    }

                    flag2 = 0;
                }

                //for (int i = 0; i < pressureMaximas.Count - 1; i++)
                //{
                //    int midPressurePeaksIndex = Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2);
                //    int k = 0;
                //    for (int j = midPressurePeaksIndex; j < pressureMaximaIndices[k + 1]; j++)
                //    {
                //        if (pressure[j] <= 0.1 && flag2 == 0)// look into start of pressure peak after the new sensor has come. 0.1 is just a place holder.
                //        {
                //            indicesofEndofPeaks.Add(j);
                //            flag2 = 1;
                //        }
                //    }
                //    flag2 = 0;
                //}
                int count = 0;
                int count1 = 0;
                float airflowValue;

                List<float> initialResistances = new List<float>();
                for (int i = 0; i < demeanedPressures.Count; i++)
                {
                    initialResistances.Add(0);
                }
                //for (int i = 0; i < pressure.Count; i++)
                //{
                //    try
                //    {
                //        initialResistances.Add(pressure[i] / airflow[i]);
                //    }
                //    catch (DivideByZeroException)
                //    {
                //        initialResistances.Add(9999);
                //    }

                //}
                for (int i = indicesofStartofPeaks[count1]; i < indicesofEndofPeaks[count1]; i++)
                {
                    if (count1 < indicesofStartofPeaks.Count)
                    {
                        try
                        {
                            initialResistances.Add(demeanedPressures[i] / demeanedAirflows[i]);
                        }
                        catch (DivideByZeroException)
                        {
                            initialResistances.Add(9999);
                        }
                    }

                    count1++;
                }

                double resistancesToBeSubtracted;
                
                for (int i = 0; i < demeanedPressures.Count; i++)
                {
                    finalResistances.Add(0);
                }
                List<double> resistancesforStatisticsCalculations = new List<double>();
                for (int i = indicesofStartofPeaks[count]; i < indicesofEndofPeaks[count]; i++)
                {
                    if (count < indicesofStartofPeaks.Count)
                    {
                        //get airflows here
                        //get pressures here
                        airflowValue = demeanedAirflows[i];
                        float closest = NCAirflows.Aggregate((x, y) => Math.Abs(x - airflowValue) < Math.Abs(y - airflowValue) ? x : y);
                        int indexOfClosestElement = NCAirflows.IndexOf(closest);
                        resistancesToBeSubtracted = NCResistances[indexOfClosestElement];
                        finalResistances[i] = initialResistances[i] - resistancesToBeSubtracted;
                        resistancesforStatisticsCalculations.Add(initialResistances[i] - resistancesToBeSubtracted);

                    }

                    count++;
                }

                double meanofPressuresatpeaks = pressureMaximas.Sum() / pressureMaximas.Count;
                double meanofairflowsatpeakpressures;
                double sum = 0;
                for (int i = 0; i < pressureMaximaIndices.Count; i++)
                {
                    sum = sum + demeanedAirflows[pressureMaximaIndices[i]];
                }

                meanofairflowsatpeakpressures = sum / pressureMaximaIndices.Count;
                double meanofresistances = resistancesforStatisticsCalculations.Sum() / resistancesforStatisticsCalculations.Count;


                double sdofpressurepeaks;
                for (int i = 0; i < pressureMaximaIndices.Count; i++)
                {
                    sum = sum + Math.Pow((pressureMaximas[i] - meanofPressuresatpeaks), 2);
                }

                sum = sum / pressureMaximas.Count;
                sdofpressurepeaks = Math.Sqrt(sum);

                double sdofairflowsatpressurepeaks;
                sum = 0;
                for (int i = 0; i < pressureMaximaIndices.Count; i++)
                {
                    sum = sum + Math.Pow((demeanedAirflows[pressureMaximaIndices[i]] - meanofairflowsatpeakpressures), 2);
                }

                sdofairflowsatpressurepeaks = Math.Sqrt(sum / pressureMaximaIndices.Count);

                sum = 0;
                for (int i = 0; i < resistancesforStatisticsCalculations.Count; i++)
                {
                    sum = sum + Math.Pow((resistancesforStatisticsCalculations[i] - meanofresistances), 2);
                }
                var sdofresistance = Math.Sqrt(sum / resistancesforStatisticsCalculations.Count);
                

                
                summaryStats.Add(Math.Round(meanofPressuresatpeaks, 3).ToString(CultureInfo.InvariantCulture));
                summaryStats.Add(Math.Round(sdofpressurepeaks, 3).ToString(CultureInfo.InvariantCulture));
                summaryStats.Add(Math.Round(meanofairflowsatpeakpressures, 3).ToString(CultureInfo.InvariantCulture));
                summaryStats.Add(Math.Round(sdofairflowsatpressurepeaks, 3).ToString(CultureInfo.InvariantCulture));
                summaryStats.Add(Math.Round(meanofresistances, 3).ToString(CultureInfo.InvariantCulture));
                summaryStats.Add(Math.Round(sdofresistance, 3).ToString(CultureInfo.InvariantCulture));
                void VPSumamryStatistics()
                {

                    _summaryStatisticsWindow.PopulateSummaryStatisticses.Clear();
                    _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() { Statistic = "Air Pressure(Peaks)", StatisticMean = (float)meanofPressuresatpeaks, StatisticSD = (float)sdofpressurepeaks});
                    _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() { Statistic = "Air flow(at Pressure Peaks)", StatisticMean = (float)meanofairflowsatpeakpressures, StatisticSD = (float)sdofairflowsatpressurepeaks });
                    _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() { Statistic = "Resistance", StatisticMean = (float)meanofresistances, StatisticSD = (float)sdofresistance });

                    _summaryStatisticsWindow.SummaryStatisticsGrid.ItemsSource = _summaryStatisticsWindow.PopulateSummaryStatisticses;

                    _summaryStatisticsWindow.Show();
                    _summaryStatisticsWindow.Topmost = true;
                    _summaryStatisticsWindow.Owner = this;
                }

                Dispatcher.BeginInvoke((Action)VPSumamryStatistics);
                


                //List<string> pressuresAirflowsandResistances = File.ReadAllLines(filepath).ToList();
                //for (int i = 0; i < pressuresAirflowsandResistances.Count; i++)
                //{
                //    pressuresAirflowsandResistances[i] = pressuresAirflowsandResistances[i] + "," + finalResistances[i];
                //}

                //for(int i = 0; i < summaryStats.Count; i++)
                //{
                //    pressuresAirflowsandResistances[i] = pressuresAirflowsandResistances[i] + "," + summaryStats[i];
                //}

                //File.WriteAllLines(filepath, pressuresAirflowsandResistances);

            }
            catch (Exception)
            {
                finalResistances.Clear();
                summaryStats.Clear();
                FormsMessageBox.Show("Bad VP token");
            }

            return (finalResistances, summaryStats);
        }

        

        public (List<double>, List<string>) CalculateLrResistance(List<float> filteredPressures, List<float> filteredAirflows, string filepath)
        {
            
          
            var pfa = CalculatePeaks(filteredPressures, filteredAirflows);
            
            PeakFinderSavitzkyGolay peakFinderSavitzky = pfa;
            peakFinderSavitzky.LocatePeaks();
            List<float> list = new List<float>();
            peakFinderSavitzky.GetAllPeaks();
            // int j = 0;
            int flag1 = 0;
            int flag2 = 0;

            List<int> pressureMaximaIndices = new List<int>();
            List<double> pressureMaximas = new List<double>();
            List<int> initialPressurePeakIndices = new List<int>();
            List<double> initialPressurePeaks = new List<double>();
            List<double> resistances = new List<double>();
            List<string> summaryStats = new List<string>();


            try
            {
                for (int p = 0; p < peakFinderSavitzky.NumberPeaks; p++)
                {
                    Extrema peak = peakFinderSavitzky[p];
                    if (peak.Y > 0.8)
                    {
                        Console.WriteLine("Found peak at = ({0},{1})", peak.X, peak.Y);
                        //pressureMaximas.Add(peak.Y);
                        //pressureMaximaIndices.Add(Convert.ToInt32(peak.X));
                        initialPressurePeaks.Add(peak.Y);
                        initialPressurePeakIndices.Add(Convert.ToInt32(peak.X));
                    }

                }

                //for (int i = 0; i < initialPressurePeaks.Count - 1; i++)
                //{
                //    if (initialPressurePeakIndices[i + 1] - initialPressurePeakIndices[i] < 80)
                //    {
                //        initialPressurePeakIndices[i] = 99999;
                //        initialPressurePeaks[i] = 99999;
                //    }
                //}

                for (int i = initialPressurePeaks.Count - 1; i > 0; i--)
                {
                    if (initialPressurePeakIndices[i] - initialPressurePeakIndices[i - 1] < 80)
                    {
                        initialPressurePeakIndices.RemoveAt(i - 1);
                        initialPressurePeaks.RemoveAt(i-1);
                    }
                }


                pressureMaximaIndices = initialPressurePeakIndices.ToList();
                pressureMaximas = initialPressurePeaks.ToList();

                if (pressureMaximas.Count >= 3)
                {
                    List<double> offsets = new List<double>();

                    for (int i = 1; i < pressureMaximas.Count - 2; i++)
                    {
                        var num = Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2);
                        offsets.Add(filteredPressures[num]);
                    }

                    List<double> pressureMeans = new List<double>();
                    for (int i = 1; i < pressureMaximas.Count - 2; i++)// Have to change to i = 1 /////-3
                    {
                        //double d = pressureMaximas[1];
                        pressureMeans.Add((pressureMaximas[i] + pressureMaximas[i + 1]) / 2);
                    }

                    int pressureMaximaCount = 1; // Have to change to pressureMaximaCount = 1
                    
                    for (int i = 0; i < filteredPressures.Count; i++)
                    {
                        resistances.Add(0);
                    }

                    //List<double> startofPeaks = new List<double>();
                    List<int> indicesofStartofPeaks = new List<int>();
                    List<int> midPressurePeaksIndices = new List<int>();
                    for (int i = 1; i < pressureMaximas.Count - 2; i++)
                    {
                        midPressurePeaksIndices.Add(Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2)); //From 2nd peak to last but one peak

                    }
                    int k = 0;
                    //for (int j = midPressurePeaksIndex; j < pressureMaximaIndices[k + 1]; j++)
                    //{
                    //    if (pressure[j] >= 0.1 && flag1 == 0)// look into start of pressure peak after the new sensor has come. 0.1 is just a place holder.
                    //    {
                    //        indicesofStartofPeaks.Add(j);
                    //        flag1 = 1;
                    //    }
                    //}
                    while (k < midPressurePeaksIndices.Count)
                    {
                        for (int i = midPressurePeaksIndices[k]; i < pressureMaximaIndices[k + 2]; i++)
                        {
                            if (filteredPressures[i] >= 0.1 && flag1 == 0)// look into start of pressure peak after the new sensor has come. 0.1 is just a place holder.
                            {
                                indicesofStartofPeaks.Add(i);
                                flag1 = 1;
                            }
                        }

                        k++;
                        flag1 = 0;
                    }



                    List<float> resistancesForStatisticCalculations = new List<float>();
                    int number = 0;
                    int r = 1;// Have to change to i = 1
                    int j = 1;// Have to change to j = 1
                              //Instead of peak to peak, take all values from where a peak startsto where a peak ends wih 0.2 threshold, so that it would be easier to claulate the resistances.
                    while (pressureMaximaCount < pressureMaximas.Count - 2)
                    {

                        float resistanceVal = 0;
                        for (number = pressureMaximaIndices[r]; number < indicesofStartofPeaks[r - 1]; number++)
                        {
                            if (filteredAirflows[number] == 0)
                            {
                                resistanceVal = 99999;
                            }
                            else
                            {
                                resistanceVal = (float)(pressureMeans[j] - offsets[j]) / filteredAirflows[number];

                            }
                            resistances[number] = resistanceVal;
                            resistancesForStatisticCalculations.Add(resistanceVal);

                        }
                        r++;
                        pressureMaximaCount++;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < resistances.Count; i += 6)
                        {
                            ResistanceLineSeriesValues.Add(resistances[i]);
                        }
                    });

                    List<float> airflowsAtRelease = new List<float>();
                    List<float> airflowsMidVowel = new List<float>();

                    for (int i = 0; i < pressureMaximaIndices.Count; i++)
                    {
                        airflowsAtRelease.Add(filteredAirflows[pressureMaximaIndices[i]]);
                    }


                    // For the statistic airflow-mid vowel, I'm taking the index of mid vowel to be the midpoint between pressure peaks.

                    //List<int> indicesOfMidPointsBetweenThePressurePeaks = new List<int>();
                    //int value;
                    //for (int i = 0; i < pressureMaximaIndices.Count; i++)
                    //{
                    //    value = Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2);
                    //    indicesOfMidPointsBetweenThePressurePeaks.Add(value);
                    //}

                    float sumOfAirflowsMidVowel = 0;
                    for (int i = 0; i < midPressurePeaksIndices.Count; i++)
                    {
                        sumOfAirflowsMidVowel = sumOfAirflowsMidVowel + filteredAirflows[midPressurePeaksIndices[i]];
                    }
                    float meanOfAirflowsAtRelease = airflowsAtRelease.Sum() / airflowsAtRelease.Count();
                    double SDofAirflowsAtRelease;
                    double temp = 0;
                    for (int i = 0; i < airflowsAtRelease.Count; i++)
                    {
                        temp += Math.Pow((airflowsAtRelease[i] - meanOfAirflowsAtRelease), 2);
                    }
                    temp = temp / airflowsAtRelease.Count;
                    SDofAirflowsAtRelease = Math.Sqrt(temp);



                    float meanOfAirflowsAtMidVowel = sumOfAirflowsMidVowel / midPressurePeaksIndices.Count();
                    temp = 0;
                    double SDofAirflowsMidVowel;
                    for (int i = 0; i < midPressurePeaksIndices.Count; i++)
                    {
                        temp += Math.Pow((filteredAirflows[midPressurePeaksIndices[i]] - meanOfAirflowsAtMidVowel), 2);
                    }
                    temp = temp / midPressurePeaksIndices.Count;
                    SDofAirflowsMidVowel = Math.Sqrt(temp);



                    float meanOfAirPressureAtPeaks = (float)pressureMaximas.Sum() / pressureMaximas.Count;
                    double SDofPressureAtPeaks;
                    temp = 0;
                    for (int i = 0; i < pressureMaximas.Count; i++)
                    {
                        temp += Math.Pow((pressureMaximas[i] - meanOfAirPressureAtPeaks), 2);
                    }
                    temp = temp / pressureMaximas.Count;
                    SDofPressureAtPeaks = Math.Sqrt(temp);


                    float meanOfResistances = resistancesForStatisticCalculations.Sum() / resistancesForStatisticCalculations.Count;
                    temp = 0;
                    for (int i = 0; i < resistancesForStatisticCalculations.Count; i++)
                    {
                        temp += Math.Pow((resistancesForStatisticCalculations[i] - meanOfResistances), 2);
                    }
                    temp = temp / resistancesForStatisticCalculations.Count;
                    var SDofResistances = Math.Sqrt(temp);

                    summaryStats.Add(Math.Round(meanOfAirflowsAtRelease, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(SDofAirflowsAtRelease, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(meanOfAirflowsAtMidVowel, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(SDofAirflowsMidVowel, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(meanOfAirPressureAtPeaks, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(SDofPressureAtPeaks, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(meanOfResistances, 3).ToString(CultureInfo.InvariantCulture));
                    summaryStats.Add(Math.Round(SDofResistances, 3).ToString(CultureInfo.InvariantCulture));
                    void LRSumamryStatistics()
                    {
                        _summaryStatisticsWindow.PopulateSummaryStatisticses.Clear();
                        _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() {Statistic = "Air flow(Release)", StatisticMean = meanOfAirflowsAtRelease, StatisticSD = (float) SDofAirflowsAtRelease});
                        _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() {Statistic = "Air flow(Mid Vowel)", StatisticMean = meanOfAirflowsAtMidVowel, StatisticSD = (float) SDofAirflowsMidVowel});
                        _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() {Statistic = "Air pressure", StatisticMean = meanOfAirPressureAtPeaks, StatisticSD = (float) SDofPressureAtPeaks});
                        _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() {Statistic = "Resistance", StatisticMean = meanOfResistances, StatisticSD = (float) SDofResistances});

                        _summaryStatisticsWindow.SummaryStatisticsGrid.ItemsSource = _summaryStatisticsWindow.PopulateSummaryStatisticses;

                        _summaryStatisticsWindow.Show();
                        _summaryStatisticsWindow.Topmost = true;
                        _summaryStatisticsWindow.Owner = this;
                    }

                    Dispatcher.BeginInvoke((Action) LRSumamryStatistics);

                    //List<string> pressuresAirflowsandResistances = File.ReadAllLines(filepath).ToList();
                    //for (int i = 0; i < pressuresAirflowsandResistances.Count - 1; i++)
                    //{
                    //    pressuresAirflowsandResistances[i] = pressuresAirflowsandResistances[i] + "," + resistances[i];
                    //}

                    //for (int i = 0; i < 8; i++)
                    //{
                    //    pressuresAirflowsandResistances[i] = pressuresAirflowsandResistances[i] + "," + summaryStats[i];
                    //}

                    //File.WriteAllLines(filepath, pressuresAirflowsandResistances);
                    
                }

                else
                {
                    //throw;
                    resistances.Clear();
                    summaryStats.Clear();
                    FormsMessageBox.Show(@"Bad LR token!", @"Bad LR token. Try recording it again.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                

                // Didn't test this yet, and still have to claculate the standard deviation values
            }

            catch (Exception e)
            {
                resistances.Clear();
                summaryStats.Clear();
                FormsMessageBox.Show("Bad LR token!", "Bad LR token. Try recording it again.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return (resistances, summaryStats); // These could be null.
            // We have : 
            //pressure peaks - gives mean and standard dev of peaks in summary
            // Airflows at pressure peaks - gives mean and standard dev of airflows at pressure peaks in summary
            // To get the airflow mid vowel, consider I'm going to consider that when pressure starts going up above 0.3 cm of H20, pressure peak is being formed.
            //So, I'm going to get the mean of points from 20 sample after the peak to 20 samples before the 0.3 cm threshold.
            //The mean of these values will give me the statistic airflow at mid vowel.


        }

        public void calculateNCResistance(List<float> pressures, List<float> airflows, string filePath)
        {
            List<float> NCResistances = new List<float>();
            for (int i = 0; i < pressures.Count; i++)
            {
                NCResistances.Add(pressures[i] / airflows[i]);
            }

            List<string> pressuresAirflowsandResistances = File.ReadAllLines(filePath).ToList();
            for (int i = 0; i < pressuresAirflowsandResistances.Count; i++)
            {
                pressuresAirflowsandResistances[i] = pressuresAirflowsandResistances[i] + "," + NCResistances[i];
            }
            this.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < NCResistances.Count; i += 6)
                {
                    ResistanceLineSeriesValues.Add(NCResistances[i]);
                }
            });

            void NCSumamryStatistics()
            {
                _summaryStatisticsWindow.PopulateSummaryStatisticses.Clear();
                _summaryStatisticsWindow.PopulateSummaryStatisticses.Add(new PopulateSummaryStatistics() { Statistic = "-" });

                _summaryStatisticsWindow.SummaryStatisticsGrid.ItemsSource = _summaryStatisticsWindow.PopulateSummaryStatisticses;

                _summaryStatisticsWindow.Show();
                _summaryStatisticsWindow.Topmost = true;
                _summaryStatisticsWindow.Owner = this;
            }

            Dispatcher.BeginInvoke((Action)NCSumamryStatistics);
            File.WriteAllLines(filePath, pressuresAirflowsandResistances);

        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {

        }

        uint ComputeCRC(byte[] data)
        {
            hashFunction.ComputeHash(data);

            uint retVal = hashFunction.Hash[1];
            retVal = (retVal << 8) + hashFunction.Hash[0];

            return retVal;
        }

        private void ClosingMethod(List<float> allPressures, List<float> allAirflows) //Calls after the 5-second interval.
        {
            int TWinSelectedIndex = 0;
            int TWinCurrentRepCount = 0;
            string selectedTokenType = null;
            StringBuilder stringBuilder = new StringBuilder();
            Dispatcher?.Invoke(() =>
            {
                TWinSelectedIndex = _tokenListWindow.TokenListGrid.SelectedIndex;
                TWinCurrentRepCount = _tokenListWindow.CurrentRepetitionCount;
                selectedTokenType = _tokenListWindow.Protocols[_tokenListWindow.TokenListGrid.SelectedIndex].TokenType;
            });
            List<double> resistances = new List<double>();
            List<string> summaryStats = new List<string>();
            List<float> filteredPressures = new List<float>();
            List<float> filteredAirflows = new List<float>();
            List<float> demeanedPressures = new List<float>();
            List<float> demeanedAirflows = new List<float>();

            var demeanedSignals = DemeanSignal(allPressures, allAirflows);
            demeanedPressures = demeanedSignals.Item1;
            demeanedAirflows = demeanedSignals.Item2;
           
            csv.Clear();
            checkButtonFlag1 = 0;
            checkButtonFlag2 = 0;  //These two values make sure that the 
            //var ncTokenHistory = TokenHistory.FindAll(x => x.TokenType == "NC");
            //ShowTokenHistoryForVPRecording(ncTokenHistory);

            // Building Savitzky Golay filter here because we are showing filtered values only after the token is recorded. 
            double[] arr = new double[allPressures.Count];

            MovingWindowFilter filter =
                new MovingWindowFilter(filterWidthLeft,filterWidthRight, peterSavitzkyCoefficients);

            for (int i = 0; i < demeanedPressures.Count; i++)
            {
                arr[i] = Convert.ToDouble(demeanedPressures[i]);
            }

            var v = new DoubleVector(arr);
            DoubleVector filteredPressureVector = filter.Filter(v);
            var prs = filteredPressureVector.ToList();
            filteredPressures = prs.Select(i => (float)i).ToList();


            for (int i = 0; i < demeanedAirflows.Count; i++)
            {
                arr[i] = Convert.ToDouble(demeanedAirflows[i]);
            }
            
            v = new DoubleVector(arr);
            DoubleVector filteredAirflowVector = filter.Filter(v);
            var airfs = filteredAirflowVector.ToList();
            filteredAirflows = airfs.Select(i => (float)i).ToList();


            PressureLineSeriesValues.Clear();
            AirFlowLineSeriesValues.Clear();


            PressureXAxisMax = 1200;
            AirflowXAxisMax = 1200;
            Dispatcher.Invoke(() =>
            {
                
                
                foreach (var item in filteredPressures)
                {
                    PressureLineSeriesValues.Add(item);
                }
            });

            Dispatcher.Invoke(() =>
            {
                foreach (var VARIABLE in filteredAirflows)
                {
                    AirFlowLineSeriesValues.Add(VARIABLE);
                }
            });
            

            if (selectedTokenType == "LR")
            {
                var reisistancesAndSummaryStatistics = CalculateLrResistance(filteredPressures, filteredAirflows, FilePathForPrandAf);
                resistances = reisistancesAndSummaryStatistics.Item1;
                summaryStats = reisistancesAndSummaryStatistics.Item2;
            }

            else if (selectedTokenType == "VP")
            {
                List<float> NCPressures = new List<float>();
                List<float> NCAirflows = new List<float>();
                List<float> NCResistances = new List<float>();

                try
                {
                    var selectedSubtractionToken = SelectedNCTokenForVPCalculation;
                    var splits = selectedSubtractionToken.TotalRepetitionCount.Split(' ');
                    var filename = DataFileName + "pr_af_" + selectedSubtractionToken.SelectedIndex + "_" + splits[0] +
                                   ".csv";
                    string filepath = System.IO.Path.Combine(pathForwavFiles, filename);
                    using (var rd = new StreamReader(filepath))
                    {
                        while (!rd.EndOfStream)
                        {
                            var splitstring = rd.ReadLine().Split(',');
                            NCPressures.Add(float.Parse(splitstring[0]));
                            NCAirflows.Add(float.Parse(splitstring[1]));
                            NCResistances.Add(float.Parse(splitstring[2]));
                        }
                    }
                    var resistancesandSummarystats = calculateVPResistance(filteredPressures, filteredAirflows, NCPressures, NCAirflows, NCResistances, FilePathForPrandAf);
                    resistances = resistancesandSummarystats.Item1;
                    summaryStats = resistancesandSummarystats.Item2;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show("Cannot find subtraction table");
                    throw;
                }
            }

            else if (selectedTokenType == "NC")
            {
                //CalculateNCResistance
                calculateNCResistance(filteredPressures,filteredAirflows, FilePathForPrandAf);
            }


            //calculateNCResistance(allPressures, allAirflows, filePathForPrandAf);

            if (selectedTokenType != "NC")
            {
                FilePathForPrandAf = System.IO.Path.Combine(pathForwavFiles, DataFileName + "pr_af" + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount + ".csv");
                if (File.Exists(FilePathForPrandAf))
                {
                    File.Delete(FilePathForPrandAf);
                }

                if (resistances.Count > 0 && summaryStats.Count > 0)
                {
                    for (int i = 0; i < summaryStats.Count; i++)
                    {
                        var line = string.Format("{0},{1},{2},{3}", filteredPressures[i], filteredAirflows[i], resistances[i], summaryStats[i]);
                        stringBuilder.AppendLine(line);
                    }

                    for (int i = summaryStats.Count; i < allPressures.Count; i++)
                    {
                        var line = string.Format("{0},{1},{2}", filteredPressures[i], filteredAirflows[i], resistances[i]);
                        stringBuilder.AppendLine(line);
                    }

                    File.WriteAllText(FilePathForPrandAf, stringBuilder.ToString());
                    stringBuilder.Clear();
                }
                else
                {
                    for (int i = 0; i < allPressures.Count; i++)
                    {
                        var line = string.Format("{0},{1}", filteredPressures[i], filteredAirflows[i]);
                        stringBuilder.AppendLine(line);
                    }
                    File.WriteAllText(FilePathForPrandAf, stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            }
            
            
        }

        private (DoubleVector,int,int) GenerateSavitzkyGolayCoefficients()
        {
            int nLeft = 4;
            int nRight = 4;
            int n = 4;
            DoubleVector peterSavitzkySmoothingCoefficients =
                MovingWindowFilter.SavitzkyGolayCoefficients(nLeft, nRight, n);
            return (peterSavitzkySmoothingCoefficients,nLeft,nRight);
        }

        // ReSharper disable once InconsistentNaming
        


        private void dataCollectionThread()
        {
            // pressures2 = new List<float>();
            List<float> listofAllPressures = new List<float>();
            List<float> listofAllAirflows = new List<float>();
            bool canRecordingBeContinued = true;
            bool checksIfweCanProceedRecordingWithAudio = true;

            while (true)
            {
                float pcomp = 0;
                float af_in_cc = 0;

                if (decodedPackets.Count > 0)
                {
                    var packet = decodedPackets.Dequeue();
                    //Debug.Print("New added packet info " + packet.ToString() );

                    switch (packet.packetType)
                    {
                        case PacketType.PressureOnly:
                            break;
                        case PacketType.PressureTemperature:
                            if (temparray2[1] != 0)
                            {
                                // Debug.Print("temparr[1] !=0");

                                var rawValuePacket = new RawValuePacket(packet);

                                RawValuePackets.Enqueue(rawValuePacket);
                                foreach (var item in rawValuePacket.PresB)
                                {
                                    pcomp = (float)((item - 30584.2) / 1251.79);
                                    pressures2.Add(pcomp);

                                }
                                foreach (var item in rawValuePacket.PresA)
                                //Channel for airflow
                                {
                                    af_in_cc = (float)(((item - 291273.1) / 234.06) * 16.667);
                                    airflows.Add(af_in_cc);
                                }

                                if (rawValuePacket.buttonData == 7 && checkButtonFlag1 == 0)
                                {
                                    PressureXAxisMax = 210;
                                    AirflowXAxisMax = 210;
                                    checkButtonFlag2 = 1;
                                    checkButtonFlag1 = 1;
                                    //after5sec = DateTime.UtcNow.AddSeconds(5);
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        PressureLineSeriesValues.Clear();
                                        AirFlowLineSeriesValues.Clear();
                                        ResistanceLineSeriesValues.Clear();
                                        AudioPoints.Clear();
                                        //StartButton.Content = "Start";
                                        //StartButton_Click(null, null);
                                        wi = new WaveIn();
                                        checksIfweCanProceedRecordingWithAudio = startRecordingDataAndCheckToProceedRecording();
                                        isAudioTokenRecorded = false;
                                        //making isVPAlertalreadyshown = true.

                                        //IsVPAlertAlreadyShown = false;
                                    });

                                    //Have to call closing method

                                }

                                if (checkButtonFlag1 == 1 && checksIfweCanProceedRecordingWithAudio)
                                {
                                    //var newLine = string.Format("{0},{1}", pcomp, af_in_cc);
                                    //var newLine = string.Format("{0},{1}", pcomp, af_in_cc);
                                    listofAllPressures.Add(pcomp);
                                    listofAllAirflows.Add(af_in_cc);
                                  //  csv.AppendLine(newLine);
                                }
                                //Have to do csv.clear() somewhere..
                                if (pressures2.Count > 5 && airflows.Count > 5 && checkButtonFlag2 == 1 && checksIfweCanProceedRecordingWithAudio)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        PressureLineSeriesValues.Add(pressures2[0]);
                                        AirFlowLineSeriesValues.Add(airflows[0]);

                                        pressures2.Clear();
                                        airflows.Clear();
                                    });

                                    if (DateTime.UtcNow.Subtract(after5sec).TotalMilliseconds > 0)
                                    {
                                        Console.WriteLine("UtcNow : " + DateTime.UtcNow + " after5sec : " + after5sec);
                                        Debug.Print("dateTime.UtcNow : " + DateTime.UtcNow);
                                        ClosingMethod(listofAllPressures, listofAllAirflows);
                                        listofAllPressures.Clear();
                                        listofAllAirflows.Clear();
                                    }

                                }

                            }
                            break;
                        case PacketType.TemperatureOnly:
                            break;
                        case PacketType.CalibrationInfo:

                            var calibrationvalues = new calibrationValues(packet);
                            var temparr = new float[28];
                            temparr = calibrationvalues.returnArrayA;
                            temparray2 = temparr;
                            var filterdata = GenerateSavitzkyGolayCoefficients();
                            peterSavitzkyCoefficients = filterdata.Item1;
                            filterWidthLeft = filterdata.Item2;
                            filterWidthRight = filterdata.Item3;
                            port.Write("go");
                            break;

                        default:
                            // Debug.Print( "Invalid packet received" );
                            break;
                    }


                }
            }

            throw new NotImplementedException();
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (port.BytesToRead > 0)
            {
                receivedData.Enqueue((byte)port.ReadByte());

                if (receivedData.Count >= QUEUE_THRESHOLD)
                {
                    ProcessQueue();
                }
            }
        }

        enum ProcessQueueStates
        {
            FindStart,
            GetType,
            GetLength,
            GrabbingPayload,
            NextIsEnd,
        }

        private ProcessQueueStates processState = ProcessQueueStates.FindStart;
        private int processLength = 0;

        Packet processPacket = new Packet();
        private Queue<byte> processPayload = new Queue<byte>();
        private Queue<byte> calculateCRC = new Queue<byte>();

        private Queue<Packet> decodedPackets = new Queue<Packet>();
        byte[] CRCTemp = new byte[2];

        private readonly byte START_BYTE = 0x59;

        private int CRCLength = 2;

        int flag = 0;

        private void ProcessQueue()
        {
            while (receivedData.Count > 0)
            {
                byte nextByte = receivedData.Dequeue();

                switch (processState)
                {
                    case ProcessQueueStates.FindStart:
                        if (nextByte == START_BYTE)
                        {
                            processState = ProcessQueueStates.GetType;
                        }
                        break;

                    case ProcessQueueStates.GetType:
                        processPacket.packetType = (PacketType)nextByte;

                        //if (processPacket.packetType == (PacketType)0x36)
                        //{
                        //    processState = ProcessQueueStates.FindStart;                            
                        //}
                        if (!(processPacket.packetType == PacketType.CalibrationInfo
                            || processPacket.packetType == PacketType.CalibrationRequest
                            || processPacket.packetType == PacketType.ConfigureRequest
                            || processPacket.packetType == PacketType.PressureOnly
                            || processPacket.packetType == PacketType.PressureTemperature
                            || processPacket.packetType == PacketType.TemperatureOnly
                            || processPacket.packetType == PacketType.Ack))
                        {

                            processState = ProcessQueueStates.FindStart;
                        }

                        //Debug.Print("Packet type : " + processPacket.packetType);
                        else { processState = ProcessQueueStates.GetLength; }

                        break;

                    case ProcessQueueStates.GetLength:
                        processLength = nextByte + 1;            //Adding 1 to accomodate the sequence number? Why add 1?
                        processPacket.PayloadLength = (byte)processLength;

                        processState = ProcessQueueStates.GrabbingPayload;
                        break;

                    case ProcessQueueStates.GrabbingPayload:
                        processPayload.Enqueue(nextByte);
                        processLength--;

                        if (processLength <= 1)           //Changing here
                        {
                            processState = ProcessQueueStates.NextIsEnd;
                        }
                        break;

                    case ProcessQueueStates.NextIsEnd: //Takes the lat two bytes, Converts them into integer,and checks if the value is equal to 0x96.  
                        // TODO: Check CRC

                        //processPacket.crc = nextByte; 
                        int i = 0;

                        float CRC = 0;

                        if (flag == 0 && nextByte == 0)
                        {
                            flag = 1;
                        }
                        if (nextByte == 1)
                        {

                        }

                        if (flag == 1 && nextByte == 0x96)
                        {
                            processPacket.payload = processPayload.ToArray();
                            decodedPackets.Enqueue(processPacket.Clone());
                            processState = ProcessQueueStates.FindStart;
                            processPayload.Clear();
                            calculateCRC.Clear();
                            CRCLength = 2;
                            flag = 0;
                        }


                        break;

                    default:
                        // Debug.Print( "Default state" );
                        break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // port.Write("sp");
            //port.Close();
        }

        public enum PacketType
        {
            PressureOnly = 0x40,
            PressureTemperature = 0x41,
            TemperatureOnly = 0x42,

            CalibrationRequest = 0x50,
            CalibrationInfo = 0x51,

            ConfigureRequest = 0x60,

            Ack = 0x70,
        }

        public class Packet
        {
            public PacketType packetType;
            public byte PayloadLength;
            public byte[] payload;
            public byte Crc;

            public override string ToString()
            {
                var payloadString = $"{payload[0]}";
                var hexString = $"0x{payload[0]:X2}";

                for (var i = 1; i < payload.Length; i++)
                {
                    payloadString += $", {payload[i]}";
                    hexString += $" {payload[i]:X2}";
                }

                return $"Packet: Type={packetType}, Length={PayloadLength}, Payload={payloadString} || {hexString}";
            }

            /// <summary>
            /// Returns a deep copy of the packet
            /// </summary>
            /// <returns></returns>
            public Packet Clone()
            {
                return new Packet()
                {
                    packetType = this.packetType,
                    PayloadLength = this.PayloadLength,
                    payload = (byte[])this.payload.Clone(),
                };
            }
        }

        public class calibrationValues
        {
            //  static int count = 0;

            public float rangeA;
            public float rangeB;

            public float minA;
            public float minB;

            public float offsetA0;
            public float offsetA1;
            public float offsetA2;
            public float offsetA3;

            public float offsetB0;
            public float offsetB1;
            public float offsetB2;
            public float offsetB3;

            public float spanA0;
            public float spanA1;
            public float spanA2;
            public float spanA3;

            public float spanB0;
            public float spanB1;
            public float spanB2;
            public float spanB3;

            public float shapeA0;
            public float shapeA1;
            public float shapeA2;
            public float shapeA3;

            public float shapeB0;
            public float shapeB1;
            public float shapeB2;
            public float shapeB3;


            public byte[] tempA;
            public byte[] tempB;
            public float[] returnArrayA = new float[28];


            public calibrationValues() { }

            public calibrationValues(Packet packet)
            {
                int j = 0;
                if (packet.packetType != PacketType.CalibrationInfo)
                {
                    throw new Exception(" Invalid packet type");
                }


                tempA = new byte[122];

                for (int i = 0; i < 4; i++)
                {
                    tempA[i] = packet.payload[i];
                }

                rangeA = System.BitConverter.ToSingle(tempA, 0);


                returnArrayA[j] = rangeA;
                j += 1;
                for (int i = 4; i < 8; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                minA = System.BitConverter.ToSingle(tempA, 4);

                returnArrayA[j] = minA;
                j += 1;
                //int unitA = packet.payload[8];


                for (int i = 13; i < 17; i++)
                {
                    tempA[i] = packet.payload[i];

                }
                offsetA0 = System.BitConverter.ToSingle(tempA, 13);
                returnArrayA[j] = offsetA0;
                j += 1;

                for (int i = 17; i < 21; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA1 = System.BitConverter.ToSingle(tempA, 17);
                returnArrayA[j] = offsetA1;
                j += 1;

                for (int i = 21; i < 25; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA2 = System.BitConverter.ToSingle(tempA, 21);
                returnArrayA[j] = offsetA2;
                j += 1;

                for (int i = 25; i < 29; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA3 = System.BitConverter.ToSingle(tempA, 25);
                returnArrayA[j] = offsetA3;
                j += 1;


                for (int i = 29; i < 33; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                spanA0 = System.BitConverter.ToSingle(tempA, 29);
                returnArrayA[j] = spanA0;
                j += 1;

                for (int i = 33; i < 37; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA1 = System.BitConverter.ToSingle(tempA, 33);
                returnArrayA[j] = spanA1;
                j += 1;

                for (int i = 37; i < 41; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA2 = System.BitConverter.ToSingle(tempA, 37);
                returnArrayA[j] = spanA2;
                j += 1;

                for (int i = 41; i < 45; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA3 = System.BitConverter.ToSingle(tempA, 41);
                returnArrayA[j] = spanA3;
                j += 1;


                for (int i = 45; i < 49; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA0 = System.BitConverter.ToSingle(tempA, 45);
                returnArrayA[j] = shapeA0;
                j += 1;

                for (int i = 49; i < 53; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA1 = System.BitConverter.ToSingle(tempA, 49);
                returnArrayA[j] = shapeA1;
                j += 1;

                for (int i = 53; i < 57; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA2 = System.BitConverter.ToSingle(tempA, 53);
                returnArrayA[j] = shapeA2;
                j += 1;

                for (int i = 57; i < 61; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA3 = System.BitConverter.ToSingle(tempA, 57);
                returnArrayA[j] = shapeA3;
                j += 1;


                for (int i = 61; i < 65; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                rangeB = System.BitConverter.ToSingle(tempA, 61);
                returnArrayA[j] = rangeB;
                j += 1;

                for (int i = 65; i < 69; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                minB = System.BitConverter.ToSingle(tempA, 65);
                returnArrayA[j] = minB;
                j += 1;

                //69 to 73 : Char values, not used.

                for (int i = 74; i < 78; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                offsetB0 = System.BitConverter.ToSingle(tempA, 74);
                returnArrayA[j] = offsetB0;
                j += 1;

                for (int i = 78; i < 82; i++)
                {
                    //offB1
                    tempA[i] = packet.payload[i];
                }
                offsetB1 = System.BitConverter.ToSingle(tempA, 78);
                returnArrayA[j] = offsetB1;
                j += 1;

                for (int i = 82; i < 86; i++)
                {
                    //offB2
                    tempA[i] = packet.payload[i];
                }
                offsetB2 = System.BitConverter.ToSingle(tempA, 82);
                returnArrayA[j] = offsetB2;
                j += 1;

                for (int i = 86; i < 90; i++)
                {
                    //offB3
                    tempA[i] = packet.payload[i];
                }
                offsetB3 = System.BitConverter.ToSingle(tempA, 86);
                returnArrayA[j] = offsetB3;
                j += 1;

                for (int i = 90; i < 94; i++)
                {
                    //SPANB0
                    tempA[i] = packet.payload[i];
                }
                spanB0 = System.BitConverter.ToSingle(tempA, 90);
                returnArrayA[j] = spanB0;
                j += 1;

                for (int i = 94; i < 98; i++)
                {
                    //SPANB1
                    tempA[i] = packet.payload[i];
                }
                spanB1 = System.BitConverter.ToSingle(tempA, 94);
                returnArrayA[j] = spanB1;
                j += 1;

                for (int i = 98; i < 102; i++)
                {
                    tempA[i] = packet.payload[i];
                    //SPANB2
                }
                spanB2 = System.BitConverter.ToSingle(tempA, 98);
                returnArrayA[j] = spanB2;
                j += 1;

                for (int i = 102; i < 106; i++)
                {
                    tempA[i] = packet.payload[i];
                    //SPANB3
                }
                spanB3 = System.BitConverter.ToSingle(tempA, 102);
                returnArrayA[j] = spanB3;
                j += 1;

                for (int i = 106; i < 110; i++)
                {
                    tempA[i] = packet.payload[i];
                    //shapesb0
                }
                shapeB0 = System.BitConverter.ToSingle(tempA, 106);
                returnArrayA[j] = shapeB0;
                j += 1;

                for (int i = 110; i < 114; i++)
                {
                    tempA[i] = packet.payload[i];
                    //shapesb1
                }
                shapeB1 = System.BitConverter.ToSingle(tempA, 110);
                returnArrayA[j] = shapeB1;
                j += 1;

                for (int i = 114; i < 118; i++)
                {
                    tempA[i] = packet.payload[i];
                    //shapesb2
                }
                shapeB2 = System.BitConverter.ToSingle(tempA, 114);
                returnArrayA[j] = shapeB2;
                j += 1;

                for (int i = 118; i < 122; i++)
                {
                    tempA[i] = packet.payload[i];
                    //shapesb3
                }
                shapeB3 = System.BitConverter.ToSingle(tempA, 118);
                returnArrayA[j] = shapeB3;
                j += 1;

            }

            //private void setReturnValues(float[] returnArray)
            //{
            //    float[] returnArr;
            //    returnArr = returnArray;
            //    throw new NotImplementedException();

            //}

            //public float[] ReturnValues()
            //{
            //    Debug.Print("returning values :" + returnArray);
            //    return returnArray;
            //}


        }

        public class RawValuePacket
        {
            public int SequenceNumber;

            public int TempA;
            public int TempB;

            public int[] PresA;
            public int[] PresB;
            public int buttonData;

            public RawValuePacket() { }

            public RawValuePacket(Packet packet)
            {
                if (packet.packetType != PacketType.PressureTemperature)
                {
                    throw new Exception("Invalid packet type");
                }

                this.SequenceNumber = packet.payload[0];

                int count = (packet.PayloadLength - 1 - 4 - 2) / (2 * 3);

                this.PresA = new int[count];
                this.PresB = new int[count];

                int offset;
                for (int i = 0; i < count; i++)
                {
                    offset = 1 + i * (2 * 3);
                    int presA = packet.payload[offset++];
                    presA = presA << 8 | packet.payload[offset++];
                    presA = presA << 8 | packet.payload[offset++];

                    //packet.payload[0]*2^16 | packet.payload[1] << 8 | packet.payload[2];
                    //(([0]*2^8 + [1])*2^8 + [2])

                    int presB = packet.payload[offset++];
                    presB = presB << 8 | packet.payload[offset++];
                    presB = presB << 8 | packet.payload[offset++];

                    this.PresA[i] = presA;
                    this.PresB[i] = presB;
                }

                // Debug.Print("Pres A : " + PresA + " PresB : " + PresB);            
                offset = packet.PayloadLength - 6;

                int tempA = packet.payload[offset++];
                tempA = tempA << 8 | packet.payload[offset++];
                //int tempA2 = packet.payload[offset++];


                int tempB = packet.payload[offset++];
                tempB = tempB << 8 | packet.payload[offset++];
                //int tempB2 = packet.payload[offset++];
                this.TempA = tempA;
                this.TempB = tempB;
                int buttonData = (int)packet.payload[offset];
                this.buttonData = buttonData;

                //Debug.Print("Btn : " + buttonData);

            }

            public override string ToString()
            {
                string printString = $"RawValuePacket: TempA={TempA}, TempB={TempB}";
                for (int i = 0; i < PresA.Length; i++)
                {
                    printString += $"\nA[{i}]={PresA[i]}, B[{i}]={PresB[i]}\n";
                }

                return printString;
            }
        }

        //*************************************************************************  PLAY AUDIO  ***********************************************************************
        #region Play Audio

        public double FirstXPos { get; private set; }
        public double FirstYPos { get; private set; }
        public double FirstArrowXPos { get; private set; }
        public double FirstArrowYPos { get; private set; }
        public object MovingObject { get; private set; }
        public bool IsAudioCaptured { get; set; }
        public bool IsAirFlowCaptured { get; set; }
        public bool IsPressureCaptured { get; set; }
        public bool IsResistanceCaptured { get; set; }
        public WaveFileReader Wfr { get; set; }

        //public double Cursor_1_value { get; set; }

        CursorWindow cWin = new CursorWindow();
        protected bool isDragging;
        private void playaudio_Click(object sender, RoutedEventArgs e)
        {


        }

        public void playAudio(string wavFilePath)
        {
            AudioPoints.Clear();
            int flag = 0;
            //Debug.Print("ht : "+LayoutRoot.Height);
            //playaudio.IsEnabled = false;
            double[] coefficients = new double[10];
            double[] a = new double[5];
            double[] b = new double[5];
            Queue<double> displaypoint = new Queue<double>();
            Queue<double> screens = new Queue<double>();
            double a1 = LayoutRoot.Width;

            Wfr = new WaveFileReader(wavFilePath);

            //Debug.Print("JH" + wfr.Length);

            SoundPlayer s = new SoundPlayer(wavFilePath);

            byte[] allBytes = File.ReadAllBytes(wavFilePath);
            byte[] points = new byte[4];

            for (int i = 44; i < allBytes.Length - 4; i += 100)
            {
                points[2] = allBytes[i];
                points[3] = allBytes[i + 1];
                points[1] = allBytes[i + 2];
                points[0] = allBytes[i + 3];


                displaypoint.Enqueue(BitConverter.ToInt32(points, 0));

            }


            double[] points2 = displaypoint.ToArray();
            double[] points3 = displaypoint.ToArray();

            coefficients = getCoefficients2();
            for (int i = 0; i < 5; i++)
            {
                a[i] = coefficients[i];

            }
            for (int i = 5; i < coefficients.Length; i++)
            {
                b[i - 5] = coefficients[i];

            }

            for (Int32 x = 4; x < points2.Length; x++)
            {
                //coefficients from file generated by MATLAB
                points3[x] = ((b[0] * x) + (b[1] * (x - 1)) + (b[2] * (x - 2)) + (b[3] * (x - 3)) + (b[4] * (x - 4)) + (a[1] * points2[x - 1]) + (a[2] * points2[x - 2]) + (a[3] * points2[x - 3]) + (a[4] * points2[x - 4]));

            }


            double val;

            for (Int32 x = 0; x < points3.Length; ++x)
            {
                val = points3[x];
                Point p = new Point();
                p.X = x;
                p.Y = points3[x];
                val = Normalize(x, p.Y);
                AudioPoints.Add((float)val);
                //if (n >= 834)
                //    flag = 1;
                //if (audioPoints.Count >= 834)
                //{
                //    flag = 1;

                //}
            }
            var apoints = AudioPoints.ToList();
            Console.WriteLine(string.Join(",", apoints));

            Console.WriteLine($"Total Aduio Points: {AudioPoints.Count}");

            //  Debug.Print("n:" + n);//n is the total number of audio points shown on screen
            s.Load();

            s.Play();
        }

        public double[] getCoefficients2()
        {
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\megha\Scripts\AeroWin2\MainWindowDesign\MainWindowDesign\Resources\coefficients.txt");


            string[] coefficients = new string[10];
            double[] coefficients1 = new double[10];
            foreach (string line in lines)
            {
                coefficients = line.Split(new char[] { ',' });

            }

            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients1[i] = double.Parse(coefficients[i]);
            }

            return (coefficients1);

        }

        double _lastPoint = 0;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string propertyName)
        {
            var handlers = PropertyChanged;

            handlers(this, new PropertyChangedEventArgs(propertyName));
        }


        double Normalize(Int32 x, double y)
        {
            Point p = new Point
            {
                Y = y / Math.Pow(2, 25)
            };

            _lastPoint = p.X;

            return p.Y;
        }
        #endregion

        private void DeviceChannels_Click(object sender, RoutedEventArgs e)
        {
            DeviceAndAIChannelsWindow DWin = new DeviceAndAIChannelsWindow();
            DWin.Show();
        }

        #region CursorMethods        
        private void clickToShowCursors(object sender, RoutedEventArgs e)
        {

            if (AudioPoints == null || !AudioPoints.Any())
            {
                FormsMessageBox.Show("Please select a file.");
                return;
            }

            cWin.Show();
            if (showCursor.IsChecked == true)
            {
                AudioCursor1.Visibility = Visibility.Visible;
                AudioCursor2.Visibility = Visibility.Visible;
                AirFlowCursor1.Visibility = Visibility.Visible;
                AirFlowCursor2.Visibility = Visibility.Visible;
                PressureCursor1.Visibility = Visibility.Visible;
                PressureCursor2.Visibility = Visibility.Visible;
                double temp1 = Math.Round(AudioPoints[463], 3);
                double temp2 = Math.Round(AudioPoints[600], 3);
                cWin.audioCur1.Text = Convert.ToString(temp1);
                cWin.audioCur2.Text = Convert.ToString(temp2);

            }
            else
            {
                AudioCursor1.Visibility = Visibility.Collapsed;
                AudioCursor2.Visibility = Visibility.Collapsed;
                AirFlowCursor1.Visibility = Visibility.Collapsed;
                AirFlowCursor2.Visibility = Visibility.Collapsed;
                PressureCursor1.Visibility = Visibility.Collapsed;
                PressureCursor2.Visibility = Visibility.Collapsed;
            }
        }

        private void AudioCursor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement source = (UIElement)sender;
            Mouse.Capture(source);
            IsAudioCaptured = true;
        }

        private void AudioCursor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            IsAudioCaptured = false;
        }

        private void AudioCursor1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double cursor1Position = e.GetPosition(LayoutRoot).X;
            double cursor2Position = AudioCursor2.X2;
            UpdateCursorInformation(IsAudioCaptured, cursor1Position, cursor2Position, true);
        }

        private void AudioCursor2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var cursor1Position = AudioCursor1.X2;
            var cursor2Position = e.GetPosition(LayoutRoot).X;
            UpdateCursorInformation(IsAudioCaptured, cursor1Position, cursor2Position, false);
        }
        private void AirFlowCursor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement source = (UIElement)sender;
            Mouse.Capture(source);
            IsAirFlowCaptured = true;
        }

        private void AirFlowCursor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            IsAirFlowCaptured = false;
        }

        private void AirFlowCursor1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double cursor1Position = e.GetPosition(LayoutRoot).X;
            double cursor2Position = AirFlowCursor2.X2;
            UpdateCursorInformation(IsAirFlowCaptured, cursor1Position, cursor2Position, true);
        }

        private void AirFlowCursor2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var cursor1Position = AirFlowCursor1.X2;
            var cursor2Position = e.GetPosition(LayoutRoot).X;
            UpdateCursorInformation(IsAirFlowCaptured, cursor1Position, cursor2Position, false);
        }

        private void PressureCursor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement source = (UIElement)sender;
            Mouse.Capture(source);
            IsPressureCaptured = true;
        }

        private void PressureCursor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            IsPressureCaptured = false;
        }

        private void PressureCursor1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double cursor1Position = e.GetPosition(LayoutRoot).X;
            double cursor2Position = PressureCursor2.X2;
            UpdateCursorInformation(IsPressureCaptured, cursor1Position, cursor2Position, true);
        }

        private void PressureCursor2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var cursor1Position = PressureCursor1.X2;
            var cursor2Position = e.GetPosition(LayoutRoot).X;
            UpdateCursorInformation(IsPressureCaptured, cursor1Position, cursor2Position, false);
        }
        private void ResistanceCursor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement source = (UIElement)sender;
            Mouse.Capture(source);
            IsResistanceCaptured = true;
        }

        private void ResistanceCursor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            IsResistanceCaptured = false;
        }

        private void ResistanceCursor1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double cursor1Position = e.GetPosition(LayoutRoot).X;
            double cursor2Position = ResistanceCursor2.X2;
            UpdateCursorInformation(IsResistanceCaptured, cursor1Position, cursor2Position, true);
        }

        private void ResistanceCursor2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var cursor1Position = ResistanceCursor1.X2;
            var cursor2Position = e.GetPosition(LayoutRoot).X;
            UpdateCursorInformation(IsResistanceCaptured, cursor1Position, cursor2Position, false);
        }

        private double CursorMoved(double cursorPosition, ChartValues<float> chartInput)
        {
            var totalPoints = chartInput.Count;
            double canvasWidth = LayoutRoot.ActualWidth;
            double mul_factor = totalPoints / canvasWidth;
            double expectedPositionOnChart = cursorPosition * mul_factor;
            double ceilingValue = Math.Ceiling(expectedPositionOnChart);
            double floorValue = Math.Floor(expectedPositionOnChart);
            double expectedIndexOfPoint;
            if ((expectedPositionOnChart - floorValue) < (ceilingValue - expectedPositionOnChart))
            {
                expectedIndexOfPoint = floorValue;
            }
            else
            {
                expectedIndexOfPoint = ceilingValue;
            }
            int expectedIndexOfPointInInteger = Convert.ToInt32(expectedIndexOfPoint);
            if (expectedIndexOfPointInInteger < 0)
                expectedIndexOfPointInInteger = 0;
            if (expectedIndexOfPointInInteger >= chartInput.Count)
                expectedIndexOfPointInInteger = chartInput.Count - 1;
            double pointValue = chartInput[expectedIndexOfPointInInteger];
            var cursorValue = Math.Round(pointValue, 3);
            return cursorValue;
        }

        public void UpdateCursorInformation(bool isValueCaptured, double cursor1Position, double cursor2Position, bool isCursor1Moved)
        {
            double maxCanvasWidth = LayoutRoot.ActualWidth;
            if (isValueCaptured && cursor1Position > 0 && cursor1Position < maxCanvasWidth && cursor2Position > 0 && cursor2Position < maxCanvasWidth)
            {
                if (isCursor1Moved)
                {
                    SetAllCursor1Positions(cursor1Position);
                }
                else
                {
                    SetAllCursor2Positions(cursor2Position);
                }
                UpdateValuesInCursorWindow(cursor1Position, cursor2Position);
            }
        }
        public void UpdateValuesInCursorWindow(double cursor1Position, double cursor2Position)
        {
            double audioCursor1Value = CursorMoved(cursor1Position, AudioPoints);
            double audioCursor2Value = CursorMoved(cursor2Position, AudioPoints);
            double airFlowCursor1Value = CursorMoved(cursor1Position, AirFlowLineSeriesValues);
            double airFlowCursor2Value = CursorMoved(cursor2Position, AirFlowLineSeriesValues);
            double pressureCursor1Value = CursorMoved(cursor1Position, PressureLineSeriesValues);
            double pressureCursor2Value = CursorMoved(cursor2Position, PressureLineSeriesValues);
            //double resistanceCursor1Value = CursorMoved(cursor1Position, null);
            //double resistanceCursor2Value = CursorMoved(cursor2Position, null);
            cWin.audioCur1.Text = audioCursor1Value.ToString();
            cWin.audioCur2.Text = audioCursor2Value.ToString();
            cWin.audioDiff.Text = Convert.ToString(audioCursor1Value - audioCursor2Value);
            cWin.airFlowCur1.Text = airFlowCursor1Value.ToString();
            cWin.airFlowCur2.Text = airFlowCursor2Value.ToString();
            cWin.airflowDiff.Text = Convert.ToString(airFlowCursor1Value - airFlowCursor2Value);
            cWin.pressureCur1.Text = pressureCursor1Value.ToString();
            cWin.pressureCur2.Text = pressureCursor2Value.ToString();
            cWin.pressureDiff.Text = Convert.ToString(pressureCursor1Value - pressureCursor2Value);
            //cWin.resistanceCur1.Text = resistanceCursor1Value.ToString();
            //cWin.resistanceCur2.Text = resistanceCursor2Value.ToString();
            //cWin.resistanceDiff.Text = Convert.ToString(pressureCursor1Value - pressureCursor2Value);
        }

        private void SetCursorPosition(double position, Line cursorLine)
        {
            if (cursorLine != null)
            {
                cursorLine.X1 = position;
                cursorLine.X2 = position;
            }
        }

        public void SetAllCursor1Positions(double cursor1Position)
        {
            SetCursorPosition(cursor1Position, AudioCursor1);
            SetCursorPosition(cursor1Position, AirFlowCursor1);
            SetCursorPosition(cursor1Position, PressureCursor1);
            SetCursorPosition(cursor1Position, ResistanceCursor1);
        }

        public void SetAllCursor2Positions(double cursor2Position)
        {
            SetCursorPosition(cursor2Position, AudioCursor2);
            SetCursorPosition(cursor2Position, AirFlowCursor2);
            SetCursorPosition(cursor2Position, PressureCursor2);
            SetCursorPosition(cursor2Position, ResistanceCursor2);
        }

        #endregion CursorMethods

        private void PlayFile_Click(object sender, RoutedEventArgs e)
        {
            //string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".wav");
            SoundPlayer s = new SoundPlayer(audioFileToBePlayed);
            s.Load();
            s.Play();
            audioLine.Visibility = Visibility.Visible;
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            // Thread.Sleep(15);
            Storyboard myStoryBoard = btn.TryFindResource("moveLine") as Storyboard;
            //Thread.Sleep(5000);
            myStoryBoard.Begin(btn);
        }


        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {

            //if (!IsTokenListWindowClosed)
            //{
            //   MessageBoxResult messageBoxResult =  MessageBox.Show(
            //        "To save a session, you must close the Token list window. This session is not yet saved. Do you want to save it?",
            //        "Session not saved!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            //   if (messageBoxResult == MessageBoxResult.Yes)
            //   {
            //       IsTokenListWindowClosed = true;
            //   }
            //}
        }

        private void ChannelRanges_OnClick(object sender, RoutedEventArgs e)
        {
            ChannelRangesWindow channelRanges = new ChannelRangesWindow();
            channelRanges.Show();
            //throw new NotImplementedException();
        }
    }
}
