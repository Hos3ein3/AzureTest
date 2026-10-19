using System.Collections.Concurrent;

namespace AzureTest.MVC.Services;

public interface IUserTracker
{
    void UserConnected(string connectionId, string userId, string? userName);
    void UserDisconnected(string connectionId);
    int GetOnlineUsersCount();
    IReadOnlyList<OnlineUserInfo> GetOnlineUsers();
}

public record OnlineUserInfo(string UserId, string UserName, int ConnectionCount);

public class UserTracker : IUserTracker
{
    // Maps connectionId -> (UserId, UserName)
    private readonly ConcurrentDictionary<string, (string UserId, string UserName)> _connections = new();
    
    // Maps UserId -> (UserName, Set of connectionIds)
    private readonly ConcurrentDictionary<string, (string UserName, HashSet<string> ConnectionIds)> _users = new();

    private readonly object _lock = new();

    public void UserConnected(string connectionId, string userId, string? userName)
    {
        var resolvedName = string.IsNullOrWhiteSpace(userName) ? "Anonymous" : userName;
        _connections[connectionId] = (userId, resolvedName);

        lock (_lock)
        {
            if (_users.TryGetValue(userId, out var userEntry))
            {
                userEntry.ConnectionIds.Add(connectionId);
                _users[userId] = (resolvedName, userEntry.ConnectionIds);
            }
            else
            {
                _users[userId] = (resolvedName, new HashSet<string> { connectionId });
            }
        }
    }

    public void UserDisconnected(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var user))
        {
            lock (_lock)
            {
                if (_users.TryGetValue(user.UserId, out var userEntry))
                {
                    userEntry.ConnectionIds.Remove(connectionId);
                    if (userEntry.ConnectionIds.Count == 0)
                    {
                        _users.TryRemove(user.UserId, out _);
                    }
                }
            }
        }
    }

    public int GetOnlineUsersCount()
    {
        return _users.Count;
    }

    public IReadOnlyList<OnlineUserInfo> GetOnlineUsers()
    {
        lock (_lock)
        {
            var list = new List<OnlineUserInfo>(_users.Count);
            foreach (var kvp in _users)
            {
                list.Add(new OnlineUserInfo(kvp.Key, kvp.Value.UserName, kvp.Value.ConnectionIds.Count));
            }
            return list;
        }
    }
}
