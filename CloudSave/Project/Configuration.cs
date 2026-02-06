using CloudSave;
using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace Configuration;

public class Configuration : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton<IGhostSave, GhostSave>();
    }
}
