#region Using
using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.NET.Pages.IndexPage.InsideMapHelper;

using LNU.Core.Models;
using Wallace.UWP.Helpers.Tools;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using LNU.Core.Models.IndexModels;
using Windows.UI;
using Wallace.UWP.Helpers.Helpers;
using LNU.Core.Tools;
#endregion

namespace LNU.NET.Pages {
    
    public sealed partial class IndexPage : Page {

        #region Constructor

        public IndexPage() {
            this.InitializeComponent();
            Current = this;
            isPureColor = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsPureColorItem) ?? false;
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var args = e.Parameter as NavigateParameter;
            GridViewResources.Source = GetResourcesInstance(args.ToFetchType);
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e) {
            var args = e.ClickedItem as AdaptiveItem;
            if (args == null)
                return;
            MainPage.Current.NavigateToBase?.Invoke(
                sender, 
                new NavigateParameter { ToUri = args.PathUri, MessageBag = args.ItemTitle , ToFetchType = args.DataType, NaviType = args.NaviType, }, 
                MainPage.InnerResources.GetFrameInstance(args.NaviType), 
                MainPage.InnerResources.GetPageType(args.NaviType));
        }

        private void BackgroundImage_Loaded(object sender, RoutedEventArgs e) {
            (sender as Image).Visibility = isPureColor ?Visibility.Collapsed:Visibility.Visible;
            AddInImageList(sender as Image);
        }

        #endregion

        #region Methods

        #endregion

        #region Inside Resources class
        /// <summary>
        /// Resources Map Helper
        /// </summary>
        internal static class InsideMapHelper {

            #region Index Page Source
            /// <summary>
            /// Index page resources.
            /// </summary>
            public static List<AdaptiveItem> IndexResList { get { return indexResources; } }
            static private List<AdaptiveItem> indexResources = new List<AdaptiveItem> {
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE162),
                    ItemTitle =GetUIString("LNU_Index_CS"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 97, 17, 171)), 
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/1.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE2AC),
                    ItemTitle=GetUIString("LNU_Index_GCI"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/tskc/index.html"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 69, 90, 172)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/2.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE75A),
                    ItemTitle =GetUIString("LNU_Index_IPOGE"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwc.lnu.edu.cn/info/news/content/22162.htm"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 193, 63)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/3.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE125),
                    ItemTitle =GetUIString("LNU_Index_SISP"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://xg.lnu.edu.cn/"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 63, 138)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/4.jpg"),
                } ,
            };
            #endregion

            #region Course&Mark Page Source
            /// <summary>
            /// Course & Mark page resources.
            /// </summary>
            public static List<AdaptiveItem> CourseMarkResList { get { return cMResources; } }
            static private List<AdaptiveItem> cMResources = new List<AdaptiveItem> {
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE187),
                    ItemTitle =GetUIString("LNU_Index_LS"),
                    Description = null,
                    NaviType = NavigateType.Login,
                    DataType = DataFetchType.LNU_Index_Login,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks_right.html"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 67, 63)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/5.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE192),
                    ItemTitle =GetUIString("LNU_Index_CP"),
                    Description = null,
                    NaviType = NavigateType.ChangePassword,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bks_login2.NewPass"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 97, 17, 171)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/6.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE156),
                    ItemTitle =GetUIString("LNU_Index_SSI"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bks_xj.xjcx"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 69, 90, 172)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/7.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE73D),
                    ItemTitle =GetUIString("LNU_Index_SC"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/xk.CourseInput"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 193, 63)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/8.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE1DC),
                    ItemTitle =GetUIString("LNU_Index_SS"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/xk.CourseView"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 63, 138)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/9.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE721),
                    ItemTitle =GetUIString("LNU_Index_TSCQ"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/qcb.kc"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 75, 21, 173)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/10.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xEC24),
                    ItemTitle =GetUIString("LNU_Index_CA"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwc.lnu.edu.cn/jwgl/xkgl.htm"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 60, 188, 98)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/12.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xEBC3),
                    ItemTitle =GetUIString("LNU_Index_TP"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bkscjcx.jxjh"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 97, 17, 171)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/13.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE11D),
                    ItemTitle =GetUIString("LNU_Index_PC"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bkscjcx.yxkc"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 254, 183, 8)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/14.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xEE92),
                    ItemTitle =GetUIString("LNU_Index_ROTT"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bkscjcx.curscopre"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 69, 90, 172)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/15.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE76E),
                    ItemTitle =GetUIString("LNU_Index_FC"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bkscjcx.bjgkc"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 141, 4, 33)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/16.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE952),
                    ItemTitle =GetUIString("LNU_Index_TEL"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/jxpg/"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 244, 78, 97)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/17.jpg"),
                } ,
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE11B),
                    ItemTitle =GetUIString("LNU_Index_EH"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/xkdoc/xk_help.html"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 217, 6, 94)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/11.jpg"),
                } ,
            };
            #endregion

            #region Page Source Selector
            public static List<AdaptiveItem> GetResourcesInstance(DataFetchType key) { return resourcesDic.ContainsKey(key) ? resourcesDic[key] : null; }
            public static bool IfContainsListResources(DataFetchType key) { return resourcesDic.ContainsKey(key); }
            /// <summary>
            /// Decide which page need to be loaded.
            /// </summary>
            static private Dictionary<DataFetchType, List<AdaptiveItem>> resourcesDic = new Dictionary<DataFetchType, List<AdaptiveItem>> {
                { DataFetchType.LNU_Index, IndexResList },
                { DataFetchType.LNU_CourseMark, CourseMarkResList },
            };
            #endregion

            public static void ChangeImageVisibility(bool isVisible) { ImageList.ForEach(i => i.Visibility = isVisible?Visibility.Visible:Visibility.Collapsed); }
            public static void AddInImageList(Image image) { ImageList.Add(image); }
            static private List<Image> ImageList = new List<Image> {
            };

        }
        #endregion

        #region Properties and state
        public static IndexPage Current;
        private bool isPureColor;
        public AdaptiveGridView adaptiveGV;
        #endregion

    }
}
