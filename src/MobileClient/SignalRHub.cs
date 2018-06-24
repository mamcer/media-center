using SignalR.Client.Hubs;

namespace MobileClient
{
    public static class SignalRHub
    {
        private static HubConnection connection = null;
        private static IHubProxy proxy = null;
        public static string remoteGroup = "XBMC";

        public static IHubProxy Proxy
        {
            get
            {
                if (proxy == null)
                {
                    InitializeSignalR();
                }

                return proxy;
            }
        }

        private static void InitializeSignalR()
        {
            string url = "http://cucarelay.azurewebsites.net/";

            connection = new HubConnection(url);
            proxy = connection.CreateProxy("RelayHub");
            connection.Start();
            while (connection.State != SignalR.Client.ConnectionState.Connected) ;
            proxy.Invoke("JoinRelay", remoteGroup);
        }
    }
}