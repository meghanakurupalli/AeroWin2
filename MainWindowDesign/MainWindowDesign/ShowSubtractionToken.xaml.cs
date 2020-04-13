using System.Collections.Generic;
using System.Windows;

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for ShowSubtractionToken.xaml
    /// </summary>
    public partial class ShowSubtractionToken : Window
    {
        public List<int> Indices { get; set; } = new List<int>();
        public List<float> Airflows { get; set; } = new List<float>();
        public List<float> Pressures { get; set; } = new List<float>();
        public List<float> Resistances { get; set; } = new List<float>();
        public ShowSubtractionToken()
        {
            InitializeComponent();
        }
    }
}
