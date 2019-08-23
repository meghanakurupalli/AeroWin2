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

        string generatedWaveFilesPath = "D:\\GIT\\AeroWin2\\GeneratedWaveFiles";
        public SeriesCollection SeriesCollection { get; set; }
        public 

        WaveIn wi;
        static WaveFileWriter wfw;
        
        string DataFileName;
       
        public string FileNameFromSFW;
        double seconds = 0;


        Queue<Point> displaypts;
        Queue<float> displaypoint;

        long count = 0;
        int numtodisplay = 2205;

        


        double time = 0;
        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = this;

            audioPoints = new ChartValues<double>();

            
            
            

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
            DataFileName = FileNameFromSFW;

            wfw = new WaveFileWriter(generatedWaveFilesPath + DataFileName + ".wav", wi.WaveFormat);
            Debug.Print("DataFileName : " + DataFileName);

            displaypts = new Queue<Point>();
            displaypoint = new Queue<float>();

            wi.StartRecording();

            this.time = time;

        }

        void wi_RecordingStopped(object sender, StoppedEventArgs e)
        {

            wi.Dispose();
            wi = null;
            wfw.Close();
            wfw.Dispose();
            wfw = null;


            // btnDownloadFile.IsEnabled = true;
        }

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            seconds += (double)(1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0);


            wfw.Write(e.Buffer, 0, e.BytesRecorded);


            //Debug.Print("Writing to file : " + e.BytesRecorded);
            //wfw.Flush();

           // Debug.Print("Seconds : " + seconds);
            if (seconds - time > 0)
            {
               //Debug.Print("inside if : " + time + ", Seconds : " + seconds);
                wi.StopRecording();
                // May try flushing here
                wfw.Flush();
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
            //this.waveCanvas.Children.Clear();
            //pl.Points.Clear();

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

            //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            //                    new Action(() =>
            //                    {
            //                        for (Int32 x = 0; x < points3.Length; ++x)
            //                        {
            //                            pl.Points.Add(Normalize(x, points3[x]));
            //                            Point p = Normalize(x, points3[x]);
            //                           // audioPoints.Add(p.Y);
            //                            Debug.Print("Y values : " + p.Y);
            //                        }
            //                    }));



            for (Int32 x = 0; x < points3.Length; ++x)
            {
                //pl.Points.Add(Normalize(x, points3[x]));
                Point p = Normalize2(x, points3[x]);
                // Debug.Print("p.Y : " + p.Y);
                audioPoints.Add(p.Y);


            }


            //this.waveCanvas.Children.Add(pl);
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Wave File|*.wav";
            saveFileDialog.Title = "Download the file";
            saveFileDialog.ShowDialog();

        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            
            SaveFileWindow sWin = new SaveFileWindow();
            sWin.Activate();
            sWin.Topmost = true;
            sWin.Show();
            DataFileName = sWin.FileName.Text.ToString();
            StartButton.IsEnabled = true;
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //SaveFileWindow sfw2 = new SaveFileWindow();
            //sfw2.startButtonSFW = this.StartButton;
            time = 5.0;
            StartRecording(time);
        }

       
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {

        }
        
    }
}
