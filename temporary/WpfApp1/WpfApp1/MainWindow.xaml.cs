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

        public MainWindow()
        {
            InitializeComponent();
            ChartValues<double> charts = new ChartValues<double> { 30, 30, 69, 0089, -57 };
            Seriescollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Chart 1",
                    Values = new ChartValues<double> {300, 400, 500, 60, 40, -93,10,38,88,274,483,38,4 }

                },
                new LineSeries
                {
                    Title = "Chart 2",
                    Values = charts
                }

            };


            Line line1 = new Line();
            line1.X1 = 10;
            line1.X2 = 10;
            line1.Y1 = 10;
            line1.Y2 = 20;



            //Visuals = new VisualElementsCollection
            //{
            //    new VisualElement
            //    {
            //        X=idkchart.Margin.Left+10,
            //        Y = idkchart.Margin.Top+10,
            //        UIElement = line1
            //    },
            //    new VisualElement
            //    {
            //        X=idkchart.Margin.Left+5,
            //        Y = idkchart.Margin.Top+5,
            //        UIElement = line1
            //    }
            //};

            idkchart.VisualElements.Add(new VisualElement // This part doesn't show up
            {
                X = 0.5,
                Y = 7,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UIElement = line1
            });

            idkchart.VisualElements.Add(new VisualElement //This part works
            {
                X = 1,
                Y = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                UIElement = new TextBlock //notice this property must be a wpf control
                {
                    Text = "Warning!",
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Opacity = 0.6
                }
            });

            Label lab = new Label();
            lab.Content = "label";
            lab.FontWeight = FontWeights.Bold;
            lab.FontSize = 16;

            idkchart.VisualElements.Add(new VisualElement //This part works
            {
                X = 10,
                Y = -200,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UIElement = new Label //notice this property must be a wpf control
                {
                    Content = "label",
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Opacity = 0.6,
                    Height = 160,
                    Width = 2,
                    Background = new SolidColorBrush(Colors.Red),

                }
            });

            
                    
            LineSeries = new LineSeries
            {
                Title = "Chart 1",
                Values = new ChartValues<double> { 3, 4, 5, 6, 400, -937 }

            };

            DataContext = this;
        }


    }

}
//For the original program, force the X axis values to be between 0 and 5 and print out what values of X are being added in the chart. 
//Use this link : https://lvcharts.net/App/examples/v1/wf/Axes 
//Forget about using lables or such kind of stuff for livecharts line. Not going to work. the label shifts wrt the chart.
