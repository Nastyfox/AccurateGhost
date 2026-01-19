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

    private bool isInitialized = false;

    public static Leaderboard leaderboardInstance;

    [Serializable]
    public class ScoreMetadata
    {
        public string chrono;
        public string pseudo;
    }

    public string GetPlayerID()
    {
        return playerID;
    }

    public bool GetIsInitialized()
    {
        return isInitialized;
    }

    private async UniTaskVoid Awake()
    {
        if (leaderboardInstance == null)
        {
            leaderboardInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        await UnityServices.InitializeAsync();
        
        await SignInAnonymously();
    }

    async UniTask SignInAnonymously()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            playerID = AuthenticationService.Instance.PlayerId;
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        isInitialized = true;
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
            return scoreResponse;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }
}
