using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Neo4jDotNetDemo.Controllers
{
    [RoutePrefix("Expert")]
    public class ExpertController : ApiController
    {
        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult GetExpertByname(string name)
        {
            //query = ("MATCH (movie:Movie {title:{title}}) "
            // "OPTIONAL MATCH (movie)<-[r]-(person:Person) "
            // "RETURN movie.title as title,"
            // "collect([person.name, "
            // "         head(split(lower(type(r)), '_')), r.roles]) as cast "
            // "LIMIT 1")

            var data = WebApiConfig.GraphClient.Cypher
               .Match("(expert:Expert {name:{name}})")
               .OptionalMatch("(document)<-[r:HAS_DOC]-(expert:Expert)")
               .WithParam("name", name)
               .Return((expert, a) => new
               {
                   expert = expert.As<Expert>().name,
                   documents = Return.As<IEnumerable<string>>("collect([document.releaseDate,document.endDate,document.type, head(split(lower(type(r)), '_')), r.HAS_DOC])")
               })
               .Limit(1)
               .Results.FirstOrDefault();

            var data_notary = WebApiConfig.GraphClient.Cypher
               .Match("(expert:Expert {name:{name}})")
               .OptionalMatch("(Notary)<-[r:HAS_notary]-(expert:Expert)")
               .WithParam("name", name)
               .Return((expert, a) => new
               {
                   expert = expert.As<Expert>().name,
                   nonaties = Return.As<IEnumerable<string>>("collect([Notary.name, head(split(lower(type(r)), '_')), r.HAS_notary])")
               })
               .Limit(1)
               .Results.FirstOrDefault();

            var result = new ExpertResult();
           // result.title = data.;

            var castresults = new List<DocumentsResult>();
            foreach (var item in data.documents)
            {
                var tempData = JsonConvert.DeserializeObject<dynamic>(item);
                var roles = tempData[4] as JArray;
                var castResult = new DocumentsResult
                {
                    releaseDate = tempData[0],
                    endDate = tempData[1],
                    DocumentType = tempData[2],
                };
                castresults.Add(castResult);
            }
            result.cast = castresults;


            var notaryresults = new List<NotaryResult>();
            foreach (var item in data_notary.nonaties)
            {
                var tempData = JsonConvert.DeserializeObject<dynamic>(item);
                var roles = tempData[1] as JArray;
                var notaryResult = new NotaryResult
                {
                    name = tempData[0]
                };
                notaryresults.Add(notaryResult);
            }
            result.notaries = notaryresults;
            return Ok(result);
        }
    }

    public class DocumentsResult
    {
        public string releaseDate  { get; set; }
        public string endDate { get; set; }

        public string DocumentType { get; set; }
        
    }

    public class NotaryResult
    {
        public string name { get; set; }
        

    }

    public class ExpertResult
    {
        public string name { get; set; }
        public IEnumerable<DocumentsResult> cast { get; set; }
        public IEnumerable<NotaryResult> notaries { get; set; }
    }
}
