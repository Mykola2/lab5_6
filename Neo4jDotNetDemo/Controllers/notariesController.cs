using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using kpz4;
using System.Web.Http.Cors;

namespace kpz4.Controllers
{
     [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class notariesController : ApiController
    {
        private RegisterEntities db = new RegisterEntities();

        // GET: api/notaries
        public IQueryable<notary> Getnotarys()
        {
            db.Configuration.ProxyCreationEnabled = false;

            return db.notarys;
        }

        // GET: api/notaries/5
        [ResponseType(typeof(notary))]
        public IHttpActionResult Getnotary(int id)
        {
            notary notary = db.notarys.Find(id);
            if (notary == null)
            {
                return NotFound();
            }

            return Ok(notary);
        }

        // PUT: api/notaries/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putnotary(int id, notary notary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != notary.idNotary)
            {
                return BadRequest();
            }

            db.Entry(notary).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!notaryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/notaries
        [ResponseType(typeof(notary))]
        public IHttpActionResult Postnotary(notary notary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.notarys.Add(notary);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = notary.idNotary }, notary);
        }

        // DELETE: api/notaries/5
        [ResponseType(typeof(notary))]
        public IHttpActionResult Deletenotary(int id)
        {
            notary notary = db.notarys.Find(id);
            if (notary == null)
            {
                return NotFound();
            }

            db.notarys.Remove(notary);
            db.SaveChanges();

            return Ok(notary);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool notaryExists(int id)
        {
            return db.notarys.Count(e => e.idNotary == id) > 0;
        }
    }
}