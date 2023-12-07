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
    using System.IO;
	public class BarCodeReaderInterface
	{
		public CommInterface commBarCodeSequence;

		private ReaderSearcher m_searcher = new ReaderSearcher();

		private ReaderAccessor m_reader = new ReaderAccessor();
		List<NicSearchResult> m_nicList = new List<NicSearchResult>();
		public LiveviewForm m_liveviewForm = new LiveviewForm();

		string barCodeipAddress;
		const int BUFLEN = 255;
		public int[] nReceiveMessage;
		private MainWindow main;
		public BarCodeReaderView m_BarcodeReader;

		public BarCodeReaderInterface()
		{

			if (main == null)
				main = MainWindow.mainWindow;

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
						m_searcher.Start((res) =>
						{
							System.Windows.Application.Current.Dispatcher.BeginInvoke(new delegateSearchResult(appendSearchResult), res);
						});

						break;

					}
				}
			}

			nReceiveMessage = new int[BUFLEN];
			LoadInforPortNumber();
			m_BarcodeReader = new BarCodeReaderView();
		}

		public void appendSearchResult(ReaderSearchResult res)
		{
			if (res.IpAddress == "")
			{
				return;
			}

			m_liveviewForm.EndReceive();
			m_liveviewForm.IpAddress = res.IpAddress;
			m_liveviewForm.ImageFormat = LiveviewForm.ImageFormatType.Jpeg;
			m_liveviewForm.BinningType = LiveviewForm.ImageBinningType.None;
			m_liveviewForm.BeginReceive();
			m_liveviewForm.Update();
			m_reader.IpAddress = res.IpAddress;
			bool bSuccess = m_reader.Connect();

			updateConnectionStatus(bSuccess);
		}

		public void updateConnectionStatus(bool bIsConnected)
		{

			System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
			{
				if (bIsConnected)
				{
					m_BarcodeReader.label_ReaderIP_Address.Content = $"{m_reader.IpAddress}";
					m_BarcodeReader.Connect.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
					MainWindow.mainWindow.label_Barcode_Status.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

				}
				else
				{
					m_BarcodeReader.label_ReaderIP_Address.Content = $"{m_reader.IpAddress}";
					m_BarcodeReader.Connect.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
					MainWindow.mainWindow.label_Barcode_Status.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

				}
			});
		}


		private delegate void delegateSearchResult(ReaderSearchResult res);
		public string getKeyFromSearchResult(ReaderSearchResult res)
		{
			return res.IpAddress + "/" + res.ReaderModel + "/" + res.ReaderName;
		}

		public void ReconnectBarCode()
        {
			bool bIsConnected = false;
			if(m_reader.ExecCommand("KEYENCE") == "")
            {
				m_reader.Disconnect();
				bIsConnected = m_reader.Connect();
			}
			updateConnectionStatus(bIsConnected);
		}

		bool bIsDownload = false;
		public string GetBarCodeStringAndImage(string strCmd = "")
		{
			if (bIsDownload)
				return "";
			bIsDownload = true;
			string str = "";
			string str2 = "";
			if (MainWindow.mainWindow.master.m_Tracks[1].m_strCurrentLot == "" || MainWindow.mainWindow.master.m_Tracks[1].m_strCurrentLot == null)
				MainWindow.mainWindow.master.m_Tracks[1].m_strCurrentLot = "Dummy";

			string strFolder = Path.Combine(Application.Application.pathImageSave, MainWindow.mainWindow.master.m_Tracks[1].m_strCurrentLot);
			if (!Directory.Exists(strFolder))
				Directory.CreateDirectory(strFolder);

			string strImageFullName;
			string strtime = string.Format("{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));

			string resp = m_reader.ExecCommand("LON,01");
			if (resp.Length > 0)
			{
				str = resp.Replace("\r", "");
				str = str.Replace(":", "");
			}

			LogMessage.WriteToDebugViewer(3, "Message responsed from Barcode Bank 1: " + str2);
			string resp2 = m_reader.ExecCommand("LON,02");
			if (resp2.Length > 0)
			{
				str2 = resp2.Replace("\r", "");
			}
			Thread.Sleep(200);


			LogMessage.WriteToDebugViewer(3, "Message responsed from Barcode Bank 2: " + str);
			strImageFullName = Path.Combine(strFolder, strtime + "_" + str + "_" + str2 + ".bmp");

			System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
			{
				m_liveviewForm.DownloadRecentImage(strImageFullName);
				MainWindow.mainWindow.master.m_Tracks[1].m_imageViews[0].UpdateNewImageMono(strImageFullName);
			});

			bIsDownload = false;
				return str + "_" + str2;
		}

		void LoadInforPortNumber()
		{
			MainWindow.mainWindow.label_Barcode_Status.Content = barCodeipAddress;
		}
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
	}
}
