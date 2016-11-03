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
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

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

            EncryptionFor(ref oldPass, ref newPass, ref confirm);

            Debug.WriteLine(string.Format(
                    "https://notificationhubforuwp.azurewebsites.net/LNU/ChangePassword?old={0}&newPass={1}&reconfirm={2}",
                    oldPass,
                    newPass,
                    confirm)
                );

            var returnMessage = await PostLNURedirectPOSTMethod(
                MainPage.LoginClient,
                string.Format(
                    "https://notificationhubforuwp.azurewebsites.net/LNU/ChangePassword?old={0}&newPass={1}&reconfirm={2}",
                    oldPass,
                    newPass,
                    confirm),
                MainPage.LoginCache.Cookie
                );

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
            Debug.WriteLine(returnMessage);
            var errorCode = default(ErrorStatus);
            errorCode = returnMessage.Contains("新密码输入不一致!") ? ErrorStatus.UnConfirmed :
                returnMessage.Contains("密码输入有误!") ? ErrorStatus.PasswordError :
                returnMessage.Contains("你已经改变了你的密码!") ? ErrorStatus.Succeed :
                ErrorStatus.Unknown;
            switch (errorCode) {
                case ErrorStatus.UnConfirmed:
                    isFirstLoaded = true;
                    PB_New.Password = "";
                    PB_Recofirm.Password = "";
                    ReportHelper.ReportAttention(GetUIString("Login_Reconfirm_Failed"));
                    break;
                case ErrorStatus.PasswordError:
                    isFirstLoaded = true;
                    ReportHelper.ReportAttention(GetUIString("Login_Pass_Input_Error"));
                    break;
                case ErrorStatus.Succeed:
                    PageSlideOutStart(VisibleWidth > 800 ? false : true);
                    ReportHelper.ReportAttention(GetUIString("Login_Pass_Changed"));
                    break;
                default:
                    isFirstLoaded = true;
                    ReportHelper.ReportAttention(GetUIString("Login_Uhandled_Error"));
                    break;
            }
        }

        private void EncryptionFor (ref string old, ref string newPass, ref string confirm ) {
            try { // password encryption is over here.

                var userToTransmit = CipherEncryptionHelper.CipherEncryption(
                    old,
                    SymmetricAlgorithmNames.AesCbcPkcs7,
                    out binaryStringEncoding,
                    out ibufferVector,
                    out cryptographicKey);

                var passToTransmit = CipherEncryptionHelper.CipherEncryption(
                    newPass,
                    SymmetricAlgorithmNames.AesCbcPkcs7,
                    out binaryStringEncoding,
                    out ibufferVector,
                    out cryptographicKey);

                var confirmToTransmit = CipherEncryptionHelper.CipherEncryption(
                    confirm,
                    SymmetricAlgorithmNames.AesCbcPkcs7,
                    out binaryStringEncoding,
                    out ibufferVector,
                    out cryptographicKey);

                /// Changes For Windows Store

                newPass = Convert.ToBase64String(passToTransmit.ToArray()).Replace("/","$");
                old = Convert.ToBase64String(userToTransmit.ToArray()).Replace("/", "$");
                confirm = Convert.ToBase64String(confirmToTransmit.ToArray()).Replace("/", "$");

                ///

            } catch (Exception e) { // if any error throws, report in debug range and do nothing in the foreground.
                Debug.WriteLine(e.StackTrace);
            }
        }

        #endregion

        #region Properties
        public static ChangePassPage Current;
        private BinaryStringEncoding binaryStringEncoding;
        private IBuffer ibufferVector;
        private CryptographicKey cryptographicKey;

        private enum ErrorStatus { Unknown, UnConfirmed, PasswordError, Succeed }
        #endregion

    }
}
