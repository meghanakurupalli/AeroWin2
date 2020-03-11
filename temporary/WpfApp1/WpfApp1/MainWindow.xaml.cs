using System;
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
using LiveCharts.Wpf;
using CenterSpace.NMath.Core;
using System.IO;
using System.Diagnostics;
using LiveCharts.Defaults;

namespace liveChartsExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public SeriesCollection Seriescollection { get; set; }
        public LineSeries LineSeries { get; set; }
        public VisualElementsCollection Visuals { get; set; }
        Vector vector = new Vector();
        double[] arr;

        public MainWindow()
        {
            InitializeComponent();
            //    ChartValues<double> charts = new ChartValues<double> { 30, 30, 69, 0089, -57 };
            //    Seriescollection = new SeriesCollection
            //    {
            //        new LineSeries
            //        {
            //            Title = "Chart 1",
            //            Values = new ChartValues<double> {300, 400, 500, 60, 40, -93,10,38,88,274,483,38,4 }

            //        },
            //        new LineSeries
            //        {
            //            Title = "Chart 2",
            //            Values = charts
            //        }

            //    };
            //    arr = new double[2000];
            //    int i = 0;
            //    using (var rd = new StreamReader(@"D:\GIT\AeroWin2\GeneratedWaveFiles\idkol\idkolpr_af_0_1.csv"))
            //    {
            //        while (!rd.EndOfStream)
            //        {

            //            var splits = rd.ReadLine().Split(',');
            //            arr[i] = double.Parse(splits[0]);
            //            i++;
            //            // column1.Add(splits[0]);
            //            //column2.Add(splits[1]);
            //        }
            //    }

            //    Dictionary<int, double> dict = new Dictionary<int, double>();
            //    for(int j = 1; j < arr.Length; j++)
            //    {
            //        if(arr[j-1]<arr[j] && arr[j]>arr[j+1] && arr[j]>0.2)
            //        {
            //            dict.Add(j, arr[j]);
            //        }
            //    }

            //    int h = 0;
            //    var v = new DoubleVector(arr);

            //    PeakFinderSavitzkyGolay pfa = new PeakFinderSavitzkyGolay(v, 50, 4);
            //    pfa.LocatePeaks();
            //    List<float> list = new List<float>();
            //    pfa.GetAllPeaks();
            //   // int j = 0;

            //    for (int p = 0; p < pfa.NumberPeaks; p++)
            //    {
            //        Extrema peak = pfa[p];
            //        if(peak.Y > 0.2)
            //        {
            //            Debug.Print("Found peak at = ({0},{1})", peak.X, peak.Y);
            //        }

            //    }

            //    Line line1 = new Line();
            //    line1.X1 = 10;
            //    line1.X2 = 10;
            //    line1.Y1 = 10;
            //    line1.Y2 = 20;



            //    //Visuals = new VisualElementsCollection
            //    //{
            //    //    new VisualElement
            //    //    {
            //    //        X=idkchart.Margin.Left+10,
            //    //        Y = idkchart.Margin.Top+10,
            //    //        UIElement = line1
            //    //    },
            //    //    new VisualElement
            //    //    {
            //    //        X=idkchart.Margin.Left+5,
            //    //        Y = idkchart.Margin.Top+5,
            //    //        UIElement = line1
            //    //    }
            //    //};

            //    idkchart.VisualElements.Add(new VisualElement // This part doesn't show up
            //    {
            //        X = 0.5,
            //        Y = 7,
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        VerticalAlignment = VerticalAlignment.Top,
            //        UIElement = line1
            //    });

            //    idkchart.VisualElements.Add(new VisualElement //This part works
            //    {
            //        X = 1,
            //        Y = 0,
            //        HorizontalAlignment = HorizontalAlignment.Center,
            //        VerticalAlignment = VerticalAlignment.Top,
            //        UIElement = new TextBlock //notice this property must be a wpf control
            //        {
            //            Text = "Warning!",
            //            FontWeight = FontWeights.Bold,
            //            FontSize = 16,
            //            Opacity = 0.6
            //        }
            //    });

            //    Label lab = new Label();
            //    lab.Content = "label";
            //    lab.FontWeight = FontWeights.Bold;
            //    lab.FontSize = 16;

            //    idkchart.VisualElements.Add(new VisualElement //This part works
            //    {
            //        X = 10,
            //        Y = -200,
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        VerticalAlignment = VerticalAlignment.Top,
            //        UIElement = new Label //notice this property must be a wpf control
            //        {
            //            Content = "label",
            //            FontWeight = FontWeights.Bold,
            //            FontSize = 16,
            //            Opacity = 0.6,
            //            Height = 160,
            //            Width = 2,
            //            Background = new SolidColorBrush(Colors.Red),

            //        }
            //    });



            //    LineSeries = new LineSeries
            //    {
            //        Title = "Chart 1",
            //        Values = new ChartValues<double> { 3, 4, 5, 6, 400, -937 }

            //    };

            //    DataContext = this;

            SeriesCollection = new SeriesCollection
            {
                new ScatterSeries
                {
                    Values = new ChartValues<ScatterPoint>
                    {
                        new ScatterPoint(5, 5, 20),
                        new ScatterPoint(3, 4, 80),
                        new ScatterPoint(7, 2, 20),
                        new ScatterPoint(2, 6, 60),
                        new ScatterPoint(8, 2, 70)
                    },
                    MinPointShapeDiameter = 15,
                    MaxPointShapeDiameter = 45
                },
                new ScatterSeries
                {
                    Values = new ChartValues<ScatterPoint>
                    {
                        new ScatterPoint(7, 5, 1),
                        new ScatterPoint(2, 2, 1),
                        new ScatterPoint(1, 1, 1),
                        new ScatterPoint(6, 3, 1),
                        new ScatterPoint(8, 8, 1)
                    },
                    PointGeometry = DefaultGeometries.Triangle,
                    MinPointShapeDiameter = 15,
                    MaxPointShapeDiameter = 45
                }
            };

            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }

        private void UpdateAllOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            foreach (var series in SeriesCollection)
            {
                foreach (var bubble in series.Values.Cast<ScatterPoint>())
                {
                    bubble.X = r.NextDouble() * 10;
                    bubble.Y = r.NextDouble() * 10;
                    bubble.Weight = r.NextDouble() * 10;
                }
            }
        }
    }


}


//For the original program, force the X axis values to be between 0 and 5 and print out what values of X are being added in the chart. 
//Use this link : https://lvcharts.net/App/examples/v1/wf/Axes 
//Forget about using lables or such kind of stuff for livecharts line. Not going to work. the label shifts wrt the chart.
