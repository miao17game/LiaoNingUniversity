using LNU.Core.Models;
using LNU.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Wallace.UWP.Helpers.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using static Wallace.UWP.Helpers.Tools.UWPStates;

namespace LNU.NET.Pages {
    
    public sealed partial class WebViewPage : Page {
        public WebViewPage() {
            this.InitializeComponent();
            Current = this;
            MainPage.DivideWindowRange(this, 800, (double?)SettingsHelper.ReadSettingsValue(SettingsSelect.SplitViewMode) ?? 0.6);
            if (VisibleWidth > 800 && !IsMobile)
                this.Margin = new Thickness(3, 0, 0, 0);
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            MainPage.Current.MainContentFrame.Content = null;
            MainPage.Current.HamburgerBox.SelectedIndex = 0;
            MainPage.ChangeTitlePath(2, null);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            if ((sender as Grid).ActualWidth > 800 && !IsMobile)
                this.Margin = new Thickness(3, 0, 0, 0);
            else
                this.Margin = new Thickness(0, 0, 0, 0);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            Scroll.Source = args.PathUri;
            if (args.MessageBag as string != null)
                navigateTitlePath.Text = args.MessageBag as string;
        }

        private void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {

        }

        private void Scroll_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {

        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        #region Properties
        public static WebViewPage Current;
        #endregion

    }
}
