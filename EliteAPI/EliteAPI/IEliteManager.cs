using EliteAPI.Models.Request;
using EliteAPI.Models.Response;

namespace EliteAPI.EliteAPI;

public interface IEliteManager
{
    public Task<ClientProfileResponse> GetClientProfile(ClientProfileRequest request);
}