using DBCSCodePage;
using Wallace.UWP.Helpers.Controls;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

using static Wallace.UWP.Helpers.Tools.UWPStates;
using LNU.Core.Models.NavigationModel;

namespace LNU.Core.Tools {
    public static class DataProcess {
        #region Properties and State
        
        #endregion

        public static async void ReportErrorAsBase(string erroeMessage) {
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                new ToastSmoothBase($"{GetUIString("FetchDataError")}\n" +erroeMessage).Show();
            });
        }

        public static async void ReportExceptionAsBase(string erroeMessage) {
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                new ToastSmoothBase(erroeMessage).Show();
            });
        }

        public static Uri ConvertToUri(string str) { return !string.IsNullOrEmpty(str) ? new Uri(str) : null; }

        public static List<NavigationBar> FetchLNUIndexFromHtml(string htmlResources) {
            
            var list = new List<NavigationBar>();

            return list;
        }

        #region Inner Methods
        private static string ToGlobalization(string input) {
            var result = default(string);
            switch (input) {
                case "首页":
                    result = "ENRZ.COM";
                    break;
                case "尤物":
                    result = GetUIString("Stunner");
                    break;
                case "资讯":
                    result = GetUIString("Information");
                    break;
                case "恋物癖":
                    result = GetUIString("LoveOfHabit");
                    break;
                case "时装":
                    result = GetUIString("Fashion");
                    break;
                case "男性人物":
                    result = GetUIString("MaleCharacters");
                    break;
                case "专题":
                    result = GetUIString("Topics");
                    break;
                case "美图":
                    result = GetUIString("Gallery");
                    break;
                case "商城":
                    result = GetUIString("Mall");
                    break;
                case "封面明星":
                    result = GetUIString("CoverStars");
                    break;
                case "新闻女郎":
                    result = GetUIString("NewsGirl");
                    break;
                case "猎奇":
                    result = GetUIString("See");
                    break;
                case "体育":
                    result = GetUIString("Sports");
                    break;
                case "地球村":
                    result = GetUIString("TheGlobalVillage");
                    break;
                case "艺和团":
                    result = GetUIString("Art");
                    break;
                case "汽车":
                    result = GetUIString("Cars");
                    break;
                case "数码":
                    result = GetUIString("Digital");
                    break;
                case "腕表":
                    result = GetUIString("Watch");
                    break;
                case "美酒":
                    result = GetUIString("Wine");
                    break;
                case "户外":
                    result = GetUIString("Outdoor");
                    break;
                case "搭配栏目":
                    result = GetUIString("Collocation");
                    break;
                case "男装大片":
                    result = GetUIString("Men_large");
                    break;
                case "美容护肤":
                    result = GetUIString("SkinCare");
                    break;
                case "后雅皮":
                    result = GetUIString("Yuppie");
                    break;
                case "男人帮":
                    result = GetUIString("Men");
                    break;
                case "甲方乙方":
                    result = GetUIString("A_B");
                    break;
                case "美女":
                    result = GetUIString("Beauty");
                    break;
                case "性感写真":
                    result = GetUIString("Photographic");
                    break;
                case "香车美女":
                    result = GetUIString("RC");
                    break;
                case "体育宝贝":
                    result = GetUIString("SportsBaby");
                    break;
                case "时尚":
                    result = GetUIString("Fashions");
                    break;
                case "秀场":
                    result = GetUIString("Show");
                    break;
                case "大片":
                    result = GetUIString("Swaths");
                    break;
                case "搭配":
                    result = GetUIString("mix");
                    break;
                case "娱乐":
                    result = GetUIString("Ent");
                    break;
                case "八卦热点":
                    result = GetUIString("HotGossip");
                    break;
                case "热辣吐槽":
                    result = GetUIString("Complain");
                    break;
                case "玩物":
                    result = GetUIString("Plaything");
                    break;
                case "座驾":
                    result = GetUIString("Car");
                    break;
                case "摄影":
                    result = GetUIString("Photography");
                    break;
                default:
                    result = input;
                    break;
            }
            return result;
        }

        #endregion

    }
}
