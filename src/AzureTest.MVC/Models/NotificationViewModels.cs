using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.MVC.Models;

public class NotificationIndexViewModel
{
    public IReadOnlyList<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
    public NotificationDto? SelectedNotification { get; set; }
    public int UnreadCount { get; set; }
    public string? SelectedId { get; set; }
    public string? SearchTerm { get; set; }
    public string? ActiveFilter { get; set; }
}
