using UnityEngine;

public enum ParticleType
{
    Land,
    Jump,
    Move,
    Dash
}

public class PlayerParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private float moveParticlesOffsetX;

    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private float dashTrailOffsetX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OffsetParticles(float signOffset, ParticleType particleType)
    {
        switch(particleType)
        {
            default:
                break;
            case ParticleType.Move:
                moveParticles.gameObject.transform.localPosition = new Vector3(moveParticlesOffsetX * signOffset, moveParticles.gameObject.transform.localPosition.y, moveParticles.gameObject.transform.localPosition.z);
                break;
            case ParticleType.Dash:
                dashTrail.gameObject.transform.localPosition = new Vector3(dashTrailOffsetX * signOffset, dashTrail.gameObject.transform.localPosition.y, dashTrail.gameObject.transform.localPosition.z);
                break;
        }
    }
    public void PlayParticles(ParticleType particleType)
    {
        if(particleType == ParticleType.Dash)
        {
            dashTrail.emitting = true;
            return;
        }

        ParticleSystem particleSystem = SelectParticleSystem(particleType);

        if (particleSystem.isPlaying)
        {
            return;
        }
        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
    }

    public void StopParticles(ParticleType particleType)
    {
        if (particleType == ParticleType.Dash)
        {
            dashTrail.emitting = false;
            return;
        }

        ParticleSystem particleSystem = SelectParticleSystem(particleType);

        particleSystem.Stop();
        particleSystem.gameObject.SetActive(false);
    }

    private ParticleSystem SelectParticleSystem(ParticleType particleType)
    {
        switch (particleType)
        {
            default:
                return null;
            case ParticleType.Land:
                return landParticles;
            case ParticleType.Jump:
                return jumpParticles;
            case ParticleType.Move:
                return moveParticles;
        }
    }

    public bool IsPlaying(ParticleType particleType)
    {
        ParticleSystem particleSystem = SelectParticleSystem(particleType);
        return particleSystem.isPlaying;
    }
}
