using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.Leaderboards.Admin.Api;
using Unity.Services.Leaderboards.Admin.Model;

public class LeaderboardAdmin
{
    private readonly ILogger<LeaderboardAdmin> _logger;
    private readonly ISecretClient _secretAPI;
    private readonly ILeaderboardsApi _leaderboardsAPI;

    public LeaderboardAdmin(IGameApiClient gameApiClient, IAdminApiClient adminApiClient, ILogger<LeaderboardAdmin> logger)
    {
        _logger = logger;
        _secretAPI = gameApiClient.SecretManager;
        _leaderboardsAPI = adminApiClient.Leaderboards;
        logger.LogWarning("GhostSave constructed");
    }

    [CloudCodeFunction("CreateLeaderboard")]
    public async Task CreateLeaderboard(IExecutionContext context, IAdminApiClient adminApiClient, string leaderboardName)
    {
        Secret serviceAccountKey;
        Secret serviceAccountSecret;

        try
        {
            serviceAccountKey = await _secretAPI.GetSecret(context, "SERVICE_ACCOUNT_KEY_ID");
            serviceAccountSecret = await _secretAPI.GetSecret(context, "SERVICE_ACCOUNT_SECRET");
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to get secret keys. Error: {Error}", ex.Message);
            throw new Exception($"Failed to get secret keys. Error: {ex.Message}");
        }

        try
        {
            await _leaderboardsAPI.CreateLeaderboardAsync(
                executionContext: context,
                serviceAccountKey: serviceAccountKey.Value,
                serviceAccountSecret: serviceAccountSecret.Value,
                projectId: Guid.Parse(context.ProjectId),
                environmentId: Guid.Parse(context.EnvironmentId),
                leaderboardIdConfig: new LeaderboardIdConfig(
                    id: leaderboardName,
                    name: leaderboardName,
                    sortOrder: SortOrder.Desc,
                    updateType: UpdateType.KeepBest
                )
            );
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to create a Leaderboard. Error: {Error}", ex.Message);
            throw new Exception($"Failed to create a Leaderboard. Error: {ex.Message}");
        }
    }
}
