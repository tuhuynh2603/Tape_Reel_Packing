﻿#pragma checksum "..\..\..\..\..\UI\UserControls\OutputLogView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "19E4DC8BFF0A765BC8E317AD5E8BA5A8F4C3B1EAD07DA9D5AE8E2BB066299ADC"
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
    /// OutputLogView
    /// </summary>
    public partial class OutputLogView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 64 "..\..\..\..\..\UI\UserControls\OutputLogView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.FlowDocumentScrollViewer flowDocumentSVOutputlog;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\..\..\..\UI\UserControls\OutputLogView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.FlowDocument outputLog;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\..\UI\UserControls\OutputLogView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SentDelete;
        
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
            System.Uri resourceLocater = new System.Uri("/TapeReelPacking;component/ui/usercontrols/outputlogview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\UI\UserControls\OutputLogView.xaml"
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
            this.flowDocumentSVOutputlog = ((System.Windows.Controls.FlowDocumentScrollViewer)(target));
            return;
            case 2:
            this.outputLog = ((System.Windows.Documents.FlowDocument)(target));
            return;
            case 3:
            this.SentDelete = ((System.Windows.Controls.MenuItem)(target));
            
            #line 80 "..\..\..\..\..\UI\UserControls\OutputLogView.xaml"
            this.SentDelete.Click += new System.Windows.RoutedEventHandler(this.SentDelete_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

