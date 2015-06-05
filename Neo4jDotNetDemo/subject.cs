using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Neo4jDotNetDemo
{
    public class subject
    {
        public subject()
        {
            this.poas = new HashSet<poa>();
        }

        public int idSubject { get; set; }
        public int Type { get; set; }
        public Nullable<int> SerId { get; set; }
        public Nullable<int> RegId { get; set; }
        public string Info { get; set; }

        [JsonIgnore]
        public virtual ICollection<poa> poas { get; set; }
    }
}