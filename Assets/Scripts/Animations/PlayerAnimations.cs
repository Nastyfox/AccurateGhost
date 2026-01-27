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
        switch (animationType)
        {
            case AnimationType.Land:
                //playerAnimator.ResetTrigger("Jump");
                //playerAnimator.ResetTrigger("Wall");
                playerAnimator.SetTrigger("Land");
                savePlayback.NotifyTrigger(Playback.TriggerType.Land);
                break;
            case AnimationType.Move:
                //playerAnimator.ResetTrigger("Land");
                playerAnimator.SetBool("IsMoving", true);
                break;
            case AnimationType.Jump:
                //playerAnimator.ResetTrigger("Land");
                playerAnimator.ResetTrigger("Wall");
                playerAnimator.SetTrigger("Jump");
                savePlayback.NotifyTrigger(Playback.TriggerType.Jump);
                break;
            case AnimationType.Wall:
                //playerAnimator.ResetTrigger("Jump");
                playerAnimator.SetTrigger("Wall");
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
            default:
                break;
        }
    }
}
