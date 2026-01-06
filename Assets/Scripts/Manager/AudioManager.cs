using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager audioManagerInstance;

    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip landSFX;
    [SerializeField] private AudioClip[] walkSFX;

    [SerializeField] private float walkSFXDelay;
    [SerializeField] private float maxDelay;
    private bool canPlayWalkSFX;
    private float walkSFXSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioManagerInstance == null)
        {
            audioManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        PlayBackgroundMusic();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayBackgroundMusic()
    {
        backgroundMusicSource.clip = backgroundMusic;
        backgroundMusicSource.Play();
    }

    public void PlayJumpSFX()
    {
        sfxSource.PlayOneShot(jumpSFX);
    }

    public void PlayLandSFX()
    {
        sfxSource.PlayOneShot(landSFX);
    }

    public void PlayWalkSFX(float horizontalVelocity)
    {
        walkSFXSpeed = horizontalVelocity;

        if (!canPlayWalkSFX)
        {
            canPlayWalkSFX = true;
            WalkSFX().Forget();
        }
    }

    public void StopWalkSFX()
    {
        canPlayWalkSFX = false;
    }

    public async UniTask WalkSFX()
    {
        while (canPlayWalkSFX)
        {
            int randomIndex = Random.Range(0, walkSFX.Length);
            sfxSource.PlayOneShot(walkSFX[randomIndex]);
            float delay = walkSFXDelay * 1000 / walkSFXSpeed;
            delay = Mathf.Min(delay, maxDelay * 1000);
            await UniTask.Delay((int)(delay));
        }
    }
}
