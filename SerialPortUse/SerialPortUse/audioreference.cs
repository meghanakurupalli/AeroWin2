using System;
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

namespace audiotrail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StartRecording(50);
        }
        WaveIn wi;
        WaveFileWriter wfw;
        Polyline pl;

        double canH = 0;
        double canW = 0;
        double plH = 0;
        double plW = 0;
        int time = 0;
        double seconds = 0;


        List<byte> totalbytes;
        Queue<Point> displaypts;
        //Queue<short> displaysht;
        Queue<Int32> displaysht;

        long count = 0;
        int numtodisplay = 2205;
        //sample 1/100, display for 5 seconds

        void StartRecording(int time)
        {
            wi = new WaveIn();
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
           // wi.RecordingStopped += new EventHandler(wi_RecordingStopped);
            wi.WaveFormat = new WaveFormat(44100, 32, 2);

            wfw = new WaveFileWriter("D:\\GIT\\aerowinrt\\audiotrail\\record.wav", wi.WaveFormat);

            canH = waveCanvas.Height;
            canW = waveCanvas.Width;

            pl = new Polyline();
            pl.Stroke = Brushes.Blue;
            pl.Name = "waveform";
            pl.StrokeThickness = 1;
            pl.MaxHeight = canH - 4;
            pl.MaxWidth = canW - 4;

            plH = pl.MaxHeight;
            plW = pl.MaxWidth;

            this.time = time;

            displaypts = new Queue<Point>();
            totalbytes = new List<byte>();
            //displaysht = new Queue<short>();
            displaysht = new Queue<Int32>();

            wi.StartRecording();
        }

        void wi_RecordingStopped(object sender, EventArgs e)
        {
            wi.Dispose();
            wi = null;
            wfw.Close();
            wfw.Dispose();

            wfw = null;
        }

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            seconds += (double)(1.0 * e.BytesRecorded / wi.WaveFormat.AverageBytesPerSecond * 1.0);
            if (seconds > time)
            {
                wi.StopRecording();
            }

            wfw.Write(e.Buffer, 0, e.BytesRecorded);
            totalbytes.AddRange(e.Buffer);

            //byte[] shts = new byte[2];
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
            //short[] shts2 = displaysht.ToArray();
            Int32[] shts2 = displaysht.ToArray();
            for (Int32 x = 0; x < shts2.Length; ++x)
            {
                pl.Points.Add(Normalize(x, shts2[x]));
            }


            this.waveCanvas.Children.Add(pl);

        }

        Point Normalize(Int32 x, Int32 y)
        {
            Point p = new Point();

            p.X = 1.0 * x / numtodisplay * plW;
            //p.Y = plH/2.0 - y / (short.MaxValue*1.0) * (plH/2.0);
            p.Y = plH / 2.0 - y / (Int32.MaxValue * 1.0) * (plH / 2.0);
            return p;
        }
    }
}
