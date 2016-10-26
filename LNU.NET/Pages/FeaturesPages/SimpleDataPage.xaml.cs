using static Wallace.UWP.Helpers.Tools.UWPStates;

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
using System.Diagnostics;
using LNU.Core.Tools;
using Wallace.UWP.Helpers.Tools;

namespace LNU.NET.Pages.FeaturesPages {
   
    public sealed partial class SimpleDataPage : BaseContentPage {

        public SimpleDataPage() {
            this.InitializeComponent();
            Current = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if (args == null || args.ToUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            contentRing.IsActive = true;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;

            if (args.MessageBag as string != null)
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;

            // DO ASYNC WORK HERE ......

            try {
                if (thisPageType == DataFetchType.Data_CourseCalender) {
                    var result = DataProcess.FetchCourseCalenderFromHtml(await WebProcess.GetHtmlResources(currentUri.ToString(), true));

                    PreSelectCS.Text = result.PreSelectCS;
                    PreSelectPH.Text = result.PreSelectPH;
                    SelectCS.Text = result.SelectCS;
                    SelectPH.Text = result.SelectPH;
                    CoverSelect.Text = result.CoverSelect;
                    QueryDate.Text = result.QueryDate;

                    SetVisibility(CourseCalenderView, true);

                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            } finally {
                contentRing.IsActive = false;
            }
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
            Current = null;
        }

        #region Properties and state
        public static SimpleDataPage Current;
        #endregion
    }
}
