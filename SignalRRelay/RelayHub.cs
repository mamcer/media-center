using SignalR.Hubs;

namespace SignalRRelay
{
    public class RelayHub : Hub
    {
        public void JoinRelay(string groupName)
        {
            Groups.Add(Context.ConnectionId, groupName);
        }

        public void PlayPause(string groupName)
        {
            Clients[groupName].PlayPause();
        }

        public void MuteUnmute(string groupName)
        {
            Clients[groupName].MuteUnmute();
        }

        public void VolumeUp(string groupName)
        {
            Clients[groupName].VolumeUp();
        }

        public void VolumeDown(string groupName)
        {
            Clients[groupName].VolumeDown();
        }

        public void Stop(string groupName)
        {
            Clients[groupName].Stop();
        }

        public void PlayMovie(int movieId, string groupName)
        {
            Clients[groupName].PlayMovie(movieId);
        }
    }
}