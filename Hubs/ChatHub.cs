using Microsoft.AspNetCore.SignalR;
using snapnow.Models;

namespace snapnow.Hubs;

public class ChatHub : Hub
{
    private readonly IDictionary<string, UserConnection> _connections;
    
    public ChatHub(IDictionary<string, UserConnection> connections)
    {
        _connections = connections;
    }
    
    public override Task OnConnectedAsync()
    {
        // Add the connection ID to the dictionary when a user connects
        string? userEmail = Context.GetHttpContext()?.Request.Query["email"];
        if (userEmail == null)
        {
            throw new ArgumentNullException();
        }
        string connectionId = Context.ConnectionId;
        AddConnectionId(userEmail, connectionId);

        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string? userEmail = Context.GetHttpContext()?.Request.Query["email"];
        if (userEmail == null)
        {
            throw new ArgumentNullException();
        }

        RemoveConnectionId(userEmail);
            
        return base.OnDisconnectedAsync(exception);
    }

    private void AddConnectionId(string userEmail, string connectionId)
    {
        lock (_connections)
        {
            _connections[userEmail] = new UserConnection { ConnectionId = connectionId };
        }
    }

    private void RemoveConnectionId(string userEmail)
    {
        lock (_connections)
        {
            _connections.Remove(userEmail);
        }
    }
}