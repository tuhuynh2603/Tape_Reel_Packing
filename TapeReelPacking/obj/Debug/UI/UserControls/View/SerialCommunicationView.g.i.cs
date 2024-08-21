﻿#pragma checksum "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "FA617CFE11FB6D0A06FBF20F9334269F2B4D2D0FB6214A42C8FD2C146E94E604"
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
using System.Windows.Interactivity;
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
using TapeReelPacking.UI.UserControls.View;
using TapeReelPacking.UI.UserControls.ViewModel;


namespace TapeReelPacking.UI.UserControls.View {
    
    
    /// <summary>
    /// SerialCommunicationView
    /// </summary>
    public partial class SerialCommunicationView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 26 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ccbSerialComm;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ccbBauRate;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Disconnect;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Connect;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_SendLastLot;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_DataWrite;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Write;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_DataRead;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Read;
        
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
            System.Uri resourceLocater = new System.Uri("/TapeReelPacking;component/ui/usercontrols/view/serialcommunicationview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\UI\UserControls\View\SerialCommunicationView.xaml"
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
            this.ccbSerialComm = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.ccbBauRate = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.btn_Disconnect = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.btn_Connect = ((System.Windows.Controls.Button)(target));
            return;
            case 5:
            this.btn_SendLastLot = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.txt_DataWrite = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.btn_Write = ((System.Windows.Controls.Button)(target));
            return;
            case 8:
            this.txt_DataRead = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.btn_Read = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

