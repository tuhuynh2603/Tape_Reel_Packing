using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TapeReelPacking.Source.Comm
{
    public class SerialCommunication
    {
        static SerialPort m_serialPort;
        public static ManualResetEvent m_SerialDataReceivedEvent = new ManualResetEvent(false);
        string m_strComm;
        int m_nBauRate;
        public static string strReadString 
        { 
            get;
            set; 
        }

        public SerialCommunication(string strComm = "COM10", int nBaurate = 115200)
        {
            strReadString = "";
            InitializeConnection(strComm, nBaurate);
            Thread ReadThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => ReadThread_Fcn()));
            ReadThread.Start();
        }
        public void InitializeConnection(string strComm, int nBaurate)
        {
            try
            {
                m_strComm = strComm;
                m_nBauRate = nBaurate;
                if (m_serialPort == null)
                {
                    m_serialPort = new SerialPort(strComm, nBaurate, Parity.None, 8, StopBits.One);
                }
                else
                {
                    m_serialPort.Close();
                    m_serialPort = new SerialPort(strComm, nBaurate, Parity.None, 8, StopBits.One);
                    Thread.Sleep(1000);
                }
                m_serialPort.ReadTimeout = 500;
                m_serialPort.WriteTimeout = 500;
                m_serialPort.Open();


                if (System.Windows.Application.Current == null)
                    return;


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                        MainWindow.mainWindow.label_Serial_Comm.Background = new SolidColorBrush(Colors.Green);
                        MainWindow.mainWindow.label_Serial_Comm.Content = $"{strComm}";
                        MainWindow.mainWindow.label_Serial_Comm.Foreground = new SolidColorBrush(Colors.Black);

                });
            }
            catch(Exception e)
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, e.ToString());

                if (System.Windows.Application.Current == null)
                    return;


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                    MainWindow.mainWindow.label_Serial_Comm.Background = new SolidColorBrush(Colors.Red);
                    MainWindow.mainWindow.label_Serial_Comm.Content = $"{strComm}";
                    MainWindow.mainWindow.label_Serial_Comm.Foreground = new SolidColorBrush(Colors.Black);

                });
            };

        }

        public void Disconnect()
        {
            if (m_serialPort == null)
                return;

            if (m_serialPort.IsOpen)
                m_serialPort.Close();

            if (System.Windows.Application.Current == null)
                return;


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.mainWindow.label_Serial_Comm.Background = new SolidColorBrush(Colors.Red);
                MainWindow.mainWindow.label_Serial_Comm.Foreground = new SolidColorBrush(Colors.Black);
            });

        }

        public void ReadThread_Fcn()
        {
            char read;
            string strTemp = "";
            while(true)
            {
                if(m_serialPort.IsOpen == false)
                {
                    continue;
                    ////InitializeConnection(m_strComm, m_nBauRate);
                }
                try
                {
                    while (m_serialPort.BytesToRead > 0)
                    {
                        read = (char)m_serialPort.ReadChar();
                            switch(read)
                            {
                                case '\r':
                                    break;
                                case '\n':
                                    if (strTemp.Length > 0)
                                    {
                                        lock (strReadString)
                                        {
                                            strReadString = strTemp;
                                            strTemp = "";
                                            //LogMessage.LogMessage.WriteToDebugViewer(1, strReadString);
                                            if (strReadString.Length > 0)
                                                m_SerialDataReceivedEvent.Set();
                                        }
                                    }


                                break;
                                default:
                                    strTemp += read;
                                    break;

                            }
                    }

                }
                catch (Exception e)
                {
                }
                ;

                Thread.Sleep(20);

            }
        }

        public static string ReadData()
        {
            lock (strReadString)
            {
                string strTemp = strReadString;
                strReadString = "";
                LogMessage.LogMessage.WriteToDebugViewer(1, strTemp);
                return strTemp;
            }
        }

        public static void WriteData(string strText)
        {
            //for (int n = 0; n < 1000; n++)
            //    strText += "234444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444fsdgdrgsrgrgdrggggggggggggggg";
            if (!m_serialPort.IsOpen)
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, $"Connection Failed! Serial COMM {m_serialPort.PortName} ");
                return;
            }
            LogMessage.LogMessage.WriteToDebugViewer(1, $"Serial COMM {m_serialPort.PortName} Write: {strText} ");
            //strText += "\n";
            m_serialPort.WriteLine(strText);
        }

        /// <summary>
        /// /////////// calculate CRC to check the synchonize of data
        /// </summary>
        /// 



        private string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                //Note: X for upper, x for lower case letters
                sb.Append(Convert.ToInt32(t).ToString("x2"));
            }
            return sb.ToString();
        }
        public byte[] HexStringToByteArray(string hexString)
        {
            hexString = hexString.Replace(" ", "");

            return Enumerable.Range(0, hexString.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                     .ToArray();
        }
        public string ByteArrayToHexString(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray);
        }

        public enum Crc16Mode : ushort
        {
            ARINC_NORMAL = 0XA02B, ARINC_REVERSED = 0xD405, ARINC_REVERSED_RECIPROCAL = 0XD015,
            CCITT_NORMAL = 0X1021, CCITT_REVERSED = 0X8408, CCITT_REVERSED_RECIPROCAL = 0X8810,
            CDMA2000_NORMAL = 0XC867, CDMA2000_REVERSED = 0XE613, CDMA2000_REVERSED_RECIPROCAL = 0XE433,
            DECT_NORMAL = 0X0589, DECT_REVERSED = 0X91A0, DECT_REVERSED_RECIPROCAL = 0X82C4,
            T10_DIF_NORMAL = 0X8BB7, T10_DIF_REVERSED = 0XEDD1, T10_DIF_REVERSED_RECIPROCAL = 0XC5DB,
            DNP_NORMAL = 0X3D65, DNP_REVERSED = 0XA6BC, DNP_REVERSED_RECIPROCAL = 0X9EB2,
            IBM_NORMAL = 0X8005, IBM_REVERSED = 0XA001, IBM_REVERSED_RECIPROCAL = 0XC002,
            OPENSAFETY_A_NORMAL = 0X5935, OPENSAFETY_A_REVERSED = 0XAC9A, OPENSAFETY_A_REVERSED_RECIPROCAL = 0XAC9A,
            OPENSAFETY_B_NORMAL = 0X755B, OPENSAFETY_B_REVERSED = 0XDDAE, OPENSAFETY_B_REVERSED_RECIPROCAL = 0XBAAD,
            PROFIBUS_NORMAL = 0X1DCF, PROFIBUS_REVERSED = 0XF3B8, PROFIBUS_REVERSED_RECIPROCAL = 0X8EE7

        }

        public ushort CalculateCRC(byte[] data)
        {
            Crc16 crcCalc = new Crc16(Crc16Mode.IBM_NORMAL);
            ushort crc = crcCalc.ComputeChecksum(data);
            return crc;
        }

        public class Crc16
        {
            readonly ushort[] table = new ushort[256];

            public ushort ComputeChecksum(params byte[] bytes)
            {
                ushort crc = 0;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(crc ^ bytes[i]);
                    crc = (ushort)((crc >> 8) ^ table[index]);
                }
                return crc;
            }

            public byte[] ComputeChecksumBytes(params byte[] bytes)
            {
                ushort crc = ComputeChecksum(bytes);
                return BitConverter.GetBytes(crc);
            }

            public Crc16(Crc16Mode mode)
            {
                ushort polynomial = (ushort)mode;
                ushort value;
                ushort temp;
                for (ushort i = 0; i < table.Length; ++i)
                {
                    value = 0;
                    temp = i;
                    for (byte j = 0; j < 8; ++j)
                    {
                        if (((value ^ temp) & 0x0001) != 0)
                        {
                            value = (ushort)((value >> 1) ^ polynomial);
                        }
                        else
                        {
                            value >>= 1;
                        }
                        temp >>= 1;
                    }
                    table[i] = value;
                }
            }
        }

    }
}
