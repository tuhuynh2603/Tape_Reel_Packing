﻿using TapeReelPacking.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Application = TapeReelPacking.Source.Application;
using System.Runtime.CompilerServices;
using TapeReelPacking.UI.UserControls.ViewModel;
using TapeReelPacking.UI.UserControls.View;
using TapeReelPacking.Source.Helper;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for MappingSetingUC.xaml
    /// </summary>
    /// 
    public partial class MappingSetingUC : UserControl
    {

        //private Dictionary<string, string> _dictMappingParam = new Dictionary<string, string>();
        public MappingSetingUC()
        {
            InitializeComponent();

        }


        //private Rectangles GetRectangles(string str)
        //{
        //    if (str == "")
        //        return new Rectangles(0, 0, 0, 0);
        //    string[] value = str.Split(':');
        //    if (value.Length == 4)
        //        return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]));
        //    else
        //        return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]), double.Parse(value[4]));

        //}
        //private List<int> ConverStringToList(string str)
        //{
        //    List<int> list = new List<int>();
        //    string[] value = str.Split(':');
        //    foreach (string s in value)
        //    {
        //        list.Add(int.Parse(s));
        //    }
        //    return list;
        //}

    }
}
