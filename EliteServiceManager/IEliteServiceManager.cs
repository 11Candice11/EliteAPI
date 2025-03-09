using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;

namespace EliteService.EliteServiceManager;

public interface IEliteServiceManager
{
    public Task<ClientProfileResponse> GetClientProfile(ClientProfileRequest request);

}