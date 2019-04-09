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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace trail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
          //  Line line = new Line();
          //  myCanvas.Children.Add(line);
            
          //  line.Stroke = Brushes.Red;
          //  line.StrokeThickness = 2;
          //  line.X1 = 0;
          //  line.Y1 = 0;
          //  line.X2 = 0;
          //  line.Y2 = 100;
          //  Storyboard sb = new Storyboard();
          //  DoubleAnimation da = new DoubleAnimation(theLine.X1, 100, new Duration(new TimeSpan(0, 0, 0, 1)));
          //  DoubleAnimation da1 = new DoubleAnimation(theLine.X2, 100, new Duration(new TimeSpan(0, 0, 0, 1)));
          ////  DoubleAnimation da2 = new DoubleAnimation(line.X2, 100, new Duration(new TimeSpan(0, 0, 1)));
          // // DoubleAnimation da3 = new DoubleAnimation(line.Y2, 100, new Duration(new TimeSpan(0, 0, 1)));
          //  Storyboard.SetTargetProperty(da, new PropertyPath("(theLine.X1)"));
          //  Storyboard.SetTargetProperty(da1, new PropertyPath("(theLine.X2)"));
          //  //Storyboard.SetTargetProperty(da2, new PropertyPath("(Line.Y2)"));
          //  //Storyboard.SetTargetProperty(da3, new PropertyPath("(Line.X2)"));
          //  sb.Children.Add(da);
          //  sb.Children.Add(da1);
          //  //sb.Children.Add(da2);
          //  //sb.Children.Add(da3);

          //  theLine.BeginStoryboard(sb);

            
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            
            myDoubleAnimation.From = myCanvas.Margin.Left;
            myDoubleAnimation.To = 600;
            myDoubleAnimation.Duration =
                new Duration(TimeSpan.FromSeconds(5));
           // myDoubleAnimation.AutoReverse = true;
           // myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTargetName(myDoubleAnimation, "theLine");
            Storyboard.SetTargetProperty(myDoubleAnimation,
                new PropertyPath(Canvas.LeftProperty));
            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            myStoryboard.Begin(theLine);

        }
    }

    
}
