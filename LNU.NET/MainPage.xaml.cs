#region Using
using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.Core.Tools.LNUWebProcess;
using static LNU.NET.MainPage.InnerResources;

using LNU.Core.Models;
using LNU.Core.Models.NavigationModel;
using LNU.NET.Controls;
using LNU.NET.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Wallace.UWP.Helpers.Helpers;
using Wallace.UWP.Helpers.Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using LNU.NET.Pages.FeaturesPages;
using Windows.Web.Http;
using LNU.Core.Tools;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Diagnostics;
using HtmlAgilityPack;
using LNU.NET.Tools;
using System.Text.RegularExpressions;
#endregion

namespace LNU.NET {

    public sealed partial class MainPage : Page {

        #region Constructor
        public MainPage() {
            this.InitializeComponent();
            Current = this;
            baseListRing.IsActive = true;
            PrepareFrame.Navigate(typeof(PreparePage));
            SetControlAccessEnabled();
            InitMainPageState();
            GetResources();
        }

        #endregion

        #region Events
        private void NavigationSplit_PaneClosed(SplitView sender, object args) {
            SetVisibility(SlideAnimaRec, true);
            OutBorder.Completed += OnOutBorderOut;
            OutBorder.Begin();
        }

        private void OnOutBorderOut(object sender, object e) {
            OutBorder.Completed -= OnOutBorderOut;
            SetVisibility(DarkDivideBorder, false);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e) {
            if (ContentFrame.Content == null) {
                if (!isNeedClose) { InitCloseAppTask(); } else { Application.Current.Exit(); }
                e.Handled = true;
                return;
            } else {
                if (ReLoginPopup.IsOpen) {
                    ReLoginPopup.IsOpen = false;
                } else {
                    if (WebViewPage.Current != null) {
                        WebViewPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        WebViewPage.Current = null;
                    }
                    if (ChangePassPage.Current != null) {
                        ChangePassPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        ChangePassPage.Current = null;
                    }
                    if (LoginPage.Current != null) {
                        LoginPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        LoginPage.Current = null;
                    }
                    if (SchedulePage.Current != null) {
                        SchedulePage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        SchedulePage.Current = null;
                    }
                    if (SimpleDataPage.Current != null) {
                        SimpleDataPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        SimpleDataPage.Current = null;
                    }
                    if (ContentPage.Current != null)
                        MainContentFrame.Content = null;
                }
            }
            e.Handled = true;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            OpenOrClosePane();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            BasePartFrame.Navigate(typeof(SettingsPage));
            HamburgerListBox.SelectedIndex = -1;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            ImagePopup.Width = (sender as Grid).ActualWidth;
            ImagePopup.Height = (sender as Grid).ActualHeight;
        }

        private void ImagePopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            ImagePopupBorder.Width = (sender as Popup).ActualWidth;
            ImagePopupBorder.Height = (sender as Popup).ActualHeight;
        }

        private void HamburgerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            baseListRing.IsActive = false;
            var model = e.AddedItems.FirstOrDefault() as NavigationBar;
            NavigationSplit.IsPaneOpen = false;
            if (model == null)
                return;
            if (model.NaviType != NavigateType.Content && model.NaviType != NavigateType.Webview)
                ChangeTitlePath(2, (sender as ListBox).SelectedIndex == 0 ? null : model.Title);
            if (IfContainsPageInstance(NaviPathTitle.RoutePath)) {
                GetFrameInstance(model.NaviType).Content = GetPageInstance(NaviPathTitle.RoutePath);
                return;
            }
            NavigateToBase?.Invoke(
                sender,
                new NavigateParameter { ToUri = model.PathUri, MessageBag = model.Title, ToFetchType = model.FetchType, NaviType = NavigateType.Webview},
                GetFrameInstance(model.NaviType),
                GetPageType(model.NaviType));
        }

        #endregion

        #region Methods

        #region Methods for this

        private void InitMainPageState() {
            NavigateManager.BackRequested += OnBackRequested;
            var isDarkOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsDarkThemeOrNot) ?? true;
            RequestedTheme = isDarkOrNot ? ElementTheme.Dark : ElementTheme.Light;
            ChangeTitlePath(1, navigateTitlePath.Text);
            StatusBarInit.InitInnerDesktopStatusBar(true);
            Window.Current.SetTitleBar(BasePartBorder);
            LoginClient = UnRedirectHttpClient;
            IfNeedAdapteVitualNavigationBar();
            InitSlideRecState();
            AutoLoginIfNeed();
        }

        /// <summary>
        /// redirect to login when login-error throws.
        /// </summary>
        private void RedirectToLoginAgain() {
            NavigateToBase?.Invoke(
                this,
                new NavigateParameter {
                    ToFetchType = DataFetchType.Index_ReLogin,
                    MessageBag = GetUIString("LNU_Index_LS"),
                    ToUri = new Uri(LoginPath),
                    NaviType = NavigateType.ReLogin
                },
                GetFrameInstance(NavigateType.ReLogin),
                typeof(LoginPage));
        }

        private void SetControlAccessEnabled() {
            HamburgerBox = this.HamburgerListBox;
            MainContentFrame = this.ContentFrame;
            ReLoginPopupFrame = this.LoginPopupFrame;
            BaseListRing = this.baseListRing;
            ReLoginPopup = this.ImagePopup;
            NavigateTitlePath = this.navigateTitlePath;
        }

        private void InitCloseAppTask() {
            isNeedClose = true;
            new ToastSmooth(GetUIString("ClickTwiceToExit")).Show();
            Task.Run(async () => {
                await Task.Delay(2000);
                isNeedClose = false;
            });
        }

        private void IfNeedAdapteVitualNavigationBar() {
            if (IsMobile) {
                AppView.VisibleBoundsChanged += (s, e) => { AdapteVitualNavigationBarWithoutStatusBar(this); };
                AdapteVitualNavigationBarWithoutStatusBar(this);
            }
        }

        private void GetResources() {
            NaviBarResouces.Source = HamburgerResList;
        }


        /// <summary>
        /// Start the dark animation when hamburger menu opened.
        /// </summary>
        private void OnPaneIsOpened() {
            SetVisibility(DarkDivideBorder, true);
            EnterBorder.Begin();
        }

        #region Login Methods

        /// <summary>
        /// login whwn app start, of course, if need.
        /// </summary>
        private async void AutoLoginIfNeed() {
            if ((bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsAutoLogin) ?? false) {
                var user = SettingsHelper.ReadSettingsValue(SettingsConstants.Email) as string;
                var pass = PasswordDecryption();
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass)) {
                    var loginReturn = await PostLNULoginCallback(LoginClient, user, pass);
                    if (loginReturn != null) {
                        var doc = new HtmlDocument();
                        if (loginReturn.HtmlResouces == null) { // login failed, redirect to the login page.
                            ReportHelper.ReportAttention(GetUIString("Login_Failed"));
                            SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
                            RedirectToLoginAgain();
                            return;
                        }
                        doc.LoadHtml(loginReturn.HtmlResouces);
                        var rootNode = doc.DocumentNode;
                        var studentStatus = rootNode.SelectSingleNode("//span[@class='t']");
                        if (studentStatus == null) { // login failed, redirect to the login page.
                            ReportHelper.ReportAttention(GetUIString("Login_Failed"));
                            SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
                            RedirectToLoginAgain();
                            return;
                        } else { // login successful, save status and do nothing.
                            SaveLoginStatus(studentStatus, loginReturn.CookieBag.FirstOrDefault());
                        }
                    } else
                        ReportHelper.ReportAttention(GetUIString("Internet_Failed"));
                }
            }
        }

        /// <summary>
        /// save status message in MainPage to be controlled.
        /// </summary>
        /// <param name="item"></param>
        public void SaveLoginStatus(HtmlNode item, HttpCookie cookie) {
            var message = item.InnerText.Replace(" ", "@").Replace(",", "@");
            var mess = message.Split('@');
            var stringColl = mess[2].Replace("(", "@").Replace(")", "@").Split('@');
            LoginCache.IsInsert = true;
            LoginCache.UserName = stringColl[0];
            LoginCache.UserID = stringColl[1];
            LoginCache.UserDepartment = mess[0].Substring(1, mess[0].Length - 1);
            LoginCache.UserCourse  = mess[1].Substring(0, mess[1].Length - 2);
            LoginCache.UserTime =  mess[3] + GetUIString("TimeAnoutation");
            LoginCache.UserIP =  new Regex("\n").Replace(mess[4].Substring(5, mess[4].Length - 5), "");
            LoginCache.CacheMiliTime = DateTime.Now;
            LoginCache.Cookie = cookie;
        }

        #endregion

        #region Password Decryption

        private string PasswordDecryption() {
            try { // password decryption over here.
                var Password = SettingsHelper.ReadSettingsValue(SettingsConstants.Password) as byte[];
                if (Password != null) { // init ibuffer vector and cryptographic key for decryption.
                    SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
                    cryptographicKey = objAlg.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(CipherEncryptionHelper.CollForKeyAndIv));
                    ibufferVector = CryptographicBuffer.CreateFromByteArray(CipherEncryptionHelper.CollForKeyAndIv);

                    return CipherEncryptionHelper.CipherDecryption( // decryption the message.
                        SymmetricAlgorithmNames.AesCbcPkcs7,
                        CryptographicBuffer.CreateFromByteArray(Password),
                        ibufferVector,
                        BinaryStringEncoding.Utf8,
                        cryptographicKey);
                }
                return null;
            } catch (Exception e) { // if any error throws, clear the password cache to prevent more errors.
                Debug.WriteLine(e.StackTrace);
                SettingsHelper.SaveSettingsValue(SettingsConstants.Password, null);
                return null;
            }
        }

        #endregion

        #endregion

        #region Global methods

        public static bool IsNeedLoginOrNot { get { return !LoginCache.IsInsert || IsMoreThan30Minutes(LoginCache.CacheMiliTime, DateTime.Now); } }

        /// <summary>
        /// well....you can know what i am doing by the name of the method......
        /// </summary>
        /// <param name="oldTime"></param>
        /// <param name="newTime"></param>
        /// <returns></returns>
        private static bool IsMoreThan30Minutes(DateTime oldTime, DateTime newTime) {
            return
                newTime.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds -
                oldTime.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds >=
                1800;
        }

        /// <summary>
        /// Change the page layout by the settings item : "Divide Screen Mode"
        /// </summary>
        /// <param name="currentFramePage">current child page instance</param>
        /// <param name="rangeNum">default number of the range to divide, is 800</param>
        /// <param name="divideNum">the percent value of divide</param>
        /// <param name="defaultDivide">defalut percent value, is 0.6</param>
        /// <param name="isDivideScreen">make sure if need to divide screen</param>
        public static void DivideWindowRange(
            Page currentFramePage, 
            double divideNum ,
            double rangeNum = 800,
            double defaultDivide = 0.6 , 
            bool isDivideScreen = true) {

            Current.SetChildPageMargin(  currentPage: currentFramePage, matchNumber: VisibleWidth, isDivideScreen: isDivideScreen);

            if (IsMobile) {
                currentFramePage.Width = VisibleWidth;
                Current.Frame.SizeChanged += (sender, args) => { currentFramePage.Width = VisibleWidth; };
            } else {
                if (!isDivideScreen) {
                    currentFramePage.Width = VisibleWidth;
                    Current.Frame.SizeChanged += (sender, args) => { currentFramePage.Width = VisibleWidth; };
                    return;
                }
                if (divideNum <= 0 || divideNum > 1)
                    divideNum = defaultDivide;
                var nowWidth = VisibleWidth;
                currentFramePage.Width = nowWidth > rangeNum ? divideNum * nowWidth : nowWidth;
                Current.Frame.SizeChanged += (sender, args) => {
                    var nowWidthEx = VisibleWidth;
                    currentFramePage.Width = nowWidthEx > rangeNum ? divideNum * nowWidthEx : nowWidthEx;
                };
            }
        }

        /// <summary>
        /// Make the page more adaptive to the settings item : "Divide Screen Mode"
        /// </summary>
        /// <param name="currentPage">current child page instance</param>
        /// <param name="matchNumber">baseGrid's width of current page </param>
        /// <param name="rangeNumber">default number of the range to divide, is 800</param>
        /// <param name="isDivideScreen">make sure if need to divide screen</param>
        public void SetChildPageMargin(
            Page currentPage,
            double matchNumber,
            bool isDivideScreen,
            double rangeNumber = 800) {
            if (matchNumber > rangeNumber && !IsMobile && isDivideScreen)
                currentPage.Margin = new Thickness(3, 0, 0, 0);
            else
                currentPage.Margin = new Thickness(0, 0, 0, 0);
        }

        /// <summary>
        /// Change app title route string.
        /// </summary>
        /// <param name="value">the new value to be written into App title.</param>
        public static void ChangeTitlePath(string value) {
            Current.NavigateTitlePath.Text = value;
        }

        /// <summary>
        /// Change app title route string.
        /// </summary>
        /// <param name="number">the route point need to be changed</param>
        /// <param name="value">value to be written into the target point</param>
        public static void ChangeTitlePath(uint number, string value) {
            if (number < 1 || number > 3)
                return;
            switch (number) {
                case 1: NaviPathTitle.Route01 = value; break;
                case 2: NaviPathTitle.Route02 = value; break;
                case 3: NaviPathTitle.Route03 = value; break;
            }
            Current.NavigateTitlePath.Text = NaviPathTitle.RoutePath;
        }

        /// <summary>
        /// If you have not login, this method will redirect you to re-login popup to finish login-action.
        /// </summary>
        /// <param name="fromUri">return-to</param>
        /// <param name="fromFetchType">return-dataType</param>
        /// <param name="returnMessage">return-messageBag</param>
        /// <param name="fromNaviType">return-navigateType</param>
        public static void ReLoginIfStatusIsInvalid(
            Uri fromUri,
            DataFetchType fromFetchType = DataFetchType.NULL,
            object returnMessage = null,
            NavigateType fromNaviType = NavigateType.Webview) {

            if (!IsNeedLoginOrNot)
                return;
            Current.ReLoginPopup.IsOpen = true;
            Current.NavigateToBase?.Invoke(
                null,
                new NavigateParameter {
                    ToFetchType = DataFetchType.Index_ReLogin,
                    ToUri = new Uri(LoginPath),
                    MessageBag = GetUIString("LNU_Index_LS"),
                    NaviType = NavigateType.ReLogin,
                    MessageToReturn = new ReturnParameter {
                        FromUri = fromUri,
                        FromFetchType = fromFetchType,
                        ReturnMessage = returnMessage,
                        FromNaviType = fromNaviType,
                    },
                },
                GetFrameInstance(NavigateType.ReLogin),
                GetPageType(NavigateType.ReLogin));
        }

        #endregion

        #endregion

        #region Inner Resources class and Structs

        #region Resources Helper 

        /// <summary>
        /// Resources Helper 
        /// </summary>
        public static class InnerResources {

            #region Hamburger resources
            public static List<NavigationBar> HamburgerResList { get { return navigationListMap; } }
            static private List<NavigationBar> navigationListMap = new List<NavigationBar> {
                new NavigationBar {
                    Title = GetUIString("LNU_Index"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/"),
                    NaviType = NavigateType.Index,
                    FetchType = DataFetchType.Index,
                },
                new NavigationBar {
                    Title = GetUIString("LNU_Search_Query"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks.html"),
                    NaviType = NavigateType.Index,
                    FetchType = DataFetchType.CourseMark,
                },
                new NavigationBar {
                    Title = GetUIString("LNU_For_Teacher"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwcjlr/cjlr.loginwindow"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_G_E"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/tsjy/index.html"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_S_T"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/S/index.html"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_T_E"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/jxpg/"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_R_R"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/gzzd/index.html"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_P_C"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/qcb.kc"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_C_I"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/kcjj/index.html"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_T_I"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/jsjj/"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_C_A"),
                    PathUri = new Uri("http://jwc.lnu.edu.cn/jwgl/xkgl.htm"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_P_G"),
                    PathUri = new Uri("http://jwc.lnu.edu.cn/info/news/content/56421.htm"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_T_O_N"),
                    PathUri = new Uri("http://jwc.lnu.edu.cn/jwgl/jxyx.htm"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_A_A_O"),
                    PathUri = new Uri("http://jwc.lnu.edu.cn"),
                    NaviType = NavigateType.Webview
                },
                new NavigationBar {
                    Title = GetUIString("LNU_U_H_P"),
                    PathUri = new Uri("http://www.lnu.edu.cn"),
                    NaviType = NavigateType.Webview
                },
            };
            #endregion

            #region Type

            public static Type GetPageType(NavigateType type) { return pagesMaps.ContainsKey(type) ? pagesMaps[type] : null; }
            public static Dictionary<NavigateType, Type> PageTypeCollection { get { return pagesMaps; } }
            static private Dictionary<NavigateType, Type> pagesMaps = new Dictionary<NavigateType, Type> {
                { NavigateType.BaseList,typeof(BaseListPage)},
                { NavigateType.Content,typeof(ContentPage)},
                { NavigateType.Settings,typeof(SettingsPage)},
                { NavigateType.Webview,typeof(WebViewPage)},
                { NavigateType.Index,typeof(IndexPage)},
                { NavigateType.Login,typeof(LoginPage)},
                { NavigateType.ReLogin,typeof(LoginPage)},
                { NavigateType.ChangePassword,typeof(ChangePassPage)},
                { NavigateType.Schedule,typeof(SchedulePage)},
                { NavigateType.SimpleData,typeof(SimpleDataPage)},
            };

            #endregion

            #region Frame

            public static Frame GetFrameInstance(NavigateType type) { return frameMaps.ContainsKey(type) ? frameMaps[type] : null; }
            static private Dictionary<NavigateType, Frame> frameMaps = new Dictionary<NavigateType, Frame> {
                { NavigateType.BaseList,Current.BasePartFrame},
                { NavigateType.Settings,Current.ContentFrame},
                { NavigateType.Content,Current.ContentFrame},
                { NavigateType.Webview,Current.ContentFrame},
                { NavigateType.Index,Current.BasePartFrame},
                { NavigateType.Login,Current.ContentFrame},
                { NavigateType.ReLogin,Current.LoginPopupFrame},
                { NavigateType.ChangePassword,Current.ContentFrame},
                { NavigateType.Schedule,Current.ContentFrame},
                { NavigateType.SimpleData,Current.ContentFrame},
            };

            #endregion

            #region Child page for cache

            public static void AddBaseListPageInstance(string key, BaseListPage instance) { if (!baseListPageMap.ContainsKey(key)) { baseListPageMap.Add(key, instance); } }
            public static BaseListPage GetPageInstance(string key) { return baseListPageMap.ContainsKey(key) ? baseListPageMap[key] : null; }
            public static bool IfContainsPageInstance(string key) { return baseListPageMap.ContainsKey(key); }
            static private Dictionary<string, BaseListPage> baseListPageMap = new Dictionary<string, BaseListPage> {
            };

            #endregion

        }

        #endregion

        #region Structs

        /// <summary>
        /// struct for route title saving.
        /// </summary>
        public struct PathTitle {
            public string Route01 { get; set; }
            public string Route02 { get; set; }
            public string Route03 { get; set; }
            public string RoutePath { get { return Route02 != null ? Route03 != null ? Route01 + "-" + Route02 + "-" + Route03 : Route01 + "-" + Route02 : Route01; } }
        }

        /// <summary>
        ///  struct for login status saving.
        /// </summary>
        public struct LoginCookies {
            public string UserName { get; set; }
            public string UserID { get; set; }
            public string UserDepartment { get; set; }
            public string UserCourse { get; set; }
            public string UserIP { get; set; }
            public string UserTime { get; set; }
            public DateTime CacheMiliTime { get; set; }
            public bool IsInsert { get; set; }
            public HttpCookie Cookie { get; set; }
        }

        #endregion

        #endregion

        #region Slide Animations

        private Storyboard slideStory;
        private TranslateTransform slideTranslateT;

        private void InitSlideRecState() {
            SlideAnimaRec.ManipulationMode = ManipulationModes.TranslateX;
            SlideAnimaRec.ManipulationCompleted += SlideAnimaRec_ManipulationCompleted;
            SlideAnimaRec.ManipulationDelta += SlideAnimaRec_ManipulationDelta;
            slideTranslateT = SlideAnimaRec.RenderTransform as TranslateTransform;
            if (slideTranslateT == null)
                SlideAnimaRec.RenderTransform = slideTranslateT = new TranslateTransform();
        }

        private void SlideAnimaRec_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args) {
            if (slideTranslateT.X + args.Delta.Translation.X < 0) {
                slideTranslateT.X = 0;
                return;
            }
            slideTranslateT.X += args.Delta.Translation.X;
        }

        private void SlideAnimaRec_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) {
            double abs_delta = Math.Abs(e.Cumulative.Translation.X);
            double speed = Math.Abs(e.Velocities.Linear.X);
            double delta = e.Cumulative.Translation.X;
            double to = 0;

            if (abs_delta < SlideAnimaRec.ActualWidth / 2 && speed < 0.7) {
                slideTranslateT.X = 0;
                return;
            }

            if (delta > 0)
                to = SlideAnimaRec.ActualWidth;
            else if (delta < 0)
                return;

            slideStory = new Storyboard();
            var doubleanimation = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromMilliseconds(0)), From = slideTranslateT.X, To = 0 };
            doubleanimation.Completed += Doubleanimation_Completed;
            Storyboard.SetTarget(doubleanimation, slideTranslateT);
            Storyboard.SetTargetProperty(doubleanimation, "X");
            slideStory.Children.Add(doubleanimation);
            slideStory.Begin();
        }

        private void Doubleanimation_Completed(object sender, object e) {
            OpenOrClosePane();
        }

        private void OpenOrClosePane() {
            NavigationSplit.IsPaneOpen = !NavigationSplit.IsPaneOpen;
            SetVisibility(SlideAnimaRec, !NavigationSplit.IsPaneOpen);
            OnPaneIsOpened();
        }

        #endregion

        #region Properties and state

        public static HttpClient LoginClient { get; set; }
        public static MainPage Current { get; private set; }
        public TextBlock NavigateTitlePath { get; private set; }
        public Frame MainContentFrame { get; private set; }
        public Frame ReLoginPopupFrame { get; private set; }
        public ProgressRing BaseListRing { get; private set; }
        public ListBox HamburgerBox { get; private set; }
        public Popup ReLoginPopup { get; private set; }

        private bool isNeedClose = false;

        public const string HomeHost = "http://jwgl.lnu.edu.cn/";
        public const string HomeHostInsert = "http://jwgl.lnu.edu.cn";
        public const string LoginPath = "http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks_right.html";

        public delegate void NavigationEventHandler(object sender, NavigateParameter parameter, Frame frame, Type type);
        public NavigationEventHandler NavigateToBase = (sender, parameter, frame, type) => { frame.Navigate(type, parameter); };

        public static PathTitle NaviPathTitle = new PathTitle();
        public static LoginCookies LoginCache = new LoginCookies { IsInsert = false };

        private BinaryStringEncoding binaryStringEncoding;
        private IBuffer ibufferVector;
        private CryptographicKey cryptographicKey;

        #endregion

    }
}
