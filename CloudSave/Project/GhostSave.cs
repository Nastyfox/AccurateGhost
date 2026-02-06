using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Api;
using Unity.Services.CloudSave.Model;

namespace CloudSave;

public interface IGhostSave
{
    Task SaveGhostData(IExecutionContext ctx, string ghostData, string ghostName);
}

public class GhostSave : IGhostSave
{
    private readonly ILogger<GhostSave> _logger;
    private readonly ICloudSaveDataApi _cloudSaveAPI;


    private string saveItemId = "Ghosts";
    private string saveKey = "GhostsDatas";

    public GhostSave(IGameApiClient gameApiClient, ILogger<GhostSave> logger)
    {
        _logger = logger;
        _cloudSaveAPI = gameApiClient.CloudSaveData;
        logger.LogWarning("GhostSave constructed");
    }

    [CloudCodeFunction("GetGhostData")]
    public async Task<Dictionary<string, string>> GetGhostData(IExecutionContext ctx)
    {
        _logger.LogInformation("Get Ghost Data");
        try
        {
            ApiResponse<GetItemsResponse> result = await _cloudSaveAPI.GetCustomItemsAsync(ctx, ctx.ServiceToken, ctx.ProjectId, saveItemId, new List<string> { saveKey });

            _logger.LogInformation("Successfully retrieved ghost data");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Data.Results.FirstOrDefault()?.Value?.ToString());
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to retrieve ghost data");
            return null;
        }
    }

    [CloudCodeFunction("SaveGhostData")]
    public async Task SaveGhostData(IExecutionContext ctx, string ghostData, string ghostName)
    {
        _logger.LogInformation("Save Ghost Data");
        Dictionary<string, string> ghostsDatasDictionary = new Dictionary<string, string>();

        try
        { 
            if (ghostsDatasDictionary != null)
            {
                ghostsDatasDictionary = await GetGhostData(ctx);

                if(ghostsDatasDictionary.Count > 0)
                {
                    if (!ghostsDatasDictionary.ContainsKey(ghostName))
                    {
                        ghostsDatasDictionary.Add(ghostName, ghostData);
                    }
                    else
                    {
                        ghostsDatasDictionary[ghostName] = ghostData;
                    }
                }
                else
                {
                    ghostsDatasDictionary[ghostName] = ghostData;
                }
            }
            else
            {
                _logger.LogError("Dictionary is null");
            }

            ApiResponse<SetItemResponse> result = await _cloudSaveAPI.SetCustomItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, "Ghosts", new SetItemBody(saveKey, ghostsDatasDictionary));

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogWarning("Save ghost data NOK");
            }
            else
            {
                _logger.LogInformation("Successfully saved ghost data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save ghost data NOK");
        }
    }

    [CloudCodeFunction("InitGhostData")]
    public async Task InitGhostData(IExecutionContext ctx, IGameApiClient apiClient)
    {
        await apiClient.CloudSaveData.SetCustomItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, saveItemId, new SetItemBody(saveKey, new Dictionary<string, string>()));
    }

}