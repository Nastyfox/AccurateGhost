using CloudSave;
using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis.Admin;

namespace Configuration;

public class Configuration : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton(AdminApiClient.Create());
        config.Dependencies.AddSingleton<IGhostSave, GhostSave>();
        config.Dependencies.AddSingleton<LeaderboardAdmin>();
    }
}
