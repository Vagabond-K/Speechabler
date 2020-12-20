using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Speechabler
{
    public static class Dialog
    {
        public static bool GetIsOk(Button obj) => (bool)obj.GetValue(IsOkProperty);

        public static void SetIsOk(Button obj, bool value) => obj.SetValue(IsOkProperty, value);

        public static readonly DependencyProperty IsOkProperty =
            DependencyProperty.RegisterAttached("IsOk", typeof(bool), typeof(Dialog), new PropertyMetadata(false, OnIsOkChanged));

        private static void OnIsOkChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is Button button
                && eventArgs.NewValue is bool newValue)
            {
                if (newValue)
                    button.Click += OkButtonClick;
                else
                    button.Click -= OkButtonClick;
            }
        }

        private static void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var window = Window.GetWindow(button);
                if (window != null)
                    window.DialogResult = true;
            }
        }
    }
}
