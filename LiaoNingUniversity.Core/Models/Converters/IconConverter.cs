using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

using static Wallace.UWP.Helpers.Tools.UWPStates;

namespace LNU.Core.Models.Converters {
    public class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ToIconCode(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        private string ToIconCode(string title) {
            return 
                title == GetUIString("LNU_Index") ? char.ConvertFromUtf32(0xE10F) :
                title == GetUIString("LNU_Search_Query") ? char.ConvertFromUtf32(0xE187) :
                title == GetUIString("LNU_For_Teacher") ? char.ConvertFromUtf32(0xE181) :
                title == GetUIString("LNU_G_E") ? char.ConvertFromUtf32(0xE190) :
                title == GetUIString("LNU_S_T") ? char.ConvertFromUtf32(0xE706) :
                title == GetUIString("LNU_T_E") ? char.ConvertFromUtf32(0xE2B2) :
                title == GetUIString("LNU_R_R") ? char.ConvertFromUtf32(0xE7B8) :
                title == GetUIString("LNU_P_C") ? char.ConvertFromUtf32(0xE094) :
                title == GetUIString("LNU_C_I") ? char.ConvertFromUtf32(0xE81E) :
                title == GetUIString("LNU_T_I") ? char.ConvertFromUtf32(0xE914) :
                title == GetUIString("LNU_C_A") ? char.ConvertFromUtf32(0xEC15) :
                title == GetUIString("LNU_P_G") ? char.ConvertFromUtf32(0xEF15) :
                title == GetUIString("LNU_T_O_N") ? char.ConvertFromUtf32(0xEE92) :
                title == GetUIString("LNU_A_A_O") ? char.ConvertFromUtf32(0xE707) :
                title == GetUIString("LNU_U_H_P") ? char.ConvertFromUtf32(0xEC08) :
                char.ConvertFromUtf32(0xE1F6);
        }
    }
}
