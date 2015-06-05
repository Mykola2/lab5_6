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
    public class peopleController : ApiController
    {
        private RegisterEntities db = new RegisterEntities();

        // GET: api/people
        public IQueryable<person> Getpersons()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.persons;
        }

        // GET: api/people/5
        [ResponseType(typeof(person))]
        public IHttpActionResult Getperson(int id)
        {
            person person = db.persons.Find(id);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        // PUT: api/people/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putperson( person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //if (id != person.idPerson)
            //{
            //    return BadRequest();
            //}

            db.Entry(person).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/people
        [ResponseType(typeof(person))]
        public IHttpActionResult Postperson(person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.persons.Add(person);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = person.idPerson }, person);
        }

        // DELETE: api/people/5
        [ResponseType(typeof(person))]
        public IHttpActionResult Deleteperson(int id)
        {
            person person = db.persons.Find(id);
            if (person == null)
            {
                return NotFound();
            }

            db.persons.Remove(person);
            db.SaveChanges();

            return Ok(person);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool personExists(int id)
        {
            return db.persons.Count(e => e.idPerson == id) > 0;
        }
    }
}