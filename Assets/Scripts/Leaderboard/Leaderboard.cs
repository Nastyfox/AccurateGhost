using Newtonsoft.Json;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [Serializable]
    public class ScoreMetadata
    {
        public string levelName;
        public int chrono;
    }

    public async void AddScoreWithMetadata(string leaderboardId, int score, string _levelName, int _chrono)
    {
        var scoreMetadata = new ScoreMetadata { levelName = _levelName, chrono = _chrono };
        var playerEntry = await LeaderboardsService.Instance
            .AddPlayerScoreAsync(
                leaderboardId,
                score,
                new AddPlayerScoreOptions { Metadata = scoreMetadata }
            );
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
    }

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void GetScoresWithMetadata(string leaderboardId)
    {
        var scoreResponse = await LeaderboardsService.Instance
            .GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { IncludeMetadata = true }
            );
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
    }

    private void Start()
    {
        AddScoreWithMetadata("AccurateGhost", 10, "Level01", 60);
    }
}
