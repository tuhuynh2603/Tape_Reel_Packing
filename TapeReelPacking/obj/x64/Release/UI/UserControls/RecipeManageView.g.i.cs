﻿#pragma checksum "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "7B56D1673E7BDC5B74D9AB9B0BFD09AC2A1A0C46205371288A32404F0D55C2A3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TapeReelPacking.UI.UserControls;


namespace TapeReelPacking.UI.UserControls {
    
    
    /// <summary>
    /// RecipeManageView
    /// </summary>
    public partial class RecipeManageView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox combo_Recipe_Name;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Delete_Recipe;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Load_Recipe;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_New_Recipe_Name;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Add_New_Recipe;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Close_Recipe_UC;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TapeReelPacking;component/ui/usercontrols/recipemanageview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.combo_Recipe_Name = ((System.Windows.Controls.ComboBox)(target));
            
            #line 14 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
            this.combo_Recipe_Name.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.combo_Recipe_Name_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.btn_Delete_Recipe = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
            this.btn_Delete_Recipe.Click += new System.Windows.RoutedEventHandler(this.btn_Delete_Recipe_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btn_Load_Recipe = ((System.Windows.Controls.Button)(target));
            
            #line 19 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
            this.btn_Load_Recipe.Click += new System.Windows.RoutedEventHandler(this.btn_Load_Recipe_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.txt_New_Recipe_Name = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.btn_Add_New_Recipe = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.btn_Close_Recipe_UC = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\..\..\UI\UserControls\RecipeManageView.xaml"
            this.btn_Close_Recipe_UC.Click += new System.Windows.RoutedEventHandler(this.btn_Close_Recipe_UC_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
