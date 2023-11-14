using Magnus_WPF_1.Source.Define;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Magnus_WPF_1.Source.Application;
using System.Diagnostics;

namespace Magnus_WPF_1.Source.Hardware
{

	using Magnus_WPF_1.Source.LogMessage;

	public class CommHiwinRobot
    {
		public CommInterface commSequence;
		string ipAddress;
		public int portSequence = 11000;
		public int portLotInfo = 11001;
		public static int ncntWord;
		public static int ncntDword;
		const int BUFLEN = 255;
		public int[] nReceiveMessage;
		public int[] nReceiveMessageBkUp;
        //private bool isVisionReady;
        //private bool isVisonGoOnline;
        //private bool isVisionBusy;
        //private bool isPLCTriggerOnline;
        //private bool isCompletedBusy;
		private MainWindow main;

		Thread CommSequenceThread;

		public CommHiwinRobot()
        {

			if (main == null)
				main = MainWindow.mainWindow;

			string defaults = "127.0.0.1";
			ipAddress = GetCommInfo("Comm::IpAddress", defaults);
			defaults = portSequence.ToString();
			portSequence = int.Parse(GetCommInfo("Comm::PortNumber[0]", defaults));
			defaults = portLotInfo.ToString();
			portLotInfo = int.Parse(GetCommInfo("Comm::PortNumber[1]", defaults));
			LogMessage.WriteToDebugViewer(1, "Init  CommInterface");
			commSequence = new CommInterface(ipAddress, portSequence);

			nReceiveMessage = new int[BUFLEN];
			nReceiveMessageBkUp = new int[BUFLEN];
			LoadInforPortNumber();
			InitThread();

		}

		void LoadInforPortNumber()
		{
			MainWindow.mainWindow.port_receive.Text = "Port: " + portSequence.ToString();
			MainWindow.mainWindow.port_sent.Text = "Port: " + portLotInfo.ToString();
		}


		private void InitThread()
        {
			CommSequenceThread = new Thread(new ThreadStart(CommMonitorSequenceThread));
			CommSequenceThread.IsBackground = true;
			CommSequenceThread.Start();
		}

        void CommMonitorSequenceThread()
		{
			byte[] recvBuf = new byte[255];
			//byte[] recvBufTemp = new byte[255];
			//nReceiveMessage = Enumerable.Repeat(-1, BUFLEN).ToArray();
			//nReceiveMessageBkUp = Enumerable.Repeat(-1, BUFLEN).ToArray();
			bool lastate = true;
			int nReceivedBufferLength;
			while (true)
			{
				// To do Register socket				
				commSequence.Connect();
				if (commSequence.isConnected)
				{
					//DebugMessage.WriteToDebugViewer(8, string.Format("Connected Port " + portSequence.ToString()));
					UpdateStateRobot(portSequence, true);
					lastate = commSequence.isConnected;
				}
				else
				{
					if (true)
					{
						//DebugMessage.WriteToDebugViewer(8, string.Format("Can Not Connected Port " + portSequence.ToString()));
						UpdateStateRobot(portSequence, false);
						lastate = false;
						continue;
					}
				}
				do
				{
					recvBuf = new byte[BUFLEN];
					nReceivedBufferLength = commSequence.ReadData(ref recvBuf);
					if (nReceivedBufferLength > 2)
					{
						Thread.Sleep(0);

						int headerLength = nReceivedBufferLength;
						string dataReceive = "";
						for (int i = 1; i < headerLength -1; i++)
						{
							dataReceive += Convert.ToChar(recvBuf[i]);
						}

						LogMessage.WriteToDebugViewer(8, "Data received from robot: " + dataReceive);
						DecodeMessageReceivedFromHIKRobot(ref dataReceive);

						Array.Clear(recvBuf, 0, BUFLEN);
					}
					else if (nReceivedBufferLength == 0)
					{
						//Connection closed by Controller
					}
					else
					{
						//close socket, break
					}
				}
				while (nReceivedBufferLength > 0);
				Thread.Sleep(10);
			}
		}


		public int m_nDecodeMessage = 0;
		public void DecodeMessageReceivedFromHIKRobot(ref string strMessage)
        {
			//string strMess = strMessage;
			//strMessage.Remove('{');
			//strMessage.Remove('}');
			string[] strSplitMessage = strMessage.Split('_');
			if (strSplitMessage.Length < 1 || strSplitMessage[0] == "")
				return;

			switch (strSplitMessage[0])
				{
				case "STARTSEQUENCE":
                    {
						//m_nDecodeMessage = START

						// Go online/ init online sequence thread
						System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
						{
							MainWindow.mainWindow.Run_Sequence();
							//  Send message to Robot
						});

						break;
                    }

				case "STOPSEQUENCE":
					{

						//m_nDecodeMessage = START

						// Go online/ init online sequence thread
						System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
						{
							MainWindow.mainWindow.Stop_Sequence();
							//  Send message to Robot
						});
						break;
					}

				case "TRIGGERCAMERA1":
					{
						Master.m_hardwareTriggerSnapEvent[(int)TRACK_TYPE.TRACK_CAM1].Set();
						//System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
						//{
						//	MainWindow.mainWindow.Stop_Sequence();
						//	//  Send message to Robot
						//});
						break;
					}
				case "TRIGGERCAMERA2":
					{
						Master.m_hardwareTriggerSnapEvent[(int)TRACK_TYPE.TRACK_CAM2].Set();
						//System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
						//{
						//	MainWindow.mainWindow.Stop_Sequence();
						//	//  Send message to Robot
						//});
						break;
					}
				//case SignalFromRobot.Robot_Swap_Recipe.ToString():
				//	{
				//		break;
				//	}
				//case SignalFromRobot.Robot_Move_Done.ToString():
				//	{
				//		break;
				//	}
				//case SignalFromRobot.Robot_Error.ToString():
				//	{
				//		break;
				//	}
				//case SignalFromRobot.Robot_Reset_Software.ToString():
				//	{
				//		break;
				//	}

				default:
					break;
			}


		}
		public void CreateAndSendMessageToHIKRobot(SignalFromVision visionSignal, string visionResult = "" )
        {
			string mess = "";

			switch (visionSignal)
			{
				case SignalFromVision.Vision_Ready:
					{
						mess = "{" + SignalFromVision.Vision_Ready.ToString() + "_" + visionResult + "}";
						break;
					}

				case SignalFromVision.Vision_Go_Home:
					{
						mess = "{" + SignalFromVision.Vision_Go_Home.ToString() + "_" + visionResult + "}";
						break;
					}

				case SignalFromVision.Vision_Go_Pick:
					{
						mess = "{" + SignalFromVision.Vision_Go_Pick.ToString() + "_" + visionResult + "}";
						break;
					}
				case SignalFromVision.Vision_Absolute_Move:
					{
						mess = "{" + SignalFromVision.Vision_Absolute_Move.ToString() + "_" + visionResult + "}";
						break;
					}
				case SignalFromVision.Vision_Relative_Move:
					{
						mess = "{" + SignalFromVision.Vision_Relative_Move.ToString() + "_" + visionResult + "}";
						break;
					}
				case SignalFromVision.Vision_Reset_Software_Done:
					{
						mess = "{" + SignalFromVision.Vision_Reset_Software_Done.ToString() + "_" + visionResult + "}";
						break;
					}
				default:
					mess = "{" + SignalFromVision.Vision_Ready.ToString() + "_" + visionResult + "}";
					break;
			}
			LogMessage.DebugMessageTo(2, true, "CreateAndSendMessageToHIKRobot: " +mess);

			byte[] data = new byte[BUFLEN];
			for (int i = 0; i < mess.Length; i++)
			{
				data[i] = (byte)(mess.ToString()[i]);
			}

			commSequence.WriteData(data);

		}


		public delegate void UpdateStateReceiveConnection(bool isConnect);
		public static UpdateStateReceiveConnection UpdateReceiveConnection;

		public delegate void UpdateStateSentConnection(bool isConnect);
		public static UpdateStateSentConnection UpdateSentConnection;
		private void UpdateStateRobot(int portNumber, bool isConneted)
        {
			if (portNumber == portSequence)
			{
				System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
				{
					if (isConneted)
					{
						main.Color_PortReceive = "Lime";
					}
					else
					{
						main.Color_PortReceive = "Red";
					}
					UpdateReceiveConnection?.Invoke(isConneted);
				});
			}
			else if (portNumber == portLotInfo)
			{
				System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
				{
					if (isConneted)
					{
						main.Color_PortSent = "Lime";
					}
					else
					{
						main.Color_PortSent = "Red";
					}
					UpdateSentConnection?.Invoke(isConneted);
				});
			}

		}

		#region TCP/IP
		public static string GetCommInfo(string key, string defaults)
		{
			RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(Application.Application.pathRegistry + "\\Comm", true);
			if ((string)registerPreferences.GetValue(key) == null)
			{
				registerPreferences.SetValue(key, defaults);
				return defaults;
			}
			else
				return (string)registerPreferences.GetValue(key);
		}
		public static string ConvertByteArrayToString(byte[] buff)
		{
			string strOutput = "";
			try
			{
				strOutput = System.Text.Encoding.UTF8.GetString(buff).Trim();
				int index = strOutput.IndexOf('\0');
				strOutput = strOutput.Substring(0, index);
			}
			catch (Exception)
			{
				return "";
			}
			return strOutput;
		}
		#endregion

	}
}
