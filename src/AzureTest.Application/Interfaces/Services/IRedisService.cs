

using System.Threading.Tasks;
using System;

namespace AzureTest.Application.Interfaces.Services;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);

    string GenerateKey(string key, string uniqueKey)
    => key.Replace("{KEY}", uniqueKey);
    
    public static  string UserPref_Key = "UserPref_{KEY}";
    public static string User_Info_Key="User_Info_{KEY}";
    public static string DebtPersons_Key="DebtPersons_{KEY}";
    public static string ExpCats_KEY="ExpCats_{KEY}";
    public static string RolePermissions__Key="RolePermissions_{KEY}";
    public static string AllPermissions = "AllPermissions";
}
