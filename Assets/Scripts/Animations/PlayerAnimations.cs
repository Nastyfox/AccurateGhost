using UnityEngine;

public enum AnimationType
{
    Idle,
    Move,
    Jump,
    Land
}

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                break;
            case AnimationType.Move:
                playerAnimator.SetBool("IsMoving", true);
                playerAnimator.ResetTrigger("Land");
                break;
            case AnimationType.Jump:
                playerAnimator.SetTrigger("Jump");
                playerAnimator.ResetTrigger("Land");
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
