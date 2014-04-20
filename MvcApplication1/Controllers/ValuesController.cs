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
        HubClient c = new HubClient(Constants.Host, Clients.Server);
        object locking = new object();
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
                c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Downloader, Message = Messages.FileInfoAvailable });                
            }
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
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault(p => !p.IsTaken && p.Id == id);
                if (part != null)
                {
                    Debug.WriteLine("{0} part taking", part.Part);

                    part.IsTaken = true;
                    context.SaveChanges();

                    //if (context.Parts.Count(t => !t.IsTaken) < 2)
                    //    c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.ContinueUploading });
                    //else
                    //    c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Uploader, Message = Messages.PauseUploading });

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
                Debug.WriteLine(string.Format("{0} parts on server", context.Parts.Count()));
                context.SaveChanges();                
                c.SendMessage(new MsgData { From = Clients.Server, To = Clients.Downloader, Message = string.Format("{0}{1}", newPart.Id, Messages.DownloadAvailable) });
            }
        }

        // POST api/values/RemovePart/{id}
        [HttpDelete]
        [ActionNameAttribute("RemovePart")]
        public void Delete(long id)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var part = context.Parts.FirstOrDefault(p => p.Id == id);
                if (part != null)
                {                    
                    context.Parts.Remove(part);
                    context.SaveChanges();
                    Debug.WriteLine("{0} part removed", part.Part);
                }
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
        }


        //private void SendMsg(string to, string msg)
        //{
            
            
        //}
    }
}