using System;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace TapeReelPacking.Source.Hardware
{
    public class CommInterface
    {
        TcpClient tcpClient;
        public bool isConnected;
        string IPaddress;
        public Stream stream;
        UTF8Encoding encoding = new UTF8Encoding();
        int port;
        object senddata = new object();
        object readdata = new object();
        public CommInterface(string ipAddress, int port)
        {
            IPaddress = ipAddress;
            this.port = port;
            isConnected = false;
        }
        public bool Connect()
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IPaddress, port);
                stream = tcpClient.GetStream();
                isConnected = true;
                return true;
            }
            catch
            {
                isConnected = false;
                stream = null;
                return false;
            }
        }
        public void CloseSocket()
        {
            try
            {
                stream.Dispose();
                stream = null;
                tcpClient.Close();
                tcpClient.Dispose();
            }
            catch { }
        }
        public int ReadData(ref byte[] data)
        {
            lock (readdata)
            {
                try
                {
                    if (stream != null && tcpClient.Connected)
                    {
                        byte[] buff = new byte[1024];
                        int length = stream.Read(buff, 0, 1024);
                        Array.Copy(buff, 0, data, 0, length);
                        //string txtdebug = $"Port{port}: ";
                        //for (int i = 0; i < length; i++)
                        //{
                        //    txtdebug += buff[i].ToString();
                        //}
                        //DebugMessage.WriteToDebugViewer(8, string.Format("Receive Sucessfully Port " + CommPLC.commSequence.port.ToString()));
                        return length;
                    }
                    else
                    {
                        //MainWindowVM.master.commPLC.UpdataStatePLC(CommPLC.commSequence.port, false);
                        //DebugMessage.WriteToDebugViewer(8, string.Format("Disconnect At Port " + CommPLC.commSequence.port.ToString()));
                    }
                }
                catch
                {
                    //MainWindowVM.master.commPLC.UpdataStatePLC(CommPLC.commSequence.port, false);
                    //DebugMessage.WriteToDebugViewer(8, string.Format("Disconnect At Port " + CommPLC.commSequence.port.ToString()));
                }
                try
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                        tcpClient.Close();
                        tcpClient.Dispose();
                    }
                }
                catch { }
                return -1;
            }
        }
        public bool WriteData(byte[] buff)
        {
            lock (senddata)
            {
                try
                {
                    if (stream != null && tcpClient.Connected)
                    {   
                        stream.Write(buff, 0, buff.Length);
                        //string timeTriggerToPLC = DateTime.Now.Year.ToString() + ":" +
                        //DateTime.Now.Month.ToString() + ":" +
                        //DateTime.Now.Day.ToString() + "_" +
                        //DateTime.Now.Hour.ToString() + ":" +
                        //DateTime.Now.Minute.ToString() + ":" +
                        //DateTime.Now.Second.ToString() + ":" +
                        //DateTime.Now.Millisecond.ToString() + "  ";
                        //MainWindowVM.master.commLog.VisionToPLCLog(timeTriggerToPLC + Application.ConvertByteArrayToString(buff));

                        //string txtdebug = $"Port{port}: ";
                        //for (int i = 0; i < buff.Length; i++)
                        //{
                        //    txtdebug += buff[i].ToString();
                        //}
                        //DebugMessage.WriteToDebugViewer(9, txtdebug);
                        LogMessage.LogMessage.WriteToDebugViewer(8, string.Format("Sent Sucessfully at " + IPaddress + ":" + port.ToString()));
                        return true;
                    }
                    else
                    {
                        //if (MainWindowVM.master != null)
                        //MainWindowVM.master.commPLC.UpdataStatePLC(CommPLC.commLotInfo.port, false);
                        //DebugMessage.WriteToDebugViewer(8, string.Format("Disconnect At Port " + CommPLC.commLotInfo.port.ToString()));
                        LogMessage.LogMessage.WriteToDebugViewer(8, string.Format("Sent Failed at " + IPaddress + ":" + port.ToString()));

                    }
                }
                catch
                {
                    //if (MainWindowVM.master != null)
                    //    MainWindowVM.master.commPLC.UpdataStatePLC(CommPLC.commLotInfo.port, false);
                    LogMessage.LogMessage.WriteToDebugViewer(8, string.Format("Sent Failed at " + IPaddress + ":" + port.ToString()));
                }
                try
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                        tcpClient.Close();
                        tcpClient.Dispose();
                    }
                }
                catch { }
                return false;
            }
        }
    }
}
