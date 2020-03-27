using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace MainWIndowDesign
{
    /// <summary>
    /// Interaction logic for SummartStatisticsWindow.xaml
    /// </summary>
    public partial class SummaryStatisticsWindow : Window
    {
        
        public ObservableCollection<PopulateSummaryStatistics> PopulateSummaryStatisticses = new ObservableCollection<PopulateSummaryStatistics>();
        public SummaryStatisticsWindow()
        {
            InitializeComponent();
        }
    }

    public class PopulateSummaryStatistics
    {
        public string Statistic { get; set; }
        public float StatisticMean { get; set; }
        public float StatisticSD { get; set; }
    }
}
