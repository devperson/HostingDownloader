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
        public WPFHubClient(string host, string clientName)
        {
            this.HostUrl = host;
            this.ClientName = clientName;            
            this.Connect();  
        }

        private async void Connect()
        {
            connection = new HubConnection(this.HostUrl, string.Format("name={0}", this.ClientName));
            IHubProxy serverHub = connection.CreateHubProxy("HubServer");
            serverHub.On<MsgData>("Send", OnMessageRecived);
            await connection.Start();
        }

        private void OnMessageRecived(MsgData data)
        {
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
