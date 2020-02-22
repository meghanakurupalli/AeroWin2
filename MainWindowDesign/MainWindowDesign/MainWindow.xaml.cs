using System;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using NAudio;
using NAudio.Wave;
using MainWindowDesign;
using System.Diagnostics;
using System.Windows.Forms;
using Nito.KitchenSink.CRC;
using System.IO.Ports;
using System.Timers;
using Microsoft.Win32;
using System.Media;
using System.IO;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using CenterSpace.NMath.Core;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChartValues<Polyline> PolylineCollection;
        

        //public GearedValues<double> audioPoints { get; set; }//= new ChartValues<double>();
        public ChartValues<double> audioPoints { get; set; }//= new ChartValues<double>();
                                                            // public SeriesCollection seriesCollection { get; set; }

        //string generatedWaveFilesPath = @"D:\GIT\AeroWin2\GeneratedWaveFiles";
        public string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        public string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public SeriesCollection SeriesCollection { get; set; }
        SaveFileWindow sWin; 

        WaveIn wi;
        static WaveFileWriter wfw;
        
        public string DataFileName;
        string ProtocolFileName;

        string ProtFileTWin;
       
        public string FileNameFromSFW;
        double seconds = 0;

        DateTime newTime = new DateTime();

        Queue<Point> displaypts;
        Queue<float> displaypoint;

        public string pathForwavFiles;
        //long count = 0;
        //int numtodisplay = 2205;

        public ChartValues<float> PressureLineSeriesValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> AirFlowLineSeriesValues { get; set; } = new ChartValues<float>();
        public ChartValues<double> TemperatureLineSeriesValues { get; set; } = new ChartValues<double>();
        Queue<double> addpr = new Queue<double>();

        private readonly int QUEUE_THRESHOLD = 8;
        SerialPort port = null;
        Queue<byte> receivedData = new Queue<byte>();
        System.Timers.Timer myTimer = new System.Timers.Timer(1);
        calibrationValues givesPacket = new calibrationValues();
        public float[] temparr;
        public static float[] temparray2 = new float[19];
        System.Timers.Timer anotherTimer = new System.Timers.Timer();

        private CRC16.Definition definition;
        private CRC16 hashFunction;

        Queue<RawValuePacket> RawValuePackets = new Queue<RawValuePacket>();
        double time = 0;
        //ObservableCollection<protocol> protocols;
        TokenHistoryWindow THWin;

        List<protocol> THWinFileList = new List<protocol>();
        List<float> pressures2 = new List<float>(); //Intermediately adds pressure values so that first value of this list can be added to livecharts once a limit is reached.
        List<float> airflows = new List<float>(); //Intermediately adds airflows values so that first value of this list can be added to livecharts once a limit is reached.
        int checkButtonFlag1, checkButtonFlag2;
        DateTime current = new DateTime();
        DateTime after5sec = new DateTime();


        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = this;

            audioPoints = new ChartValues<double>();

            definition = new CRC16.Definition() { TruncatedPolynomial = 0x8005 };
            hashFunction = new CRC16(definition);
            hashFunction.Initialize();

            //using (port = new SerialPort("COM7", 117000, Parity.None, 8, StopBits.One))
            //{
            //    port.Open();
            //    port.Write("sp");
            //};

           

            //playaudio.IsEnabled = true;

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            
            //StartButton_Click(null, null);


        }

        TokenListWindow TWin;

        private void SaveFileWindow_SaveClicked(object sender, EventArgs e)
        {
            port = new SerialPort("COM7", 117000, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpen == false)
                {
                    port.Open();
                    port.Write("sp");

                }
            }
            catch(Exception)
            {
                System.Windows.Forms.MessageBox.Show("Equipment not connected!","Please meake sure that the eqipment is conncetd and you have given the right port number.",MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            

            Thread backgroundThread = new Thread(dataCollectionThread);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();

            port.DataReceived += SerialDataReceived;
            DataFileName = sWin.Data_File_Name;
            ProtocolFileName = sWin.Protocol_File_Name;
            ProtFileTWin = ProtocolFileName;

            pathForwavFiles = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName);// pathforwavFiles is also the path for saving pressure, airflow and resistance files.
            System.IO.Directory.CreateDirectory(pathForwavFiles);

            string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
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
            TWin = new TokenListWindow(ProtFileTWin);

            TWin.Owner = this;
            TWin.Show();
            Debug.Print("Data File name from SFW : " + DataFileName + " Prot : " + ProtocolFileName);
        }

        

        public float[] getCoefficients()
        {
            string[] lines = System.IO.File.ReadAllLines(@"D:\GIT\AeroWin2\AudioUse\coefficients.txt");

            string[] coefficients = new string[10];
            float[] coefficients1 = new float[10];
            foreach (string line in lines)
            {
                coefficients = line.Split(new char[] { ',' });

            }

            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients1[i] = float.Parse(coefficients[i]);
            }

            return (coefficients1);
        }

        static int countProtocolsInPF = 0;

        List<byte> AudioData;
        string FilePath;
        void StartRecording(double time)
        {
            TWin.PreviousButton.IsEnabled = false;
            TWin.NextButton.IsEnabled = false;
            countProtocolsInPF++;
            wi = new WaveIn();
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            wi.RecordingStopped += new EventHandler<StoppedEventArgs>(wi_RecordingStopped);
            wi.WaveFormat = new WaveFormat(4000, 32, 1); //Downsampled audio from 44KHz to 4kHz 

            AudioData = new List<byte>();
            //pathForwavFiles = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName);
            //System.IO.Directory.CreateDirectory(pathForwavFiles);

            int TWinSelectedIndex = TWin.TokenListGrid.SelectedIndex;
            int TWinCurrentRepCount = TWin.givesCurrentRepCount;
            Debug.Print("na_moham : " + TWinCurrentRepCount);
            Debug.Print("File Name created : " + DataFileName + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount);
            FilePath = System.IO.Path.Combine(pathForwavFiles, DataFileName +"_"+ TWinSelectedIndex+"_"+TWinCurrentRepCount + ".wav");
            if(File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            //wfw = new WaveFileWriter(path, wi.WaveFormat);
            //Debug.Print("DataFileName : " + DataFileName);

            displaypts = new Queue<Point>();
            displaypoint = new Queue<float>();

            wi.StartRecording();

            this.time = time;
            //audioTimer.Elapsed += AudioTimer_Elapsed;
            //audioTimer.Start();
            //TWinSelectedIndex = 0;
            //TWinCurrentRepCount = 0;

        }
        StringBuilder csv = new StringBuilder();
        string header = string.Format("{0},{1},{2},{3},{4},{5}", "Token_Type", "Utterance", "Rate", "Intensity", "Repetition_Count","Selected_Index");
        static int count_for_appending_header = 0;
        
        void wi_RecordingStopped(object sender, StoppedEventArgs e)
        {
            wi.StopRecording();
            wi.Dispose();

            // stop recording

            using (var wfw2 = new WaveFileWriter(FilePath, wi.WaveFormat))
            {
                wfw2.Write(AudioData.ToArray(), 0, AudioData.Count);            
            }

            AudioData = null;

            var pathForTempFile = System.IO.Path.Combine(pathForwavFiles, DataFileName + "temp" + ".csv");
            var pathForTHFile = System.IO.Path.Combine(pathForwavFiles, DataFileName + "TH" + ".csv");
            var temp = string.Format("{0},{1},{2},{3},{4},{5}", TWin.protocols[TWin.TokenListGrid.SelectedIndex].TokenType, TWin.protocols[TWin.TokenListGrid.SelectedIndex].Utterance, TWin.protocols[TWin.TokenListGrid.SelectedIndex].Rate, TWin.protocols[TWin.TokenListGrid.SelectedIndex].Intensity, TWin.protocols[TWin.TokenListGrid.SelectedIndex].TotalRepetitionCount, TWin.TokenListGrid.SelectedIndex.ToString());
            
            //Add selected index to the temp string too so that it does not remove stuff from Token history file when same token types have different indices.
            
            //csv.AppendLine(temp);//sends multiple lines 
            //string temp = TWin.protocols[TWin.TokenListGrid.SelectedIndex].TokenType + "," + TWin.protocols[TWin.TokenListGrid.SelectedIndex].Utterance + "," + TWin.protocols[TWin.TokenListGrid.SelectedIndex].Rate + "," + TWin.protocols[TWin.TokenListGrid.SelectedIndex].Intensity + "," + TWin.protocols[TWin.TokenListGrid.SelectedIndex].TotalRepetitionCount;



            using (StreamWriter sw1 = File.AppendText(pathForTempFile))
            {
                if (count_for_appending_header < 1)
                {
                    sw1.WriteLine(header);
                    count_for_appending_header++;
                }
                sw1.WriteLine(temp);
            }

            HashSet<string> ScannedRecords = new HashSet<string>();

            var dtCSV = ConvertCSVtoDataTable(pathForTempFile);
            
            int i = 0;

            foreach (var row in dtCSV.Rows)
            {
                // Build a string that contains the combined column values
                StringBuilder sb = new StringBuilder();
                //sb.AppendFormat("[{0}={1}]", col, row[col].ToString());                
                sb.AppendFormat("{0},{1},{2},{3},{4},{5}", dtCSV.Rows[i][0], dtCSV.Rows[i][1], dtCSV.Rows[i][2], dtCSV.Rows[i][3], dtCSV.Rows[i][4], dtCSV.Rows[i][5]);
                //Debug.Print("sb to string : " + sb.ToString());

                ScannedRecords.Add(sb.ToString());
                i++;

            }

            var scannedRecordList = ScannedRecords.ToList();
            
            File.Delete(pathForTHFile);
            

            foreach (var item in scannedRecordList)
            {
                string[] tempo = item.Split(',');
                var writeline = tempo[0] + "," + tempo[1] + "," + tempo[2] + "," + tempo[3] + "," + tempo[4]+ "," + tempo[5];
                //File.AppendAllText(generatedWaveFilesPath + "tempfile2.csv", writeline);
                using (StreamWriter sw = File.AppendText(pathForTHFile))
                {
                    sw.WriteLine(writeline);
                }
            }

            StartButton.IsEnabled = true;
            TWin.PreviousButton.IsEnabled = true;
            TWin.NextButton.IsEnabled = true;
            seconds = 0;
            audioPoints.Clear();
            TWin.ChangeIndexSelection();

            //Following code is for subtraction table. 
            int changedIndex = TWin.TokenListGrid.SelectedIndex;
            //if()

           // File.Delete(pathForTempFile);

        }

        DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
            while (!sr.EndOfStream)
            {
                string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }


        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            seconds += (double)(1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0);

            for (int i = 0; i < e.BytesRecorded; i++)
            {
                AudioData.Add(e.Buffer[i]);
            }

            //wfw.Write(e.Buffer,0, e.BytesRecorded);

            if (seconds - time > 0)
            {
                //Debug.Print("inside if : " + time + ", Seconds : " + seconds);                
                wi.StopRecording();
                
                // May try flushing here
                //wfw.Flush();
               // audioTimer.Stop();
                //TWin.Close();
                //Debug.Print("stop recording");
            }
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
            

            audioPoints.Clear();
            float[] points2 = displaypoint.ToArray();
            float[] points3 = displaypoint.ToArray();

            coefficients = getCoefficients();
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

            for (Int32 x = 0; x < points3.Length; ++x)
            {
                //pl.Points.Add(Normalize(x, points3[x]));
                Point p = Normalize2(x, points3[x]);
                // Debug.Print("p.Y : " + p.Y);
                audioPoints.Add(p.Y);


            }
            

        }


        /*********** NOT USING THIS METHOD *********/
        private void AudioTimer_Elapsed(object sender, ElapsedEventArgs e) 
        {
            Debug.Print("audio points before they are cleared : " + audioPoints.Count());
            if (audioPoints != null && audioPoints.Count > 0)
            {
                //audioPoints.Clear();
                 audioPoints[500] = 2000;
                
               // Debug.Print("audio points after they are cleared : " + audioPoints.Count());
            }
            
            TWin.sayThisHappens();
            //throw new NotImplementedException();
        }
        /*********** NOT USING THIS METHOD *********/


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
            /// double h = p.X;
            //File.AppendAllText(@"D:\GIT\aerowinrt\audio_use\textfile1.txt", "(" + h.ToString("#.###") + "," + k.ToString(" #.##### ") + "),");
            return p;
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
            //{
            //    Filter = "Wave File|*.wav",
            //    Title = "Download the file"
            //};
            //saveFileDialog.ShowDialog();

        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Content = "Start";
            sWin = new SaveFileWindow();
            sWin.Activate();
            sWin.Topmost = true;
            sWin.Show();
            //StartButton.IsEnabled = true;
            sWin.SaveButtonClicked += new EventHandler(SaveFileWindow_SaveClicked);

        }

        public string openExtFile_THFName;
        //int openExtFile_PFCount;
        string audioFileToBePlayed;
        string pressureAirflowFileToBeDisplayed;

        private void OpenExistingFileButton_Click(object sender, RoutedEventArgs e)
        {


            audioPoints.Clear();
            StartButton.Content = "Play";
            StartButton.IsEnabled = true;
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


                //send datafilename to THWin.
                //ProtocolFileName.Text = _protocolFileName;
            }
            else {
                return;
            }

            string temp = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
            StreamReader streamReader = new StreamReader(File.OpenRead(temp));
            string PFName_ext = streamReader.ReadLine();
            string PFName = System.IO.Path.GetFileNameWithoutExtension(PFName_ext);
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

        public void enableStartButton()
        {
            StartButton.IsEnabled = true;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        public bool startButtonClicked = false;        
        int noOfProtocolsInPF = 0;
        private void StartButton_Click(object sender, RoutedEventArgs Exception)
        {
            startButtonClicked = true;

            

            if (StartButton.Content.ToString() == "Start")
            {

                after5sec = DateTime.UtcNow.AddSeconds(5);
                
                
                try
                {

                    string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
                    //Debug.Print("Path : " + path);
                    string[] txtFileContentGivesProtocolFileName_ext = File.ReadAllLines(path);
                    DateTime oldTime = DateTime.UtcNow;
                    newTime = oldTime.AddSeconds(5);

                    string txtFileContentGivesProtocolFileName = System.IO.Path.GetFileNameWithoutExtension(txtFileContentGivesProtocolFileName_ext[0]);

                    string pathToProtocolFileCount = System.IO.Path.Combine(generatedProtocolFilesPath, txtFileContentGivesProtocolFileName + ".csv");
                    //Debug.Print("pathToProtocolFileCount : " + pathToProtocolFileCount);
                    string[] allLines = File.ReadAllLines(pathToProtocolFileCount);
                    noOfProtocolsInPF = allLines.Count() - 1;
                    time = 5;
                    StartRecording(time);
                }
                catch(Exception e)
                {
                    System.Windows.MessageBox.Show("Exception : " + e);
                }
                

            }
            
            else if(StartButton.Content.ToString()=="Play")
            {
                audioPoints.Clear();
                string tot_rep_count = THWin.THWprotocols[THWin.TokenHistoryGrid.SelectedIndex].TotalRepetitionCount; //Gets repetition count and split it for audio file path.
                string[] splits = tot_rep_count.Split(' '); // subtracting 1 because when the repetition count is 1 of 2, the labeling is done as _0_1
                var splits0 = Int32.Parse(splits[0]);
                //var splits1 = Int32.Parse(splits[2]) - 1;
                string rep_count = "_" + THWin.TokenHistoryGrid.SelectedIndex + "_" + splits0;
                audioFileToBePlayed = System.IO.Path.Combine(pathForwavFiles, DataFileName + rep_count + ".wav");
                string prAfrep_count = "pr_af_" + THWin.TokenHistoryGrid.SelectedIndex + "_" + splits0;
                pressureAirflowFileToBeDisplayed = System.IO.Path.Combine(pathForwavFiles, DataFileName + prAfrep_count + ".csv");
                //THWin_prev_Clicked = true;

                try
                {
                    playAudio(audioFileToBePlayed);
                    displayPressureandAirflow(pressureAirflowFileToBeDisplayed);//Have to write this method
                }
                catch(Exception e)
                {
                    System.Windows.MessageBox.Show("File path incorrect","Cannot play Required file!",MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                PlayFile.IsEnabled = true;

            }


        }

        public void displayPressureandAirflow(string pressureAirflowFileToBeDisplayed)
        {
            //throw new NotImplementedException();
            PressureLineSeriesValues.Clear();
            AirFlowLineSeriesValues.Clear();
            List<float> pressure = new List<float>();
            List<float> airflow = new List<float>();
            using (var reader = new StreamReader(pressureAirflowFileToBeDisplayed))
            {
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    pressure.Add(float.Parse(values[0]));
                    airflow.Add(float.Parse(values[1]));


                }

                for(int i = 0; i < pressure.Count; i += 6)
                {
                    PressureLineSeriesValues.Add(pressure[i]);
                    AirFlowLineSeriesValues.Add(airflow[i]);
                }
            }
            calcuateLRResistance(pressure, airflow);
        }


        //better get pressures directly from the files and then calculate the resisitance, put it in a csv file and just display it along with pressure and airflow when an existing file is opened.
        public void calcuateLRResistance(List<float> pressure, List<float> airflow)
        {
            //calcuate the mean of first hundred samples and subtract them form the whole file
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
            
            //Assigning all pressures less than 0.02 to be zero.

            for(int i = 0; i < pressure.Count; i++)
            {
                if(pressure[i] < 0.02)
                {
                    pressure[i] = 0;
                }
            }

            
            //Calculate the local maximas now

            //Dictionary<int, double> pressureMaximas = new Dictionary<int, double>();

            //for (int i = 1; i < pressure.Count-1; i++)
            //{
            //    if((pressure[i-1] < pressure[i]) && (pressure[i] > pressure[i+1]) && pressure[i] > 0.2  ) //Threshold for pressure
            //    {
            //        pressureMaximas.Add(i,pressure[i]);
            //    }
            //}

            double[] arr = new double[2000];
            for(int i = 0; i < pressure.Count; i++)
            {
                arr[i] = Convert.ToDouble(pressure[i]);
            }

            var v = new DoubleVector(arr);

            PeakFinderSavitzkyGolay pfa = new PeakFinderSavitzkyGolay(v, 50, 4);
            pfa.LocatePeaks();
            List<float> list = new List<float>();
            pfa.GetAllPeaks();
            // int j = 0;

            List<int> pressureMaximaIndices = new List<int>();
            List<double> pressureMaximas = new List<double>();

            for (int p = 0; p < pfa.NumberPeaks; p++)
            {
                Extrema peak = pfa[p];
                if (peak.Y > 0.2)
                {
                    Debug.Print("Found peak at = ({0},{1})", peak.X, peak.Y);
                    pressureMaximas.Add(peak.Y);
                    pressureMaximaIndices.Add(Convert.ToInt32(peak.X));
                }

            }
            List<double> offsets = new List<double>();

            for (int i = 0; i < pressureMaximas.Count-1;i++)
            {
                var num = Convert.ToInt32((pressureMaximaIndices[i] + pressureMaximaIndices[i + 1]) / 2);
                offsets.Add(pressure[num]);
            }



            List<float> airflowsAtMaximumPressures = new List<float>();
            
            // We have : 
            //pressure peaks - gives mean and standard dev of peaks in summary
            // Airflows at pressure peaks - gives mean and standard dev of airflows at pressure peaks in summary
            // To get the airflow mid vowel, consider I'm going to consider that when pressure starts going up above 0.3 cm of H20, pressure peak is being formed.
            //So, I'm going to get the mean of points from 20 sample after the peak to 20 samples before the 0.3 cm threshold.
            //The mean of these values will give me the statistic airflow at mid vowel.


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

        string filePathForPrandAf;

        private void closingMethod() //Calls after the 5-second interval.
        {
            int TWinSelectedIndex = 0;
            int TWinCurrentRepCount = 0; 
            this.Dispatcher.Invoke(() =>
            {
                TWinSelectedIndex = TWin.TokenListGrid.SelectedIndex;
                TWinCurrentRepCount = TWin.givesCurrentRepCount;
            });
            
            // Debug.Print("na_moham : " + TWinCurrentRepCount);
            //Debug.Print("File Name created : " + DataFileName + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount);
            filePathForPrandAf = System.IO.Path.Combine(pathForwavFiles, DataFileName + "pr_af" + "_" + TWinSelectedIndex + "_" + TWinCurrentRepCount + ".csv");
            if(File.Exists(filePathForPrandAf))
            {
                File.Delete(filePathForPrandAf);
            }

            port.Write("sp");
            //var myFile = File.Create(FilePath);
            File.WriteAllText(filePathForPrandAf, csv.ToString());
            csv.Clear();
            checkButtonFlag1 = 0;
            checkButtonFlag2 = 0;  //These two values make sure that the 
        }

        private void dataCollectionThread()
        {
            // pressures2 = new List<float>();


            while (true)
            {
                float[] temparr = new float[14];
                float pint1 = 0;
                float pint2 = 0;
                float pcomp_FS = 0;
                float pcomp = 0;

                float afint1 = 0;
                float afint2 = 0;
                float afcomp_FS = 0;
                float afcomp = 0;
                float af = 0;
                float af_in_cc = 0;
                float prs = 0;



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

                                    pint1 = item - (float)((temparray2[19] * Math.Pow(rawValuePacket.TempB, 3)) + (temparray2[18] * Math.Pow(rawValuePacket.TempB, 2)) + (temparray2[17] * rawValuePacket.TempB) + temparray2[16]);
                                    pint2 = pint1 / (float)(temparray2[23] * Math.Pow(rawValuePacket.TempB, 3) + temparray2[22] * Math.Pow(rawValuePacket.TempB, 2) + temparray2[21] * rawValuePacket.TempB + temparray2[20]);
                                    pcomp_FS = (float)(temparray2[27] * Math.Pow(pint2, 3) + temparray2[26] * Math.Pow(pint2, 2) + temparray2[25] * pint2 + temparray2[24]);
                                    pcomp = pcomp_FS * temparray2[14] + temparray2[15];
                                    prs = pcomp * (float)2.53746 - (float)24.4539;
                                    pressures2.Add(prs);
                                    //Change here when you know how to get the pressure right
                                    //var idk = pressures.ToList();

                                    //Does nothing
                                }
                                foreach (var item in rawValuePacket.PresA)
                                //Channel for airflow
                                {

                                    afint1 = item - (float)((temparray2[5] * Math.Pow(rawValuePacket.TempA, 3)) + (temparray2[4] * Math.Pow(rawValuePacket.TempA, 2)) + (temparray2[3] * rawValuePacket.TempA) + temparray2[2]);
                                    afint2 = afint1 / (float)(temparray2[9] * Math.Pow(rawValuePacket.TempA, 3) + temparray2[8] * Math.Pow(rawValuePacket.TempA, 2) + temparray2[7] * rawValuePacket.TempA + temparray2[6]);
                                    afcomp_FS = (float)(temparray2[13] * Math.Pow(afint2, 3) + temparray2[12] * Math.Pow(afint2, 2) + temparray2[11] * afint2 + temparray2[10]);
                                    afcomp = afcomp_FS * temparray2[0] + temparray2[1];
                                    //af = afcomp; //- (float)0.07945;
                                    //This equation is for calibrating the airflow. Obtained by taking a graph of pressure against airflow.
                                    af_in_cc = (float)4543.25 * afcomp - (float)364.758;
                                    airflows.Add(af_in_cc);
                                    //airflows.Add(item);
                                    //no_of_times++;
                                    //airFlowLineSeriesValues.Add(afcomp);

                                }

                                if (rawValuePacket.buttonData == 7 && checkButtonFlag1 == 0)
                                {
                                    checkButtonFlag2 = 1;
                                    checkButtonFlag1 = 1;
                                    //after5sec = DateTime.UtcNow.AddSeconds(5);
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        PressureLineSeriesValues.Clear();
                                        AirFlowLineSeriesValues.Clear();
                                        StartButton.Content = "Start";
                                        StartButton_Click(null, null);
                                    });
                                    //Have to call closing method

                                }
                                if (checkButtonFlag1 == 1)
                                {
                                    var newLine = string.Format("{0},{1}", prs, af_in_cc);
                                    csv.AppendLine(newLine);
                                }
                                //Have to do csv.clear() somewhere..
                                if (pressures2.Count > 5 && airflows.Count > 5 && checkButtonFlag2 == 1)
                                {
                                    //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                    //{
                                    //    // float temp = pressures2[0];
                                    //    //Debug.Print("Invoked at : " + DateTime.Now.ToString("hh.mm.ss.ffffff"));

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        PressureLineSeriesValues.Add(pressures2[0]);
                                        AirFlowLineSeriesValues.Add(airflows[0]);
                                        pressures2.Clear();
                                        airflows.Clear();
                                    });



                                    //PressureLineSeriesValues.Add(pressures2[0]);
                                    //AirFlowLineSeriesValues.Add(airflows[0]);
                                    //pressures2.Clear();
                                    //airflows.Clear();
                                   
                                    if (DateTime.UtcNow.Subtract(after5sec).TotalMilliseconds > 0)
                                    {
                                        Debug.Print("dateTime.UtcNow : "+ DateTime.UtcNow);
                                        closingMethod();
                                    }

                                }

                            }
                            break;
                        case PacketType.TemperatureOnly:
                            break;
                        case PacketType.CalibrationInfo:

                            var calibrationvalues = new calibrationValues(packet);
                            temparr = new float[28];
                            temparr = calibrationvalues.returnArrayA;
                            temparray2 = temparr;
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
            public byte crc;

            public override string ToString()
            {
                string payloadString = $"{payload[0]}";
                string hexString = $"0x{payload[0]:X2}";

                for (int i = 1; i < payload.Length; i++)
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

        public double FirstXPos { get; private set; }
        public double FirstYPos { get; private set; }
        public double FirstArrowXPos { get; private set; }
        public double FirstArrowYPos { get; private set; }
        public object MovingObject { get; private set; }

        CursorWindow cWin = new CursorWindow();
        WaveFileReader wfr;
        protected bool isDragging;

        bool captured = false;
        double x_shape, x_canvas;
        UIElement source = null;
        double cursor_1_position, cursor_2_position, cursor_1_value, cursor_2_value;
        double xValueOnChart1, xValueOnChart2;

        int n = 0;

        private void playaudio_Click(object sender, RoutedEventArgs e)
        {


        }

        public void playAudio(string wavFilePath)
        {
            audioPoints.Clear();
            int flag = 0;
            //Debug.Print("ht : "+LayoutRoot.Height);
            //playaudio.IsEnabled = false;
            double[] coefficients = new double[10];
            double[] a = new double[5];
            double[] b = new double[5];
            Queue<double> displaypoint = new Queue<double>();
            Queue<double> screens = new Queue<double>();
            double a1 = LayoutRoot.Width;
            
            wfr = new WaveFileReader(wavFilePath);

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
                audioPoints.Add(val);
                n++;
                //if (n >= 834)
                //    flag = 1;
                //if (audioPoints.Count >= 834)
                //{
                //    flag = 1;

                //}
            }

          //  Debug.Print("n:" + n);//n is the total number of audio points shown on screen
            s.Load();

            s.Play();
        }

        public double[] getCoefficients2()
        {
            string[] lines = System.IO.File.ReadAllLines(@"D:\GIT\AeroWin2\AudioUse\coefficients.txt");


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

        double lastPoint = 0;
        double Normalize(Int32 x, double y)
        {
            Point p = new Point
            {
                Y = y / Math.Pow(2, 25)
            };

            lastPoint = p.X;

            return p.Y;
        }

        public static Point ElementPointToScreenPoint(UIElement element, Point pointOnElement)
        {
            return element.PointToScreen(pointOnElement);
        }

        private void clickToShowCursors(object sender, RoutedEventArgs e)
        {

            cWin.Show();
            if (showCursor.IsChecked == true)
            {
                Cursor1.Visibility = Visibility.Visible;
                Cursor2.Visibility = Visibility.Visible;
                double temp1 = Math.Round(audioPoints[463], 3);
                double temp2 = Math.Round(audioPoints[600], 3);
                cWin.audioCur1.Text = Convert.ToString(temp1);
                cWin.audioCur2.Text = Convert.ToString(temp2);

            }
            else
            {
                Cursor1.Visibility = Visibility.Collapsed;
                Cursor2.Visibility = Visibility.Collapsed;
            }


        }

        private void Cursor1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(LayoutRoot).X;
        }

        private void Cursor2_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(LayoutRoot).X;
        }

        private void Cursor1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }

        private void Cursor2_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }

        private void Cursor1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (captured)
            {
                cursor_1_position = e.GetPosition(LayoutRoot).X;
                x_shape += cursor_1_position - x_canvas;
                Canvas.SetLeft(source, x_shape);
                x_canvas = cursor_1_position;

                cursor1moved();
            }
        }

        private void Cursor2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (captured)
            {
                cursor_2_position = e.GetPosition(LayoutRoot).X;
                x_shape += cursor_2_position - x_canvas;
                Canvas.SetLeft(source, x_shape);
                x_canvas = cursor_2_position;
                //Canvas.SetTop(source, y_shape);
                //Debug.Print("x2: " + cursor_2_position);
                cursor2moved();

            }
        }

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

        private void DeviceChannels_Click(object sender, RoutedEventArgs e)
        {
            DeviceAndAIChannelsWindow DWin = new DeviceAndAIChannelsWindow();
            DWin.Show();
        }

        private void DeviceChannels_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void cursor1moved()
        {
            double canW = LayoutRoot.Width;
            double mul_factor = n / canW; // Gives 1.04 
            Debug.Print("Mul factor : " + mul_factor);
            double[] arr = new double[834];

            //double difference = cursor_1_position - cursor_2_position;
            //Debug.Print("ucr1pos : " + cursor_1_position + "cur2pos : " + cursor_2_position);
            //Debug.Print("in new function : " + difference);
            //Debug.Print("Last position : " + lastPoint);
            double posOnLiveChart1 = cursor_1_position * mul_factor;
            double ceil1 = Math.Ceiling(posOnLiveChart1);
            double floor1 = Math.Floor(posOnLiveChart1);

            if ((posOnLiveChart1 - floor1) < (ceil1 - posOnLiveChart1))
            {
                xValueOnChart1 = floor1;
            }
            else
            {
                xValueOnChart1 = ceil1;
            }
            // Debug.Print("posOnLiveChart : " + posOnLiveChart + " Ceiling : " + ceil + " flooring : " + floor);
            // Debug.Print("for value of canvas = " + cursor_1_position + " value on chart is : " + xValueOnChart + " Corresponding value is : "+audioPoints[Convert.ToInt32(xValueOnChart)]);
            int temp1 = Convert.ToInt32(xValueOnChart1);
            if (temp1 < 0)
                temp1 = 0;
            if (temp1 >= audioPoints.Count)
                temp1 = audioPoints.Count - 1;
            Debug.Print("temp1 : " + temp1);
            double temp2 = audioPoints[temp1];
            cursor_1_value = Math.Round(temp2, 3);
            cWin.audioCur1.Text = Convert.ToString(cursor_1_value);
            updateDifference();


        }

        private void cursor2moved()
        {
            double canW = LayoutRoot.Width;
            double mul_factor = n / canW; // Gives 1.04 
            Debug.Print("Mul factor : " + mul_factor);
            //double[] arr = new double[834];

            double posOnLiveChart2 = cursor_2_position * mul_factor;
            double ceil2 = Math.Ceiling(posOnLiveChart2);
            double floor2 = Math.Floor(posOnLiveChart2);

            if ((posOnLiveChart2 - floor2) < (ceil2 - posOnLiveChart2))
            {
                xValueOnChart2 = floor2;
            }
            else
            {
                xValueOnChart2 = ceil2;
            }
            // Debug.Print("posOnLiveChart : " + posOnLiveChart + " Ceiling : " + ceil + " flooring : " + floor);
            // Debug.Print("for value of canvas = " + cursor_1_position + " value on chart is : " + xValueOnChart + " Corresponding value is : "+audioPoints[Convert.ToInt32(xValueOnChart)]);
            int temp = Convert.ToInt32(xValueOnChart2);
            if (temp < 0)
                temp = 0;
            if (temp >= audioPoints.Count)
                temp = audioPoints.Count;

            double temp1 = audioPoints[temp];
            cursor_2_value = Math.Round(temp1, 3);
            cWin.audioCur2.Text = Convert.ToString(cursor_2_value);
            updateDifference();
        }

        private void Cursor1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor1.Cursor = System.Windows.Input.Cursors.Cross;

        }
        private void Cursor2_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor2.Cursor = System.Windows.Input.Cursors.Cross;

        }

        private void updateDifference()
        {
            cWin.audioDiff.Text = Convert.ToString(cursor_1_value - cursor_2_value);
        }
    }
}
