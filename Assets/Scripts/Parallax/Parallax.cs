using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startPos;
    private float length;
    [SerializeField]
    private GameObject mainCamera;
    [SerializeField]
    private float parallaxEffect;

    private void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        float distance = mainCamera.transform.position.x * parallaxEffect;
        float movement = mainCamera.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
        if(movement > startPos + length)
        {
            startPos += length;
        }
        else if(movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
