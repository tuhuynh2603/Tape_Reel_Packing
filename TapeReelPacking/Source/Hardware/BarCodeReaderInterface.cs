using TapeReelPacking.Source.Define;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Keyence.AutoID.SDK;
using TapeReelPacking.Source.Application;
using System.Diagnostics;
namespace TapeReelPacking.Source.Hardware
{
    using TapeReelPacking.Source.LogMessage;
    using System.IO;
    using TapeReelPacking.UI.UserControls.ViewModel;

    public class BarCodeReaderInterface:BaseVM
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


		public BarcodeSetting barcodeSetting = new BarcodeSetting();

		public BarCodeReaderInterface()
		{

			if (main == null)
				main = MainWindow.mainWindow;

			m_BarcodeReader = new BarCodeReaderView();
			updateConnectionStatus(false);

			string defaults = "127.0.0.1";
			barCodeipAddress = Application.Application.GetCommInfo("Barcode Comm::IpAddress", defaults);

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
					m_BarcodeReader.Save.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
					MainWindow.mainWindow.label_Barcode_Status.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
					MainWindow.mainWindow.label_Barcode_Status.Content = $"{m_reader.IpAddress}";
				}
				else
				{
					m_BarcodeReader.label_ReaderIP_Address.Content = $"{m_reader.IpAddress}";
					m_BarcodeReader.Save.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
					MainWindow.mainWindow.label_Barcode_Status.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
					MainWindow.mainWindow.label_Barcode_Status.Content = $"{m_reader.IpAddress}";

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

		int nDeviceID = 0;
		string nstrFolderBackup = "Trigger";
		public string GetBarCodeStringAndImage(out string strFullPathImageOut,  int nID, string strCurrentLot)
		{
			strFullPathImageOut = "";

			if (bIsDownload)
				return "";
			bIsDownload = true;
			string strDeviceID = "";
			//string str2 = "";

			string strImageFullName;


			string resp = m_reader.ExecCommand($"LON,0{barcodeSetting.brankID}");
			//if (resp.Length < 1)
			//{
   //             LogMessage.WriteToDebugViewer(3, $"Failed. Retry with next bank {barcodeSetting.brankID}: ");

   //             resp = m_reader.ExecCommand($"LON,0{barcodeSetting.brankID + 2}");
                
   //         }

            if (resp.Length > 0)
            {
                strDeviceID = resp.Replace("\r", "");
                strDeviceID = strDeviceID.Replace(":", "");
                strDeviceID = strDeviceID.Replace("-", "");

            }

            LogMessage.WriteToDebugViewer(3, "Message responsed from Barcode Bank 1: " + strDeviceID);
			//string resp2 = m_reader.ExecCommand("LON,02");
			//if (resp2.Length > 0)
			//{
			//	str2 = resp2.Replace("\r", "");
			//}
			Thread.Sleep(250);
			int nResult = -(int)ERROR_CODE.PASS;
			if(strDeviceID.Length < 1)
            {
				nResult = -(int)ERROR_CODE.PROCESS_ERROR;
			}

			strImageFullName = MainWindow.mainWindow.master.createImageFilePathToSave(nID, nResult, "Barcode", strCurrentLot, strDeviceID);
			strFullPathImageOut = strImageFullName;
			System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
			{
                if (!m_liveviewForm.IsHandleCreated)
                {
					m_liveviewForm.CreateControl();
					LogMessage.WriteToDebugViewer(3, "FAILED!!!!!!! Recreate Live View Control");

                }

                m_liveviewForm.DownloadRecentImage(strImageFullName);

                MainWindow.mainWindow.master.m_Tracks[1].m_imageViews[0].UpdateNewImageMono(strImageFullName);
			});
			//if (strDeviceID.Length < 1)
			//{
			//	m_reader.ExecCommand("LOFF");
			//}
			bIsDownload = false;
			return strDeviceID;
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

		public void CloseConnection()
        {
			m_reader.Disconnect();
        }


		public  void LoadBarcodeSetting()
		{
			string strRecipePath = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
			string fullpathCam = Path.Combine(strRecipePath, "BarcodeSetting.cfg");
			IniFile ini = new IniFile(fullpathCam);
			barcodeSetting.brankID = ini.ReadValue("Barcode Setting", "brank ID", "1");
			if (!Directory.Exists(strRecipePath))
			{
				Directory.CreateDirectory(strRecipePath);
				WriteBarcodeSetting();
			}
		}
		public  void WriteBarcodeSetting()
		{
			string strRecipePath = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
			string fullpathCam = Path.Combine(strRecipePath, "BarcodeSetting.cfg");
			IniFile ini = new IniFile(fullpathCam);
			ini.WriteValue("Barcode Setting", "brank ID", barcodeSetting.brankID);
			if (!Directory.Exists(strRecipePath))
			{
				Directory.CreateDirectory(strRecipePath);
				WriteBarcodeSetting();
			}
		}
	}
}
