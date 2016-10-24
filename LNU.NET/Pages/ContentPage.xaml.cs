using static Wallace.UWP.Helpers.Tools.UWPStates;

using LNU.Core.Tools;
using LNU.Core.Models;
using Wallace.UWP.Helpers.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Wallace.UWP.Helpers.Helpers;
using LNU.NET.Controls;

namespace LNU.NET.Pages {
    
    public sealed partial class ContentPage : BaseContentPage {
        public ContentPage() {
            this.InitializeComponent();
            Current = this;
        }

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            //    contentRing.IsActive = true;
            //    var args = e.Parameter as NavigateParameter;
            //    if (args == null) {
            //        contentRing.IsActive = false;
            //        return;
            //    }
            //    var source = DataProcess.GetPageInnerContent(
            //            (await WebProcess.GetHtmlResources(
            //                args.PathUri.ToString(), false))
            //                .ToString());
            //    navigateTitlePath.Text = source.Title;
            //    int Count = source.ContentImage.Count + source.ContentString.Count;
            //    for (int index = 1; index <= Count; index++) {
            //        object item = default(object);
            //        ContentType type =
            //            (item = source.ContentString.Find(i => i.Index == index)) != null ? ContentType.String :
            //            (item = source.ContentImage.Find(i => i.Index == index)) != null ? ContentType.Image :
            //            (item = source.ContentGif.Find(i => i.Index == index)) != null ? ContentType.Gif :
            //            (item = source.ContentVideo.Find(i => i.Index == index)) != null ? ContentType.Video :
            //            (item = source.ContentFlash.Find(i => i.Index == index)) != null ? ContentType.Flash :
            //            (item = source.ContentSelfUri.Find(i => i.Index == index)) != null ? ContentType.SelfUri :
            //            ContentType.None;

            //        switch (type) {
            //            case ContentType.String:
            //                var textBlock = new TextBlock {
            //                    Text = (item as ContentStrings).Content,
            //                    TextWrapping = TextWrapping.WrapWholeWords,
            //                    Margin = new Thickness(10, 5, 10, 5),
            //                };
            //                ContentStack.Children.Add(textBlock);
            //                break;

            //            case ContentType.Image:
            //                var grid = new Grid();
            //                grid.Children.Add(new Image {
            //                    Source = new BitmapImage((item as ContentImages).ImageSource),
            //                    Margin = new Thickness(10, 5, 10, 5),
            //                    Stretch = Stretch.UniformToFill,
            //                });
            //                var button = new Button {
            //                    HorizontalAlignment = HorizontalAlignment.Stretch,
            //                    VerticalAlignment = VerticalAlignment.Stretch,
            //                    Background = new SolidColorBrush(Windows.UI.Colors.Transparent),
            //                    Style = Application.Current.Resources["MainPageButtonBackHamburgerStyle"] as Style,
            //                };
            //                button.Click += (sender, clickArgs) => { MainPage.ShowImageInScreen((item as ContentImages).ImageSource); };
            //                grid.Children.Add(button);
            //                ContentStack.Children.Add(grid);
            //                break;

            //            default:break;
            //        }
            //    }
            //    contentRing.IsActive = false;
        }

        private void BaseHamburgerButton_Click(object sender, RoutedEventArgs e) {
            PageSlideOutStart(VisibleWidth > 800 ? false : true);
            Current = null;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            MainPage.Current.SetChildPageMargin(
                currentPage: this,
                matchNumber: VisibleWidth,
                isDivideScreen: isDivideScreen);
        }

        #endregion

        #region Methods

        #endregion

        #region Properties and state
        public static ContentPage Current;
        private enum ContentType { None = 0, String = 1, Image = 2, Gif = 3, Video = 4, Flash = 5, SelfUri = 6 }
        #endregion

    }
}
