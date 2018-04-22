using System.Collections.Generic;
using System.Web.Http;

namespace YellowServer.Controllers
{
    public class FeedController : ApiController
    {
        
        public FeedController()
        {
        }

        // POST feed/postarticle
        [HttpPost]
        [Route("feed/postarticle")]
        public IEnumerable<string> myAction()
        {
            return new[] { "value2" };
        }

        [HttpGet]
        [Route("feed/getfeed/{request}")]
        public IEnumerable<string> GetNewsFeed(string request)
        {
            return new[] { "value1", "value2" };
        }

    }
}
