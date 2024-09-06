using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using System.Collections.Concurrent;
using TFGModel = OrientaTFG.Shared.Infrastructure.Model.TFG;

namespace OrientaTFG.TFG.Core.Utils.ChatHub;

public class ChatHub : Hub, IChatHub
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> GroupUserConnectionStatus = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }


    public async Task JoinChat(int tfgId, string sender)
    {
        try
        {
            string groupName = GetGroupName(tfgId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Conexión {Context.ConnectionId} se unió al grupo {groupName} como {sender}");

            var userRole = Context.User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (userRole != null ) 
            {
                var userStatuses = GroupUserConnectionStatus.GetOrAdd(groupName, _ => new ConcurrentDictionary<string, bool>());
                userStatuses[userRole] = true;
                await NotifyUsersAboutStatusChange(groupName, userRole, true);

                await NotifyCurrentStatusToNewUser(groupName);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al unir al grupo: {ex.Message}");
        }
    }

    public async Task LeaveChat(int tfgId, string sender)
    {
        try
        {
            string groupName = GetGroupName(tfgId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Conexión {Context.ConnectionId} abandonó el grupo {groupName} como {sender}");

            var userRole = Context.User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (userRole != null)
            {
                if (GroupUserConnectionStatus.TryGetValue(groupName, out var userStatuses))
                {
                    userStatuses[userRole] = false;
                    await NotifyUsersAboutStatusChange(groupName, userRole, false);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al salir del grupo: {ex.Message}");
        }
    }

    private async Task NotifyCurrentStatusToNewUser(string groupName)
    {
        if (GroupUserConnectionStatus.TryGetValue(groupName, out var userStatuses))
        {
            foreach (var status in userStatuses)
            {
                await Clients.Caller.SendAsync("UserStatusChanged", status.Key, status.Value);
            }
        }
    }

    private async Task NotifyUsersAboutStatusChange(string groupName, string userRole, bool isOnline)
    {
        await Clients.Group(groupName).SendAsync("UserStatusChanged", userRole, isOnline);
        Console.WriteLine($"El {userRole} en el grupo {groupName} está online: {isOnline}");
    }

    private string GetGroupName(int tfgId)
    {
        return $"chat-{tfgId}";
    }

    public async Task SendMessage(int tfgId, string message, string sender)
    {
        try
        {
            string groupName = GetGroupName(tfgId);
            Console.WriteLine($"Enviando mensaje del {sender} al grupo {groupName}: {message}");
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message, sender, DateTime.Now);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al enviar el mensaje: {ex.Message}");
        }
    }
}

