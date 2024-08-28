using TapeReelPacking.Source.Algorithm;
using TapeReelPacking.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Application = TapeReelPacking.Source.Application;
using TapeReelPacking.UI.UserControls.ViewModel;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;
using TapeReelPacking.Source.Repository;

namespace TapeReelPacking
{
    /// <summary>
    /// Interaction logic for TeachParametersUC.xaml
    /// </summary>
    public partial class TeachParametersUC : UserControl
    {
        //private Dictionary<string, string> _dictTeachParam = new Dictionary<string, string>();
        //int m_nTrackSelected = 0;
        //public CategoryTeachParameter categoriesTeachParam = new CategoryTeachParameter();
        //public int nDefectROIIndex = 0;

        public TeachParametersUC(CategoryTeachParameterService service)
        {
            InitializeComponent();
            DataContext = new TeachParameterVM(service);
            //this.DataContext = new TeachParameterVM();
            //TeachParameterVM teachparameterVM = (TeachParameterVM)this.DataContext;
            //pgr_PropertyGrid_Teach.DataContext = Application.Application.categoriesTeachParam;

            //TeachParameterVM.UpdatePropertyGridAction = () => { int a = 1; };
            //pgr_PropertyGrid_Teach.Update();
            //this.DataContext = Application.Application.categoriesTeachParam;
        }

        //private void pgr_PropertyGrid_Teach_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        //{
        //    // Access old and new values directly
        //    var oldValue = e.OldValue;
        //    var newValue = e.NewValue;

        //    // Check if event arguments are null and handle accordingly
        //    if (e == null)
        //    {
        //        MessageBox.Show("Event arguments are null.");
        //        return;
        //    }

        //    // Optional: Notify the ViewModel if needed
        //    var viewModel = DataContext as TeachParameterVM;
        //    viewModel?.OnPropertyChanged(e);  // Call ViewModel method with event args
        //}


        //public bool UpdateTeachParamFromDictToUI(Dictionary<string, string> dictTeachParam)
        //{
        //    if (dictTeachParam == null)
        //        return false;
        //    //_dictTeachParam = dictTeachParam;
        //    object category = Application.Application.categoriesTeachParam;
        //    //object category_local = categoriesTeachParam;

        //    bool bSuccess = Application.Application.UpdateParamFromDictToUI(dictTeachParam, ref category);
        //    Application.Application.categoriesTeachParam = (CategoryTeachParameter)category;
        //    //categoriesTeachParam = (CategoryTeachParameter)category_local;
        //    //pgr_PropertyGrid_Teach.Update();

        //    return bSuccess;
        //}

        //private void track_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (m_nTrackSelected != track_ComboBox.SelectedIndex)
        //        m_nTrackSelected = track_ComboBox.SelectedIndex;
        //    {
        //        ReloadTeachParameterUI(m_nTrackSelected, nDefectROIIndex);
        //    }

        //}

        //public void ReloadTeachParameterUI(int nTrack, int nArea = 0)
        //{
        //    Application.Application.dictTeachParam.Clear();
        //    Application.Application.LoadTeachParamFromFileToDict(ref nTrack);
        //    //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
        //    UpdateTeachParamFromDictToUI(Application.Application.dictTeachParam);

        //    ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
        //    pgr_PropertyGrid_Teach.Update();

        //}



        //public void ReloadAreaParameterUI(int nTrack, int nArea = 0)
        //{
        //    Application.Application.dictTeachParam.Clear();
        //    Application.Application.LoadAreaParamFromFileToDict(ref nTrack, nDefectROIIndex);
        //    //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
        //    UpdateTeachParamFromDictToUI(Application.Application.dictPVIAreaParam[nDefectROIIndex]);
        //    pgr_PropertyGrid_Teach.Update();

        //}

        //private void pgr_PropertyGrid_Teach_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        //{
        //    if (nDefectROIIndex != (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex)
        //    {
        //        nDefectROIIndex = (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex;
        //        ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
        //    }

        //}



        ////public void LoadTeachParamFromUIToDict()
        ////{
        ////    System.Reflection.PropertyInfo[] infos = Application.Application.categoriesTeachParam.GetType().GetProperties();
        ////    //System.Reflection.PropertyInfo[] infosApp = Application.Application.categoriesTeachParam.GetType().GetProperties();


        ////    for (int i = 0; i < infos.Length; i++)
        ////    {
        ////        var attributes = infos[i].GetCustomAttributes(typeof(BrowsableAttribute), true);
        ////        bool isBrowsable = true;
        ////        if (attributes.Length > 0)
        ////        {
        ////            BrowsableAttribute browseAttr = (BrowsableAttribute)attributes[0];
        ////            isBrowsable = browseAttr.Browsable;
        ////            //Console.WriteLine($"Is MyProperty browsable? {isBrowsable}");
        ////        }

        ////        if (isBrowsable == true)
        ////        {
        ////            //infosApp[i].SetValue(Application.Application.categoriesTeachParam, infos[i].GetValue(categoriesTeachParam));
        ////            Type type = infos[i].PropertyType;
        ////            _dictTeachParam[infos[i].Name] = infos[i].GetValue(Application.Application.categoriesTeachParam).ToString();
        ////        }
        ////    }
        ////}

        //private void btn_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    SaveParameterTeachDefault();
        //}


        //public bool SaveParameterTeachDefault()
        //{
        //    try
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;
        //        //LoadTeachParamFromUIToDict();
        //        MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
        //        mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
        //        mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore((int)Application.Application.categoriesTeachParam.DR_DefectROIIndex);       
        //        mainWindow.master.WriteTeachParam(m_nTrackSelected);
        //        Application.Application.LoadTeachParamFromFileToDict(ref m_nTrackSelected);
        //        Application.Application.LoadAreaParamFromFileToDict(ref m_nTrackSelected, (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex);
        //        pgr_PropertyGrid_Teach.Update();

        //        Mouse.OverrideCursor = null;
        //        //mainWindow.teach_parameters_btn.IsChecked = false;
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        //private void btn_Cancel_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    UpdateTeachParamFromDictToUI(Application.Application.dictTeachParam);
        //    pgr_PropertyGrid_Teach.Update();

        //    MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
        //    mainWindow.teach_parameters_btn.IsChecked = false;
        //}

    }
}
