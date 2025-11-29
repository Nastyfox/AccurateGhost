using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : MonoBehaviour
{
    string VersionId { get; set; }
    int Offset { get; set; }
    int Limit { get; set; }
    int RangeLimit { get; set; }
    List<string> FriendIds { get; set; }

    [Serializable]
    public class ScoreMetadata
    {
        public string levelName;
        public string chrono;
        public string pseudo;
    }

    public async UniTask InitializeLeaderboardService()
    {
        await UnityServices.InitializeAsync();

        await SignInAnonymously();
    }

    async UniTask SignInAnonymously()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async UniTask AddScoreWithMetadata(string leaderboardId, int score, string _levelName, string _chrono, string _pseudo)
    {
        var scoreMetadata = new ScoreMetadata { levelName = _levelName, chrono = _chrono, pseudo = _pseudo };
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
}
