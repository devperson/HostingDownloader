using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values/AnyPart
        [ActionNameAttribute("AnyPart")]
        public dynamic AnyPart()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault();
                if (part != null)
                    return part;
                else
                    return null;
            }            
        }

        // POST api/values/AddPart
        [ActionNameAttribute("AddPart")]
        public void Post(FilePart newPart)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                context.Parts.Add(newPart);
                context.SaveChanges();
            }
        }

        // POST api/values/RemovePart/{id}
        [ActionNameAttribute("RemovePart")]
        public void Delete(int id)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault(p => p.Id == id);
                context.Parts.Remove(part);
                context.SaveChanges();
            }
        }
    }
}