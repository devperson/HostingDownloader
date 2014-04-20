using DataAccess.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class ApiService
    {
        RestClient client;                

        public ApiService()
        {            
            this.client = new RestClient(Constants.Host);
            this.client.AddDefaultHeader("Accept", "application/json");
        }


        public async void GetFileInfo(Action<FileInfo> onCompleted)
        {
            var result = await Task.Run<FileInfo>(() =>
            {
                var restResponse = this.client.Execute(new RestRequest("/api/values/GetFileInfo", Method.GET));
                if (restResponse.Content != "null")
                    return JsonConvert.DeserializeObject<FileInfo>(restResponse.Content);
                else
                    return null;
            });

            onCompleted(result);
        }

        public async void UploadFileInfo(FileInfo file, Action onCompleted=null)
        {
            await Task.Run(() =>
            {
                var req = new RestRequest("/api/values/PostFileInfo", Method.POST);
                req.RequestFormat = DataFormat.Json;
                req.AddBody(file);
                this.client.Execute(req);
            });

            if (onCompleted != null)
                onCompleted();
        }

        public async void RemoveFileInfo(long id, Action onCompleted = null)
        {
            await Task.Run(() =>
            {
                this.client.Execute(new RestRequest(string.Format("/api/values/RemoveFileInfo/{0}", id), Method.DELETE));
            });
            if (onCompleted != null)
                onCompleted();
        }


        public async void GetPart(long id, Action<FilePart> onCompleted)
        {
            var result = await Task.Run<FilePart>(() =>
            {                
                var restResponse = this.client.Execute(new RestRequest(string.Format("/api/values/GetPart/{0}", id), Method.GET));
                if (restResponse.Content != "null")
                    return JsonConvert.DeserializeObject<FilePart>(restResponse.Content);
                else
                    return null;
            });

            onCompleted(result);
        }

        public async void UploadPart(FilePart part, Action onCompleted)
        {
            await Task.Run(() =>
            {
                var req = new RestRequest("/api/values/AddPart", Method.POST);
                req.RequestFormat = DataFormat.Json;
                req.AddBody(part);
                this.client.Execute(req);
            });

            onCompleted();
        }

        //public async void RemovePart(long id, Action onCompleted = null)
        //{
        //    await Task.Run(() =>
        //    {
        //        this.client.Execute(new RestRequest(string.Format("/api/values/RemovePart/{0}", id), Method.DELETE));
        //    });
        //    if (onCompleted != null)
        //        onCompleted();
        //}

        public async void ClearDb(Action onCompleted = null)
        {
            await Task.Run(() =>
            {
                this.client.Execute(new RestRequest("/api/values/ClearDb", Method.DELETE));
            });
            if (onCompleted != null)
                onCompleted();
        }      
    }
}
