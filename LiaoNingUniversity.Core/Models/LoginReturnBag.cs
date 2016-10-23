using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace LNU.Core.Models {
    public class LoginReturnBag {
        public string HtmlResouces { get; set; }
        public HttpCookieCollection CookieBag { get; set; }
    }
}
