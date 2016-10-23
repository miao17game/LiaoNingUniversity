using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.Core.Tools.LNUWebProcess;

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
            Current = this;
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
            Submit.IsEnabled = MainPage.LoginCache.IsInsert;
            var args = e.Parameter as NavigateParameter;
            if (args == null || args.ToUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            if (args.MessageBag as string != null)
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;
            thisNaviType = args.NaviType;
            if (MainPage.IsNeedLoginOrNot)
                RedirectToLogin();
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        private async void Submit_Click(object sender, RoutedEventArgs e) {
            contentRing.IsActive = true;

            var oldPass = PB_Old.Password;
            var newPass = PB_New.Password;
            var confirm = PB_Recofirm.Password;

            var returnMessage = await PostLNUChangePassword(
                MainPage.LoginClient,
                string.Format(
                    "http://jwgl.lnu.edu.cn/pls/wwwbks//bks_login2.ChangePass?p_oldpass={0}&p_newpass1={1}&p_newpass2={2}",
                    oldPass,
                    newPass,
                    confirm));

            if (returnMessage == null)
                RedirectToLogin();
            else
                ChangePasswordAndReport(returnMessage);
        }

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

        /// <summary>
        /// if need, redirect to login popup because you have no access to do this . you need login to get the access.
        /// </summary>
        /// <param name="rootNode"></param>
        private void RedirectToLogin() {
            MainPage.LoginCache.IsInsert = false;
            MainPage.ReLoginIfStatusIsInvalid(currentUri, thisPageType, navigateTitlePath.Text, thisNaviType);
            ReportHelper.ReportAttention(GetUIString("Login_First"));
        }

        private void ChangePasswordAndReport(string returnMessage) {
            contentRing.IsActive = false;
            var doc = new HtmlDocument();
            doc.LoadHtml(returnMessage);
            var rootNode = doc.DocumentNode;
            var requestResult = rootNode.SelectSingleNode("//span[@class='t']");
            if (requestResult != null) {
                switch (requestResult.InnerText.Substring(1, requestResult.InnerText.Length - 2)) {
                    case "新密码输入不一致!":
                        isFirstLoaded = true;
                        PB_New.Password = "";
                        PB_Recofirm.Password = "";
                        ReportHelper.ReportAttention(GetUIString("Login_Reconfirm_Failed"));
                        break;
                    case "密码输入有误!":
                        isFirstLoaded = true;
                        ReportHelper.ReportAttention(GetUIString("Login_Pass_Input_Error"));
                        break;
                    case "你已经改变了你的密码!":
                        PageSlideOutStart(VisibleWidth > 800 ? false : true);
                        ReportHelper.ReportAttention(GetUIString("Login_Pass_Changed"));
                        break;
                    default:
                        isFirstLoaded = true;
                        ReportHelper.ReportAttention(GetUIString("Login_Uhandled_Error"));
                        break;
                }
            } else {
                // e...oh...
            }
        }

        #endregion

        #region Properties
        public static ChangePassPage Current;
        #endregion

    }
}
