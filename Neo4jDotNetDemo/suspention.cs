using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Neo4jDotNetDemo
{
    public class suspention
    {
        public suspention()
        {
            this.poas = new HashSet<poa>();
        }

        public int idSuspention { get; set; }
        public string NameOfDocument { get; set; }
        public int idNotary { get; set; }
        public int idPerson { get; set; }
        public System.DateTime DateOfDocument { get; set; }

        public virtual notary notary { get; set; }
        public virtual person person { get; set; }
        public virtual ICollection<poa> poas { get; set; }
    }
}