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
            GridViewResources.Source = AdaptiveResList;
        }
        #endregion

        #region Events

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e) {

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

            public static List<AdaptiveItem> AdaptiveResList { get { return contentAdaptiveItemsList; } }
            static private List<AdaptiveItem> contentAdaptiveItemsList = new List<AdaptiveItem> {
                new AdaptiveItem {
                    ItemTitle = char.ConvertFromUtf32(0xE162),
                    Description ="选课日程",
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 243, 209, 147)), 
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/79.jpg"),
                } ,
                new AdaptiveItem {
                    ItemTitle = char.ConvertFromUtf32(0xE2AC),
                    Description ="辽宁大学通识课程简介",
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwgl.lnu.edu.cn/tskc/index.html"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 111, 183, 128)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/124.jpg"),
                } ,
                new AdaptiveItem {
                    ItemTitle = char.ConvertFromUtf32(0xE75A),
                    Description ="辽宁大学通识教育实施方案",
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://jwc.lnu.edu.cn/info/news/content/22162.htm"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 243, 175, 147)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/46.jpg"),
                } ,
                new AdaptiveItem {
                    ItemTitle = char.ConvertFromUtf32(0xE125),
                    Description ="学生一体化服务平台",
                    NaviType = NavigateType.Webview,
                    PathUri = new Uri("http://xg.lnu.edu.cn/"),
                    Background = new SolidColorBrush(Color.FromArgb(255, 243, 175, 147)),
                    IconForeground = new SolidColorBrush(Colors.White),
                    TitleForeground = new SolidColorBrush(Colors.White),
                    BackImage = new Uri("ms-appx:///Assets/56.jpg"),
                } ,
            };

        }
        #endregion

        #region Properties and state
        public AdaptiveGridView adaptiveGV;
        private string ArgsPathKey;
        #endregion

    }
}
