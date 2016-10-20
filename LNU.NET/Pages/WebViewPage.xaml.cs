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
#endregion

namespace LNU.NET.Pages {
    
    public sealed partial class WebViewPage : Page {

        #region Constructor

        public WebViewPage() {
            this.InitializeComponent();
            Current = this;
            InitPageState();
        }

        #endregion

        #region Events

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            MainPage.Current.MainContentFrame.Content = null;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            MainPage.Current.SetChildPageMargin(
                currentPage: this, 
                matchNumber: VisibleWidth,
                isDivideScreen: isDivideScreen);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if(args == null || args.PathUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            contentRing.IsActive = true;
            currentUri = args.PathUri;
            thisPageType = args.DataType;
            if (args.MessageBag as string != null) // if there is a title to be saved, save it.
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;
            if (thisPageType == DataFetchType.LNU_Index_Login) { // if need login, do something...
                SetVisibility(Scroll, false);
                if (!MainPage.LoginCache.IsInsert || IsMoreThan30Minutes ( MainPage.LoginCache.CacheMiliTime, DateTime.Now ))  // need to login.
                    SetVisibility(MainPopupGrid, true);
                else { // don not need to login but only get the login-message from mainpage.
                    UserName.Text = MainPage.LoginCache.UserName;
                    UserID.Text = MainPage.LoginCache.UserID;
                    UserDepartment.Text = MainPage.LoginCache.UserDepartment;
                    UserCourse.Text = MainPage.LoginCache.UserCourse;
                    UserTime.Text = MainPage.LoginCache.UserTime;
                    UserIP.Text = MainPage.LoginCache.UserIP;
                    SetVisibility(StatusGrid, true);
                    return; // if don not need to login, must return here or the events will go wrong!
                }
            }
            Scroll.Source = currentUri;
        }

        private void LoginPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            contentGrid.Width = (sender as Popup).ActualWidth;
            contentGrid.Height = (sender as Popup).ActualHeight;
        }

        #region Web Events

        /// <summary>
        /// for log-out action completed check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Scroll_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
            Scroll.NavigationCompleted -= Scroll_NavigationCompleted;
            ReportHelper.ReportAttention(GetUIString("LogOut_Success"));
            isFristNavigate = true;
            MainPage.Current.NavigateToBase?.Invoke(
                this,
                new NavigateParameter { DataType = DataFetchType.LNU_Index_Login, MessageBag = navigateTitle, PathUri = currentUri },
                MainPage.InnerResources.GetFrameInstance(NavigateType.Webview),
                typeof(WebViewPage));
        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {
            
        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        /// <summary>
        /// handled when webview loaded successfully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            contentRing.IsActive = false;
            await SetLoginPageStateIfNeed();
        }

        /// <summary>
        /// receive message when the js in the webview send message to window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScriNotifypt(object sender, NotifyEventArgs e) {
            Scroll.ScriptNotify -= OnScriNotifypt;
            Submit.IsEnabled = Abort.IsEnabled = true;
            SubitRing.IsActive = false;
            CheckIfLoginSucceed(JsonHelper.FromJson<string[]>(e.Value)[1]);
        }

        #endregion

        #region Focus Changed
        private void EmailBox_GotFocus(object sender, RoutedEventArgs e) {
            EmailBorderness.BorderBrush = Application.Current.Resources["ENRZForeground02"] as Brush;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness.BorderBrush = Application.Current.Resources["ENRZForeground02"] as Brush;
        }

        private void EmailBox_LostFocus(object sender, RoutedEventArgs e) {
            EmailBorderness.BorderBrush = Application.Current.Resources["AppScrollViewerForeground02"] as Brush;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e) {
            PasswordBorderness.BorderBrush = Application.Current.Resources["AppScrollViewerForeground02"] as Brush;
        }
        #endregion

        /// <summary>
        /// send login-command to the target web or apis.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Submit_Click(object sender, RoutedEventArgs e) {
            Submit.IsEnabled = Abort.IsEnabled = false;
            SubitRing.IsActive = true;
            var user = EmailBox.Text;
            var pass = PasswordBox.Password;

            //// Protect a message to the local user.
            //IBuffer buffProtected = await this.ProtectAsync(
            //    pass,
            //    userForPasswordLocal,
            //    BinaryStringEncoding.Utf8);

            SettingsHelper.SaveSettingsValue(SettingsConstants.Email, user);
            //SettingsHelper.SaveSettingsValue(SettingsConstants.PasswordDisqus, buffProtected.ToArray());

            //if(new Regex("{d}"))
            await InsertLoginMessage(user, pass);
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e) {
            Scroll.NavigationCompleted += Scroll_NavigationCompleted;
            MainPage.LoginCache.IsInsert = false;
            Scroll.Source = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bks_login2.Logout");
        }

        private void PasswordCheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void AutoLoginCheckBox_Checked(object sender, RoutedEventArgs e) {

        }

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
        /// well....you can know what i am doing by the name of the method......
        /// </summary>
        /// <param name="oldTime"></param>
        /// <param name="newTime"></param>
        /// <returns></returns>
        private bool IsMoreThan30Minutes(DateTime oldTime, DateTime newTime) {
            return 
                newTime.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds - 
                oldTime.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds >= 
                1800;
        }

        /// <summary>
        /// Init login-page state, and if need, fetch the message of login-action whether action succed or not.
        /// </summary>
        /// <returns></returns>
        private async Task SetLoginPageStateIfNeed() {
            if (thisPageType == DataFetchType.LNU_Index_Login && isFristNavigate) {

                // DO ASYNC WORK ... MAYBE AWAIT

                EmailBox.Text = (string)SettingsHelper.ReadSettingsValue(SettingsConstants.Email) ?? "";
                
                Scroll.ScriptNotify += OnScriNotifypt;
            }
            try {
                if (isFristNavigate)
                    return;
                var js = @"window.external.notify(
                                    JSON.stringify(
                                        new Array (
                                            document.body.innerText,
                                            document.body.innerHTML)));";
                await Scroll.InvokeScriptAsync("eval", new[] { js });
                isFristNavigate = false;
            } catch {
                Debug.WriteLine("Error");
                ReportHelper.ReportAttention(GetUIString("UnhandledError"));
            } finally {
                isFristNavigate = false;
            }
        }

        /// <summary>
        /// if login failed, re-navigate to the target Uri, otherwise, show status detail of you.
        /// </summary>
        /// <param name="htmlBodyContent">html of websites</param>
        private void CheckIfLoginSucceed(string htmlBodyContent) {
            var doc = new HtmlDocument();
            doc.LoadHtml(@"<html>
                                             <head>
                                             <title>......</title >
                                             <link href='style.css' rel='stylesheet' type='text/css'>
                                             <script language='JavaScript1.2' src='nocache.js'></script >
                                             </head><body>" + htmlBodyContent + "</body></html>");
            var rootNode = doc.DocumentNode;
            var studentStatus = rootNode.SelectSingleNode("//span[@class='t']");
            if (studentStatus == null) {
                ReportHelper.ReportAttention(GetUIString("Login_Failed"));
                isFristNavigate = true;
                //Scroll.Source = currentUri;
                //Scroll.Refresh();
                MainPage.Current.NavigateToBase?.Invoke(
                    this,
                    new NavigateParameter { DataType = DataFetchType.LNU_Index_Login, MessageBag = navigateTitle, PathUri = currentUri },
                    MainPage.InnerResources.GetFrameInstance(NavigateType.Webview),
                    typeof(WebViewPage));
            } else {
                SaveLoginStatus(studentStatus);
                SetVisibility(StatusGrid, true);
                SetVisibility(MainPopupGrid, false);
                LoginPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// save status message in MainPage to be controlled.
        /// </summary>
        /// <param name="item"></param>
        private void SaveLoginStatus(HtmlNode item) {
            var message = item.InnerText.Replace(" ", "@").Replace(",", "@");
            var mess = message.Split('@');
            ReportHelper.ReportAttention(GetUIString("Login_Success"));
            var stringColl = mess[2].Replace("(", "@").Replace(")", "@").Split('@');
            MainPage.LoginCache.IsInsert = true;
            MainPage.LoginCache.UserName = UserName.Text = stringColl[0];
            MainPage.LoginCache.UserID = UserID.Text = stringColl[1];
            MainPage.LoginCache.UserDepartment = UserDepartment.Text = mess[0].Substring(1, mess[0].Length - 1);
            MainPage.LoginCache.UserCourse = UserCourse.Text = mess[1].Substring(0, mess[1].Length - 2);
            MainPage.LoginCache.UserTime = UserTime.Text = mess[3] + GetUIString("TimeAnoutation");
            MainPage.LoginCache.UserIP = UserIP.Text = new Regex("\n").Replace(mess[4].Substring(5, mess[4].Length - 5), "");
            MainPage.LoginCache.CacheMiliTime = DateTime.Now;
        }

        #region Founctions Cab
        /// <summary>
        /// show the popup of id and password
        /// </summary>
        /// <param name="ID">your cache id</param>
        /// <param name="Password">your cache password</param>
        private void ShowAccountCache(string ID, string Password) {
            EmailBox.Text = ID ?? "";
            PasswordBox.Password = Password ?? "";
        }

        /// <summary>
        /// insert id and password into webview from popup
        /// </summary>
        /// <param name="user">your cache id</param>
        /// <param name="pass">your cache password</param>
        /// <returns></returns>
        private async Task InsertLoginMessage(string user, string pass) {
            try {
                var newJSFounction = $@"
                            var node_list = document.getElementsByTagName('input');
                                for (var i = 0; i < node_list.length; i++) {"{"}
                                var node = node_list[i];
                                    if (node.getAttribute('type') == 'submit') 
                                        node.click();
                                    if (node.getAttribute('type') == 'text') 
                                        node.innerText = '{user}';
                                    if (node.getAttribute('type') == 'password') 
                                        node.innerText = '{pass}';
                                {"}"} ";
                await Scroll.InvokeScriptAsync("eval", new[] { newJSFounction });
            } catch (Exception) {
                Submit.IsEnabled = Abort.IsEnabled = true;
                SubitRing.IsActive = false;
                ReportHelper.ReportAttention("Error");
            }
        }
        #endregion

        #endregion

        #region Properties
        public static WebViewPage Current;
        private bool isDivideScreen = true;
        private const string userForPasswordLocal = "LOCAL=user";
        private DataFetchType thisPageType;
        private bool isFristNavigate = true;
        private string navigateTitle;
        private Uri currentUri;
        #endregion

    }
}
