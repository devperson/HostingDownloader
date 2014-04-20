using DataAccess.Models;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class WPFHubClient
    {
        HubConnection connection;
        IHubProxy serverHub;
        string ClientName;
        string HostUrl;
        Action<MsgData> Callback;

        public WPFHubClient(string host, string clientName, Action<MsgData> callback)
        {
            this.HostUrl = host;
            this.ClientName = clientName;            
            this.Connect();  
        }

        private async void Connect()
        {
            connection = new HubConnection(this.HostUrl, string.Format("name={0}", this.ClientName));
            serverHub = connection.CreateHubProxy("HubServer");
            serverHub.On<MsgData>("Send", Callback);
            await connection.Start();
        }       


        /// <summary>
        /// Specify From; To; and Message;
        /// </summary>
        /// <param name="data"></param>
        public async void SendMessage(MsgData data)
        {
            await serverHub.Invoke("send", new object[] { data });            
        }      
    }
}
