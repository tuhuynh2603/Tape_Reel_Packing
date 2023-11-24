using Magnus_WPF_1.Source.Define;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Keyence.AutoID.SDK;
using Magnus_WPF_1.Source.Application;
using System.Diagnostics;
namespace Magnus_WPF_1.Source.Hardware
{
    using Magnus_WPF_1.Source.LogMessage;
    //using System.Windows.Forms;

    public class BarCodeReaderInterface
    {
		public CommInterface commBarCodeSequence;

		private ReaderSearcher m_searcher = new ReaderSearcher();
		private Dictionary<string, ReaderAccessor> m_resisterdReaders = new Dictionary<string, ReaderAccessor>();
		List<NicSearchResult> m_nicList = new List<NicSearchResult>();
		private string m_strKey = "";
		public LiveviewForm vvv = new LiveviewForm();

		string barCodeipAddress;
		const int BUFLEN = 255;
		public int[] nReceiveMessage;
		private MainWindow main;
		public BarCodeReaderView m_BarcodeReader;

		Thread m_Thread;

		public BarCodeReaderInterface()
        {
			LogMessage.WriteToDebugViewer(2, "1");

			if (main == null)
				main = MainWindow.mainWindow;
			LogMessage.WriteToDebugViewer(2, "2");

			string defaults = "127.0.0.1";
			barCodeipAddress = GetCommInfo("Barcode Comm::IpAddress", defaults);

			m_nicList = m_searcher.ListUpNic();
			if (m_nicList != null)
			{
				for (int i = 0; i < m_nicList.Count; i++)
				{
					if (m_nicList[i].NicIpAddr == barCodeipAddress)
					{
						m_searcher.SelectedNicSearchResult = m_nicList[i];
						m_searcher.Start((res) => {
							System.Windows.Application.Current.Dispatcher.BeginInvoke(new delegateSearchResult(appendSearchResult), res);
						});
					}
						//string key = getKeyFromSearchResult();
					//nicComboBox.Items.Add(m_nicList[i].NicName + "/" + m_nicList[i].NicIpAddr + "/" + m_nicList[i].NicIpv4Mask);
				}
			}
			LogMessage.WriteToDebugViewer(2, "3");

			nReceiveMessage = new int[BUFLEN];
			LoadInforPortNumber();
			m_BarcodeReader = new BarCodeReaderView();
			//InitThread();

		}

		public void appendSearchResult(ReaderSearchResult res)
        {
			if (res.IpAddress == "")
			{
				//searchUIControl(false);
				return;
			}
			m_strKey = getKeyFromSearchResult(res);
		}

		private delegate void delegateSearchResult(ReaderSearchResult res);
		public string getKeyFromSearchResult(ReaderSearchResult res)
        {
			return res.IpAddress + "/" + res.ReaderModel + "/" + res.ReaderName;
		}

		private ReaderSearchResult getSearchResultFromKey(string key)
		{
			String[] readerInfo = key.Split('/');
			if (readerInfo.Length == 3)
			{
				return new ReaderSearchResult(readerInfo[1], readerInfo[2], readerInfo[0]);
			}
			return new ReaderSearchResult();
		}

		private bool resisterReaders(string key)
		{
			if (m_resisterdReaders.ContainsKey(key)) return false;

			ReaderSearchResult result = getSearchResultFromKey(key);
			m_resisterdReaders.Add(key, new ReaderAccessor(result.IpAddress));


			return true;
		}
		private bool removeReaders(string key)
		{
			if (m_resisterdReaders.ContainsKey(key))
			{
				m_resisterdReaders[key].Dispose();
				m_resisterdReaders.Remove(key);
				return true;
			}
			return false;
		}

		public void sendCommandToAllReaders(string strCmd)
		{
			resisterReaders(m_strKey);

			//LiveviewForm liveviewForm = new LiveviewForm();

			//liveviewForm.EndReceive();

			//liveviewForm.IpAddress = barCodeipAddress;

			//liveviewForm.BeginReceive();



			string cmd = strCmd;
			foreach (ReaderAccessor reader in m_resisterdReaders.Values)
			{
				reader.Connect();
				string resp = reader.ExecCommand(cmd);
				string str= resp.Replace("\r","");
				LogMessage.WriteToDebugViewer(2, "Message responsed from Barcode: " + str);
				//commandResponseText.AppendText("[" + reader.IpAddress + "][" + DateTime.Now + "]" + resp + "\r\n");

				//liveviewForm.DownloadRecentImage(Application.Application.pathImageSave + "BarcodeImage\\image.bmp");
				reader.Disconnect();
			}
			//if (!commandTxt.Items.Contains(commandTxt.Text)) commandTxt.Items.Add(commandTxt.Text);
		}

		void LoadInforPortNumber()
		{
			MainWindow.mainWindow.label_Barcode_Status.Content = barCodeipAddress;
		}

		private void InitThread()
        {
			m_Thread = new Thread(new ThreadStart(BarcodeReaderThread));
			m_Thread.IsBackground = true;
			m_Thread.Start();
		}
        void BarcodeReaderThread()
		{
			byte[] recvBuf = new byte[255];
			bool lastate = true;
			int nReceivedBufferLength;
			while (true)
			{
				// To do Register socket				
				//commBarCodeSequence.Connect();
				if (commBarCodeSequence.isConnected)
				{
                    UpdateStateBarCodeReader( true);
                    lastate = commBarCodeSequence.isConnected;
				}
				else
				{
					if (true)
					{
                        UpdateStateBarCodeReader( false);
                        lastate = false;
						continue;
					}
				}
				do
				{
					recvBuf = new byte[BUFLEN];
					nReceivedBufferLength = commBarCodeSequence.ReadData(ref recvBuf);
					if (nReceivedBufferLength > 2)
					{
						Thread.Sleep(0);

						int headerLength = nReceivedBufferLength;
						string dataReceive = "";
						for (int i = 1; i < headerLength -1; i++)
						{
							dataReceive += Convert.ToChar(recvBuf[i]);
						}

						LogMessage.WriteToDebugViewer(8, "Data received from Barcode: " + dataReceive);
						DecodeMessageReceivedFromBarcode(ref dataReceive);

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
		public void DecodeMessageReceivedFromBarcode(ref string strMessage)
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
							//MainWindow.mainWindow.Run_Sequence();
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
							//MainWindow.mainWindow.Stop_Sequence();
							//  Send message to Robot
						});
						break;
					}

				case "TRIGGERCAMERA1":
					{
						Master.m_hardwareTriggerSnapEvent[(int)TRACK_TYPE.TRACK_CAM1].Set();

						break;
					}
				case "TRIGGERCAMERA2":
					{
						Master.m_hardwareTriggerSnapEvent[(int)TRACK_TYPE.TRACK_CAM2].Set();

						break;
					}

				default:
					break;
			}


		}
		public void CreateAndSendMessageToBarcode(SignalFromVision visionSignal, string visionResult = "" )
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
			LogMessage.DebugMessageTo(2, true, "CreateAndSendMessageToBarCode: " + mess);

			byte[] data = new byte[BUFLEN];
			for (int i = 0; i < mess.Length; i++)
			{
				data[i] = (byte)(mess.ToString()[i]);
			}

			commBarCodeSequence.WriteData(data);

		}

		public delegate void UpdateStateReceiveConnection(bool isConnect);
		public static UpdateStateReceiveConnection UpdateReceiveConnection;

		public delegate void UpdateStateSentConnection(bool isConnect);
		public static UpdateStateSentConnection UpdateSentConnection;
		private void UpdateStateBarCodeReader( bool isConneted)
        {

			System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
			{
				if (isConneted)
				{
					main.color_barcodeReaderStatus = "Lime";
				}
				else
				{
					main.color_barcodeReaderStatus = "Red";
				}
				UpdateReceiveConnection?.Invoke(isConneted);
			});

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
