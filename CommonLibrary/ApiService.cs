using DataAccess.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class ApiService
    {
        private RestClient client;
        public ApiService()
        {
            client = new RestClient(Constants.Host);
            client.Timeout = 2000;
            client.AddDefaultHeader("Accept", "application/json");            
        }        

        public async void GetFileInfo(Action<FileInfo> onCompleted)
        {
            var result = await Task.Run<FileInfo>(() =>
            {                
                var restResponse = client.Execute(new RestRequest("/api/values/GetFileInfo", Method.GET));
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
                try
                {
                    var req = new RestRequest("/api/values/PostFileInfo", Method.POST);
                    req.RequestFormat = DataFormat.Json;
                    req.AddBody(file);

                    client.Execute(req);
                }
                catch (Exception ex)
                {
 
                }
            });

            if (onCompleted != null)
                onCompleted();
        }

        public async void RemoveFileInfo(long id, Action onCompleted = null)
        {
            await Task.Run(() =>
            {                
                client.Execute(new RestRequest(string.Format("/api/values/RemoveFileInfo/{0}", id), Method.PUT));
            });
            if (onCompleted != null)
                onCompleted();
        }
          
        public async void GetPart(long id, Action<FilePart, long> onCompleted)
        {            
            var resultObject = await Task.Run<FilePart>(() =>
            {
                try
                {
                    string url = string.Format("api/values/GetPart/{0}", id);                    
                    var _request = (HttpWebRequest)HttpWebRequest.Create(Constants.Host + url);
                    _request.Accept = "application/json, text/plain, */*";                                 
                    _request.Referer = Constants.Host;
                    _request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";

                    _request.Timeout = 1000;
                    var response = (HttpWebResponse)_request.GetResponse();

                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        var val = reader.ReadToEnd();                        
                        return JsonConvert.DeserializeObject<FilePart>(val);
                    }
                }
                catch (Exception ex)
                {                    
                    return null;
                }
            });
            
            onCompleted(resultObject, id);

            //System.Net.WebClient w = new System.Net.WebClient();                      
            //w.DownloadStringCompleted += (s, e) =>
            //{
            //    if (e.Error == null && !e.Cancelled)
            //        onCompleted(JsonConvert.DeserializeObject<FilePart>(e.Result), id);
            //    else
            //        onCompleted(null, id);
            //};
            //w.DownloadStringAsync(new Uri(Constants.Host + string.Format("/api/values/GetPart/{0}", id)));

            //var str = await w.DownloadStringTaskAsync(Constants.Host + string.Format("/api/values/GetPart/{0}", id));
            //onCompleted(JsonConvert.DeserializeObject<FilePart>(str), id);

            //var result = await Task.Run<FilePart>(() =>
            //{                                
                //var restResponse = client.Execute(new RestRequest(string.Format("/api/values/GetPart/{0}", id), Method.GET));
                //if (restResponse.Content != "null" && restResponse.ErrorException == null)
                //    return JsonConvert.DeserializeObject<FilePart>(restResponse.Content);

                //return null;
            //});
            //onCompleted(result, id);
        }      

        public async void UploadPart(FilePart part, Action onCompleted)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Thread.Sleep(TimeSpan.FromSeconds(5));
                    var req = new RestRequest("/api/values/AddPart", Method.POST);
                    req.RequestFormat = DataFormat.Json;
                    req.AddBody(part);

                    client.Execute(req);
                }
                catch (Exception e)
                {
                }
            });

            onCompleted();
        }

        public async void RemovePart(long id, Action onCompleted = null)
        {
            await Task.Run(() =>
            {
                var req = new RestRequest(string.Format("/api/values/RemovePart/{0}", id), Method.DELETE);                

                client.Execute(req);
            });
            if (onCompleted != null)
                onCompleted();
        }

        public async void ClearDb(Action onCompleted = null)
        {
            await Task.Run(() =>
            {                
                client.Execute(new RestRequest("/api/values/ClearDb", Method.PUT));
            });
            if (onCompleted != null)
                onCompleted();
        }      
    }
}
