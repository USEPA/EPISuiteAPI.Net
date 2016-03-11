using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;

namespace EPISuiteAPI.Controllers
{
    [RoutePrefix("episuite")]
    public class TestController : ApiController
    {
        [Route("estimated/boilingPtDegC")]
        [HttpGet]
        public HttpResponseMessage Test1()
        {
            return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Test1: ");
        }

        [Route("estimated/meltingPtDegC")]
        [HttpGet]
        public HttpResponseMessage Test2(string type, string property)
        {
            return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Test2: ");
        }
    }
}
