using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Neo4jDotNetDemo
{
    public class person
    {
        public person()
        {
            this.poas = new HashSet<poa>();
            this.suspentions = new HashSet<suspention>();
        }

        public int idPerson { get; set; }
        public string Name { get; set; }
        public string Middlename { get; set; }
        public string LastName { get; set; }
        public int Code { get; set; }
        public PersonType Type { get; set; }


        [JsonIgnore]
        public virtual ICollection<poa> poas { get; set; }
        [JsonIgnore]
        public virtual ICollection<suspention> suspentions { get; set; }
    }
}