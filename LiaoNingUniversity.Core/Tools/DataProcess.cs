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
using LNU.Core.Models.ContentModels;
using DQD.Core.Tools.PersonalExpressions;

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

        public static List<ScheduleItem> FetchScheduleListFromHtml(string htmlResources) {
            var list = new List<ScheduleItem>();
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(EscapeReplace.ToEscape(htmlResources));
                var rootNode = doc.DocumentNode;
                var target = rootNode
                    .SelectSingleNode("//body[@topmargin='5']")
                    .SelectNodes("table[@bgcolor='#EAE2F3']").ElementAt(1)
                    .SelectSingleNode("tr")
                    .SelectSingleNode("td")
                    .SelectSingleNode("table[@bgcolor='#F2EDF8']")
                    .SelectNodes("tr");
                
                foreach (var tr in target) {
                    var tds = tr.SelectNodes("td").ToList();
                    try {
                        list.Add(new ScheduleItem {
                            Title = tds[0].InnerText,
                            Description = tds[1].InnerText,
                            CourseID = tds[2].InnerText,
                            SerialNumber = tds[3].InnerText,
                            CourceProperty = tds[4].InnerText,
                            ExamType = tds[5].InnerText,
                            Place = tds[6].InnerText,
                            Time = tds[7].InnerText,
                            WeeklyRound = tds[8].InnerText,
                        });
                    } catch { /* ignore */ }
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
                return list;
            }
            return list;
        }

        public static List<ScheduleTip> FetchScheduleTableFromHtml(string htmlResources) {
            var list = new List<ScheduleTip>();
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(EscapeReplace.ToEscape(htmlResources));
                var rootNode = doc.DocumentNode;

                var target = rootNode
                    .SelectSingleNode("//body[@topmargin='5']")
                    .SelectNodes("table[@bgcolor='#EAE2F3']").ElementAt(0)
                    .SelectSingleNode("tr")
                    .SelectSingleNode("td")
                    .SelectSingleNode("table[@bgcolor='#F2EDF8']")
                    .SelectNodes("tr");

                int num = 0;
                foreach (var tr in target) {
                    var tds = tr.SelectNodes("td").ToList();
                    int lick = 0;
                    if (num > 0) 
                        foreach (var td in tds) 
                            try { if (lick > 0) 
                                    if(td.InnerText!="" && td.InnerText.Substring(1, td.InnerText.Length-1)!="")
                                        list.Add(new ScheduleTip {
                                            WholeTitle = td.InnerText.Substring(1, td.InnerText.Length - 2),
                                            Row = num,
                                            Column = lick
                                        });
                                lick++;
                            } catch { /* ignore */ }
                    num++;
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
                return list;
            }
            return list;
        }

        public static CourseCalender FetchCourseCalenderFromHtml(string htmlResources) {
            var cc = new CourseCalender();
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(EscapeReplace.ToEscape(htmlResources));
                var rootNode = doc.DocumentNode;

                var target = rootNode
                    .SelectSingleNode("//table[@width='490']")
                    .SelectNodes("tr");

                cc.PreSelectCS = target[1].SelectNodes("td").ElementAt(1).InnerText;
                cc.PreSelectPH = target[1].SelectNodes("td").ElementAt(2).InnerText;
                cc.SelectCS = target[2].SelectNodes("td").ElementAt(1).InnerText;
                cc.SelectPH = target[2].SelectNodes("td").ElementAt(2).InnerText;
                cc.CoverSelect = target[3].SelectNodes("td").ElementAt(1).InnerText;
                cc.QueryDate = target[4].SelectNodes("td").ElementAt(1).InnerText;

            } catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
                return cc;
            }
            return cc;
        }

    }
}
