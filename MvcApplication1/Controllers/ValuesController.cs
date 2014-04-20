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
                {
                    part.IsTaken = true;
                    context.SaveChanges();
                    return part;
                }
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

                if (context.Parts.Count() > 8)
                    this.SendMsg(Clients.Uploader, Messages.PauseUploading);
                else
                    this.SendMsg(Clients.Downloader, Messages.ContinueUploading);

                this.SendMsg(Clients.Downloader, Messages.DownloadAvailable);
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

        private void SendMsg(string to, string msg)
        {
            HubClient c = new HubClient(this.Request.RequestUri.Host, Clients.Server);
            c.SendMessage(new MsgData { From = Clients.Server, To = to, Message = msg });
        }
    }
}