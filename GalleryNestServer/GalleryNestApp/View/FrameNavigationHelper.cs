using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GalleryNestApp.View
{
    public static class FrameNavigationHelper
    {
        public static readonly DependencyProperty DisableNavigationProperty =
            DependencyProperty.RegisterAttached(
                "DisableNavigation",
                typeof(bool),
                typeof(FrameNavigationHelper),
                new PropertyMetadata(false, OnDisableNavigationChanged));

        public static bool GetDisableNavigation(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisableNavigationProperty);
        }

        public static void SetDisableNavigation(DependencyObject obj, bool value)
        {
            obj.SetValue(DisableNavigationProperty, value);
        }

        private static void OnDisableNavigationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Frame frame)
            {
                frame.Navigating -= Frame_Navigating;
                if ((bool)e.NewValue)
                {
                    frame.Navigating += Frame_Navigating;
                }
            }
        }

        private static void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.Refresh)

            {
                e.Cancel = true;
            }
            else
            {
                Console.WriteLine("meow");
            }
        }
    }
}
