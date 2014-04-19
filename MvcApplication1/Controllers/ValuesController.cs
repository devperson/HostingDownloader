using CommonLibrary.Models;
using DataAccess;
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
        // GET api/<controller>
        public IEnumerable<FilePart> Get()
        {
            List<FilePart> files = new List<FilePart>();
            files.Add(new FilePart { Bytes = new byte[] { 1, 2, 3 }, FileName = "music.avi", Part = 3, Id = 4 });
            files.Add(new FilePart { Bytes = new byte[] { 1, 2, 3 }, FileName = "music.avi", Part = 4, Id = 5 });
            return files;
        }

        // GET api/<controller>/5
        [ActionNameAttribute("First")]
        public dynamic GetFirstAvailablePart()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.SingleOrDefault();
                if (part != null)
                    return part;
                else
                    return null;
            }            
        }

        // POST api/<controller>
        [ActionNameAttribute("AddPart")]
        public void Post(FilePart newPart)
        {
        }

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