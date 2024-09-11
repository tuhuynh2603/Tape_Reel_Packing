using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TapeReelPacking.Source.Behaviors
{
    public static class FocusBehavior
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = d as TextBox;
            if (passwordBox != null && (bool)e.NewValue)
            {
                passwordBox.Loaded += (sender, args) => ((TextBox)sender).Focus();
            }
        }

    }
}
