﻿#pragma checksum "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "0DF5457E761B88CEB13A99D6BD5513087E892D23889A23CFCA9851C2A08A68FB"
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
using TapeReelPacking.Source.Hardware;


namespace TapeReelPacking.Source.Hardware {
    
    
    /// <summary>
    /// PLCCOMM
    /// </summary>
    public partial class PLCCOMM : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 27 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label_PLC_IPAddress;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button_PLC_Connect;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox combo_PLC_Comm_Function;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox text_MemoryAdress;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox text_MemoryAdress_Status;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_SendToPLC;
        
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
            System.Uri resourceLocater = new System.Uri("/TapeReelPacking;component/source/hardware/plccomm.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
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
            this.label_PLC_IPAddress = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.button_PLC_Connect = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
            this.button_PLC_Connect.Click += new System.Windows.RoutedEventHandler(this.button_PLC_Connect_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.combo_PLC_Comm_Function = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.text_MemoryAdress = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.text_MemoryAdress_Status = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.btn_SendToPLC = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\..\..\Source\Hardware\PLCCOMM.xaml"
            this.btn_SendToPLC.Click += new System.Windows.RoutedEventHandler(this.btn_SendToPLC_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

