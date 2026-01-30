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

    [SerializeField] private float offsetWall;

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

    public void SetCameraOffset(float velocityX, int wallDirection)
    {
        if(cinemachineCamera != null)
        {
            cinemachinePositionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            if (wallDirection != 0)
            {
                cinemachinePositionComposer.TargetOffset.x = Mathf.Lerp(cinemachinePositionComposer.TargetOffset.x, offsetWall * -wallDirection, transitionSpeed * Time.fixedDeltaTime);
            }
            else
            {
                cinemachinePositionComposer.TargetOffset.x = Mathf.Lerp(cinemachinePositionComposer.TargetOffset.x, offsetFactor.x * velocityX, transitionSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
