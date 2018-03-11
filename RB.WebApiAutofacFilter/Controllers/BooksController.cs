using RB.WebApiAutofacFilter.Logging;
using System.Collections.Generic;
using System.Web.Http;

namespace RB.WebApiAutofacFilter.Controllers
{
    [LoggerFilter]
    public class BooksController : ApiController
    {
        // GET api/books
        public IEnumerable<string> Get()
        {
            return new string[] { "book1", "book2" };
        }
    }
}