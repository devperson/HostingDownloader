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
        //GET api/values/GetFileInfo
        [HttpGet]
        [ActionNameAttribute("GetFileInfo")]
        public FileInfo GetFileInfo()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var f = context.Files.FirstOrDefault();
                if (f != null)
                {
                    context.Files.Remove(f);
                    context.SaveChanges();
                    return f;
                }
                return null;
            }
        }

        //POST api/values/PostFileInfo
        [HttpPost]
        [ActionNameAttribute("PostFileInfo")]        
        public void PostFileInfo(FileInfo info)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                context.Files.Add(info);
                context.SaveChanges();
                this.SendMsg(Clients.Downloader, Messages.FileInfoAvailable);
            }
        }

        // POST api/values/RemoveFileInfo/{id}
        [ActionNameAttribute("RemoveFileInfo")]
        public void DeleteFileInfo(long id)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var file = context.Files.FirstOrDefault(p => p.Id == id);
                context.Files.Remove(file);
                context.SaveChanges();
            }
        }


        // GET api/values/AnyPart
        [HttpGet]
        [ActionNameAttribute("AnyPart")]
        public FilePart AnyPart()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault();
                if (part != null)
                {                    
                    part.IsTaken = true;
                    context.SaveChanges();

                    if (context.Parts.Count(t => !t.IsTaken) < 3)
                        this.SendMsg(Clients.Uploader, Messages.ContinueUploading);

                    return part;
                }
                else
                    return null;
            }            
        }

        // POST api/values/AddPart
        [HttpPost]
        [ActionNameAttribute("AddPart")]
        public void Post(FilePart newPart)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                context.Parts.Add(newPart);
                context.SaveChanges();

                if (context.Parts.Count(t => !t.IsTaken) > 8)
                    this.SendMsg(Clients.Uploader, Messages.PauseUploading);
                else
                    this.SendMsg(Clients.Uploader, Messages.ContinueUploading);

                if (context.Files.Any())
                    this.SendMsg(Clients.Downloader, Messages.FileInfoAvailable);

                this.SendMsg(Clients.Downloader, Messages.DownloadAvailable);
            }
        }

        // POST api/values/RemovePart/{id}
        [ActionNameAttribute("RemovePart")]
        public void Delete(long id)
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
            HubClient c = new HubClient(Constants.Host, Clients.Server);
            c.SendMessage(new MsgData { From = Clients.Server, To = to, Message = msg });
        }
    }
}