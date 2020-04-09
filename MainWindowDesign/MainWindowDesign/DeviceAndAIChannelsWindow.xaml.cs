using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace MainWindowDesign
{
    /// <summary>
    /// Interaction logic for DeviceAndAIChannelsWindow.xaml
    /// </summary>
    public partial class DeviceAndAIChannelsWindow : Window
    {
        List<string> ports;
        public string COM_Port;
        public string defaultPort { get; set; }
        public DeviceAndAIChannelsWindow()
        {
            ports = new List<string>();
            InitializeComponent();
            foreach (string s in SerialPort.GetPortNames())
            {
                ports.Add(s);
            }

            if (ports.Count > 0)
            {
                defaultPort = ports[0];
                listOfPorts.ItemsSource = ports;
            }

        }

        private void COMPorts_OK_Button_Click(object sender, RoutedEventArgs e)
        {
            COM_Port = listOfPorts.SelectedItem.ToString();
            Close();
        }
    }
}
