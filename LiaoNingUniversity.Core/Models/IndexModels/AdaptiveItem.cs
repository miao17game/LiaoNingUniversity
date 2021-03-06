﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace LNU.Core.Models.IndexModels {
    public class AdaptiveItem {

        #region Properties
        private DataFetchType dataType = DataFetchType.NULL;
        private NavigateType naviType = NavigateType.Webview;
        #endregion

        public string ItemIcon { get; set; }
        public string ItemTitle { get; set; }
        public Uri PathUri { get; set; }
        public string Description { get; set; }
        public Brush Background { get; set; }
        public Brush IconForeground { get; set; }
        public Brush TitleForeground { get; set; }
        public Uri BackImage { get; set; }
        public NavigateType NaviType {
            get { return naviType; }
            set { naviType = value; }
        }
        public DataFetchType DataType {
            get { return dataType; }
            set { dataType = value; }
        }
    }
}
