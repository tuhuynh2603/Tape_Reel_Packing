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

namespace TapeReelPacking.UI.UserControls.View
{
    public partial class TeachParametersUC : UserControl
    {

        public TeachParametersUC()
        {
            InitializeComponent();
            //DataContext = new TeachParameterVM(service);
        }

    }
}
