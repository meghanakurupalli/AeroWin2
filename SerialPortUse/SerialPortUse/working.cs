using LiveCharts;
using Nito.KitchenSink.CRC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Timers;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public ChartValues<int> PressureLineSeriesValues { get; set; } = new ChartValues<int>();
        public ChartValues<double> TemperatureLineSeriesValues { get; set; } = new ChartValues<double>();

        private readonly int QUEUE_THRESHOLD = 8;
        SerialPort port = null;
        Queue<byte> receivedData = new Queue<byte>();
        Timer myTimer = new Timer( 500 );
        

        public MainWindow()
        {
            InitializeComponent();

            definition = new CRC16.Definition() { TruncatedPolynomial = 0x8005 };
            hashFunction = new CRC16( definition );
            hashFunction.Initialize();

            port = new SerialPort( "COM7", 19200, Parity.None, 8, StopBits.One );

            port.Open();
            port.DataReceived += SerialDataReceived;
            myTimer.Elapsed += MyTimer_Elapsed;
            myTimer.Start();

            DataContext = this;
        }

        private CRC16.Definition definition;
        private CRC16 hashFunction;

        // uint retVal = ComputeCRC( new byte[] { 0x55, 0x66, 0x77, 0x88 } ); returns 0x32d6
        uint ComputeCRC( byte[] data )
        {
            hashFunction.ComputeHash( data );

            uint retVal = hashFunction.Hash[ 1 ];
            retVal = ( retVal << 8 ) + hashFunction.Hash[ 0 ];

            return retVal;
        }

        Queue<RawValuePacket> RawValuePackets = new Queue<RawValuePacket>();

        private void MyTimer_Elapsed( object sender, ElapsedEventArgs e )
        {
            // float off = caliberationValues.offsetA0;
            float[] temp;
            
            if ( decodedPackets.Count > 0 )
            {
                var packet = decodedPackets.Dequeue();
                Debug.Print("New added packet info " + packet.ToString() );

                switch ( packet.packetType )
                {
                    case PacketType.PressureOnly:
                        break;
                    case PacketType.PressureTemperature:
                        
                        //var caliberationvalues1 = new caliberationValues(packet);
                        var rawValuePacket = new RawValuePacket( packet );
                         
                        RawValuePackets.Enqueue( rawValuePacket );

                        
                        //Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Background,
                        //    new Action( () =>
                        //    {
                        //        foreach ( var item in rawValuePacket.PresA )
                        //        {
                        //            PressureLineSeriesValues.Add( item );
                        //        }
                        //        TemperatureLineSeriesValues.Add( rawValuePacket.TempA * 0.03125 );
                        //    } ) );
                        Debug.Print( rawValuePacket.ToString() );
                        break;
                    case PacketType.TemperatureOnly:
                        break;
                    case PacketType.CalibrationInfo:
                        
                        var caliberationvalues = new caliberationValues(packet);
                        //Debug.Print("temp = " + temp);
                       // offsetA0 = caliberationvalues.offsetA0;
                        // pull calibration information out here
                        break;                
                    default:
                        Debug.Print( "Invalid packet received" );
                        break;
                }


            }
        }

        private void SerialDataReceived( object sender, SerialDataReceivedEventArgs e )
        {
            while ( port.BytesToRead > 0 )
            {
                receivedData.Enqueue( (byte)port.ReadByte() );

                if ( receivedData.Count >= QUEUE_THRESHOLD )
                {
                    ProcessQueue();
                }
            }
        }

        enum ProcessQueueStates
        {
            FindStart,
            GetType,
            GetLength,
            GrabbingPayload,
            NextIsEnd,
        }

        private ProcessQueueStates processState = ProcessQueueStates.FindStart;
        private int processLength = 0;

        Packet processPacket = new Packet();
        private Queue<byte> processPayload = new Queue<byte>();

        private Queue<Packet> decodedPackets = new Queue<Packet>();

        private readonly byte START_BYTE = 0x59;

        private void ProcessQueue()
        {
            while ( receivedData.Count > 0 )
            {
                byte nextByte = receivedData.Dequeue();

                switch ( processState )
                {
                    case ProcessQueueStates.FindStart:
                        if ( nextByte == START_BYTE )
                        {
                            processState = ProcessQueueStates.GetType;
                        }
                        break;

                    case ProcessQueueStates.GetType:
                        processPacket.packetType = (PacketType)nextByte;

                        processState = ProcessQueueStates.GetLength;
                        break;

                    case ProcessQueueStates.GetLength:
                        processPacket.PayloadLength = nextByte;
                        processLength = nextByte;

                        processState = ProcessQueueStates.GrabbingPayload;
                        break;

                    case ProcessQueueStates.GrabbingPayload:
                        processPayload.Enqueue( nextByte );
                        processLength--;

                        if ( processLength <= 0 )
                        {
                            processState = ProcessQueueStates.NextIsEnd;
                        }
                        break;

                    case ProcessQueueStates.NextIsEnd:
                        // TODO: Check CRC

                        processPacket.crc = nextByte;
                        processPacket.payload = processPayload.ToArray();
                        decodedPackets.Enqueue( processPacket.Clone() );

                        processState = ProcessQueueStates.FindStart;
                        processPayload.Clear();
                        break;

                    default:
                        Debug.Print( "Default state" );
                        break;
                }
            }
        }
    }

    public enum PacketType
    {
        PressureOnly = 0x40,
        PressureTemperature = 0x41,
        TemperatureOnly = 0x42,

        CalibrationRequest = 0x50,
        CalibrationInfo = 0x51,

        ConfigureRequest = 0x60,

        Ack = 0x70,
    }

    public class Packet
    {
        public PacketType packetType;
        public byte PayloadLength;
        public byte[] payload;
        public byte crc;

        public override string ToString()
        {
            string payloadString = $"{payload[ 0 ]}";
            string hexString = $"0x{payload[ 0 ]:X2}";

            for ( int i = 1; i < payload.Length; i++ )
            {
                payloadString += $", {payload[ i ]}";
                hexString += $" {payload[ i ]:X2}";
            }

            return $"Packet: Type={packetType}, Length={PayloadLength}, Payload={payloadString} || {hexString}";
        }
        
        /// <summary>
        /// Returns a deep copy of the packet
        /// </summary>
        /// <returns></returns>
        public Packet Clone()
        {
            return new Packet()
            {
                packetType = this.packetType,
                PayloadLength = this.PayloadLength,
                payload = (byte[])this.payload.Clone(),
            };
        }
    }
    public class caliberationValues
    {
        static int count = 0;
        
        public float SequenceNumber;

        public float rangeA;

        public float minA;

        public float offsetA0;
        public float offsetA1;
        public float offsetA2;
        public float offsetA3;

        public float spanA0;
        public float spanA1;
        public float spanA2;
        public float spanA3;

        public float shapeA0;
        public float shapeA1;
        public float shapeA2;
        public float shapeA3;

        
        public byte[] tempA;
        public float[] returnArray;
        
        public caliberationValues() { }

        public caliberationValues(Packet packet)
        {
            returnArray = new float[14];
            int j = 0;
            if (packet.packetType != PacketType.CalibrationInfo)
            {
                throw new Exception(" Invalid packet type");
            }
            this.SequenceNumber = packet.payload[0];

            tempA = new byte[122];

            for (int i = 0; i < 4; i++)
            {                
                tempA[i] = packet.payload[i];
            }
            
            rangeA = System.BitConverter.ToSingle(tempA, 0);
            
            returnArray[j] = rangeA;
            j += 1;
            for (int i = 4; i < 8; i++)
            {          
                tempA[i] = packet.payload[i];
            }
            minA = System.BitConverter.ToSingle(tempA, 4);
            
            returnArray[j] = minA;
            j += 1;
            //int unitA = packet.payload[8];
            //for (int i = 8; i < 13; i++)
            //{
            //    int j = i + 1;
            //    unitA = unitA << 8 | packet.payload[j];
            //}

            
            for (int i = 13; i < 17; i++)
            {
                tempA[i] = packet.payload[i];
                
            }
            offsetA0 = System.BitConverter.ToSingle(tempA, 13);
            returnArray[j] = offsetA0;
            j += 1;

            for (int i = 17; i < 21; i++)
            {

                tempA[i] = packet.payload[i];
            }
            offsetA1 = System.BitConverter.ToSingle(tempA, 17);
            returnArray[j] = offsetA1;
            j += 1;

            for (int i = 21; i < 25; i++)
            {

                tempA[i] = packet.payload[i];
            }
            offsetA2 = System.BitConverter.ToSingle(tempA, 21);
            returnArray[j] = offsetA2;
            j += 1;

            for (int i = 25; i < 29; i++)
            {

                tempA[i] = packet.payload[i];
            }
            offsetA3 = System.BitConverter.ToSingle(tempA, 25);
            returnArray[j] = offsetA3;
            j += 1;


            for (int i = 29; i < 33; i++)
            {

                tempA[i] = packet.payload[i];
            }
            spanA0 = System.BitConverter.ToSingle(tempA, 29);
            returnArray[j] = spanA0;
            j += 1;

            for (int i = 33; i < 37; i++)
            {
                tempA[i] = packet.payload[i];
            }
            spanA1 = System.BitConverter.ToSingle(tempA, 33);
            returnArray[j] = spanA1;
            j += 1;

            for (int i = 37; i < 41; i++)
            {
                tempA[i] = packet.payload[i];
            }
            spanA2 = System.BitConverter.ToSingle(tempA, 37);
            returnArray[j] = spanA2;
            j += 1;

            for (int i = 41; i < 45; i++)
            {
                tempA[i] = packet.payload[i];
            }
            spanA3 = System.BitConverter.ToSingle(tempA, 41);
            returnArray[j] = spanA3;
            j += 1;


            for (int i = 45; i < 49; i++)
            {
                tempA[i] = packet.payload[i];
            }
            shapeA0 = System.BitConverter.ToSingle(tempA, 45);
            returnArray[j] = shapeA0;
            j += 1;

            for (int i = 49; i < 53; i++)
            {
                tempA[i] = packet.payload[i];
            }
            shapeA1 = System.BitConverter.ToSingle(tempA, 49);
            returnArray[j] = shapeA1;
            j += 1;

            for (int i = 53; i < 57; i++)
            {
                tempA[i] = packet.payload[i];
            }
            shapeA2 = System.BitConverter.ToSingle(tempA, 53);
            returnArray[j] = shapeA2;
            j += 1;

            for (int i = 57; i < 61; i++)
            {
                tempA[i] = packet.payload[i];
            }
            shapeA3 = System.BitConverter.ToSingle(tempA, 57);
            returnArray[j] = shapeA3;
            
            count += 1;
            Debug.Print("count : " + count);
        }

        public float[] ReturnValues()
        {
            return returnArray;
        }

    }
    public class RawValuePacket
    {
        public int SequenceNumber;

        public int TempA;
        public int TempB;

        public int[] PresA;
        public int[] PresB;

        public RawValuePacket() { }

        public RawValuePacket( Packet packet )
        {
            if (packet.packetType != PacketType.PressureTemperature)
            {
                throw new Exception("Invalid packet type");
            }

            this.SequenceNumber = packet.payload[ 0 ];

            int count = ( packet.PayloadLength - 1 - 4 ) / ( 2 * 3 );

            this.PresA = new int[ count ];
            this.PresB = new int[ count ];

            int offset;
            for ( int i = 0; i < count; i++ )
            {
                offset = 1 + i * ( 2 * 3 );
                int presA = packet.payload[ offset++ ];
                presA = presA << 8 | packet.payload[ offset++ ];
                presA = presA << 8 | packet.payload[ offset++ ];

                //packet.payload[0]*2^16 | packet.payload[1] << 8 | packet.payload[2];
                //(([0]*2^8 + [1])*2^8 + [2])

                int presB = packet.payload[ offset++ ];
                presB = presB << 8 | packet.payload[ offset++ ];
                presB = presB << 8 | packet.payload[ offset++ ];

                this.PresA[ i ] = presA;
                this.PresB[ i ] = presB;
            }

            offset = packet.PayloadLength - 4;

            int tempA = packet.payload[ offset++ ];
            tempA = tempA << 8 | packet.payload[ offset++ ];

            int tempB = packet.payload[ offset++ ];
            tempB = tempB << 8 | packet.payload[ offset++ ];

            this.TempA = tempA;
            this.TempB = tempB;

        }

        public override string ToString()
        {
            string printString = $"RawValuePacket: TempA={TempA}, TempB={TempB}";
            for ( int i = 0; i < PresA.Length; i++ )
            {
                printString += $"\nA[{i}]={PresA[ i ]}, B[{i}]={PresB[ i ]}\n";
            }

            return printString;
        }
    }
}
