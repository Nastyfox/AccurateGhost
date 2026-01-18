using UnityEngine;

public enum ParticleType
{
    Land,
    Jump,
    Move,
    Dash,
    Speed,
    WallSlide,
    WallJump
}

public class ParticlesManager : MonoBehaviour
{
    public static ParticlesManager particlesManagerInstance;

    private ParticleSystem landParticles;
    private ParticleSystem jumpParticles;
    private ParticleSystem moveParticles;
    private ParticleSystem wallSlideParticles;
    private ParticleSystem wallJumpParticles;
    [SerializeField] private float moveParticlesOffsetX;
    private ParticleSystem speedParticles;
    [SerializeField] private float speedParticlesOffsetX;
    [SerializeField] private float speedParticlesEmissionRotation;
    [SerializeField] private float speedParticlesSpeedFactor;
    [SerializeField] private Vector2 minSpeedParticlesStartSpeed;
    [SerializeField] private float wallSlideParticlesOffsetX;
    [SerializeField] private float wallJumpParticlesOffsetX;

    private float lastOffsetSign;

    private TrailRenderer dashTrail;
    [SerializeField] private float dashTrailOffsetX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (particlesManagerInstance == null)
        {
            particlesManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerParticles(ParticleSystem landParticles, ParticleSystem jumpParticles, ParticleSystem moveParticles, ParticleSystem wallSlideParticles, ParticleSystem wallJumpParticles, ParticleSystem speedParticles, TrailRenderer dashTrail)
    {
        this.landParticles = landParticles;
        this.jumpParticles = jumpParticles;
        this.moveParticles = moveParticles;
        this.wallSlideParticles = wallSlideParticles;
        this.wallJumpParticles = wallJumpParticles;
        this.speedParticles = speedParticles;
        this.dashTrail = dashTrail;
    }

    public void OffsetParticles(float signOffset)
    {
        if (lastOffsetSign != signOffset)
        {
            lastOffsetSign = signOffset;

            moveParticles.gameObject.transform.localPosition = new Vector3(moveParticlesOffsetX * signOffset, moveParticles.gameObject.transform.localPosition.y, moveParticles.gameObject.transform.localPosition.z);

            wallSlideParticles.gameObject.transform.localPosition = new Vector3(wallSlideParticlesOffsetX * -signOffset, wallSlideParticles.gameObject.transform.localPosition.y, wallSlideParticles.gameObject.transform.localPosition.z);

            wallJumpParticles.gameObject.transform.localPosition = new Vector3(wallJumpParticlesOffsetX * -signOffset, wallJumpParticles.gameObject.transform.localPosition.y, wallJumpParticles.gameObject.transform.localPosition.z);

            dashTrail.gameObject.transform.localPosition = new Vector3(dashTrailOffsetX * signOffset, dashTrail.gameObject.transform.localPosition.y, dashTrail.gameObject.transform.localPosition.z);

            //Deactivate gameobject to move it (because of rate over distance for particle system)
            speedParticles.gameObject.SetActive(false);
            speedParticles.gameObject.transform.localPosition = new Vector3(speedParticlesOffsetX * -signOffset, speedParticles.gameObject.transform.localPosition.y, speedParticles.gameObject.transform.localPosition.z);
            ParticleSystem.ShapeModule shape = speedParticles.shape;
            shape.rotation = new Vector3(shape.rotation.x, speedParticlesEmissionRotation * signOffset, shape.rotation.z);
            speedParticles.gameObject.SetActive(true);
            speedParticles.Play();
        }
        
    }

    public void ChangeSpeedParticles(float horizontalVelocity, ParticleType particleType)
    {
        switch (particleType)
        {
            default:
                break;
            case ParticleType.Speed:
                ParticleSystem.MainModule speedParticlesMain = speedParticles.main;
                ParticleSystem.MinMaxCurve startSpeed = speedParticlesMain.startSpeed;
                startSpeed.constantMin = minSpeedParticlesStartSpeed.x * horizontalVelocity * speedParticlesSpeedFactor;
                startSpeed.constantMax = minSpeedParticlesStartSpeed.x * horizontalVelocity * speedParticlesSpeedFactor;
                startSpeed.constantMin = Mathf.Max(startSpeed.constantMin, minSpeedParticlesStartSpeed.x);
                startSpeed.constantMax = Mathf.Max(startSpeed.constantMax, minSpeedParticlesStartSpeed.y);
                speedParticlesMain.startSpeed = startSpeed;
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
            case ParticleType.Speed:
                return speedParticles;
            case ParticleType.WallSlide:
                return wallSlideParticles;
            case ParticleType.WallJump:
                return wallJumpParticles;
        }
    }

    public bool IsPlaying(ParticleType particleType)
    {
        ParticleSystem particleSystem = SelectParticleSystem(particleType);
        return particleSystem.isPlaying;
    }
}
