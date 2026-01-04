using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private CinemachinePositionComposer cinemachinePositionComposer;
    private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Vector3 offsetFactor;
    [SerializeField] private float transitionSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cinemachineCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCameraOffset(Vector3 newOffset)
    {
        cinemachinePositionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
        cinemachinePositionComposer.TargetOffset.x = Mathf.Lerp(cinemachinePositionComposer.TargetOffset.x, offsetFactor.x * newOffset.x, transitionSpeed * Time.fixedDeltaTime);
    }
}
