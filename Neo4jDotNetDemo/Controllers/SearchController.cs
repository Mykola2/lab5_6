using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Neo4jDotNetDemo.Controllers
{
    [RoutePrefix("search")]
    public class SearchController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult SearchDicByRD(string q)
        {
            //    query = ("MATCH (movie:Movie) "
            //         "WHERE movie.title =~ {title} "
            //         "RETURN movie")
            //    params={"title": "(?i).*" + q + ".*"}

            var data = WebApiConfig.GraphClient.Cypher
               .Match("(m:Expert)")
               .Where("m.name =~ {name}")
               .WithParam("name", "(?i).*" + q + ".*")
               .Return<Expert>("m")
               .Results.ToList();

            return Ok(data.Select(c => new { expert = c}));
        }
    }
}
