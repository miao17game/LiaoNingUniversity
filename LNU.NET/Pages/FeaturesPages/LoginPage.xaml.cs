#region Using
using static Wallace.UWP.Helpers.Tools.UWPStates;

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
#endregion

namespace LNU.NET.Pages.FeaturesPages {

    public sealed partial class LoginPage : BaseContentPage {

        #region Constructor

        public LoginPage() {
            this.InitializeComponent();
            Current = this;
            InitPageState();
        }

        #endregion

        #region Events

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            MainPage.Current.SetChildPageMargin(
                currentPage: this,
                matchNumber: VisibleWidth,
                isDivideScreen: isDivideScreen);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var args = e.Parameter as NavigateParameter;
            if (args == null || args.ToUri == null) { // make sure the navigation action is right.
                ReportHelper.ReportAttention(GetUIString("WebViewLoadError"));
                return;
            }
            contentRing.IsActive = true;
            currentUri = args.ToUri;
            thisPageType = args.ToFetchType;

            if(args.MessageToReturn != null) {
                fromUri = args.MessageToReturn.FromUri;
                fromPageType = args.MessageToReturn.FromFetchType;
                fromNavigateTitle = args.MessageToReturn.ReturnMessage as string;
                fromNaviType = args.MessageToReturn.FromNaviType;
            }

            if (args.MessageBag as string != null) // if there is a title to be saved, save it.
                navigateTitle = navigateTitlePath.Text = args.MessageBag as string;
            if (thisPageType == DataFetchType.LNU_Index_Login || thisPageType == DataFetchType.LNU_Index_ReLogin) { // if need login, do something...

                InitLoginPopupState();

                SetVisibility(Scroll, false);
                Submit.IsEnabled = Abort.IsEnabled = false;

                if (!MainPage.LoginCache.IsInsert || MainPage.IsMoreThan30Minutes(MainPage.LoginCache.CacheMiliTime, DateTime.Now))  // need to login.
                    SetVisibility(MainPopupGrid, true);
                else { // don not need to login but only get the login-message from mainpage.

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
            Scroll.Source = currentUri;
        }

        private void MainPopupGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            
        }

        private void LoginPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            contentGrid.Width = (sender as Popup).ActualWidth;
            contentGrid.Height = (sender as Popup).ActualHeight;
        }

        #region Web Events

        /// <summary>
        /// for log-out action completed check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Scroll_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
            Scroll.NavigationCompleted -= Scroll_NavigationCompleted;
            ReportHelper.ReportAttention(GetUIString("LogOut_Success"));
            isFristNavigate = true;
            MainPage.Current.NavigateToBase?.Invoke(
                this,
                new NavigateParameter { ToFetchType = DataFetchType.LNU_Index_Login, MessageBag = navigateTitle, ToUri = currentUri , NaviType = NavigateType.Login},
                MainPage.InnerResources.GetFrameInstance(NavigateType.Login),
                typeof(LoginPage));
        }

        private void Scroll_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void Scroll_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        /// <summary>
        /// handled when webview loaded successfully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Scroll_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            contentRing.IsActive = false;
            await SetLoginPageStateIfNeed();
        }

        /// <summary>
        /// receive message when the js in the webview send message to window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScriNotifypt(object sender, NotifyEventArgs e) {
            Scroll.ScriptNotify -= OnScriNotifypt;
            Submit.IsEnabled = true;
            SubitRing.IsActive = false;
            CheckIfLoginSucceed(JsonHelper.FromJson<string[]>(e.Value)[1]);
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
            if (thisPageType == DataFetchType.LNU_Index_ReLogin) {
                MainPage.Current.ReLoginPopup.IsOpen = false;
                return;
            }
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
        }

        /// <summary>
        /// send login-command to the target web or apis.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Submit_Click(object sender, RoutedEventArgs e) {
            await ClickSubmitButtonIfAuto();
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e) {
            StatusRing.IsActive = true;
            LogOutButton.IsEnabled = false;
            Scroll.NavigationCompleted += Scroll_NavigationCompleted;
            MainPage.LoginCache.IsInsert = false;
            Scroll.Source = new Uri("http://jwgl.lnu.edu.cn/pls/wwwbks/bks_login2.Logout");
            SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
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

        private void InitPageState() {
            isDivideScreen = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsDivideScreen) ?? true;
            MainPage.DivideWindowRange(
                currentFramePage: this,
                divideNum: (double?)SettingsHelper.ReadSettingsValue(SettingsSelect.SplitViewMode) ?? 0.6,
                isDivideScreen: isDivideScreen);
        }

        /// <summary>
        /// make ui of popup right anyway.
        /// </summary>
        private void InitLoginPopupState() {
            if (thisPageType == DataFetchType.LNU_Index_ReLogin)
                if (VisibleWidth <= 800 || IsMobile) {
                    this.Width = VisibleWidth - 60;
                    this.Height = VisibleHeight - 60;
                }
        }

        /// <summary>
        /// Open methods to change state when the theme mode changed.
        /// </summary>
        public static void ChangeStateByRequestTheme() {

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
            await InsertLoginMessage(user, pass);
        }

        /// <summary>
        /// Init login-page state, and if need, fetch the message of login-action whether action succed or not.
        /// </summary>
        /// <returns></returns>
        private async Task SetLoginPageStateIfNeed() {
            if (( thisPageType == DataFetchType.LNU_Index_Login || thisPageType == DataFetchType.LNU_Index_ReLogin ) && isFristNavigate) {

                // DO ASYNC WORK ... MAYBE AWAIT

                PasswordCheckBox.IsChecked = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsSavePassword) ?? false;
                AutoLoginCheckBox.IsChecked = (bool?)SettingsHelper.ReadSettingsValue(SettingsSelect.IsAutoLogin) ?? false;
                AutoLoginCheckBox.IsChecked = PasswordCheckBox.IsChecked ?? false ? AutoLoginCheckBox.IsChecked : false;
                AutoLoginCheckBox.IsEnabled = PasswordCheckBox.IsChecked ?? false;

                EmailBox.Text = (string)SettingsHelper.ReadSettingsValue(SettingsConstants.Email) ?? "";
                PasswordDecryption();

                Scroll.ScriptNotify += OnScriNotifypt;
            }
            try {
                if (isFristNavigate) { // if first comes, auto-login if need.
                    Submit.IsEnabled = Abort.IsEnabled = true;
                    if (AutoLoginCheckBox.IsChecked ?? false)
                        await ClickSubmitButtonIfAuto();
                    return;
                } // if not, ask the webview to send message back to window.
                await AskWebViewToCallback();
            } catch (Exception e) { // if any error throws, toast to foreground and clear the auto-login cache to prevent more errors.
                Debug.WriteLine(e.StackTrace);
                ReportHelper.ReportAttention(GetUIString("UnhandledError"));
                SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
            } finally { // anyway, mark the flag that the first coming is over, this page state will go to next step for receiving message from webview.
                isFristNavigate = false;
            }
        }

        #region Password Encryption & Decryption

        private void PasswordEncryption(string pass) {
            if (PasswordCheckBox.IsChecked ?? false) {
                try { // password encryption is over here.
                    var finalToSave = CipherEncryption(
                        pass,
                        SymmetricAlgorithmNames.AesCbcPkcs7,
                        256,
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
                    cryptographicKey = objAlg.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(collForKeyAndIv));
                    ibufferVector = CryptographicBuffer.CreateFromByteArray(collForKeyAndIv);

                    PasswordBox.Password = CipherDecryption( // decryption the message.
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

        #region Login Status Changed

        /// <summary>
        /// if login failed, re-navigate to the target Uri, otherwise, show status detail of you.
        /// </summary>
        /// <param name="htmlBodyContent">html of websites</param>
        private void CheckIfLoginSucceed(string htmlBodyContent) {
            var doc = new HtmlDocument();
            doc.LoadHtml(@"<html>
                                             <head>
                                             <title>......</title >
                                             <link href='style.css' rel='stylesheet' type='text/css'>
                                             <script language='JavaScript1.2' src='nocache.js'></script >
                                             </head><body>" + htmlBodyContent + "</body></html>");
            var rootNode = doc.DocumentNode;
            var studentStatus = rootNode.SelectSingleNode("//span[@class='t']");
            if (studentStatus == null) { // login failed, redirect to the login page.
                ReportHelper.ReportAttention(GetUIString("Login_Failed"));
                SettingsHelper.SaveSettingsValue(SettingsSelect.IsAutoLogin, false);
                isFristNavigate = true;
                RedirectToLoginAgain();
            } else { // login successful, save login status and show it.
                SaveLoginStatus(studentStatus);
                LoginPopup.IsOpen = false;
                SetVisibility(MainPopupGrid, false);
                Debug.WriteLine(fromNaviType);
                if (thisPageType == DataFetchType.LNU_Index_ReLogin) {
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
            if (thisPageType == DataFetchType.LNU_Index_ReLogin) {
                MainPage.Current.NavigateToBase?.Invoke(
                    this,
                    new NavigateParameter {
                        ToFetchType = DataFetchType.LNU_Index_ReLogin,
                        MessageBag = navigateTitle,
                        ToUri = currentUri,
                        NaviType = NavigateType.ReLogin
                    },
                    MainPage.InnerResources.GetFrameInstance(NavigateType.ReLogin),
                    typeof(LoginPage));
                return;
            }
            MainPage.Current.NavigateToBase?.Invoke(
                this,
                new NavigateParameter {
                    ToFetchType = DataFetchType.LNU_Index_Login,
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
        private void SaveLoginStatus(HtmlNode item) {
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
        }

        #endregion

        #region JS Founctions Cab

        /// <summary>
        /// insert id and password into webview from popup, and after that, click the submit button.
        /// </summary>
        /// <param name="user">your cache id</param>
        /// <param name="pass">your cache password</param>
        /// <returns></returns>
        private async Task InsertLoginMessage(string user, string pass) {
            try { // insert js and run it, so that we can insert message into the target place and click the submit button.
                var newJSFounction = $@"
                            var node_list = document.getElementsByTagName('input');
                                for (var i = 0; i < node_list.length; i++) {"{"}
                                var node = node_list[i];
                                    if (node.getAttribute('type') == 'submit') 
                                        node.click();
                                    if (node.getAttribute('type') == 'text') 
                                        node.innerText = '{user}';
                                    if (node.getAttribute('type') == 'password') 
                                        node.innerText = '{pass}';
                                {"}"} ";
                await Scroll.InvokeScriptAsync("eval", new[] { newJSFounction });
            } catch ( Exception ) { // if any error throws, reset the UI and report errer.
                Submit.IsEnabled = true;
                SubitRing.IsActive = false;
                ReportHelper.ReportAttention("Error");
            }
        }

        /// <summary>
        /// send message to windows so that we can get message of login-success whether or not.
        /// </summary>
        /// <returns></returns>
        private async Task AskWebViewToCallback() { // js to callback
            var js = @"window.external.notify(
                                    JSON.stringify(
                                        new Array (
                                            document.body.innerText,
                                            document.body.innerHTML)));";
            await Scroll.InvokeScriptAsync("eval", new[] { js });
        }

        #endregion

        #region AES CBC PKCS7

        public IBuffer CipherEncryption(
            string strMsg,
            string strAlgName,
            uint keyLength,
            out BinaryStringEncoding encoding,
            out IBuffer iv,
            out CryptographicKey key) { 
            iv = null;  // Initialize the initialization vector because some type encryptions do not need it.
            encoding = BinaryStringEncoding.Utf8;

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Open a symmetric algorithm provider for the specified algorithm. 
            var objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Determine whether the message length is a multiple of the block length.
            // This is not necessary for PKCS #7 algorithms which automatically pad the
            // message to an appropriate length.
            if (!strAlgName.Contains("PKCS7")) 
                if ((buffMsg.Length % objAlg.BlockLength) != 0) 
                    throw new Exception("Message buffer length must be multiple of block length.");

            // Create a symmetric key.
            // IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(keyLength);
            IBuffer keyMaterial = CryptographicBuffer.CreateFromByteArray(collForKeyAndIv);
            key = objAlg.CreateSymmetricKey(keyMaterial);

            // CBC algorithms require an initialization vector. Here, a random number is used for the vector.
            if (strAlgName.Contains("CBC")) 
                //iv = CryptographicBuffer.GenerateRandom(objAlg.BlockLength);   // drop it.
                iv = CryptographicBuffer.CreateFromByteArray(collForKeyAndIv);

            // Encrypt the data and return.
            IBuffer buffEncrypt = CryptographicEngine.Encrypt(key, buffMsg, iv);
            return buffEncrypt;
        }


        public string CipherDecryption(
            string strAlgName,
            IBuffer buffEncrypt,
            IBuffer iv,
            BinaryStringEncoding encoding,
            CryptographicKey key) {
            // Declare a buffer to contain the decrypted data.
            IBuffer buffDecrypted;

            // Open an symmetric algorithm provider for the specified algorithm. 
            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // The input key must be securely shared between the sender of the encrypted message
            // and the recipient. The initialization vector must also be shared but does not
            // need to be shared in a secure manner. If the sender encodes a message string 
            // to a buffer, the binary encoding method must also be shared with the recipient.
            buffDecrypted = CryptographicEngine.Decrypt(key, buffEncrypt, iv);

            // Convert the decrypted buffer to a string (for display). If the sender created the
            // original message buffer from a string, the sender must tell the recipient what 
            // BinaryStringEncoding value was used. Here, BinaryStringEncoding.Utf8 is used to
            // convert the message to a buffer before encryption and to convert the decrypted
            // buffer back to the original plaintext.
            return CryptographicBuffer.ConvertBinaryToString(encoding, buffDecrypted);
        }

        #endregion

        #endregion

        #region Properties and state
        public static LoginPage Current;
        private bool isDivideScreen = true;
        private bool isFristNavigate = true;
        private Uri currentUri;
        private DataFetchType thisPageType;
        private string navigateTitle;
        private Uri fromUri;
        private DataFetchType fromPageType;
        private string fromNavigateTitle;
        private NavigateType fromNaviType;
        private BinaryStringEncoding binaryStringEncoding;
        private IBuffer ibufferVector;
        private CryptographicKey cryptographicKey;
        private byte[] collForKeyAndIv = new byte[16] { 234, 123, 231, 44, 25, 16, 7, 68, 11, 206, 137, 44, 95, 67, 173, 108 };
        #endregion

    }
}
