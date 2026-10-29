using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class IdentitySetting : ISingleRecordSetting
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string RedirectAfterRegister { get; set; } = "/";
    public string RedirectAfterLogin { get; set; } = "/";
    public List<string> RolesForRegister { get; set; } = new();
    public bool StayLoginAfterRegister { get; set; } = true;
    public bool LogoutOtherDevicesOnLogin { get; set; } = false;
}