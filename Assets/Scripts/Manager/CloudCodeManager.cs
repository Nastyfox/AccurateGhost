using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

public class CloudCodeManager : MonoBehaviour
{
    public static CloudCodeManager cloudCodeManagerInstance;

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    private GhostSaveBindings ghostSaveBindings;

    private bool isDataRecovered = false;


    private void Awake()
    {
        if (cloudCodeManagerInstance == null)
        {
            cloudCodeManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private async UniTaskVoid Start()
    {
        while (!Leaderboard.leaderboardInstance.GetIsInitialized())
        {
            await UniTask.Yield();
        }

        ghostSaveBindings = new GhostSaveBindings(CloudCodeService.Instance);

        globalDataScriptableObject.ghostsDatas = await ghostSaveBindings.GetGhostData();

        isDataRecovered = true;
    }
    
    public bool IsDataRecovered()
    {
        return isDataRecovered;
    }
}
