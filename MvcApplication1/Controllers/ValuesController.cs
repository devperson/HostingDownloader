using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        static HubClient c = new HubClient(Constants.Host, Clients.Server);
        static object locking = new object();
        //static List<FilePart> parts = new List<FilePart>();

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
            }
            c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Downloader, Message = Messages.FileInfoAvailable });
        }

        // DELTE api/values/RemoveFileInfo/{id}
        [HttpDelete]
        [ActionNameAttribute("RemoveFileInfo")]
        public void DeleteFileInfo(long id)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var file = context.Files.FirstOrDefault(p => p.Id == id);
                if (file != null)
                {
                    context.Files.Remove(file);
                    context.SaveChanges();
                }
            }
        }





        // GET api/values/AnyPart
        [HttpGet]
        [ActionNameAttribute("GetPart")]
        public FilePart GetPart(long id)
        {
            //var part = parts.FirstOrDefault(p => p.Id == id);
            //if (part != null)
            //{
            //    parts.Remove(part);
            //    if (!parts.Any())
            //        c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.ContinueUploading });

            //    return part;
            //}
            //else
            //    return null;
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault(p => p.Id == id);
                return part;
            }
        }

        // POST api/values/AddPart
        [HttpPost]
        [ActionNameAttribute("AddPart")]
        public void Post(FilePart newPart)
        {
            //newPart.Id = parts.Any() ? parts.Max(p => p.Id) + 1 : 1;
            //parts.Add(newPart);
            //c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Downloader, Message = string.Format("{0}{1}", newPart.Id, Messages.DownloadAvailable) });
            //if (parts.Count() > 2)
            //    c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.PauseUploading });
            lock (locking)
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    context.Parts.Add(newPart);
                    context.SaveChanges();


                    c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Downloader, Message = string.Format("{0}{1}", newPart.Id, Messages.DownloadAvailable) });


                    if (context.Parts.Count() > 4)
                        c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.PauseUploading });
                }
            }
        }

        [HttpDelete]
        [ActionNameAttribute("RemovePart")]
        public void RemovePart(long id)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault(p => p.Id == id);
                if (part != null)
                {
                    context.Parts.Remove(part);
                    context.SaveChanges();
                }

                if (!context.Parts.Any())
                    c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.ContinueUploading });
            }
        }

        [HttpDelete]
        [ActionNameAttribute("ClearDb")]
        public void ClearDb()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext;
                objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Parts]");
                objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Files]");
            }

            // parts.Clear();
        }

    }
}