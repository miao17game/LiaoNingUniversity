using static Wallace.UWP.Helpers.Tools.UWPStates;

using LNU.NET.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Wallace.UWP.Helpers.Helpers;
using LNU.Core.Tools;
using System.Diagnostics;
using LNU.NET.Tools;
using LNU.Core.Models;
using HtmlAgilityPack;

namespace LNU.NET.Pages.FeaturesPages {
   
    public sealed partial class ChangePassPage : BaseContentPage {
        public ChangePassPage() {
            this.InitializeComponent();
            InitPageState();
        }

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
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        private async void Submit_Click(object sender, RoutedEventArgs e) {
            var oldPass = PB_Old.Password;
            var newPass = PB_New.Password;
            var confirm = PB_Recofirm.Password;

            await InsertLoginMessage(oldPass, newPass, confirm);
        }

        #region WebView Events

        private async void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            contentRing.IsActive = false;
            Scroll.ScriptNotify += OnNotify;
            await AskWebViewToCallback();
        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        private void OnNotify(object sender, NotifyEventArgs e) {
            Scroll.ScriptNotify -= OnNotify;
            var result = JsonHelper.FromJson<string[]>(e.Value);

            var doc = new HtmlDocument();
            doc.LoadHtml(@"<html>
                                             <head>
                                             <title>......</title >
                                             <link href='style.css' rel='stylesheet' type='text/css'>
                                             <script language='JavaScript1.2' src='nocache.js'></script >
                                             </head><body>" + result[1] + "</body></html>");
            var rootNode = doc.DocumentNode;

            if (isFirstLoaded) 
                RedirectToLoginIfNeed(rootNode);
            else 
                ChangePasswordAndReport(rootNode);
        }

        #endregion

        #region Focus Events

        private void PasswordBox01_GotFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness01.BorderBrush = Application.Current.Resources["ENRZForeground02"] as Brush;
        }

        private void PasswordBox01_LostFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness01.BorderBrush = Application.Current.Resources["AppScrollViewerForeground02"] as Brush;
        }

        private void PasswordBox02_GotFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness02.BorderBrush = Application.Current.Resources["ENRZForeground02"] as Brush;
        }

        private void PasswordBox02_LostFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness02.BorderBrush = Application.Current.Resources["AppScrollViewerForeground02"] as Brush;
        }

        private void PasswordBox03_GotFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness03.BorderBrush = Application.Current.Resources["ENRZForeground02"] as Brush;
        }

        private void PasswordBox03_LostFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness03.BorderBrush = Application.Current.Resources["AppScrollViewerForeground02"] as Brush;
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

        private void RedirectToLoginIfNeed(HtmlNode rootNode) {
            var changePostForm = rootNode.SelectSingleNode("//form[@method='POST']");
            if (changePostForm == null) {
                ReportHelper.ReportAttention(GetUIString("Login_First"));
                MainPage.ReLoginIfStatusIsInvalid(currentUri, thisPageType, navigateTitlePath.Text, thisNaviType);
            } else {
                // do something? still have no idea......
            }
            isFirstLoaded = false;
        }

        private void ChangePasswordAndReport(HtmlNode rootNode) {
            var requestResult = rootNode.SelectSingleNode("//span[@class='t']");
            if (requestResult != null) {
                switch (requestResult.InnerText.Substring(1, requestResult.InnerText.Length - 2)) {
                    case "新密码输入不一致!":
                        isFirstLoaded = true;
                        Scroll.Source = currentUri;
                        PB_New.Password = "";
                        PB_Recofirm.Password = "";
                        ReportHelper.ReportAttention(GetUIString("Login_Reconfirm_Failed"));
                        break;
                    case "密码输入有误!":
                        isFirstLoaded = true;
                        Scroll.Source = currentUri;
                        ReportHelper.ReportAttention(GetUIString("Login_Pass_Input_Error"));
                        break;
                    case "你已经改变了你的密码!":
                        PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        ReportHelper.ReportAttention(GetUIString("Login_Pass_Changed"));
                        break;
                    default:
                        isFirstLoaded = true;
                        Scroll.Source = currentUri;
                        ReportHelper.ReportAttention(GetUIString("Login_Uhandled_Error"));
                        break;
                }
            } else {
                // e...oh...
            }
        }

        #region JS Founctions Cab

        /// <summary>
        /// insert id and password into webview from popup, and after that, click the submit button.
        /// </summary>
        /// <param name="user">your cache id</param>
        /// <param name="pass">your cache password</param>
        /// <returns></returns>
        private async Task InsertLoginMessage(string oldPass, string newPass, string cofirm) {
            try { // insert js and run it, so that we can insert message into the target place and click the submit button.
                var js001 = $@"document.getElementsByName('p_oldpass')[0].innerText = '{oldPass}' ;";
                await Scroll.InvokeScriptAsync("eval", new[] { js001 });
                var js002 = $@"document.getElementsByName('p_newpass1')[0].innerText = '{newPass}' ;";
                await Scroll.InvokeScriptAsync("eval", new[] { js002 });
                var js003 = $@"document.getElementsByName('p_newpass2')[0].innerText = '{cofirm}' ;";
                await Scroll.InvokeScriptAsync("eval", new[] { js003 });
                var newJSFounction = $@"
                            var node_list = document.getElementsByTagName('input');
                                for (var i = 0; i < node_list.length; i++) {"{"}
                                var node = node_list[i];
                                    if (node.getAttribute('type') == 'submit') 
                                        node.click();
                                {"}"} ";
                await Scroll.InvokeScriptAsync("eval", new[] { newJSFounction });
            } catch (Exception e) { // if any error throws, reset the UI and report errer.
                Submit.IsEnabled = true;
                SubitRing.IsActive = false;
                ReportHelper.ReportAttention("Error");
                Debug.WriteLine(e.StackTrace);
            }
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
            } catch {
                Debug.WriteLine("JS Error");
            } 
        }

        #endregion

        #endregion

        #region Properties
        private bool isFirstLoaded = true;
        private bool isDivideScreen = true;
        private DataFetchType thisPageType;
        private NavigateType thisNaviType;
        private Uri currentUri;
        #endregion

    }
}
