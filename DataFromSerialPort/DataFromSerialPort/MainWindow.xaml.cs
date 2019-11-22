using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataFromSerialPort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort port = null;
        public MainWindow()
        {
            InitializeComponent();
          
            port = new SerialPort("COM7", 117000, Parity.None, 8, StopBits.One);
            port.Open();
            port.Write("go");
            Queue<string> packetContents = new Queue<string>();
            string[] idk = new string[20];
            while (true)
            {
                
                var value = port.ReadByte();
                string hexOutput = "";
                hexOutput += String.Format("{0:X2}", value);
                
                //Debug.Print(hexOutput + " " + t);
            }
        }
    }
}
