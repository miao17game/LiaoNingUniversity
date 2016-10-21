using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNU.Core.Models {
    public class NavigateParameter {
        public Uri ToUri { get; set; }
        public DataFetchType ToFetchType { get; set; }
        public List<BarItemModel> Items { get; set; }
        public object MessageBag { get; set; }
        public NavigateType NaviType { get; set; }
        public ReturnParameter MessageToReturn { get; set; }
    }
}
