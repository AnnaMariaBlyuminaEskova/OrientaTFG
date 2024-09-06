using OrientaTFG.Shared.Infrastructure.Enums;

namespace OrientaTFG.TFG.Core.Utils.ChatHub;

public interface IChatHub
{
    Task OnConnectedAsync();

    Task OnDisconnectedAsync(Exception exception);

    Task JoinChat(int tfgId, string sender);
    Task LeaveChat(int tfgId, string sender);

    Task SendMessage(int tfgId, string message, string sender);
}
