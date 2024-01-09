using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.Source.Application;
using Application = Magnus_WPF_1.Source.Application.Application;
using Magnus_WPF_1.Source.LogMessage;

namespace Magnus_WPF_1.UI.UserControls
{
	public partial class LoginUser : UserControl

	{
		public DataTable tableAccount = new DataTable();
		private MainWindow main;
		public LoginUser()
		{
			InitializeComponent();
		}
		public void AssignMainWindow()
		{
			if (main == null)
				main = MainWindow.mainWindow;
		}
		private void SetupAccount()
		{
			DataColumn column;

			//DataRow row;
			column = new DataColumn();
			column.ColumnName = "username";
			column.DataType = typeof(string);
			column.Unique = true;
			tableAccount.Columns.Add(column);

			column = new DataColumn();
			column.ColumnName = "access";
			column.DataType = typeof(AccessLevel);
			column.Unique = false;
			tableAccount.Columns.Add(column);

			column = new DataColumn();
			column.ColumnName = "password";
			column.DataType = typeof(string);
			column.Unique = false;
			tableAccount.Columns.Add(column);
		}
		#region USER AND PASSWORD
		private string EncryptPass(string password)
		{

			string enscryptPassword = "";
			password += "ADMN";
			foreach (char c in password)
			{
				enscryptPassword += (char)(c - 15);
			}

			return enscryptPassword;
		}
		private bool CheckUsername(string username)
		{
			if (username != "")
			{
				foreach (DataRow row in tableAccount.Rows)
				{
					if (row["username"].ToString() == username)
						return true;
				}
			}
			return false;
		}
		public AccessLevel GetAccessLevel(string username)
		{
			foreach (DataRow row in tableAccount.Rows)
			{
				if (row["username"].ToString() == username)
					return (AccessLevel)row["access"];
			}
			return AccessLevel.None;
		}
		private string GetPassword(string username)
		{
			foreach (DataRow row in tableAccount.Rows)
			{
				if (row["username"].ToString() == username)
                {
					string strPass = row["password"].ToString();
					return strPass;

				}
			}
			return null;
		}
		string AccessLevelString(AccessLevel accessLevel)
		{
			switch (accessLevel)
			{
				case AccessLevel.Engineer:
					return "Engineer";
				case AccessLevel.Operator:
					return "Operator";
				case AccessLevel.User:
					return "User";
				default:
					return "None";
			}
		}
		public bool ChangePasswordDataTable(string username, string newPassword)
		{
			foreach (DataRow row in tableAccount.Rows)
			{
				if (row["username"].ToString() == username)
				{
					row["password"] = EncryptPass(newPassword);
					SaveLogAccount();
					return true;
				}
			}
			return false;
		}
		public void SaveLogAccount()
		{
			List<string> listaccounts = new List<string>();
			listaccounts.Add("[NUM USER]");
			listaccounts.Add(string.Format("NoOfUsers={0}", tableAccount.Rows.Count));
			listaccounts.Add("");
			int i = 0;
			foreach (DataRow row in tableAccount.Rows)
			{
				listaccounts.Add(string.Format("[User{0}]", i++));
				listaccounts.Add(string.Format("Name={0}", row["username"]));
				listaccounts.Add(string.Format("Level={0}", AccessLevelString((AccessLevel)row["access"])));
				listaccounts.Add(string.Format("Pswd={0}", row["password"]));
				listaccounts.Add("");
			}

			string pathFile = System.IO.Path.Combine(Application.pathRecipe, "LogAccount.lgn");
			if (!File.Exists(pathFile))
				return;
			using (StreamWriter Files = new StreamWriter(pathFile))
			{
				foreach (string line in listaccounts)
					Files.WriteLine(line);
			}
			//DebugMessage.WriteToDebugViewer(0, "Save Log Account file");
		}
		private AccessLevel GetCheckedRadionbutton()
		{
			if (engineerLevel.IsChecked == true)
				return AccessLevel.Engineer;
			else if (operatorLevel.IsChecked == true)
				return AccessLevel.Operator;
			else if (userLevel.IsChecked == true)
				return AccessLevel.User;
			else
				return AccessLevel.None;
		}
		private DataRow GetRow(string username)
		{
			foreach (DataRow row in tableAccount.Rows)
			{
				if (row["username"].ToString() == username)
					return row;
			}
			return null;
		}

		void ResetTextBox()
		{
			// For Login
			userName.Text = "USERNAME"; userName.Foreground = Brushes.Gray;
			passWord.Password = "1111111"; passWord.Foreground = Brushes.Gray;

			// For create new account
			userNameNew.Text = "USERNAME"; userNameNew.Foreground = Brushes.Gray;
			passWordNew.Password = "1111111"; passWordNew.Foreground = Brushes.Gray;
			ConfirmPassWordNew.Password = "1111111"; ConfirmPassWordNew.Foreground = Brushes.Gray;

			// For change password
			NewPassWord.Password = "1111111"; NewPassWord.Foreground = Brushes.Gray;
			ConfirmNewPassWord.Password = "1111111"; ConfirmNewPassWord.Foreground = Brushes.Gray;

			NotifyLogin.Text = "";
			NotifyNewUser.Text = "";
			NotifyChangePw.Text = "";
		}
		public void KeyShortcut(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					if (Panel.GetZIndex(panelLogIn) == 2 || Panel.GetZIndex(panelLogIn) == 3 && !loginOk.IsFocused)
						Login_Click(sender, e);
					else if (Panel.GetZIndex(panelCreateUser) == 2 && !createOk.IsFocused)
						CreateNewUser_Click(sender, e);
					else if (Panel.GetZIndex(panelChangePassword) == 2 && !chagneOk.IsFocused)
						ChangePW_Click(sender, e);
					break;
				default:
					break;
			}
		}
		public class ConvertColorToBool : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return (Brush)value == Brushes.Gray ? false : true;
			}
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Brushes.Gray;
			}
		}
		#endregion

		#region EVENT CLICK BUTTON

		private void Login_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string pw = "";
			if (userName.Foreground == Brushes.Gray && userName.Text == "")
			{
				NotifyLogin.Text = "Type your Username !";
				return;
			}
			if (passWord.Foreground == Brushes.Gray)
				pw = EncryptPass("");
			else pw = EncryptPass(passWord.Password);
			if (!CheckUsername(userName.Text))
			{
				NotifyLogin.Text = "Username is wrong !";
				return;
			}
			else if (pw == GetPassword(userName.Text))
			{
				main.IsFisrtLogin = true;
				//main.AddHotKey();
				MainWindow.accountUser = userName.Text;
				MainWindow.accessLevel = GetAccessLevel(userName.Text);

				MainWindow.UICurrentState = UISTate.IDLE_STATE;
				//for (int itrack = 0; itrack < Source.Application.Application.m_nTrack; itrack++)
				//{
				//	if (!main.master.m_Tracks[itrack].isAvailable)
				//	{
				//		MainWindow.UICurrentState = UISTate.IDLE_NOCAM_STATE;
				//		break;
				//	}
				//}
				currentUser.Content = userName.Text;
				currentAccesslevel.Content = AccessLevelString(GetAccessLevel(userName.Text));
				LogMessage.WriteToDebugViewer(0, string.Format("Login to user '{0}' Success", userName.Text));
				ResetTextBox();

				//main.ChangeUIState();
				main.btnLogIn.IsChecked = false;
				main.btnLogIn.IsEnabled = true;
				main.btnLogIn.Content = MainWindow.accountUser.ToString();
				main.acessLevel.Text = MainWindow.accessLevel.ToString();
				ResetTextBox();
				main.enableButton(true);
				//main.RegistryPreference();
				//main.master.inspectionParameter.InitReferenceBoxSize();

				//bool isFullCam = true;
				//for (int itrack = 0; itrack < Application.Application.num_track; itrack++)
				//{
				//	if (!main.master.trackSF[itrack].isAvailable)
				//	{
				//		isFullCam = false;
				//		break;
				//	}		
				//}
				//if (isFullCam)
				//	CommPLC.PlcCommMode = PLCCommMode.PLC_ONLINE;

				//else
				//	CommPLC.PlcCommMode = PLCCommMode.PLC_SIMULATOR;

				//main.master.commPLC.isVisionReady = true;
				//main.master.commPLC.UpdateSentMessageToPLC((int)VisionProcess.VISION_READY);

				//KeepHeartBeat();
				return;
			}
			else
			{
				LogMessage.WriteToDebugViewer(0, string.Format("Login to user '{0}' Fail, Password is wrong.", userName.Text));
				NotifyLogin.Text = "Password is wrong !";
				return;
			}

		}
		//private System.Windows.Threading.DispatcherTimer dispatcherTimer;
		//private int time = 0;
		//private void KeepHeartBeat()
		//{
		//	dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
		//	dispatcherTimer.Tick += new EventHandler(DoCountTimeOut);
		//	dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
		//	dispatcherTimer.Start();
		//	main.master.commPLC.UpdateSentMessageToPLC((int)VisionProcess.VISION_HEART_BEAT);
		//}
		//private void DoCountTimeOut(object sender, EventArgs e)
		//{
		//	time += 1;
		//	if (main.master.commPLC.isHeatBeatError)
		//	{
		//		if (time == 1)
		//		{
		//			main.master.commPLC.UpdateSentMessageToPLC((int)VisionProcess.VISION_HEART_BEAT);
		//		}
		//		else if (time == 2)
		//		{
		//			//MessageBox.Show("Connection error", "Heart Beat Status", MessageBoxButton.OK, MessageBoxImage.Information);
		//			time = 0;
		//			dispatcherTimer.Stop();
		//			dispatcherTimer.Start();
		//			main.master.commPLC.UpdateSentMessageToPLC((int)VisionProcess.VISION_HEART_BEAT);
		//		}
		//	}
		//	else
		//	{
		//		time = 0;
		//		dispatcherTimer.Stop();
		//		dispatcherTimer.Start();
		//		main.master.commPLC.UpdateSentMessageToPLC((int)VisionProcess.VISION_HEART_BEAT);
		//	}
		//	main.master.commPLC.isHeatBeatError = true;
		//}
		private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			int currentTabIndex = main.tab_controls.SelectedIndex;
			main.btnLogIn.IsChecked = false;
			main.btnLogIn.IsEnabled = true;
			if (main.IsFisrtLogin)
			{
				//main.AddHotKey();
				//if (main.master.trackSF[0].isAvailable)
				//{
				//	main.tabRibbon.IsEnabled = true;

				//	main.tabItem_View.IsEnabled = true;
				//}
				//else
				//{
				//	main.tabRibbon.IsEnabled = true;
				//	main.tabItem_View.IsEnabled = true;
				//}
			}
			main.tab_controls.SelectedIndex = currentTabIndex;
			ResetTextBox();
		}
		private void ChangePW_Click(object sender, System.Windows.RoutedEventArgs e)
		{

			if (NewPassWord.Foreground == Brushes.Gray) NewPassWord.Password = "";
			if (ConfirmNewPassWord.Foreground == Brushes.Gray) ConfirmNewPassWord.Password = "";
			if (NewPassWord.Password != ConfirmNewPassWord.Password)
			{
				NotifyChangePw.Text = "Password and Confirm password is not match !\nCheck again.";
				return;
			}
			else
			{
				if (NewPassWord.Foreground == Brushes.Gray) NewPassWord.Password = "";
				string newpass = NewPassWord.Password;
				ResetTextBox();
				if (ChangePasswordDataTable(MainWindow.accountUser, newpass))
				{
					NotifyChangePw.Text = "Password is changed !";
					LogMessage.WriteToDebugViewer(0, string.Format("User '{0}' changed Password", MainWindow.accountUser));
				}
				else
					NotifyChangePw.Text = "Password can not change !";
			}
		}

		private void CreateNewUser_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (passWordNew.Foreground == Brushes.Gray) passWordNew.Password = "";
			if (ConfirmPassWordNew.Foreground == Brushes.Gray) ConfirmPassWordNew.Password = "";
			if (userNameNew.Foreground == Brushes.Gray || userNameNew.Text == "")
			{
				NotifyNewUser.Text = "Type Username !";
				return;
			}

			if (CheckUsername(userNameNew.Text))
			{
				NotifyNewUser.Text = "Username is exist! Create another Username";
				return;
			}
			if (passWordNew.Password != ConfirmPassWordNew.Password)
			{
				NotifyNewUser.Text = "Password and Confirm password is not match !\n Check again.";
				return;
			}

			DataRow row = tableAccount.NewRow();
			row["username"] = userNameNew.Text;
			row["password"] = EncryptPass(passWordNew.Password);
			row["access"] = GetCheckedRadionbutton();
			tableAccount.Rows.Add(row);
			LogMessage.WriteToDebugViewer(0, string.Format("Create new Account '{0}' Success", userName.Text));
			SaveLogAccount();
			ResetTextBox();
			NotifyNewUser.Text = string.Format("Created new account Username: '{0}',\nAccess Level: {1}", row["username"], AccessLevelString((AccessLevel)row["access"]));


		}

		#endregion

		#region EVENT MOUSEDOWN
		private void logInMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//main.CleanHotKey();
			//foreach (COMMAND_CODE cmd in Master.cmdCode)
			//{
			//	if (cmd != COMMAND_CODE.IDLE)
			//	{
			//		if (MessageBox.Show("Can not Login Software, Software is busy", "Log In", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
			//		{
			//			if (main.master.trackSF[0].isAvailable)
			//			{
			//				MainWindow.UICurrentState = UISTate.IDLE_STATE;
			//			}
			//			else
			//			{
			//				MainWindow.UICurrentState = UISTate.IDLE_NOCAM_STATE;

			//			}
			//			main.ChangeUIState();
			//			main.btnLogIn.IsChecked = false;
			//			main.btnLogIn.IsEnabled = true;

			//			return;
			//		}
			//	}
			//}
			InitLogInDialog();
		}

		public void InitLogInDialog()
        {
			panelLogIn.IsEnabled = true;
			panelLogIn.Visibility = Visibility.Visible;

			panelChangePassword.IsEnabled = false;
			panelChangePassword.Visibility = Visibility.Collapsed;

			panelCreateUser.IsEnabled = false;
			panelCreateUser.Visibility = Visibility.Collapsed;

            Panel.SetZIndex(panelLogIn, 2);
            Panel.SetZIndex(panelChangePassword, 0);
            Panel.SetZIndex(panelCreateUser, 0);
            ResetTextBox();
			userName.Focus();
			//userName.IsTabStop = true;
		}

		private void NewUserMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//main.CleanHotKey();
			if (MainWindow.accountUser != "None")
			{
                Panel.SetZIndex(panelLogIn, 0);
                Panel.SetZIndex(panelChangePassword, 0);
                Panel.SetZIndex(panelCreateUser, 2);

                panelLogIn.IsEnabled = false ;
				panelLogIn.Visibility = Visibility.Collapsed;

				panelChangePassword.IsEnabled = false;
				panelChangePassword.Visibility = Visibility.Collapsed;

				panelCreateUser.IsEnabled = true;
				panelCreateUser.Visibility = Visibility.Visible;

				ResetTextBox();
				userNameNew.Focus();
				engineerLevel.IsEnabled = true;
				operatorLevel.IsEnabled = true;
				userLevel.IsEnabled = true;
				if (MainWindow.accessLevel == AccessLevel.Engineer)
				{
					engineerLevel.IsEnabled = true;
					engineerLevel.IsChecked = true;
				}
				else if (MainWindow.accessLevel == AccessLevel.Operator)
				{
					engineerLevel.IsEnabled = false;
					operatorLevel.IsChecked = true;
				}
				else if (MainWindow.accessLevel == AccessLevel.User)
				{
					engineerLevel.IsEnabled = false;
					operatorLevel.IsEnabled = false;
					userLevel.IsChecked = true;
				}
			}
			else
			{
				MessageBox.Show("Log in before create new account", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
		private void ChangePWMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//main.CleanHotKey();
			if (MainWindow.accountUser != "None")
			{
                Panel.SetZIndex(panelLogIn, 0);
                Panel.SetZIndex(panelChangePassword, 2);
                Panel.SetZIndex(panelCreateUser, 0);
                panelLogIn.IsEnabled = false;
				panelLogIn.Visibility = Visibility.Collapsed;

				panelChangePassword.IsEnabled = true;
				panelChangePassword.Visibility = Visibility.Visible;

				panelCreateUser.IsEnabled = false;
				panelCreateUser.Visibility = Visibility.Collapsed;

				ResetTextBox();
				NewPassWord.Focus();
			}
			else
			{
				MessageBox.Show("Log in before change password", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
		private void DeleteUserMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (MainWindow.accountUser != "None")
			{
				//foreach (COMMAND_CODE cmd in Master.cmdCode)
				//{
				//	if (cmd != COMMAND_CODE.IDLE)
				//	{
				//		if (MessageBox.Show("Can not Delete current account, Software is busy", "Delete current account", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
				//		{
				//			main.btnLogIn.IsChecked = false;
				//			main.btnLogIn.IsEnabled = true;
				//			return;
				//		}
				//	}
				//}
				if (MessageBox.Show(string.Format("Are you sure delete this account '{0}'", MainWindow.accountUser), "Information", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
				{

					MainWindow.accessLevel = GetAccessLevel(MainWindow.accountUser);
					string pw = GetPassword(MainWindow.accountUser);
					if (MainWindow.accountUser == "Engineer" && MainWindow.accessLevel == AccessLevel.Engineer && pw == Source.Application.Application.PwsDefault)
					{
						MessageBox.Show("Can not Delete this Acount", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
					else
					{
						if (MainWindow.accessLevel != (int)AccessLevel.None)
						{
							tableAccount.Rows.Remove(GetRow(MainWindow.accountUser));
							SaveLogAccount();
							MainWindow.UICurrentState = UISTate.LOGOUT_STATE;
							//main.ChangeUIState();
							LogMessage.WriteToDebugViewer(0, string.Format("Delete user '{0}'", currentUser.Content));
							currentUser.Content = "None";
							currentAccesslevel.Content = "None";
							main.btnLogIn.IsChecked = false;
							main.btnLogIn.IsEnabled = true;
							main.btnLogIn.Label = currentUser.Content.ToString();
							main.acessLevel.Text = currentAccesslevel.Content.ToString();
						}
						main.IsFisrtLogin = false;
					}
				}
				else
				{
					int currentTabIndex = main.tab_controls.SelectedIndex;
					//if (main.master.trackSF[0].isAvailable)
					//{

					//	main.tab_Production.IsEnabled = true;
					//	main.tab_Offline.IsEnabled = true;
					//	main.tab_Hardware.IsEnabled = true;
					//	main.tab_View.IsEnabled = true;
					//	main.tab_Parameter.IsEnabled = true;
					//}
					//else
					//{
					//	main.tab_Production.IsEnabled = true;
					//	main.tab_Offline.IsEnabled = true;
					//	main.tab_Hardware.IsEnabled = false;
					//	main.tab_View.IsEnabled = true;
					//	main.tab_Parameter.IsEnabled = true;
					//}
					main.btnLogIn.IsChecked = false;
					main.btnLogIn.IsEnabled = true;
					//main.AddHotKey();
					main.tab_controls.SelectedIndex = currentTabIndex;
				}
			}
			else
			{
				MessageBox.Show("Log in before delete account", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
		private void LogOutMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (MainWindow.accountUser != "None")
			{
				//foreach (COMMAND_CODE cmd in Master.cmdCode)
				//{
				//	if (cmd != COMMAND_CODE.IDLE)
				//	{
				//		if (MessageBox.Show("Can not Logout Software, Software is busy", "Log Out", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
				//		{
				//			main.btnLogIn.IsChecked = false;
				//			main.btnLogIn.IsEnabled = true;
				//			ResetTextBox();
				//			return;
				//		}
				//	}
				//}
				if (MessageBox.Show("Log Out This Account ?", "Question ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{

					currentUser.Content = "None";
					currentAccesslevel.Content = "None";
						
					main.IsFisrtLogin = false;
					//main.CleanHotKey();
					MainWindow.UICurrentState = UISTate.LOGOUT_STATE;
					//main.ChangeUIState();
					LogMessage.WriteToDebugViewer(0, string.Format("Loged out of user '{0}'", currentUser.Content));
					currentUser.Content = "None";
					currentAccesslevel.Content = "None";
					main.acessLevel.Text = "None";
					main.btnLogIn.Label = "None";
					main.btnLogIn.Content = "None";
					main.btnLogIn.IsChecked = false;
					main.btnLogIn.IsEnabled = true;
					main.enableButton(false);


				}
				ResetTextBox();

				//else
				//{
				//	int currentTabIndex = main.tab_controls.SelectedIndex;
				//	if (main.master.trackSF[0].isAvailable)
				//	{
				//		main.tab_Production.IsEnabled = true;
				//		main.tab_Offline.IsEnabled = true;
				//		main.tab_Hardware.IsEnabled = true;
				//		main.tab_View.IsEnabled = true;
				//		main.tab_Parameter.IsEnabled = true;
				//	}
				//	else
				//	{
				//		main.tab_Production.IsEnabled = true;
				//		main.tab_Offline.IsEnabled = true;
				//		main.tab_Hardware.IsEnabled = false;
				//		main.tab_View.IsEnabled = true;
				//		main.tab_Parameter.IsEnabled = true;
				//	}
				//	main.btnLogIn.IsChecked = false;
				//	main.btnLogIn.IsEnabled = true;
				//	main.AddHotKey();
				//	ResetTextBox();
				//	main.tab_controls.SelectedIndex = currentTabIndex;
				//}
			}
		}
		#endregion

		#region ANIMATION USER AND PASSWORD
		private void PWGotFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			var obj = sender as PasswordBox;
			if (obj != null && obj.Foreground == Brushes.Gray)
			{
				obj.Password = "";
				obj.Foreground = Brushes.Black;
			}
		}
		private void PWLostFoucs(object sender, System.Windows.RoutedEventArgs e)
		{
			var obj = sender as PasswordBox;
			if (obj != null && obj.Password == "")
			{
				obj.Password = "1111111111";
				obj.Foreground = Brushes.Gray;
			}
		}
		private void TextBoxLostFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			var obj = sender as TextBox;
			if (obj != null && obj.Text == "")
			{
				obj.Text = "USERNAME";
				obj.Foreground = Brushes.Gray;
			}
		}
		private void TextboxGotFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			var obj = sender as TextBox;
			if (obj != null && obj.Foreground == Brushes.Gray)
			{
				obj.Text = "";
				obj.Foreground = Brushes.Black;
			}
		}
		#endregion

	}

}
