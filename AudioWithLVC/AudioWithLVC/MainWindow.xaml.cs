﻿using LiveCharts;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using NAudio.Wave;
using System.Diagnostics;

namespace AudioWithLVC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChartValues<Polyline> PolylineCollection;
       // string generatedWaveFilesPath = System.Configuration.ConfigurationManager.AppSettings["GeneratedWaveFilesPath"];
        double time = 0;
        public MainWindow()
        {
            InitializeComponent();
            time = 5.0;
            StartRecording(time);
            btnDownloadFile.IsEnabled = false;
        }
        WaveIn wi;
        static WaveFileWriter wfw;
        Polyline pl;

        double seconds = 0;


        Queue<Point> displaypts;
        Queue<float> displaypoint;

        long count = 0;
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

        void StartRecording(double time)
        {
            wi = new WaveIn();
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            wi.RecordingStopped += new EventHandler<StoppedEventArgs>(wi_RecordingStopped);
            wi.WaveFormat = new WaveFormat(4000, 32, 1); //Downsampled audio from 44KHz to 4kHz 

            wfw = new WaveFileWriter(generatedWaveFilesPath + @"\record4.wav", wi.WaveFormat);



            //pl = new Polyline();
            /*
            pl.Stroke = Brushes.Black;
            pl.Name = "waveform";
            pl.StrokeThickness = 1;
            pl.MaxHeight = canH - 4;
            pl.MaxWidth = canW - 4;
            // pl.m
            plH = pl.MaxHeight;
            plW = pl.MaxWidth; */

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

            Debug.Print("Seconds : " + seconds);
            if (seconds - time > 0)
            {
                wi.StopRecording();
                wfw.Flush();
                Debug.Print("stop recording");
            }
            //double secondsRecorded = (double)(1.0 * wfw.Length / wfw.WaveFormat.AverageBytesPerSecond * 1.0);

            byte[] points = new byte[4];


            for (int i = 0; i < e.BytesRecorded - 1; i += 100)
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
                pl.Points.Add(Normalize(x, points3[x]));

            }

            //this.waveCanvas.Children.Add(pl);
        }

        Point Normalize(Int32 x, float y)
        {

            Point p = new Point
            {

                // X = 1.99 * x / 1800 * plW,
                X = 1.99 * x / 1670 * plW,
                Y = plH / 2.0 - y / (Math.Pow(2, 28) * 1.0) * (plH)

            };

            double k = p.Y;
            double h = p.X;
            //File.AppendAllText(@"D:\GIT\aerowinrt\audio_use\textfile1.txt", "(" + h.ToString("#.###") + "," + k.ToString(" #.##### ") + "),");
            return p;
        }
    }
}
