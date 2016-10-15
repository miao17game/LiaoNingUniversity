using Edi.UWP.Helpers;
using Wallace.UWP.Helpers.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using static Wallace.UWP.Helpers.Tools.UWPStates;
using static LiaoNingUniversity.NET.Pages.SettingsPage.InsideResources;
using LiaoNingUniversity.NET.Tools;
using LiaoNingUniversity.NET.Controls;

namespace LiaoNingUniversity.NET.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page {
        public SettingsPage() {
            this.InitializeComponent();
            Current = this;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            VersionMessage.Text = GetUIString("VersionMessage") + Utils.GetAppVersion();
            ThemeSwitch.IsOn = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsDarkThemeOrNot) ?? true;
            LanguageCombox.SelectedItem = GetComboItemFromTag((string)SettingsHelper.ReadSettingsValue(SettingsSelect.Language) ?? "zh-CN");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            MainPage.ChangeTitlePath(2, GetUIString("SettingsString"));
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if ((sender as Pivot).SelectedIndex == 0) {
                MainPage.ChangeTitlePath(3, null);
                return;
            }
            MainPage.ChangeTitlePath(
                3, (e.AddedItems.FirstOrDefault() as PivotItem).Header as string != GetUIString("SettingsString") ? 
                (e.AddedItems.FirstOrDefault() as PivotItem).Header as string :
                null);
        }

        private async void FeedBackBtn_Click(object sender, RoutedEventArgs e) {
            await ReportError(null, "N/A", true);
        }

        /// <summary>
        /// ReportError Method
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="msg"></param>
        /// <param name="pageSummary"></param>
        /// <param name="includeDeviceInfo"></param>
        /// <returns></returns>
        public static async Task ReportError(string msg = null, string pageSummary = "N/A", bool includeDeviceInfo = true) {
            var deviceInfo = new EasClientDeviceInformation();

            string subject = GetUIString("Feedback_Subject");
            string body = $"{GetUIString("Feedback_Body")}：{msg}  " +
                          $"（{GetUIString("Feedback_Version")}：{Utils.GetAppVersion()} ";

            if (includeDeviceInfo) {
                body += $", {GetUIString("Feedback_FriendlyName")}：{deviceInfo.FriendlyName}, " +
                          $"{GetUIString("Feedback_OS")}：{deviceInfo.OperatingSystem}, " +
                          $"SKU：{deviceInfo.SystemSku}, " +
                          $"{GetUIString("Feedback_SPN")}：{deviceInfo.SystemProductName}, " +
                          $"{GetUIString("Feedback_SMF")}：{deviceInfo.SystemManufacturer}, " +
                          $"{GetUIString("Feedback_SFV")}：{deviceInfo.SystemFirmwareVersion}, " +
                          $"{GetUIString("Feedback_SHV")}：{deviceInfo.SystemHardwareVersion}）";
            } else {
                body += ")";
            }

            string to = "miao17game@qq.com";
            await Tasks.OpenEmailComposeAsync(to, subject, body);
        }

        private void ThemeSwitch_Toggled(object sender, RoutedEventArgs e) {
            GetSwitchHandler((sender as ToggleSwitch).Name)
               .Invoke((sender as ToggleSwitch).Name);
        }

        private void OnThemeSwitchToggled(ToggleSwitch sender) {
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsDarkThemeOrNot, sender.IsOn);
            MainPage.Current.RequestedTheme = sender.IsOn ? ElementTheme.Dark : ElementTheme.Light;
        }

        internal static class InsideResources {

            public static ComboBoxItem GetComboItemInstance(string buttonName) { return comboItemsMaps.ContainsKey(buttonName) ? comboItemsMaps[buttonName] : null; }
            public static string GetLanguageTag(ComboBoxItem button) { return languageSaveTagsMaps.ContainsKey(button) ? languageSaveTagsMaps[button] : null; }
            public static ComboBoxItem GetComboItemFromTag(string tag) { return languageSaveTagsMaps.ContainsValue(tag) ? languageSaveTagsMaps.Where(i => i.Value == tag).FirstOrDefault().Key : null; }
            static private Dictionary<string, ComboBoxItem> comboItemsMaps = new Dictionary<string, ComboBoxItem> {
            {Current.enUSSelect.Name,Current.enUSSelect},
            {Current.zhCNSelect.Name,Current.zhCNSelect},
        };
            static private Dictionary<ComboBoxItem, string> languageSaveTagsMaps = new Dictionary<ComboBoxItem, string> {
            {Current.enUSSelect,"en-US"},
            {Current.zhCNSelect,"zh-CN"},
        };

            public static ToggleSwitch GetSwitchInstance(string str) { return switchSettingsMaps.ContainsKey(str) ? switchSettingsMaps[str] : null; }
            static private Dictionary<string, ToggleSwitch> switchSettingsMaps = new Dictionary<string, ToggleSwitch> {
            {Current.ThemeSwitch.Name,Current.ThemeSwitch},
        };

            public static SwitchEventHandler GetSwitchHandler(string switchName) { return switchHandlerMaps.ContainsKey(switchName) ? switchHandlerMaps[switchName] : null; }
            static private Dictionary<string, SwitchEventHandler> switchHandlerMaps = new Dictionary<string, SwitchEventHandler> {
            {Current.ThemeSwitch.Name, new SwitchEventHandler(instance=> { Current.OnThemeSwitchToggled(GetSwitchInstance(instance)); }) },
        };

        }

        #region Properties and state
        public static SettingsPage Current;
        private bool InitViewOrNot = true;
        public delegate void SwitchEventHandler(string instance);
        #endregion

        private void LanguageCombox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SettingsHelper.SaveSettingsValue(
                SettingsSelect.Language, 
                GetLanguageTag(
                    GetComboItemInstance(
                        (e.AddedItems.FirstOrDefault() as ComboBoxItem)
                        .Name as string)));
            if (InitViewOrNot) { InitViewOrNot = false; return; }
            new ToastSmooth(GetUIString("ReStartToChangeLanguage")).Show();
        }
    }
}
