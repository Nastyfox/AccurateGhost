using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager cameraManagerInstance;

    private CinemachinePositionComposer cinemachinePositionComposer;
    private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Vector3 offsetFactor;
    [SerializeField] private float transitionSpeed;

    private async UniTaskVoid Awake()
    {
        if (cameraManagerInstance == null)
        {
            cameraManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);

            await UniTask.DelayFrame(1);
            cinemachineCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
            cinemachinePositionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            cinemachinePositionComposer.TargetOffset = Vector3.zero;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCameraOffset(Vector3 newOffset)
    {
        if(cinemachineCamera != null)
        {
            cinemachinePositionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            cinemachinePositionComposer.TargetOffset.x = Mathf.Lerp(cinemachinePositionComposer.TargetOffset.x, offsetFactor.x * newOffset.x, transitionSpeed * Time.fixedDeltaTime);
        }
    }
}
