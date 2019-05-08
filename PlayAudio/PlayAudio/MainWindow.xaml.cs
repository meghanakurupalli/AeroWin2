using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using NAudio.Wave;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace play_audio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // public ChartValues<Polyline> PolylineCollection;
        string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        public ChartValues<float> audioPoints { get; set; }
        public VisualElementsCollection Visuals { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            audioPoints = new ChartValues<float>();
            Line line2 = new Line();
            line2.X1 = 20;
            line2.X2 = 20;
            line2.Y1 = 0;
            line2.Y2 = 160;
            line2.Visibility = Visibility.Visible;


            // StartRecording(5);
            playaudio.IsEnabled = true;
            Visuals = new VisualElementsCollection
            {
                new VisualElement
                {X = 25,
                Y = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                //UIElement = new Line { Name = "line1", X1 = 20, X2 = 20, Y1 = 0, Y2 = 160, Visibility = Visibility.Visible }
                UIElement = line2},

                new VisualElement
                {
                    X =50,Y=100, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, UIElement = line2
                }
            };


            DataContext = this;

        }

        
        WaveFileReader wfr;
        WaveOut wo;

        Polyline pl;
        Line line;
        int n = 0;


        double canH = 0;
        double canW = 0;
        double plH = 0;
        double plW = 0;
        int time = 0;
        double seconds = 0;
        int numtodisplay = 2205; //No of samples displayed in a second

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


        

        private void playaudio_Click(object sender, RoutedEventArgs e)
        {
            playaudio.IsEnabled = false;
            float[] coefficients = new float[10];
            float[] a = new float[5];
            float[] b = new float[5];
            Queue<float> displaypoint = new Queue<float>();
            Queue<float> screens = new Queue<float>();
            

            canH = waveCanvas.Height;
            canW = waveCanvas.Width;

            pl = new Polyline();
            pl.Stroke = Brushes.Black;
            pl.Name = "waveform";
            pl.StrokeThickness = 1;
            pl.MaxHeight = canH - 4;
            pl.MaxWidth = canW - 4;
            plH = pl.MaxHeight;
            plW = pl.MaxWidth;

            
            anotherLine.X1 = 0;
            anotherLine.X2 = 00;
            anotherLine.Y1 = 0;
            anotherLine.Y2 = canH;
            anotherLine.Visibility = Visibility.Visible;

            //CartChart.VisualElements.Add(new VisualElement
            //{
            //    Name = "Visuals",
            //    X = 5,
            //    Y = 150,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    UIElement = new Line { Name = "line1", X1 = 20, X2 = 20, Y1 = 0, Y2 = 160, Visibility = Visibility.Visible }
            //});

            

            //var wout = new WaveOut();

            wfr = new WaveFileReader(generatedWaveFilesPath + @"\record4.wav");
            
            Debug.Print("JH" +wfr.Length);

            DispatcherTimer timer = new DispatcherTimer();
            // timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Interval = TimeSpan.FromMilliseconds(0.4);
            timer.Tick += Timer_Tick;

            SoundPlayer s = new SoundPlayer(generatedWaveFilesPath + @"\record4.wav");

            //MediaPlayer m = new MediaPlayer();
            //m.Open(new Uri("D:\\GIT\\AeroWin2\\GeneratedWaveFiles\\record4.wav"));
            

            //Debug.Print("wfr format : " + wfr.WaveFormat.SampleRate);
            
           

            byte[] allBytes = File.ReadAllBytes(generatedWaveFilesPath + @"\record4.wav");
            //Debug.Print("allBytes length : " + allBytes.Length);


           // wfr.Read(allBytes, 0, allBytes.Length);
            byte[] points = new byte[4];


            for (int i = 44; i < allBytes.Length - 4; i += 100)
            {
                points[2] = allBytes[i];
                points[3] = allBytes[i + 1];
                points[1] = allBytes[i + 2];
                points[0] = allBytes[i + 3];

                displaypoint.Enqueue(BitConverter.ToInt32(points, 0));

            }


            
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


            double val;

            for (Int32 x = 0; x < points3.Length; ++x)
            {
                
                pl.Points.Add(Normalize(x, points3[x]));
                val = points3[x];
                screens.Enqueue(points3[x]);
                audioPoints.Add(points3[x]);
                n++;
            }
            //audioPoints.Add(points3[x]);
            audioPoints.AddRange(screens);
            this.waveCanvas.Children.Add(pl);

            s.Load();
            
            timer.Start();
            s.Play();
            //s.p
            //m.Play();
           
            //Debug.Print("lastPoint : " + lastPoint);


        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            if (anotherLine.X1 < lastPoint )//&& anotherLine.X1 < canW)
            {
                //anotherLine.X1 = anotherLine.X1 + lastPoint/19700;
                anotherLine.X1 = anotherLine.X1 + 0.692;

                //Sync not working properly


                anotherLine.X2 = anotherLine.X1;
                //anotherLine.Visibility = Visibility.Visible;
            }

            //if (line1 < lastPoint)//&& anotherLine.X1 < canW)
            //{
            //    //anotherLine.X1 = anotherLine.X1 + lastPoint/19700;
            //    anotherLine.X1 = anotherLine.X1 + 0.692;

            //    //Sync not working properly


            //    anotherLine.X2 = anotherLine.X1;
            //    //anotherLine.Visibility = Visibility.Visible;
            //}
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // throw new NotImplementedException();
            Debug.Print("On playback stopped event handler");
        }

        double lastPoint = 0;
        Point Normalize(Int32 x, float y)
        {
            Point p = new Point
            {

                X = 1.99 * x / 1670 * plW,
                //Y = plH / 2.0 - y / (Math.Pow(2, 28) * 1.0) * (plH)
                Y = y / Math.Pow(2, 25)
            };

            lastPoint = p.X;

            return p;
        }

        public static Point ElementPointToScreenPoint(UIElement element, Point pointOnElement)
        {
            return element.PointToScreen(pointOnElement);
        }



        //private void playaudio_Click(object sender, RoutedEventArgs e)
        //{
        //    playaudio.IsEnabled = false;
        //    waveCanvas.Children.Clear();


        //}
    }
}
