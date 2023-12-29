using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Magnus_WPF_1.UI.UserControls.ViewModel
{
    public class RecipeManageVM
    {

        public string m_strNewRecipe = "";
        private string _m_strNewRecipe
        { get; set; }
        public CommandBase Cmd_AddNewRecipe { get; set; }

        public RecipeManageVM()
        {
            Cmd_AddNewRecipe = new CommandBase(AddNewRecipe);
        }


        public static void AddNewRecipe(string strTxt)
        {
            if (strTxt.Replace(" ", "") == "")
            {
                MessageBox.Show("This name can not be empty");
                return;
            }

            if (RecipeManageView.m_dict_Recipes.ContainsKey(strTxt))
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


    }
}
