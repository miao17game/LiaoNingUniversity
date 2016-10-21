using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNU.Core.Models {
    public class ReturnParameter {
        public Uri FromUri { get; set; }
        public DataFetchType FromFetchType { get; set; }
        public object ReturnMessage { get; set; }
        public NavigateType FromNaviType { get; set; }
    }
}
