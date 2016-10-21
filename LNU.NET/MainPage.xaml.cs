#region Using
using static Wallace.UWP.Helpers.Tools.UWPStates;
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
                    if (WebViewPage.Current != null)
                        WebViewPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                    if (ChangePassPage.Current != null)
                        ChangePassPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
                    if (LoginPage.Current != null)
                        LoginPage.Current.PageSlideOutStart(VisibleWidth > 800 ? false : true);
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

        private void InitMainPageState() {
            NavigateManager.BackRequested += OnBackRequested;
            var isDarkOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsDarkThemeOrNot) ?? true;
            RequestedTheme = isDarkOrNot ? ElementTheme.Dark : ElementTheme.Light;
            ChangeTitlePath(1, navigateTitlePath.Text);
            StatusBarInit.InitInnerDesktopStatusBar(true);
            Window.Current.SetTitleBar(BasePartBorder);
            IfNeedAdapteVitualNavigationBar();
            InitSlideRecState();
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

        /// <summary>
        /// well....you can know what i am doing by the name of the method......
        /// </summary>
        /// <param name="oldTime"></param>
        /// <param name="newTime"></param>
        /// <returns></returns>
        public static bool IsMoreThan30Minutes(DateTime oldTime, DateTime newTime) {
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

        private void GetResources() { 
            NaviBarResouces.Source = HamburgerResList;
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
            bool isDivideScreen , 
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

            if (LoginCache.IsInsert && !IsMoreThan30Minutes(LoginCache.CacheMiliTime, DateTime.Now))
                return;
            Current.ReLoginPopup.IsOpen = true;
            Current.NavigateToBase?.Invoke(
                null,
                new NavigateParameter {
                    ToFetchType = DataFetchType.LNU_Index_ReLogin,
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

        /// <summary>
        /// Start the dark animation when hamburger menu opened.
        /// </summary>
        private void OnPaneIsOpened() {
            SetVisibility(DarkDivideBorder, true);
            EnterBorder.Begin();
        }

        #endregion

        #region Inner Resources class and Structs
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
                    FetchType = DataFetchType.LNU_Index,
                },
                new NavigationBar {
                    Title = GetUIString("LNU_Search_Query"),
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks.html"),
                    NaviType = NavigateType.Index,
                    FetchType = DataFetchType.LNU_CourseMark,
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

            public static Type GetPageType(NavigateType type) { return pagesMaps.ContainsKey(type) ? pagesMaps[type] : null; }
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
            };

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
            };

            public static void AddBaseListPageInstance(string key, BaseListPage instance) { if (!baseListPageMap.ContainsKey(key)) { baseListPageMap.Add(key, instance); } }
            public static BaseListPage GetPageInstance(string key) { return baseListPageMap.ContainsKey(key) ? baseListPageMap[key] : null; }
            public static bool IfContainsPageInstance(string key) { return baseListPageMap.ContainsKey(key); }
            static private Dictionary<string, BaseListPage> baseListPageMap = new Dictionary<string, BaseListPage> {
            };

        }

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
        }

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
        public static MainPage Current;
        public TextBlock NavigateTitlePath;
        public Frame MainContentFrame;
        public Frame ReLoginPopupFrame;
        public ProgressRing BaseListRing;
        public ListBox HamburgerBox;
        public Popup ReLoginPopup;
        private bool isNeedClose = false;
        public delegate void NavigationEventHandler(object sender, NavigateParameter parameter, Frame frame, Type type);
        public NavigationEventHandler NavigateToBase = (sender, parameter, frame, type) => { frame.Navigate(type, parameter); };
        public static PathTitle NaviPathTitle = new PathTitle();
        public static LoginCookies LoginCache = new LoginCookies { IsInsert = false };
        public const string HomeHost = "http://jwgl.lnu.edu.cn/";
        public const string HomeHostInsert = "http://jwgl.lnu.edu.cn";
        public const string LoginPath = "http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks_right.html";
        #endregion

    }
}
