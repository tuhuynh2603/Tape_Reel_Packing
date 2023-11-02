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

			DataRow row;
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
					return row["password"].ToString();
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
		}
		private System.Windows.Threading.DispatcherTimer dispatcherTimer;
		private int time = 0;
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

		}
		private void ChangePW_Click(object sender, System.Windows.RoutedEventArgs e)
		{


		}

		private void CreateNewUser_Click(object sender, System.Windows.RoutedEventArgs e)
		{


		}

		#endregion

		#region EVENT MOUSEDOWN
		private void logInMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
		}
		private void NewUserMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{

		}
		private void ChangePWMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
		}
		private void DeleteUserMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{

		}
		private void LogOutMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
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
