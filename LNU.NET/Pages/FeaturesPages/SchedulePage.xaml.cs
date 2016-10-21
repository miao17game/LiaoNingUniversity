using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.NET.Pages.FeaturesPages.SchedulePage.InsideMapHelper;

using LNU.Core.Models;
using LNU.NET.Controls;
using System;
using System.Collections.Generic;
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
using LNU.NET.Tools;
using LNU.Core.Models.ContentModels;
using Wallace.UWP.Helpers.Tools;
using LNU.Core.Tools;
using System.Diagnostics;

namespace LNU.NET.Pages.FeaturesPages {

    public sealed partial class SchedulePage : BaseContentPage {
        public SchedulePage() {
            this.InitializeComponent();
            Current = this;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            MainPage.Current.SetChildPageMargin(
                currentPage: this,
                matchNumber: VisibleWidth,
                isDivideScreen: isDivideScreen);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if (args == null || args.ToUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            if (args.MessageBag as string != null)
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;
            contentRing.IsActive = true;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;
            thisNaviType = args.NaviType;
            var htmlResources = await WebProcess.GetHtmlResources(args.ToUri.ToString(), true);
            Debug.WriteLine(htmlResources + "<=============");
            var list = DataProcess.FetchScheduleListFromHtml(htmlResources);
            list.ForEach(i => Debug.WriteLine(i.Title));
            ScheduleQueue.AddRange(list);
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        #region Inside Resources

        internal static class InsideMapHelper {

            #region Schedule table source

            public static List<ScheduleItem> ScheduleQueue { get { return scheduleList; } }
            static private List<ScheduleItem> scheduleList = new List<ScheduleItem> { };

            private static string GetLecturerName(string wholeTitle) {
                return wholeTitle.Replace("/", "@").Split('@')[1];
            }

            private static string GetSingleTitle(string wholeTitle) {
                var tun = wholeTitle.Replace("(", "@").Split('@')[0];
                return tun.Substring(1, tun.Length-1);
            }

            #endregion

        }

        #endregion

        #region Properties
        public static SchedulePage Current;
        #endregion

    }
}
