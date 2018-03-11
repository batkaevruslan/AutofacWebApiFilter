using RB.WebApiAutofacFilter.Logging;
using System.Collections.Generic;
using System.Web.Http;

namespace RB.WebApiAutofacFilter.Controllers
{
    public class AuthorsController : ApiController
    {
        // GET api/authors
        [LoggerFilter]
        public IEnumerable<string> Get()
        {
            return new string[] { "author1", "author2" };
        }        
    }
}