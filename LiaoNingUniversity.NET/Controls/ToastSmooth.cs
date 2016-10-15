using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallace.UWP.Helpers.Controls;
using Wallace.UWP.Helpers.Helpers;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LiaoNingUniversity.NET.Controls {
    public sealed class ToastSmooth : ToastSmoothBase {

        public ToastSmooth() {
            var isDark = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsDarkThemeOrNot) ?? false;
            ToastBackground = !isDark ? 
                new SolidColorBrush(Color.FromArgb(255, 67, 104, 203)) : 
                new SolidColorBrush(Color.FromArgb(255, 202, 0, 62));
            ToastForeground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// 构造特定时间的Toast
        /// </summary>
        /// <param name="content"></param>
        /// <param name="showTime"></param>
        public ToastSmooth(string content, TimeSpan showTime) : this() {
            this.TextContent = content;
            this.WholeTime = showTime;
        }

        /// <summary>
        /// 默认构造两秒的Toast
        /// </summary>
        /// <param name="content"></param>
        public ToastSmooth(string content) : this(content, TimeSpan.FromSeconds(2)) { }

    }
}
