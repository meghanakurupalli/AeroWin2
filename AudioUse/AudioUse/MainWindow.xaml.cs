using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio;
using NAudio.Wave;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using System.Diagnostics;
using System.IO;

namespace audiotrail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChartValues<Polyline> PolylineCollection;
        string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        public MainWindow()
        {
            InitializeComponent();

            StartRecording(5);
            start_button.IsEnabled = false;
        }
        WaveIn wi;
        static WaveFileWriter wfw;
        static WaveFileWriter wfw2;
        Polyline pl;
      

        double canH = 0;
        double canW = 0;
        double plH = 0;
        double plW = 0;
        int time = 0;
        double seconds = 0;


        Queue<Point> displaypts;
        Queue<float> displaysht;

        long count = 0;
        int numtodisplay = 2205; //No of samples displayed in a second

        public float[] getCoefficients()
        {
            string[] lines = System.IO.File.ReadAllLines(@"D:\GIT\aerowinrt\audio_use\coefficients.txt");            

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
        

        void StartRecording(int time)
        {
            wi = new WaveIn();
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            wi.RecordingStopped += new EventHandler<StoppedEventArgs>(wi_RecordingStopped);
            wi.WaveFormat = new WaveFormat(4000, 32, 1); //Downsampled audio from 44KHz to 4kHz 

            wfw = new WaveFileWriter(generatedWaveFilesPath + @"\record4.wav", wi.WaveFormat);
            

            canH = waveCanvas.Height;
            canW = waveCanvas.Width;

            pl = new Polyline();
            pl.Stroke = Brushes.Black;
            pl.Name = "waveform";
            pl.StrokeThickness = 1;
            pl.MaxHeight = canH - 4;
            pl.MaxWidth = canW - 4;
            // pl.m
            plH = pl.MaxHeight;
            plW = pl.MaxWidth;

            displaypts = new Queue<Point>();
            displaysht = new Queue<float>();

            wi.StartRecording();

            this.time = time;

        }

        void wi_RecordingStopped(object sender, StoppedEventArgs e)
        {
            string idk;
 
            wi.Dispose();
            wi = null;
            idk = wfw.Filename;
            Debug.Print("idk : " + idk);
            //WaveFileWriter wfw2 = new WaveFileWriter(@"record3", wi.WaveFormat);
            //wfw.CopyTo(wfw2);
            wfw.Close();
            wfw.Dispose();
            wfw = null;

            
            start_button.IsEnabled = true;
        }

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            seconds += (double)(1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0);
            
            wfw.Write(e.Buffer, 0, e.BytesRecorded);
            
            Debug.Print("Writing to file : " + e.BytesRecorded);
            //wfw.Flush();
            
            if (seconds > time)
            {
                wi.StopRecording();
                // May try flushing here
                wfw.Flush();
                Debug.Print("stop recording");
            }
            double secondsRecorded = (double)(1.0 * wfw.Length / wfw.WaveFormat.AverageBytesPerSecond * 1.0);

            byte[] shts = new byte[4];


            for (int i = 0; i < e.BytesRecorded - 1; i += 100)
            {
                shts[0] = e.Buffer[i];
                shts[1] = e.Buffer[i + 1];
                shts[2] = e.Buffer[i + 2];
                shts[3] = e.Buffer[i + 3];
                if (count < numtodisplay)
                {
                    displaysht.Enqueue(BitConverter.ToInt32(shts, 0));
                    ++count;
                }
                else
                {
                    displaysht.Dequeue();
                    displaysht.Enqueue(BitConverter.ToInt32(shts, 0));
                }
            }
            this.waveCanvas.Children.Clear();
            pl.Points.Clear();
            float[] shts2 = displaysht.ToArray();
            float[] shts3 = displaysht.ToArray();
            
            coefficients = getCoefficients();
            for(int i = 0; i < 5; i++)
            {
                a[i] = coefficients[i];
               
            }
            for (int i = 5; i < coefficients.Length; i++)
            {
                b[i-5] = coefficients[i];
               
            }
            
            for (Int32 x = 4; x < shts2.Length; x++)
            {
                //coefficients from file generated by MATLAB
                shts3[x] = ((b[0] * x) + (b[1] * (x - 1)) + (b[2] * (x - 2)) + (b[3] * (x - 3)) + (b[4] * (x - 4)) + (a[1] * shts2[x - 1]) + (a[2] * shts2[x - 2]) + (a[3] * shts2[x - 3]) + (a[4] * shts2[x - 4]));
               
            }

            


            for (Int32 x = 0; x < shts3.Length; ++x)
            {
                pl.Points.Add(Normalize(x, shts3[x]));
                
            }

            this.waveCanvas.Children.Add(pl);
        }

        Point Normalize(Int32 x, float y)
        {
            
            Point p = new Point
            {
                
                X = 1.99 * x / 1800 * plW,
                Y = plH / 2.0 - y / (Math.Pow(2, 28) * 1.0) * (plH)
            };
            //Debug.Print("Points added : " + p);
            double k = p.Y;
            double h = p.X;
            File.AppendAllText(@"D:\GIT\aerowinrt\audio_use\textfile1.txt", "(" + h.ToString("#.###") + "," + k.ToString(" #.##### ") + "),");
            return p;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = false;
            waveCanvas.Children.Clear();
            WaveFileReader wfr;
            wfr = new WaveFileReader("record3.wav");
            Debug.Print("wfr.length : "+ wfr.Length);

        }
    }
}
