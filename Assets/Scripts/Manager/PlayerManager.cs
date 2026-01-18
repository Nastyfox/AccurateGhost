using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private ParticleSystem wallSlideParticles;
    [SerializeField] private ParticleSystem wallJumpParticles;
    [SerializeField] private ParticleSystem speedParticles;
    [SerializeField] private TrailRenderer dashTrail;

    private void Start()
    {
        ParticlesManager.particlesManagerInstance.SetPlayerParticles(
            landParticles,
            jumpParticles,
            moveParticles,
            wallSlideParticles,
            wallJumpParticles,
            speedParticles,
            dashTrail
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
