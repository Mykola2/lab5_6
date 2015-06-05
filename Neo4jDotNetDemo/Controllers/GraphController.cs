using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Neo4jDotNetDemo.Controllers
{
    [RoutePrefix("graph")]
    public class GraphController : ApiController
    {
        [HttpGet]
        [Route("{limit:int?}", Name = "getgraph")]
        public IHttpActionResult Index(int limit = 100)
        {
            //query = ("MATCH (m:Movie)<-[:ACTED_IN]-(a:Person) "
            // "RETURN m.title as movie, collect(a.name) as cast "
            // "LIMIT {limit}")

            var query = WebApiConfig.GraphClient.Cypher
                .Match("(m:Document)<-[:HAS_DOC]-(a:Expert)")
                .Return((m, a) => new
                {
                    expert = a.As<Expert>().name,
                    documents = Return.As<string>("collect(m.releaseDate)")
                })
                .Limit(limit);

            //You can see the cypher query here when debugging
            var data = query.Results.ToList();

            var nodes = new List<NodeResult>();
            var rels = new List<object>();
            int i = 0, target;
            foreach (var item in data)
            {
                nodes.Add(new NodeResult { title = item.expert, label = "expert" });
                target = i;
                i++;
                if (!string.IsNullOrEmpty(item.documents))
                {
                    var casts = JsonConvert.DeserializeObject<JArray>(item.documents);
                    foreach (var cast in casts)
                    {
                        var source = nodes.FindIndex(c => c.title == cast.Value<string>());
                        if (source == -1)
                        {
                            nodes.Add(new NodeResult { title = cast.Value<string>(), label = "document" });
                            source = i;
                            i += 1;
                        }
                        rels.Add(new { source = source, target = target });
                    }
                }
            }

            return Ok(new { nodes = nodes, links = rels });
        }
    }

    public class NodeResult
    {
        public string title { get; set; }
        public string label { get; set; }
    }

    public class Expert
    {

        public int idExperts { get; set; }
        public string name { get; set; }
        public string workplace { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
    }
}
