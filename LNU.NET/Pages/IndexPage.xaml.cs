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

using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.NET.Pages.IndexPage.InsideResources;
using LNU.Core.Models.IndexModels;
using Windows.UI;

namespace LNU.NET.Pages {
    
    public sealed partial class IndexPage : Page {

        #region Constructor
        public IndexPage() {
            this.InitializeComponent();
        }
        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var args = e.Parameter as NavigateParameter;
            GridViewResources.Source = GetResourcesInstance(args.DataType);
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e) {
            var args = e.ClickedItem as AdaptiveItem;
            if (args == null)
                return;
            MainPage.Current.NavigateToBase?.Invoke(
                sender, 
                new NavigateParameter { PathUri = args.PathUri, MessageBag = args.ItemTitle }, 
                MainPage.InnerResources.GetFrameInstance(args.NaviType), 
                MainPage.InnerResources.GetPageType(args.NaviType));
        }

        #endregion

        #region Methods

        #endregion

        #region Inside Resources class
        /// <summary>
        /// Resources Helper
        /// </summary>
        internal static class InsideResources {

            public static void AddAGVInDec(string key, AdaptiveGridView instance) { if (!AdaptiveGridViewMap.ContainsKey(key)) { AdaptiveGridViewMap.Add(key, instance); } }
            public static AdaptiveGridView GetAGVInstance(string key) { return AdaptiveGridViewMap.ContainsKey(key) ? AdaptiveGridViewMap[key] : null; }
            public static bool IfContainsAGVInstance(string key) { return AdaptiveGridViewMap.ContainsKey(key); }
            static private Dictionary<string, AdaptiveGridView> AdaptiveGridViewMap = new Dictionary<string, AdaptiveGridView> {
            };

            #region Index Page Source
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
                    BackImage = new Uri("ms-appx:///Assets/79.jpg"),
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
                    BackImage = new Uri("ms-appx:///Assets/124.jpg"),
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
                    BackImage = new Uri("ms-appx:///Assets/46.jpg"),
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
                    BackImage = new Uri("ms-appx:///Assets/56.jpg"),
                } ,
            };
            #endregion

            #region Course&Mark Page Source
            public static List<AdaptiveItem> CourseMarkResList { get { return cMResources; } }
            static private List<AdaptiveItem> cMResources = new List<AdaptiveItem> {
                new AdaptiveItem {
                    ItemIcon = char.ConvertFromUtf32(0xE162),
                    ItemTitle =GetUIString("LNU_Index_LS"),
                    Description = null,
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/zhxt_bks/zhxt_bks_right.html"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 67, 63)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/73.jpg"),
                } ,
            };
            #endregion

            public static List<AdaptiveItem> GetResourcesInstance(DataFetchType key) { return resourcesDic.ContainsKey(key) ? resourcesDic[key] : null; }
            public static bool IfContainsListResources(DataFetchType key) { return resourcesDic.ContainsKey(key); }
            static private Dictionary<DataFetchType, List<AdaptiveItem>> resourcesDic = new Dictionary<DataFetchType, List<AdaptiveItem>> {
                { DataFetchType.LNU_Index, IndexResList },
                { DataFetchType.LNU_Course_Mark, CourseMarkResList },
            };

        }
        #endregion

        #region Properties and state
        public AdaptiveGridView adaptiveGV;
        private string ArgsPathKey;
        #endregion

    }
}
