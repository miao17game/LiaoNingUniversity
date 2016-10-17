using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNU.Core.Models.NavigationModel {
    public class NavigationBar {

        #region Fields
        private DataFetchType dataType = DataFetchType.NULL;
        #endregion

        public string Title { get; set; }
        public Uri PathUri { get; set; }
        public NavigateType NaviType { get; set; }
        public DataFetchType FetchType {
            get { return dataType; }
            set { dataType = value; }
        }
    }
}
