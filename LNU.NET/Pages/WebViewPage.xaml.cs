#region Using
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
using System.Threading.Tasks;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using System.Text;
using System.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using LNU.NET.Controls;
#endregion

namespace LNU.NET.Pages {

    public sealed partial class WebViewPage : BaseContentPage {

        #region Constructor

        public WebViewPage() {
            this.InitializeComponent();
            Current = this;
            InitPageState();
        }

        #endregion

        #region Events

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            MainPage.Current.SetChildPageMargin(
                currentPage: this,
                matchNumber: VisibleWidth,
                isDivideScreen: isDivideScreen);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if (args == null || args.ToUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            if (args.MessageBag as string != null)
                navigateTitlePath.Text = args.MessageBag as string;
            contentRing.IsActive = true;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;
            thisNaviType = args.NaviType;
            Scroll.Source = currentUri;

            //// wait fo developing...
            //MainPage.ReLoginIfStatusIsInvalid(currentUri, thisPageType, navigateTitlePath.Text, thisNaviType);
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        #region Web Events

        private void Scroll_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {

        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        private async void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            contentRing.IsActive = false;
            Scroll.ScriptNotify += OnNotify;
            await AskWebViewToCallback();
        }

        private void OnNotify(object sender, NotifyEventArgs e) {
            Scroll.ScriptNotify -= OnNotify;
            var result = JsonHelper.FromJson<string[]>(e.Value);
            result.ToList().ForEach(i => Debug.WriteLine(i + "\n#################\n"));
        }

        #endregion

        #endregion

        #region Methods

        private void InitPageState() {
            isDivideScreen = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsDivideScreen) ?? true;
            MainPage.DivideWindowRange(
                currentFramePage: this,
                divideNum: (double?)SettingsHelper.ReadSettingsValue(SettingsSelect.SplitViewMode) ?? 0.6,
                isDivideScreen: isDivideScreen);
        }

        /// <summary>
        /// Open methods to change state when the theme mode changed.
        /// </summary>
        public static void ChangeStateByRequestTheme() {

        }

        /// <summary>
        /// send message to windows so that we can get message of login-success whether or not.
        /// </summary>
        /// <returns></returns>
        private async Task AskWebViewToCallback() { // js to callback
            try {
                var js = @"window.external.notify(
                                    JSON.stringify(
                                        new Array (
                                            document.body.innerText,
                                            document.body.innerHTML)));";
                await Scroll.InvokeScriptAsync("eval", new[] { js });
            } catch { Debug.WriteLine("JS Error"); }
        }

        #endregion

        #region Properties and state
        public static WebViewPage Current;
        private bool isDivideScreen = true;
        private DataFetchType thisPageType;
        private NavigateType thisNaviType;
        private Uri currentUri;
        #endregion

    }
}
