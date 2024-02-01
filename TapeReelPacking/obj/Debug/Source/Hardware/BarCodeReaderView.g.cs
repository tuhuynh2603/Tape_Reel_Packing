﻿#pragma checksum "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "09961234F05FDB89E76B3A6EDE2BBBEED482411DD52A11E7DBEE502E3C16D15F"
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


namespace TapeReelPacking.Source.Hardware {
    
    
    /// <summary>
    /// BarCodeReaderView
    /// </summary>
    public partial class BarCodeReaderView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 24 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label_ReaderIP_Address;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Connect;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox combo_commandSendToBarCode;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button_Trigger;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label_DataReceived;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button_Clear;
        
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
            System.Uri resourceLocater = new System.Uri("/TapeReelPacking;component/source/hardware/barcodereaderview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
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
            this.label_ReaderIP_Address = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.Connect = ((System.Windows.Controls.Button)(target));
            
            #line 25 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
            this.Connect.Click += new System.Windows.RoutedEventHandler(this.Connect_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.combo_commandSendToBarCode = ((System.Windows.Controls.ComboBox)(target));
            
            #line 37 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
            this.combo_commandSendToBarCode.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.combo_commandSendToBarCode_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.button_Trigger = ((System.Windows.Controls.Button)(target));
            
            #line 38 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
            this.button_Trigger.Click += new System.Windows.RoutedEventHandler(this.button_Trigger_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.label_DataReceived = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.button_Clear = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\..\Source\Hardware\BarCodeReaderView.xaml"
            this.button_Clear.Click += new System.Windows.RoutedEventHandler(this.button_Clear_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
