using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    private string playerID; 

    [Serializable]
    public class ScoreMetadata
    {
        public string chrono;
        public string pseudo;
    }

    public async UniTask<string> InitializeLeaderboardService()
    {
        await UnityServices.InitializeAsync();

        await SignInAnonymously();

        return playerID;
    }

    async UniTask SignInAnonymously()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            playerID = AuthenticationService.Instance.PlayerId;
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async UniTask AddScoreWithMetadata(string leaderboardId, int score, string _chrono, string _pseudo)
    {
        var scoreMetadata = new ScoreMetadata { chrono = _chrono, pseudo = _pseudo };
        var playerEntry = await LeaderboardsService.Instance
            .AddPlayerScoreAsync(
                leaderboardId,
                score,
                new AddPlayerScoreOptions { Metadata = scoreMetadata }
            );
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
    }

    public async UniTask<List<Unity.Services.Leaderboards.Models.LeaderboardEntry>> GetScoresWithMetadata(string leaderboardId)
    {
        var scoreResponse = await LeaderboardsService.Instance
            .GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = 50, IncludeMetadata = true }
            );
        string scoreData = JsonConvert.SerializeObject(scoreResponse.Results);
        Debug.Log(scoreData);
        return scoreResponse.Results;
    }

    public async UniTask<Unity.Services.Leaderboards.Models.LeaderboardEntry> GetPlayerScoreWithMetadata(string leaderboardId)
    {
        try
        {
            var scoreResponse = await LeaderboardsService.Instance
            .GetPlayerScoreAsync(
                leaderboardId,
                new GetPlayerScoreOptions { IncludeMetadata = true }
            );

            string scoreData = JsonConvert.SerializeObject(scoreResponse);
            Debug.Log(scoreData);
            return scoreResponse;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }
}
