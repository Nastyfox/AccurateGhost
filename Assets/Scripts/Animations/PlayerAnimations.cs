using UnityEngine;

public enum AnimationType
{
    Idle,
    Move,
    Jump,
    Land,
    Wall
}

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private Playback savePlayback;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        savePlayback = GameObject.Find("GameManager").GetComponent<Playback>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAnimation(AnimationType animationType, float animationSpeed)
    {
        switch(animationType)
        {
            case AnimationType.Land:
                playerAnimator.SetTrigger("Land");
                playerAnimator.ResetTrigger("Jump");
                playerAnimator.ResetTrigger("Wall");
                savePlayback.NotifyTrigger(Playback.TriggerType.Land);
                break;
            case AnimationType.Move:
                playerAnimator.SetBool("IsMoving", true);
                playerAnimator.ResetTrigger("Land");
                break;
            case AnimationType.Jump:
                playerAnimator.SetTrigger("Jump");
                playerAnimator.ResetTrigger("Land");
                playerAnimator.ResetTrigger("Wall");
                savePlayback.NotifyTrigger(Playback.TriggerType.Jump);
                break;
            case AnimationType.Wall:
                playerAnimator.SetTrigger("Wall");
                playerAnimator.ResetTrigger("Jump");
                savePlayback.NotifyTrigger(Playback.TriggerType.Wall);
                break;
            default:
                break;
        }

        playerAnimator.speed = animationSpeed;
    }

    public void StopAnimation(AnimationType animationType)
    {
        switch(animationType)
        {
            case AnimationType.Move:
                playerAnimator.SetBool("IsMoving", false);
                break;
            case AnimationType.Land:
                playerAnimator.ResetTrigger("Land");
                break;
            default:
                break;
        }
    }
}
