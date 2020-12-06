using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hotel_Web.Models;
using Microsoft.AspNet.SignalR;

namespace Hotel_Web
{
    public class ChatHub : Hub
    {
        dbEntities1 db = new dbEntities1();
        private static List<object> list = new List<object>();

        public void SendText(string name, string text)
        {
            list.Add(new { name, text });
            while (list.Count > 10) list.RemoveAt(0);

            // Clients.All.ReceiveText(name, text);
            Clients.Caller.ReceiveText(name, text, "me");
            //Clients.All.ReceiveText(name, text, "admin");
            Clients.Others.ReceiveText(name, text);
        }

        public override Task OnConnected()
        {
            Clients.Caller.initialize(list);
            return base.OnConnected();
        }

        public Boolean CheckPass(string pass)
        {
            return db.Admins.Any(a => a.HashPass == pass);
        }
    }
}