using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AzureTest.MVC.Hubs;

public record AuctionBidDto(
    Guid PlayerId,
    string PlayerName,
    string Position,
    string? PhotoUrl,
    decimal CurrentBid,
    Guid HighestBidderTeamId,
    string HighestBidderTeamName,
    int SecondsRemaining
);

public class AuctionRoomState
{
    public Guid LeagueId { get; set; }
    public Guid? CurrentPlayerId { get; set; }
    public string? CurrentPlayerName { get; set; }
    public string? CurrentPlayerPosition { get; set; }
    public string? CurrentPlayerPhoto { get; set; }
    public decimal CurrentPrice { get; set; }
    public Guid? HighestBidderTeamId { get; set; }
    public string? HighestBidderTeamName { get; set; }
    public Guid? CurrentNominatorUserId { get; set; }
    public string? CurrentNominatorName { get; set; }
    public int SecondsRemaining { get; set; } = 10;
    public bool IsActive { get; set; }
}

public class FantasyAuctionHub : Hub
{
    // Thread-safe auction room state per league
    private static readonly ConcurrentDictionary<Guid, AuctionRoomState> _auctionRooms = new();

    public async Task JoinAuctionRoom(string leagueIdStr, string teamIdStr, string teamName)
    {
        if (Guid.TryParse(leagueIdStr, out var leagueId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, leagueIdStr);

            var room = _auctionRooms.GetOrAdd(leagueId, id => new AuctionRoomState
            {
                LeagueId = id,
                IsActive = true,
                CurrentNominatorName = teamName
            });

            await Clients.Caller.SendAsync("AuctionStateUpdated", room);
            await Clients.Group(leagueIdStr).SendAsync("ManagerJoined", teamName);
        }
    }

    public async Task NominatePlayer(string leagueIdStr, string playerIdStr, string playerName, string pos, string photoUrl, decimal basePrice, string teamIdStr, string teamName)
    {
        if (Guid.TryParse(leagueIdStr, out var leagueId) && Guid.TryParse(playerIdStr, out var playerId) && Guid.TryParse(teamIdStr, out var teamId))
        {
            var room = _auctionRooms.GetOrAdd(leagueId, id => new AuctionRoomState { LeagueId = id });

            room.CurrentPlayerId = playerId;
            room.CurrentPlayerName = playerName;
            room.CurrentPlayerPosition = pos;
            room.CurrentPlayerPhoto = photoUrl;
            room.CurrentPrice = basePrice;
            room.HighestBidderTeamId = teamId;
            room.HighestBidderTeamName = teamName;
            room.SecondsRemaining = 10;
            room.IsActive = true;

            await Clients.Group(leagueIdStr).SendAsync("PlayerNominated", new AuctionBidDto(
                playerId,
                playerName,
                pos,
                photoUrl,
                basePrice,
                teamId,
                teamName,
                10
            ));
        }
    }

    public async Task PlaceBid(string leagueIdStr, string teamIdStr, string teamName, decimal newBid)
    {
        if (Guid.TryParse(leagueIdStr, out var leagueId) && Guid.TryParse(teamIdStr, out var teamId))
        {
            if (_auctionRooms.TryGetValue(leagueId, out var room) && room.CurrentPlayerId.HasValue)
            {
                if (newBid > room.CurrentPrice)
                {
                    room.CurrentPrice = newBid;
                    room.HighestBidderTeamId = teamId;
                    room.HighestBidderTeamName = teamName;
                    room.SecondsRemaining = 10; // Reset 10-second countdown on new bid

                    await Clients.Group(leagueIdStr).SendAsync("BidPlaced", new AuctionBidDto(
                        room.CurrentPlayerId.Value,
                        room.CurrentPlayerName ?? "Player",
                        room.CurrentPlayerPosition ?? "POS",
                        room.CurrentPlayerPhoto,
                        newBid,
                        teamId,
                        teamName,
                        10
                    ));
                }
            }
        }
    }

    public async Task PlayerSold(string leagueIdStr, string playerIdStr, string teamIdStr, decimal finalPrice)
    {
        if (Guid.TryParse(leagueIdStr, out var leagueId) && Guid.TryParse(playerIdStr, out var playerId) && Guid.TryParse(teamIdStr, out var teamId))
        {
            if (_auctionRooms.TryGetValue(leagueId, out var room))
            {
                var soldPlayerName = room.CurrentPlayerName ?? "Player";
                var winnerName = room.HighestBidderTeamName ?? "Team";

                room.CurrentPlayerId = null;
                room.CurrentPlayerName = null;
                room.CurrentPrice = 0;
                room.HighestBidderTeamId = null;
                room.HighestBidderTeamName = null;
                room.SecondsRemaining = 0;

                await Clients.Group(leagueIdStr).SendAsync("PlayerHammerSold", new
                {
                    playerId = playerId,
                    playerName = soldPlayerName,
                    teamId = teamId,
                    teamName = winnerName,
                    price = finalPrice
                });
            }
        }
    }
}
