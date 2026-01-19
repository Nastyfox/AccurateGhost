using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startPosX;
    private float startPosY;
    private float length;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float parallaxEffectX;

    private void Start()
    {
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0f);
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        float distanceX = mainCamera.transform.position.x * parallaxEffectX;
        float movementX = mainCamera.transform.position.x * (1 - parallaxEffectX);

        transform.position = new Vector3(startPosX + distanceX, mainCamera.transform.position.y, transform.position.z);
        if(movementX > startPosX + length)
        {
            startPosX += length;
        }
        else if(movementX < startPosX - length)
        {
            startPosX -= length;
        }
    }
}
