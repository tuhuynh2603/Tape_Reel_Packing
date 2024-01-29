using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace TapeReelPacking.UI.UserControls
{
    /// <summary>
    /// Interaction logic for RecipeManageView.xaml
    /// </summary>
    public partial class RecipeManageView : UserControl
    {
        public static Dictionary<string, int> m_dict_Recipes = new Dictionary<string, int>();
        public RecipeManageView()
        {
            InitializeComponent();
            DataContext = new ViewModel.RecipeManageVM();
        }

        public void InitComboRecipe()
        {
            if (Source.Application.Application.pathRecipe == null)
                return;
            string[] oldfolders = Directory.GetDirectories(Source.Application.Application.pathRecipe);
            m_dict_Recipes.Clear();
            for (int n = 0; n < oldfolders.Length; n++)
            {
                string strRight = oldfolders[n].Replace(Source.Application.Application.pathRecipe, "");
                string[] strSplits = strRight.Split('\\');
                string strName = strSplits[1];
                if (strName == "")
                    continue;
                //if (!m_dict_Recipes.ContainsKey(strName))
                //{
                    m_dict_Recipes.Add(strName, n);
                    combo_Recipe_Name.Items.Add(strName);
                //}
            }

            for (int n = 0; n < combo_Recipe_Name.Items.Count; n++)
                if (combo_Recipe_Name.Items[n].ToString() == Source.Application.Application.currentRecipe)
                    combo_Recipe_Name.SelectedIndex = n;
        }

        private void combo_Recipe_Name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_Load_Recipe_Click(object sender, RoutedEventArgs e)
        {

            MainWindow.mainWindow.master.LoadRecipe(combo_Recipe_Name.SelectedItem.ToString());
        }

        private void btn_Delete_Recipe_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void btn_Add_New_Recipe_Click(object sender, RoutedEventArgs e)
        //{
        //    AddNewRecipe(txt_New_Recipe_Name.Text);
        //}
        private void btn_Close_Recipe_UC_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.mainWindow.Close
        }

    }
}
