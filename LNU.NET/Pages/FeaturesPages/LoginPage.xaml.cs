#region Using
using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LNU.Core.Tools.LNUWebProcess;

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
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using System.Text;
using System.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using LNU.NET.Controls;
using Windows.Web.Http;
#endregion

namespace LNU.NET.Pages.FeaturesPages {

    public sealed partial class LoginPage : BaseContentPage {

        #region Constructor

        public LoginPage() {
            this.InitializeComponent();
            Current = this;
        }

        #endregion

        #region Events

        #region Page and Controls Events

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
            contentRing.IsActive = true;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;

            if (args.MessageToReturn != null) { // id need, save the massage to be sent to 'from-url'.
                fromUri = args.MessageToReturn.FromUri;
                fromPageType = args.MessageToReturn.FromFetchType;
                fromNavigateTitle = args.MessageToReturn.ReturnMessage as string;
                fromNaviType = args.MessageToReturn.FromNaviType;
            }

            if (args.MessageBag as string != null) // if there is a title to be saved, save it.
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;

            if (thisPageType == DataFetchType.Index_Login || thisPageType == DataFetchType.Index_ReLogin) { // if need login, do something...

                InitLoginPopupState();

                if (MainPage.IsNeedLoginOrNot) { // need to login.
                    SetVisibility(MainPopupGrid, true);

                    // DO ASYNC WORK ... MAYBE AWAIT

                    PasswordCheckBox.IsChecked = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsSavePassword) ?? false;
                    AutoLoginCheckBox.IsChecked = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsAutoLogin) ?? false;
                    AutoLoginCheckBox.IsChecked = PasswordCheckBox.IsChecked ?? false ? AutoLoginCheckBox.IsChecked : false;
                    AutoLoginCheckBox.IsEnabled = PasswordCheckBox.IsChecked ?? false;

                    EmailBox.Text = (string)SettingsHelper.ReadSettingsValue(SettingsConstants.Email) ?? "";
                    PasswordDecryption();

                    if (AutoLoginCheckBox.IsChecked ?? false)
                        await ClickSubmitButtonIfAuto();

                } else { // don not need to login but only get the login-message from main-page.

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
        }

        private void MainPopupGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            
        }

        private void LoginPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            contentGrid.Width = (sender as Popup).ActualWidth;
            contentGrid.Height = (sender as Popup).ActualHeight;
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

        #region Button Events

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            if (thisPageType == DataFetchType.Index_ReLogin) {
                MainPage.Current.ReLoginPopup.IsOpen = false;
                MainPage.Current.MainContentFrame.Content = null;
                return;
            }
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        /// <summary>
        /// send login-command to the target apis.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Submit_Click(object sender, RoutedEventArgs e) {
            await ClickSubmitButtonIfAuto();
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e) {
            StatusRing.IsActive = true;
            LogOutButton.IsEnabled = false;
            SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
            var message = await LNULogOutCallback(MainPage.LoginClient, "http://jwgl.lnu.edu.cn/pls/wwwbks/bks_login2.Logout");
            if (message == null)
                Debug.WriteLine("logout_failed");
            else {
                MainPage.LoginCache.IsInsert = false;
                UnRedirectCookiesManager.DeleteCookie(MainPage.LoginCache.Cookie);
                RefreshHttpClient();
                ReportHelper.ReportAttention(GetUIString("LogOut_Success"));
                MainPage.Current.NavigateToBase?.Invoke(
                    this,
                    new NavigateParameter { ToFetchType = DataFetchType.Index_Login, MessageBag = navigateTitle, ToUri = currentUri, NaviType = NavigateType.Login },
                    MainPage.InnerResources.GetFrameInstance(NavigateType.Login),
                    typeof(LoginPage));
            }
        }

        #endregion

        #region CheckBox Events

        private void PasswordCheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void AutoLoginCheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void AutoLoginCheckBox_Click(object sender, RoutedEventArgs e) {
            SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, (sender as CheckBox).IsChecked ?? false);
        }

        private void PasswordCheckBox_Click(object sender, RoutedEventArgs e) {
            var isChecked = (sender as CheckBox).IsChecked ?? false;
            SettingsHelper.SaveSettingsValue(SettingsSelect.IsSavePassword, isChecked);
            if (!isChecked) {
                SettingsHelper.SaveSettingsValue(SettingsConstants.Password, null);
                AutoLoginCheckBox.IsChecked = false;
                AutoLoginCheckBox.IsEnabled = false;
            } else
                AutoLoginCheckBox.IsEnabled = true;
        }

        #endregion

        #endregion

        #region Methods

        #region State and Command

        /// <summary>
        /// make ui of popup right anyway.
        /// </summary>
        private void InitLoginPopupState() {
            if (thisPageType == DataFetchType.Index_ReLogin)
                if (VisibleWidth <= 800 || IsMobile) {
                    this.Width = VisibleWidth - 60;
                    this.Height = VisibleHeight - 60;
                } else {
                    this.MaxHeight = 1000;
                }
        }

        /// <summary>
        /// Open methods to change state when the theme mode changed.
        /// </summary>
        public void ChangeStateByRequestTheme() {

        }

        /// <summary>
        /// if need, run this method for auto-login.
        /// </summary>
        /// <returns></returns>
        private async Task ClickSubmitButtonIfAuto() {
            Submit.IsEnabled = false;
            SubitRing.IsActive = true;
            var user = EmailBox.Text;
            var pass = PasswordBox.Password;

            SettingsHelper.SaveSettingsValue(SettingsConstants.Email, user);
            PasswordEncryption(pass);

            // set the abort button with keybord-focus, so that the vitual keyboad in the mobile device with disappear.
            Abort.Focus(FocusState.Keyboard);

            //await InsertLoginMessage(user, pass);
            var loginReturn = await PostLNULoginCallback(MainPage.LoginClient, user, pass);
            if(loginReturn!=null)
                CheckIfLoginSucceed(loginReturn);
            else { 
                ReportHelper.ReportAttention(GetUIString("Internet_Failed"));
                Submit.IsEnabled = true;
                SubitRing.IsActive = false;
            }
        }

        #endregion

        #region Login Status Changed

        /// <summary>
        /// if login failed, re-navigate to the target Uri, otherwise, show status detail of you.
        /// </summary>
        /// <param name="htmlContent">html of websites</param>
        private void CheckIfLoginSucceed(LoginReturnBag loginReturn) {
            var doc = new HtmlDocument();
            if(loginReturn.HtmlResouces == null) { // login failed, redirect to the login page.
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
            } else { // login successful, save login status and show it.
                SaveLoginStatus(studentStatus, loginReturn.CookieBag.FirstOrDefault());
                LoginPopup.IsOpen = false;
                SetVisibility(MainPopupGrid, false);
                if (thisPageType == DataFetchType.Index_ReLogin) {
                    RedirectToPageBefore();
                    return;
                }
                SetVisibility(StatusGrid, true);
            }
        }

        /// <summary>
        /// Go back to the page which navigate you to come here.
        /// </summary>
        private void RedirectToPageBefore() {
            MainPage.Current.ReLoginPopup.IsOpen = false;
            MainPage.Current.NavigateToBase?.Invoke(
            null,
            new NavigateParameter { ToFetchType = fromPageType, MessageBag = fromNavigateTitle, ToUri = fromUri, NaviType = fromNaviType },
            MainPage.InnerResources.GetFrameInstance(fromNaviType),
            MainPage.InnerResources.GetPageType(fromNaviType));
        }

        /// <summary>
        /// redirect to login when login-error throws.
        /// </summary>
        private void RedirectToLoginAgain() {
            if (thisPageType == DataFetchType.Index_ReLogin) {
                MainPage.Current.NavigateToBase?.Invoke(
                    this,
                    new NavigateParameter {
                        ToFetchType = DataFetchType.Index_ReLogin,
                        MessageBag = navigateTitle,
                        ToUri = currentUri,
                        NaviType = NavigateType.ReLogin,
                        MessageToReturn = new ReturnParameter {
                            FromUri = fromUri,
                            FromFetchType = fromPageType,
                            FromNaviType = fromNaviType,
                            ReturnMessage = fromNavigateTitle,
                        },
                    },
                    MainPage.InnerResources.GetFrameInstance(NavigateType.ReLogin),
                    typeof(LoginPage));
                return;
            }
            MainPage.Current.NavigateToBase?.Invoke(
                this,
                new NavigateParameter {
                    ToFetchType = DataFetchType.Index_Login,
                    MessageBag = navigateTitle,
                    ToUri = currentUri,
                    NaviType = NavigateType.Login
                },
                MainPage.InnerResources.GetFrameInstance(NavigateType.Login),
                typeof(LoginPage));
        }

        /// <summary>
        /// save status message in MainPage to be controlled.
        /// </summary>
        /// <param name="item"></param>
        private void SaveLoginStatus(HtmlNode item, HttpCookie cookie) {
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
            MainPage.LoginCache.Cookie = cookie;
        }

        #endregion

        #region Password Encryption & Decryption

        private void PasswordEncryption(string pass) {
            if (PasswordCheckBox.IsChecked ?? false) {
                try { // password encryption is over here.
                    var finalToSave = CipherEncryptionHelper.CipherEncryption(
                        pass,
                        SymmetricAlgorithmNames.AesCbcPkcs7,
                        out binaryStringEncoding,
                        out ibufferVector,
                        out cryptographicKey);

                    SettingsHelper.SaveSettingsValue(SettingsConstants.Password, finalToSave.ToArray());

                } catch (Exception e) { // if any error throws, report in debug range and do nothing in the foreground.
                    Debug.WriteLine(e.StackTrace);
                }
            }
        }

        private void PasswordDecryption() {
            try { // password decryption over here.
                var Password = SettingsHelper.ReadSettingsValue(SettingsConstants.Password) as byte[];
                if (Password != null) { // init ibuffer vector and cryptographic key for decryption.
                    SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
                    cryptographicKey = objAlg.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(CipherEncryptionHelper.CollForKeyAndIv));
                    ibufferVector = CryptographicBuffer.CreateFromByteArray(CipherEncryptionHelper.CollForKeyAndIv);

                    PasswordBox.Password = CipherEncryptionHelper.CipherDecryption( // decryption the message.
                        SymmetricAlgorithmNames.AesCbcPkcs7,
                        CryptographicBuffer.CreateFromByteArray(Password),
                        ibufferVector,
                        BinaryStringEncoding.Utf8,
                        cryptographicKey);
                }
            } catch (Exception e) { // if any error throws, clear the password cache to prevent more errors.
                Debug.WriteLine(e.StackTrace);
                SettingsHelper.SaveSettingsValue(SettingsConstants.Password, null);
            }
        }

        #endregion

        #endregion

        #region Properties and state

        #region Fields for this
        public static LoginPage Current;
        private BinaryStringEncoding binaryStringEncoding;
        private IBuffer ibufferVector;
        private CryptographicKey cryptographicKey;
        #endregion

        #region Fields for return
        private Uri fromUri;
        private DataFetchType fromPageType;
        private string fromNavigateTitle;
        private NavigateType fromNaviType;
        #endregion

        #endregion

    }
}
