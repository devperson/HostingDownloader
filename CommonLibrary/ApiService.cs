using DataAccess.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class ApiService
    {
        public RestClient Client;
        public static string HostUrl { get; set; }

        static ApiService()
        {
            HostUrl = "http://localhost:55578";
        }

        public ApiService()
        {            
            this.Client = new RestClient(HostUrl);
            this.Client.AddDefaultHeader("Accept", "application/json");
        }

        public async void GetAnyPart(Action<FilePart> onCompleted)
        {
            var result = await Task.Run<FilePart>(() =>
            {
                var restResponse = this.Client.Execute(new RestRequest("/api/values/AnyPart", Method.GET));
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
                var restResponse = this.Client.Execute(new RestRequest("/api/values/AddPart", Method.POST));
            });

            onCompleted();
        }

        public async void RemovePart(int id, Action onCompleted)
        {
            await Task.Run(() =>
            {
                this.Client.Execute(new RestRequest(string.Format("/api/values/RemovePart/{0}", id), Method.DELETE));
            });

            onCompleted();
        }
    }
}
