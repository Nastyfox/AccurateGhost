using Unity.Cinemachine;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startPosX;
    private float length;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float parallaxEffectX;

    private void Start()
    {
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0f);
        startPosX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!GameManager.gameManagerInstance.GetIsCameraFollowingGhost())
        {
            ParallaxEffect();
        }
#else
        ParallaxEffect();
#endif
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (GameManager.gameManagerInstance.GetIsCameraFollowingGhost())
        {
            ParallaxEffect();
        }
#endif
    }

    private void ParallaxEffect()
    {
        float distanceX = mainCamera.transform.position.x * parallaxEffectX;
        float movementX = mainCamera.transform.position.x * (1 - parallaxEffectX);

        transform.position = new Vector3(startPosX + distanceX, mainCamera.transform.position.y, transform.position.z);
        if (movementX > startPosX + length)
        {
            startPosX += length;
        }
        else if (movementX < startPosX - length)
        {
            startPosX -= length;
        }
    }
}
