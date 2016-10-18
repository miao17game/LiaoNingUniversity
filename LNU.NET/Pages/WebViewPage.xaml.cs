using static Wallace.UWP.Helpers.Tools.UWPStates;

using LNU.Core.Models;
using LNU.Core.Tools;
using LNU.NET.Tools;
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

namespace LNU.NET.Pages {
    
    public sealed partial class WebViewPage : Page {
        public WebViewPage() {
            this.InitializeComponent();
            Current = this;
            InitPageState();
        }

        #region Events

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            MainPage.Current.MainContentFrame.Content = null;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {

        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if(args == null || args.PathUri == null) {
                ReportHelper.ReportError(GetUIString("WebViewLoadError"));
                return;
            }
            contentRing.IsActive = true;
            Scroll.Source = args.PathUri;
            if (args.MessageBag as string != null)
                navigateTitlePath.Text = args.MessageBag as string;
        }

        private void Scroll_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {

        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {
            
        }

        private void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            contentRing.IsActive = false;
        }

        #endregion

        #region Mehods

        private void InitPageState() {
            isDivideScreen = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsDivideScreen) ?? true;
            MainPage.DivideWindowRange(
                currentFramePage: this,
                divideNum: (double?)SettingsHelper.ReadSettingsValue(SettingsSelect.SplitViewMode) ?? 0.6,
                isDivideScreen: isDivideScreen);
        }

        #endregion

        #region Properties
        public static WebViewPage Current;
        private bool isDivideScreen = true;
        #endregion

    }
}
