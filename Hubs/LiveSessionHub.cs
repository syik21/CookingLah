using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace CookingLah.Hubs
{
    public class LiveSessionHub : Hub
    {
        // Add the browser to a live session group.
        public Task JoinLiveSession(string liveSessionId)
        {
            string groupName =
                "live-session-" + liveSessionId;

            return Groups.Add(
                Context.ConnectionId,
                groupName
            );
        }

        // Remove the browser from a live session group.
        public Task LeaveLiveSession(string liveSessionId)
        {
            string groupName =
                "live-session-" + liveSessionId;

            return Groups.Remove(
                Context.ConnectionId,
                groupName
            );
        }

        // Temporary method for testing SignalR.
        public void SendTestMessage(
            string liveSessionId,
            string message)
        {
            string groupName =
                "live-session-" + liveSessionId;

            Clients.Group(groupName)
                .receiveTestMessage(message);
        }
    }
}