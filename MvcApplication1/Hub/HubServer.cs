using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using DataAccess.Models;
using System.Diagnostics;

namespace MvcApplication1
{
    public class HubServer : Hub
    {
        private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();       

        public void Send(MsgData data)
        {            
            foreach (var connectionId in _connections.GetConnections(data.To))
            {                
                Clients.Client(connectionId).Send(data);
            }
        }

        public override Task OnConnected()
        {
            string name = Context.QueryString["name"];
            Debug.WriteLine(string.Format("{0} Connected", name));
            _connections.Add(name, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            string name = Context.QueryString["name"];
            Debug.WriteLine(string.Format("{0} Disconnected", name));
            if (!string.IsNullOrEmpty(name))
                _connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected();
        }        
    }
}