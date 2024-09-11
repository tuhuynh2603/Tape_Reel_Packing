using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class RecipeManageVM:BaseVM
    {

        private Visibility _isVisible = Visibility.Collapsed;
        public Visibility isVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(isVisible));
            }
        }

        private string _m_strNewRecipe = "";
        public string m_strNewRecipe{
            get => _m_strNewRecipe;
            set {
                _m_strNewRecipe = value;
                OnPropertyChanged(nameof(m_strNewRecipe));
            } }

        public ICommand Cmd_AddNewRecipe { get; set; }

        public RecipeManageVM()
        {
            Cmd_AddNewRecipe =new RelayCommand<RecipeManageVM>((p) => { return true; },
                                     (p) =>
                                     {
                                         AddNewRecipe(m_strNewRecipe);
                                     });
            InitComboRecipe();
        }


        public void AddNewRecipe(string strTxt)
        {
            if (strTxt.Replace(" ", "") == "")
            {
                MessageBox.Show("This name can not be empty");
                return;
            }

            var recipes = comboRecipes.Where(recipe=> recipe.Contains(strTxt)).ToList();

            if (recipes.Count >0)
            {
                MessageBox.Show("This name has been used, please try with different name!");
                return;
            }

            string pathOldFile = Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe);
            string strFullPathRecipe = System.IO.Path.Combine(Source.Application.Application.pathRecipe, strTxt);
            Directory.CreateDirectory(strFullPathRecipe);
            CopyFolder(pathOldFile, Path.Combine(Source.Application.Application.pathRecipe, strTxt), strTxt, true);
        }



        public static void CopyFolder(string oldPathFolder, string newPathFolder, string strNewRecipeName, bool isHoldName = false, bool isChild = false, bool hasExtension = false)
        {
            string[] oldfiles = Directory.GetFiles(oldPathFolder);
            string[] oldfolders = Directory.GetDirectories(oldPathFolder);
            foreach (string oldFolder in oldfolders)
            {
                string namefolder = oldFolder.Split('\\').Last();
                string newFolder = System.IO.Path.Combine(newPathFolder, namefolder);
                Directory.CreateDirectory(newFolder);
                if (namefolder == "Data" || isChild)
                    CopyFolder(oldFolder, newFolder, strNewRecipeName, true, true, true);
                else
                    CopyFolder(oldFolder, newFolder, strNewRecipeName, true, false, true);
            }
            foreach (string oldFile in oldfiles)
            {
                CopyFile(oldFile, newPathFolder, strNewRecipeName, isHoldName, hasExtension);
            }
        }
        public static void CopyFile(string oldfile, string newPathfile, string strNewRecipeName, bool isHoldName = false, bool hasExtension = false)
        {
            string namefile = System.IO.Path.GetFileName(oldfile);
            if (isHoldName)
            {
                if (hasExtension)
                    namefile = Path.GetFileName(oldfile);
                string newfile = System.IO.Path.Combine(newPathfile, namefile);
                if (!File.Exists(newfile))
                    File.Copy(oldfile, newfile);
            }
            else
            {
                if (Char.IsDigit(namefile.Last()))
                {
                    string newfile = System.IO.Path.Combine(newPathfile, string.Format("{0}{1}{2}",
                                                            strNewRecipeName, namefile.Last(), System.IO.Path.GetExtension(oldfile)));
                    if (!File.Exists(newfile))
                        File.Copy(oldfile, newfile);
                }
                else
                {
                    string newfile = System.IO.Path.Combine(newPathfile, string.Format("{0}{1}",
                                                             strNewRecipeName, System.IO.Path.GetExtension(oldfile)));
                    if (!File.Exists(newfile))
                        File.Copy(oldfile, newfile);
                }
            }
        }


        private object _comboRecipeSelectedItem;

        public object comboRecipeSelectedItem
        {
            get => _comboRecipeSelectedItem;
            set
            {
                _comboRecipeSelectedItem = value;
                OnPropertyChanged(nameof(comboRecipeSelectedItem));
            }
        }


        private ActionCommand deleteRecipeCommand;

        public ICommand DeleteRecipeCommand
        {
            get
            {
                if (deleteRecipeCommand == null)
                {
                    deleteRecipeCommand = new ActionCommand(DeleteRecipe);
                }

                return deleteRecipeCommand;
            }
        }

        private void DeleteRecipe()
        {
        }

        private ActionCommand loadRecipeCommand;

        public ICommand LoadRecipeCommand
        {
            get
            {
                if (loadRecipeCommand == null)
                {
                    loadRecipeCommand = new ActionCommand(LoadRecipe);
                }

                return loadRecipeCommand;
            }
        }

        private void LoadRecipe()
        {
            MainWindowVM.master.LoadRecipe(comboRecipeSelectedItem.ToString());
        }

        private ActionCommand closeRecipeCommand;

        public ICommand CloseRecipeCommand
        {
            get
            {
                if (closeRecipeCommand == null)
                {
                    closeRecipeCommand = new ActionCommand(CloseRecipe);
                }

                return closeRecipeCommand;
            }
        }

        private void CloseRecipe()
        {
        }

        private ObservableCollection<string> _comboRecipes = new ObservableCollection<string>();

        public ObservableCollection<string> comboRecipes { get => _comboRecipes; set => SetProperty(ref _comboRecipes, value); }

        public void InitComboRecipe()
        {
            if (Source.Application.Application.pathRecipe == null)
                return;
            string[] oldfolders = Directory.GetDirectories(Source.Application.Application.pathRecipe);
            for (int n = 0; n < oldfolders.Length; n++)
            {
                string strRight = oldfolders[n].Replace(Source.Application.Application.pathRecipe, "");
                string[] strSplits = strRight.Split('\\');
                string strName = strSplits[1];
                if (strName == "")
                    continue;
                comboRecipes.Add(strName);
            }
            comboRecipeSelectedItem = comboRecipes.Where(recipe => recipe == Source.Application.Application.currentRecipe).FirstOrDefault();

        }
    }
}
