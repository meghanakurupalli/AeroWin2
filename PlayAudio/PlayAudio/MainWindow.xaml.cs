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
using System.Windows.Input;
using PlayAudio;
using Microsoft.Win32;

namespace play_audio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // public ChartValues<Polyline> PolylineCollection;
        string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        public ChartValues<float> audioPoints { get; set; } = new ChartValues<float>();

        public double FirstXPos { get; private set; }
        public double FirstYPos { get; private set; }
        public double FirstArrowXPos { get; private set; }
        public double FirstArrowYPos { get; private set; }
        public object MovingObject { get; private set; }

        public MainWindow()
        {
            InitializeComponent();  
            //cWin.Owner = this;
            playaudio.IsEnabled = true;
            DataContext = this;
            SaveFileDialog dialog = new SaveFileDialog();
            

        }

        cursorWindow cWin = new cursorWindow();
        WaveFileReader wfr;
        protected bool isDragging;
        

        bool captured = false;
        double x_shape, x_canvas;
        UIElement source = null;
        double cursor_1_position, cursor_2_position, cursor_1_value, cursor_2_value;
        double xValueOnChart1, xValueOnChart2;
        byte[] partOfallBytes = new byte[20050];
        
        int n = 0;
        

        

        public double[] getCoefficients()
        {
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\megha\Scripts\AeroWin2\PlayAudio\coefficients.txt");


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

        


        private void playaudio_Click(object sender, RoutedEventArgs e)
        {

            int flag = 0;
            //Debug.Print("ht : "+LayoutRoot.Height);
            playaudio.IsEnabled = false;
            double[] coefficients = new double[10];
            double[] a = new double[5];
            double[] b = new double[5];
            Queue<double> displaypoint = new Queue<double>();
            Queue<double> screens = new Queue<double>();
            double a1 = LayoutRoot.Width;
            //Debug.Print("Lay : " + a1);
            //var wout = new WaveOut();

            wfr = new WaveFileReader(@"C:\Users\megha\Scripts\AeroWin2\GeneratedFiles\GeneratedWaveFiles\dccd\dccd_0_1.wav");

            //Debug.Print("JH" + wfr.Length);

            SoundPlayer s = new SoundPlayer(@"C:\Users\megha\Scripts\AeroWin2\GeneratedFiles\GeneratedWaveFiles\dccd\dccd_0_1.wav");

            byte[] allBytes = File.ReadAllBytes(@"C:\Users\megha\Scripts\AeroWin2\GeneratedFiles\GeneratedWaveFiles\dccd\dccd_0_1.wav");

           // Array.Copy(allBytes, 44, partOfallBytes, 44, 20000);
            //Array.Copy(allBytes, 0, partOfallBytes, 0, 44);

            
            //WaveOutEvent wout = new WaveOutEvent();
            //In the main window design, for playing the audio from the file, use indexes and play chunks of audio whenever required. for display, use the above method to copy header and bytes required. 
            //Check if the length of audio being recorded is always same, or varies some times.

            byte[] points = new byte[4];

            //i=44
            //for (int i = 44; i < allBytes.Length - 4; i += 100)
            //{
            //    points[2] = allBytes[i];
            //    points[3] = allBytes[i + 1];
            //    points[1] = allBytes[i + 2];
            //    points[0] = allBytes[i + 3];

            //    //points[2] = allBytes[i];
            //    //points[3] = allBytes[i + 1];
            //    //points[1] = allBytes[i + 2];
            //    //points[0] = allBytes[i + 3];

            //    displaypoint.Enqueue(BitConverter.ToInt32(points, 0));

            //}

            for (int i = 44; i < allBytes.Length - 4; i += 100)
            {
                points[2] = allBytes[i];
                points[3] = allBytes[i + 1];
                points[1] = allBytes[i + 2];
                points[0] = allBytes[i + 3];

                //points[2] = allBytes[i];
                //points[3] = allBytes[i + 1];
                //points[1] = allBytes[i + 2];
                //points[0] = allBytes[i + 3];

                displaypoint.Enqueue(BitConverter.ToInt32(points, 0));

            }


            double[] points2 = displaypoint.ToArray();
            double[] points3 = displaypoint.ToArray();

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
                val = points3[x];
                Point p = new Point();
                p.X = x;
                p.Y = points3[x];
                val = Normalize(x, p.Y);
                audioPoints.Add((float)val);
                n++;
                //if (n >= 834)
                //    flag = 1;
                //if (audioPoints.Count >= 834)
                //{
                //    flag = 1;
                    
                //}
            }
            
            Debug.Print("n:" + n);//n is the total number of audio points shown on screen
            //s.Load();
            
            //s.Play();
            if(flag==1)
            {
               
                audioLine.Visibility = Visibility.Visible;
                Button btn = sender as Button;
               // Thread.Sleep(15);
                Storyboard myStoryBoard = btn.TryFindResource("moveLine") as Storyboard;
                //Thread.Sleep(5000);
                myStoryBoard.Begin(btn);
            }
            
            

            
            
            Debug.Print("allbytes len : " + allBytes.Length);

            
        }



        

        double lastPoint = 0;
        double Normalize(Int32 x,double y)
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
            if (showCursor.IsChecked==true)
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

        private void cursor1moved()
        {
            double canW = LayoutRoot.Width;
            double mul_factor = n/canW; // Gives 1.04 
            Debug.Print("Mul factor : " + mul_factor);
            double[] arr = new double[834]; 

            //double difference = cursor_1_position - cursor_2_position;
            //Debug.Print("ucr1pos : " + cursor_1_position + "cur2pos : " + cursor_2_position);
            //Debug.Print("in new function : " + difference);
            //Debug.Print("Last position : " + lastPoint);
            double posOnLiveChart1 = cursor_1_position * mul_factor;
            double ceil1 = Math.Ceiling(posOnLiveChart1);
            double floor1 = Math.Floor(posOnLiveChart1);
            
            if((posOnLiveChart1-floor1)<(ceil1-posOnLiveChart1))
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
                temp1 = audioPoints.Count-1;
            Debug.Print("temp1 : " + temp1);
            double temp2 = audioPoints[temp1];
            cursor_1_value = Math.Round(temp2, 3);
            cWin.audioCur1.Text =Convert.ToString(cursor_1_value);
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



        private void Cursor1_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor1.Cursor = Cursors.Cross;
            
        }
        private void Cursor2_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor2.Cursor = Cursors.Cross;

        }      

        private void updateDifference()
        {
            cWin.audioDiff.Text = Convert.ToString(cursor_1_value - cursor_2_value);
        }
    }
        
}


