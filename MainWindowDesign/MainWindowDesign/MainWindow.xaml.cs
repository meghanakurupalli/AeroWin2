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
        string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        string generatedProtocolFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedProtocolFilesPath"];
        public SeriesCollection SeriesCollection { get; set; }
        SaveFileWindow sWin; 

        WaveIn wi;
        static WaveFileWriter wfw;
        
        string DataFileName;
        string ProtocolFileName;

        string ProtFileTWin;
       
        public string FileNameFromSFW;
        double seconds = 0;

        

        Queue<Point> displaypts;
        Queue<float> displaypoint;

        long count = 0;
        int numtodisplay = 2205;

        public ChartValues<float> PressureLineSeriesValues { get; set; } = new ChartValues<float>();
        public ChartValues<double> TemperatureLineSeriesValues { get; set; } = new ChartValues<double>();
        Queue<double> addpr = new Queue<double>();

        private readonly int QUEUE_THRESHOLD = 8;
        SerialPort port = null;
        Queue<byte> receivedData = new Queue<byte>();
        System.Timers.Timer myTimer = new System.Timers.Timer(1);
        caliberationValues givesPacket = new caliberationValues();
        public float[] temparr;
        public static float[] temparray2 = new float[19];
        System.Timers.Timer anotherTimer = new System.Timers.Timer();

        private CRC16.Definition definition;
        private CRC16 hashFunction;

        Queue<RawValuePacket> RawValuePackets = new Queue<RawValuePacket>();
        double time = 0;

        
        
        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = this;

            audioPoints = new ChartValues<double>();

            definition = new CRC16.Definition() { TruncatedPolynomial = 0x8005 };
            hashFunction = new CRC16(definition);
            hashFunction.Initialize();

            port = new SerialPort("COM7", 19200, Parity.None, 8, StopBits.One);

            //playaudio.IsEnabled = true;

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();


        }

        TokenListWindow TWin;

        private void SaveFileWindow_SaveClicked(object sender, EventArgs e)
        {
            DataFileName = sWin.Data_File_Name;
            ProtocolFileName = sWin.Protocol_File_Name;
            ProtFileTWin = ProtocolFileName;

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

            TWin = new TokenListWindow(ProtocolFileName);
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

        void StartRecording(double time)
        {
            wi = new WaveIn();
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            wi.RecordingStopped += new EventHandler<StoppedEventArgs>(wi_RecordingStopped);
            wi.WaveFormat = new WaveFormat(4000, 32, 1); //Downsampled audio from 44KHz to 4kHz 
            string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".wav");
            wfw = new WaveFileWriter(path, wi.WaveFormat);
            //Debug.Print("DataFileName : " + DataFileName);

            displaypts = new Queue<Point>();
            displaypoint = new Queue<float>();

            wi.StartRecording();

            this.time = time;
            audioTimer.Elapsed += AudioTimer_Elapsed;
            audioTimer.Start();

        }

        void wi_RecordingStopped(object sender, StoppedEventArgs e)
        {

            wi.Dispose();
            wi = null;
            wfw.Close();
            wfw.Dispose();
            wfw = null;
            audioPoints.Clear();
            Debug.Print("Total no of bytes Recorded : " + byteRecordCount);

            // btnDownloadFile.IsEnabled = true;
        }

        System.Timers.Timer audioTimer = new System.Timers.Timer(5000);
        static int byteRecordCount = 0;

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            seconds += (double)(1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0);

            //Debug.Print("DaFaq seconds : " + seconds);

            byteRecordCount = byteRecordCount + e.BytesRecorded;
            wfw.Write(e.Buffer,0, e.BytesRecorded);
            
            Debug.Print("e.BytesRecorded : " + e.BytesRecorded);
            //Debug.Print("Writing to file : " + e.BytesRecorded);
            //wfw.Flush();
            
           // Debug.Print("Seconds : " + seconds);
           //if(seconds == 5|| seconds==10||seconds==15||seconds==20)
           //{
           //     audioPoints.Clear();
           //     TWin2.sayThisHappens();
           //}

            if (seconds - time > 0)
            {
               //Debug.Print("inside if : " + time + ", Seconds : " + seconds);
                wi.StopRecording();
                // May try flushing here
                wfw.Flush();
                audioTimer.Stop();
                TWin2.Close();
                //Debug.Print("stop recording");
            }
            //double secondsRecorded = (double)(1.0 * wfw.Length / wfw.WaveFormat.AverageBytesPerSecond * 1.0);

            byte[] points = new byte[4];


            for (int i = 0; i < e.BytesRecorded - 4; i += 100)
            //Check how things work when I take a sample for every 20 samples. 
            //Check why the canvas is not getting filled up properly
            //Instead of assuming arbitrary values with trail and error, see what aspects actally matter.
            {
                points[0] = e.Buffer[i];
                points[1] = e.Buffer[i + 1];
                points[2] = e.Buffer[i + 2];
                points[3] = e.Buffer[i + 3];
                if (count < numtodisplay)
                {
                    displaypoint.Enqueue(BitConverter.ToInt32(points, 0));
                    ++count;
                }
                else
                {
                    displaypoint.Dequeue();
                    displaypoint.Enqueue(BitConverter.ToInt32(points, 0));
                }
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

        private void AudioTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Debug.Print("audio points before they are cleared : " + audioPoints.Count());
            if (audioPoints != null && audioPoints.Count > 0)
            {
                //audioPoints.Clear();
                 audioPoints[500] = 2000;
                
               // Debug.Print("audio points after they are cleared : " + audioPoints.Count());
            }
            
            TWin2.sayThisHappens();
            //throw new NotImplementedException();
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
            StartButton.IsEnabled = true;
            sWin.SaveButtonClicked += new EventHandler(SaveFileWindow_SaveClicked);

            //TokenListWindow TWin = new TokenListWindow();
            //TWin.ProtocolFileName = ProtocolFileName;
            //Debug.Print("Protocol file name for twin : " + ProtocolFileName);
        }

        private void OpenExistingFileButton_Click(object sender, RoutedEventArgs e)
        {
            audioPoints.Clear();
            StartButton.Content = "Play";
            StartButton.IsEnabled = true;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            string filter = "WAV file (*.wav)|*.wav| All Files (*.*)|*.*";
            //When integrated with the pressure sensor, show csv files instead of wav files.
            openFileDialog.Filter = filter;
            openFileDialog.InitialDirectory = generatedWaveFilesPath;
            if (openFileDialog.ShowDialog() == true)
            {
                string DataFileName_ext;
                DataFileName_ext = openFileDialog.ToString();
                DataFileName = System.IO.Path.GetFileNameWithoutExtension(DataFileName_ext);
                Debug.Print("Data File name : " + DataFileName);
                //ProtocolFileName.Text = _protocolFileName;

            }

            
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
        TokenListWindow TWin2;
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startButtonClicked = true;

            

            if (StartButton.Content.ToString() == "Start")
            {
                TWin.Close();
                //Debug.Print("ProtFileTWin : " + ProtFileTWin);
                //TWin2 = new TokenListWindow(ProtFileTWin, startButtonClicked);
                //TWin2.Show();

                TWin2 = new TokenListWindow(ProtFileTWin);
                TWin2.Show();
                string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".txt");
                string[] txtFileContentGivesProtocolFileName = File.ReadAllLines(path);

                //Debug.Print("txtFileContentGivesProtocolFileName count :" + txtFileContentGivesProtocolFileName[0]+"done");

                //Debug.Print("txtFileContentGivesProtocolFileName : " + txtFileContentGivesProtocolFileName);
                string pathToProtocolFileCount = System.IO.Path.Combine(generatedProtocolFilesPath, txtFileContentGivesProtocolFileName[0] + ".csv");
                string[] allLines = File.ReadAllLines(pathToProtocolFileCount);
                int noOfProtocolsInPF = allLines.Count() - 1;

                //string pathidk = generatedProtocolFilesPath + txtFileContentGivesProtocolFileName + ".csv";

                //int count = File.ReadLines().Count();
                //Debug.Print("pathidk : " + pathidk);
                Debug.Print("pathToProtocolFileCount : " + pathToProtocolFileCount);
                time = 5.0*noOfProtocolsInPF;
                //time = 5;
                StartRecording(time);

                //port.Open();
                //port.DataReceived += SerialDataReceived;
                //myTimer.Elapsed += MyTimer_Elapsed;
                //myTimer.Start();
            }
            //SaveFileWindow sfw2 = new SaveFileWindow();
            //sfw2.startButtonSFW = this.StartButton;
            else if(StartButton.Content.ToString()=="Play")
            {


                int flag = 0;
                //Debug.Print("ht : "+LayoutRoot.Height);
                //playaudio.IsEnabled = false;
                double[] coefficients = new double[10];
                double[] a = new double[5];
                double[] b = new double[5];
                Queue<double> displaypoint = new Queue<double>();
                Queue<double> screens = new Queue<double>();
                //double a1 = LayoutRoot.Width;
                //Debug.Print("Lay : " + a1);
                //var wout = new WaveOut();

                string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".wav");
                Debug.Print("Printing protocol file name in play button clicked : " + ProtocolFileName);

                wfr = new WaveFileReader(path);

                //Debug.Print("JH" + wfr.Length);

                byte[] allBytes = File.ReadAllBytes(path);

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
                    if (audioPoints.Count >= 834)
                    {
                        flag = 1;

                    }
                }

                Debug.Print("n:" + n);//n is the total number of audio points shown on screen
                Debug.Print("allbytes len : " + allBytes.Length);
                PlayFile.IsEnabled = true;
            }


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

        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // float off = caliberationValues.offsetA0;
            float[] temparr = new float[14];
            float pint1 = 0;
            float pint2 = 0;
            float pcomp_FS = 0;
            float pcomp = 0;

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

                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    foreach (var item in rawValuePacket.PresA)
                                    {
                                        pint1 = item - (float)((temparray2[5] * Math.Pow(rawValuePacket.TempA, 3)) + (temparray2[4] * Math.Pow(rawValuePacket.TempA, 2)) + (temparray2[3] * rawValuePacket.TempA) + temparray2[2]);
                                        pint2 = pint1 / (float)(temparray2[9] * Math.Pow(rawValuePacket.TempA, 3) + temparray2[8] * Math.Pow(rawValuePacket.TempA, 2) + temparray2[7] * rawValuePacket.TempA + temparray2[6]);
                                        pcomp_FS = (float)(temparray2[13] * Math.Pow(pint2, 3) + temparray2[12] * Math.Pow(pint2, 2) + temparray2[11] * pint2 + temparray2[10]);
                                        pcomp = pcomp_FS * temparray2[0] + temparray2[1];
                                        PressureLineSeriesValues.Add(pcomp);
                                        addpr.Enqueue(pcomp);
                                        Debug.Print("Pressure Val : " + pcomp);
                                    }
                                    // TemperatureLineSeriesValues.Add(rawValuePacket.TempA * 0.03125);
                                }));
                            // Debug.Print(rawValuePacket.ToString());
                        }
                        break;
                    case PacketType.TemperatureOnly:
                        break;
                    case PacketType.CalibrationInfo:

                        var caliberationvalues = new caliberationValues(packet);
                        temparr = new float[19];
                        temparr = caliberationvalues.returnArray;
                        for (int i = 0; i < 14; i++)
                        {
                            // Debug.Print("temparr[" + i + "] = " + temparr[i]);
                        }
                        temparray2 = temparr;
                        break;
                    default:
                        // Debug.Print( "Invalid packet received" );
                        break;
                }

                //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                //   new Action(() =>
                //   {
                //       foreach (var item in rawValuePacket.PresA)
                //       {
                //           PressureLineSeriesValues.Add(item);
                //       }
                //       TemperatureLineSeriesValues.Add(rawValuePacket.TempA * 0.03125);
                //   }));
            }
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

        private Queue<Packet> decodedPackets = new Queue<Packet>();

        private readonly byte START_BYTE = 0x59;

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

                        processState = ProcessQueueStates.GetLength;
                        break;

                    case ProcessQueueStates.GetLength:
                        processPacket.PayloadLength = nextByte;
                        processLength = nextByte;

                        processState = ProcessQueueStates.GrabbingPayload;
                        break;

                    case ProcessQueueStates.GrabbingPayload:
                        processPayload.Enqueue(nextByte);
                        processLength--;

                        if (processLength <= 0)
                        {
                            processState = ProcessQueueStates.NextIsEnd;
                        }
                        break;

                    case ProcessQueueStates.NextIsEnd:
                        // TODO: Check CRC

                        processPacket.crc = nextByte;
                        processPacket.payload = processPayload.ToArray();
                        decodedPackets.Enqueue(processPacket.Clone());

                        processState = ProcessQueueStates.FindStart;
                        processPayload.Clear();
                        break;

                    default:
                        // Debug.Print( "Default state" );
                        break;
                }
            }
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

        public class caliberationValues
        {
            //  static int count = 0;

            public float SequenceNumber;

            public float rangeA;

            public float minA;

            public float offsetA0;
            public float offsetA1;
            public float offsetA2;
            public float offsetA3;

            public float spanA0;
            public float spanA1;
            public float spanA2;
            public float spanA3;

            public float shapeA0;
            public float shapeA1;
            public float shapeA2;
            public float shapeA3;


            public byte[] tempA;
            public float[] returnArray = new float[19];


            public caliberationValues() { }

            public caliberationValues(Packet packet)
            {
                //returnArray = new float[14];
                int j = 0;
                if (packet.packetType != PacketType.CalibrationInfo)
                {
                    throw new Exception(" Invalid packet type");
                }
                this.SequenceNumber = packet.payload[0];

                tempA = new byte[122];

                for (int i = 0; i < 4; i++)
                {
                    tempA[i] = packet.payload[i];
                }

                rangeA = System.BitConverter.ToSingle(tempA, 0);

                returnArray[j] = rangeA;
                j += 1;
                for (int i = 4; i < 8; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                minA = System.BitConverter.ToSingle(tempA, 4);

                returnArray[j] = minA;
                j += 1;
                //int unitA = packet.payload[8];


                for (int i = 13; i < 17; i++)
                {
                    tempA[i] = packet.payload[i];

                }
                offsetA0 = System.BitConverter.ToSingle(tempA, 13);
                returnArray[j] = offsetA0;
                j += 1;

                for (int i = 17; i < 21; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA1 = System.BitConverter.ToSingle(tempA, 17);
                returnArray[j] = offsetA1;
                j += 1;

                for (int i = 21; i < 25; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA2 = System.BitConverter.ToSingle(tempA, 21);
                returnArray[j] = offsetA2;
                j += 1;

                for (int i = 25; i < 29; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                offsetA3 = System.BitConverter.ToSingle(tempA, 25);
                returnArray[j] = offsetA3;
                j += 1;


                for (int i = 29; i < 33; i++)
                {

                    tempA[i] = packet.payload[i];
                }
                spanA0 = System.BitConverter.ToSingle(tempA, 29);
                returnArray[j] = spanA0;
                j += 1;

                for (int i = 33; i < 37; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA1 = System.BitConverter.ToSingle(tempA, 33);
                returnArray[j] = spanA1;
                j += 1;

                for (int i = 37; i < 41; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA2 = System.BitConverter.ToSingle(tempA, 37);
                returnArray[j] = spanA2;
                j += 1;

                for (int i = 41; i < 45; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                spanA3 = System.BitConverter.ToSingle(tempA, 41);
                returnArray[j] = spanA3;
                j += 1;


                for (int i = 45; i < 49; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA0 = System.BitConverter.ToSingle(tempA, 45);
                returnArray[j] = shapeA0;
                j += 1;

                for (int i = 49; i < 53; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA1 = System.BitConverter.ToSingle(tempA, 49);
                returnArray[j] = shapeA1;
                j += 1;

                for (int i = 53; i < 57; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA2 = System.BitConverter.ToSingle(tempA, 53);
                returnArray[j] = shapeA2;
                j += 1;

                for (int i = 57; i < 61; i++)
                {
                    tempA[i] = packet.payload[i];
                }
                shapeA3 = System.BitConverter.ToSingle(tempA, 57);
                returnArray[j] = shapeA3;
                j += 1;

                for (int i = 8; i < 13; i++)
                {
                    tempA[i] = packet.payload[i];
                    //Debug.Print("Units : " + tempA[i]);
                    j += 1;
                    //unitA = unitA << 8 | packet.payload[j];
                }


                //count += 1;
                //   Debug.Print("count : " + returnArray[j]);
                // setReturnValues(returnArray);
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

            public RawValuePacket() { }

            public RawValuePacket(Packet packet)
            {
                if (packet.packetType != PacketType.PressureTemperature)
                {
                    throw new Exception("Invalid packet type");
                }

                this.SequenceNumber = packet.payload[0];

                int count = (packet.PayloadLength - 1 - 4) / (2 * 3);

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

                offset = packet.PayloadLength - 4;

                int tempA = packet.payload[offset++];
                tempA = tempA << 8 | packet.payload[offset++];

                int tempB = packet.payload[offset++];
                tempB = tempB << 8 | packet.payload[offset++];

                this.TempA = tempA;
                this.TempB = tempB;

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
            string path = System.IO.Path.Combine(generatedWaveFilesPath, DataFileName + ".wav");
            SoundPlayer s = new SoundPlayer(path);
            s.Load();
            s.Play();
            audioLine.Visibility = Visibility.Visible;
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            // Thread.Sleep(15);
            Storyboard myStoryBoard = btn.TryFindResource("moveLine") as Storyboard;
            //Thread.Sleep(5000);
            myStoryBoard.Begin(btn);
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
