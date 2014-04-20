using DataAccess.Models;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1
{
    public class HubClient
    {
        HubConnection connection;
        IHubProxy serverHub;
        string ClientName;
        public HubClient(string host, string clientName)
        {
            this.ClientName = clientName;
            connection = new HubConnection(host, string.Format("name={0}", this.ClientName));
            serverHub = connection.CreateHubProxy("HubServer");
            serverHub.On<MsgData>("Send", OnMessageRecived);
            connection.Start().Wait();
        }        


        private void OnMessageRecived(MsgData data)
        {
            
        }

        /// <summary>
        /// Specify From; To; and Message;
        /// </summary>
        /// <param name="data"></param>
        public void SendMessage(MsgData data)
        {
            serverHub.Invoke("Send", new object[] { data });
            //connection.Disconnect();
        }
    }
}