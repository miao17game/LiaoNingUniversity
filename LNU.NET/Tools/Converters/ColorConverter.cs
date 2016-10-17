using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

using static Wallace.UWP.Helpers.Tools.UWPStates;

namespace LNU.NET.Tools.Converters {
    public class ColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ToColorSolidBrush(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        private Brush ToColorSolidBrush(string title) {
            SolidColorBrush result = new SolidColorBrush();
            result.Color = 
                title == GetUIString("LNU_Index") ? Color.FromArgb(255, 75, 21, 173) :
                title == GetUIString("LNU_Search_Query") ? Color.FromArgb(255, 217, 6, 94) :
                title == GetUIString("LNU_For_Teacher") ? Color.FromArgb(255, 60, 188, 98) :
                title == GetUIString("LNU_G_E") ? Color.FromArgb(255, 97, 17, 171) :
                title == GetUIString("LNU_S_T") ? Color.FromArgb(255, 254, 183, 8) :
                title == GetUIString("LNU_T_E") ? Color.FromArgb(255, 69, 90, 172) :
                title == GetUIString("LNU_R_R") ? Color.FromArgb(255, 141, 4, 33) :
                title == GetUIString("LNU_P_C") ? Color.FromArgb(255, 244, 78, 97) :
                title == GetUIString("LNU_C_I") ? Color.FromArgb(255, 255, 193, 63) :
                title == GetUIString("LNU_T_I") ? Color.FromArgb(255, 49, 199, 155) :
                title == GetUIString("LNU_C_A") ? Color.FromArgb(255, 255, 63, 138) :
                title == GetUIString("LNU_P_G") ? Color.FromArgb(255, 255, 120, 63) :
                title == GetUIString("LNU_T_O_N") ? Color.FromArgb(255, 255, 67, 63) :
                title == GetUIString("LNU_A_A_O") ? Color.FromArgb(255, 222, 135, 119) :
                title == GetUIString("LNU_U_H_P") ? Color.FromArgb(255, 53, 132, 154) :
                Color.FromArgb(255, 82, 82, 82);
            return result;
        }
    }
}
