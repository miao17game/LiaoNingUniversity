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
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Wallace.UWP.Helpers.Helpers;

namespace LNU.NET.Pages.FeaturesPages {

    public sealed partial class SchedulePage : BaseContentPage {

        #region Constructor

        public SchedulePage() {
            this.InitializeComponent();
            Current = this;
            TableBackImage.Source = new BitmapImage(
                MainPage.Current.RequestedTheme == ElementTheme.Dark ?
                new Uri("ms-appx:///Assets/15.jpg") :
                new Uri("ms-appx:///Assets/10.jpg"));
        }

        #endregion

        #region Events

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

            var cache = await CacheHelpers.ReadSpecificCacheValue(MainPage.LoginCache.UserID);
            var htmlResources = cache ?? await LNUWebProcess.GetLNUFromLeftRequest(MainPage.LoginClient, args.ToUri.ToString());
            if (cache == null && htmlResources != null)
                await CacheHelpers.SaveSpecificCacheValue(MainPage.LoginCache.UserID, htmlResources);
            var nameList = DataProcess.FetchScheduleTableFromHtml(htmlResources);

            // Show the schedule table
            InitTableView(nameList);

            var list = DataProcess.FetchScheduleListFromHtml(htmlResources);
            ScheduleQueue.Clear();
            ScheduleQueue.AddRange(list);
            nameList.ForEach(item => { // finish lecture message into ScheduleQueue items .
                ScheduleQueue.FindAll(i => i.Title == GetSingleTitle(item.WholeTitle)).ForEach(s => { if (s.Lecturer == null) s.Lecturer = GetLecturerName(item.WholeTitle); });
            });

            CourseListView.ItemsSource = ScheduleQueue;

            contentRing.IsActive = false;
        }
        
        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
            Current = null;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
            var item = e.ClickedItem as ScheduleItem;
            PopupBackground.Background = Application.Current.Resources["ENRZForeground02"] as Brush;
            SetPopupOpenStatus(item);
        }

        private void popup_SizeChanged(object sender, SizeChangedEventArgs e) {
            PopupBackground.Width = (sender as Popup).ActualWidth;
            PopupBackground.Height = (sender as Popup).ActualHeight;
        }

        private void popup_Closed(object sender, object e) {
            SetVisibility(popupBorder, false);
            OutBorder.Begin();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e) {
            contentRing.IsActive = true;

            var htmlResources = await LNUWebProcess.GetLNUFromLeftRequest(MainPage.LoginClient, currentUri.ToString());
            await CacheHelpers.SaveSpecificCacheValue(MainPage.LoginCache.UserID, htmlResources);
            var nameList = DataProcess.FetchScheduleTableFromHtml(htmlResources);

            TableGrid.Children.Clear();
            // Show the schedule table
            InitTableView(nameList);

            var list = DataProcess.FetchScheduleListFromHtml(htmlResources);
            ScheduleQueue.Clear();
            ScheduleQueue.AddRange(list);
            nameList.ForEach(item => { // finish lecture message into ScheduleQueue items .
                ScheduleQueue.FindAll(i => i.Title == GetSingleTitle(item.WholeTitle)).ForEach(s => { if (s.Lecturer == null) s.Lecturer = GetLecturerName(item.WholeTitle); });
            });

            CourseListView.ItemsSource = ScheduleQueue;

            contentRing.IsActive = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Open methods to change state when the theme mode changed.
        /// </summary>
        public void ChangeStateByRequestTheme() {

        }

        private void InitTableView(List<Core.Models.ContentModels.ScheduleTip> nameList) {
            var random = new Random();
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    var find = nameList.FindIndex(item => item.Row == row && item.Column == column);
                    var newTip = find == -1 ?
                         new Controls.ScheduleTip { // the (row, column) has no course to set in.
                             Background = (row == 0 || column == 0) ?
                             TableFrameBackColor :
                             TableContentBackColor,
                             ClickVisible = Visibility.Collapsed,
                         } :
                        new Controls.ScheduleTip {
                            Background = Background = GetBackgroundRandom(random.Next() % 16),
                            TipTitle = GetSingleTitle(nameList[find].WholeTitle),
                            TipLecture = GetLecturerName(nameList[find].WholeTitle),
                            ClickVisible = Visibility.Visible,
                            Tag = column + "-" + row,
                        };
                    if (find != -1) {
                        newTip.InnerButton.CommandParameter = newTip.Tag;
                        newTip.InnerButton.Click += (sender, args) => {
                            PopupBackground.Background = newTip.Background;
                            SetPopupOpenStatus(ScheduleQueue.Find(i => { return i.Time == (sender as Button).CommandParameter as string; }));
                        };
                    }
                    if (column == 0)
                        newTip.Width = 50;
                    if (row == 0)
                        newTip.Height = 50;
                    if (row == 0 && column > 0)
                        newTip.TipFront = GetWeekDay(column);
                    if (row > 0 && column == 0)
                        newTip.TipFront = row + "";
                    TableGrid.Children.Add(newTip);
                    Grid.SetRow(newTip, row);
                    Grid.SetColumn(newTip, column);
                }
            }
        }

        private void SetPopupOpenStatus(ScheduleItem item) {
            TX01.Text = item.Title;
            TX02.Text = item.Lecturer;
            TX03.Text = item.CourseID;
            TX04.Text = item.SerialNumber;
            TX05.Text = item.CourceProperty;
            TX06.Text = item.ExamType;
            TX07.Text = item.Place;
            TX08.Text = item.Time;
            TX09.Text = item.WeeklyRound;
            popup.IsOpen = true;
            SetVisibility(popupBorder, true);
            EnterBorder.Begin();
        }

        private string GetWeekDay(int num) {
            return num == 1 ? GetUIString("Monday") :
                num == 2 ? GetUIString("Tuesday") :
                num == 3 ? GetUIString("Wednesday") :
                num == 4 ? GetUIString("Thursday") :
                num == 5 ? GetUIString("Friday") :
                num == 6 ? GetUIString("Saturday") :
                GetUIString("Sunday");
        }

        private SolidColorBrush GetBackgroundRandom(int title) {
            SolidColorBrush result = new SolidColorBrush();
            result.Color =
                title == 0 ? Color.FromArgb(255, 244, 78, 97) :
                title == 1 ? Color.FromArgb(255, 255, 193, 63) :
                title == 2 ? Color.FromArgb(255, 49, 199, 155) :
                title == 3 ? Color.FromArgb(255, 255, 63, 138) :
                title == 4 ? Color.FromArgb(255, 255, 120, 63) :
                title == 5 ? Color.FromArgb(255, 255, 67, 63) :
                title == 6 ? Color.FromArgb(255, 222, 135, 119) :
                title == 7 ? Color.FromArgb(255, 53, 132, 154) :
                title == 8 ? Color.FromArgb(255, 75, 21, 173) :
                title == 9 ? Color.FromArgb(255, 217, 6, 94) :
                title == 10 ? Color.FromArgb(255, 60, 188, 98) :
                title == 11 ? Color.FromArgb(255, 97, 17, 171) :
                title == 12 ? Color.FromArgb(255, 254, 183, 8) :
                title == 13 ? Color.FromArgb(255, 69, 90, 172) :
                title == 14 ? Color.FromArgb(255, 141, 4, 33) :
                Color.FromArgb(255, 82, 82, 82);
            return result;
        }

        #endregion

        #region Inside Resources

        internal static class InsideMapHelper {

            #region Schedule table source

            public static List<ScheduleItem> ScheduleQueue { get { return scheduleList; } }
            static private List<ScheduleItem> scheduleList = new List<ScheduleItem> { };

            internal static string GetLecturerName(string wholeTitle) {
                return wholeTitle.Replace("/", "@").Split('@')[1];
            }

            internal static string GetSingleTitle(string wholeTitle) {
                return wholeTitle.Replace("(", "@").Split('@')[0];
            }

            #endregion

        }

        #endregion

        #region Properties
        public static SchedulePage Current;
        public SolidColorBrush TableFrameBackColor = MainPage.Current.RequestedTheme == ElementTheme.Dark ? BackFrameColorDark : BackFrameColorLight;
        public SolidColorBrush TableContentBackColor = MainPage.Current.RequestedTheme == ElementTheme.Dark ? ContentBackDark : ContentBackLight;
        public static SolidColorBrush BackFrameColorDark = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));
        public static SolidColorBrush BackFrameColorLight = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        public static SolidColorBrush ContentBackDark = new SolidColorBrush(Color.FromArgb(30, 240, 240, 240));
        public static SolidColorBrush ContentBackLight = new SolidColorBrush(Color.FromArgb(30, 32, 32, 32));
        #endregion

    }
}
