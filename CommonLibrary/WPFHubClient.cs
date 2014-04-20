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

        public WPFHubClient(string host, string clientName, Action<MsgData> callback)
        {
            connection = new HubConnection(host, string.Format("name={0}", clientName));
            serverHub = connection.CreateHubProxy("HubServer");
            serverHub.On<MsgData>("Send", callback);
            connection.Start().Wait();
        }        


        /// <summary>
        /// Specify From; To; and Message;
        /// </summary>
        /// <param name="data"></param>
        public async void SendMessage(MsgData data)
        {
            await serverHub.Invoke("Send", new object[] { data });
        }      
    }
}
