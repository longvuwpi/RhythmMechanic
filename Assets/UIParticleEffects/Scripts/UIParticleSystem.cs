using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls particle effects in the UI.
/// </summary>
public class UIParticleSystem : MonoBehaviour
{
    /// <summary>
    /// Particle Image
    /// </summary>
    [SerializeField]
    private Sprite particle;

    /// <summary>
    /// Play Duration
    /// </summary>
    [SerializeField]
    private float duration;

    /// <summary>
    /// Loop Emission
    /// </summary>
    [SerializeField]
    private bool looping;

    /// <summary>
    /// Play Lifetime (if not loopable)
    /// </summary>
    [SerializeField]
    private float lifetime;

    /// <summary>
    /// Particle Emission Speed
    /// </summary>
    [SerializeField]
    public float Speed;

    /// <summary>
    /// Particle Size (will be multiplied with the size over lifetime)
    /// </summary>
    [SerializeField]
    private float size;

    /// <summary>
    /// Particle Rotation per Second
    /// </summary>
    [SerializeField]
    private float rotation;

    /// <summary>
    /// Play Particle Effect On Awake
    /// </summary>
    [SerializeField]
    private bool playOnAwake;

    /// <summary>
    /// Gravity
    /// </summary>
    [SerializeField]
    private float gravity;

    /// <summary>
    /// Emission Per Second
    /// </summary>
    [SerializeField]
    private float emissionsPerSecond;

    /// <summary>
    /// Initial Direction
    /// </summary>
    [SerializeField]
    private Vector2 emissionDirection = new Vector2(0, 1f);

    /// <summary>
    /// Random Range where particles are emitted
    /// </summary>
    [SerializeField]
    private float emissionAngle;

    /// <summary>
    /// Color Over Lifetime
    /// </summary>
    [SerializeField]
    private Gradient colorOverLifetime;

    /// <summary>
    /// Size Over Lifetime
    /// </summary>
    [SerializeField]
    private AnimationCurve sizeOverLifetime;

    /// <summary>
    /// Speed Over Lifetime
    /// </summary>
    [SerializeField]
    private AnimationCurve speedOverLifetime;

    [HideInInspector]
    public bool IsPlaying { get; protected set; }

    protected float Playtime = 0f;
    protected Image[] ParticlePool;
    protected int ParticlePoolPointer;


    // Use this for initialization
    void Start()
    {
    }

    void Awake()
    {
        if(ParticlePool == null)
            Init();
        if(playOnAwake)
            Play();
    }

    private void Init()
    {
        ParticlePoolPointer = 0;
        ParticlePool = new Image[(int)(lifetime * emissionsPerSecond * 1.1f + 1)];
        for(int i = 0; i < ParticlePool.Length; i++)
        {

            var gameObject = new GameObject("Particle");
            gameObject.transform.SetParent(transform);
            gameObject.SetActive(false);
            ParticlePool[i] = gameObject.AddComponent<Image>();
            ParticlePool[i].transform.localRotation = Quaternion.identity;
            ParticlePool[i].transform.localPosition = Vector3.zero;
            ParticlePool[i].sprite = particle;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play()
    {
        IsPlaying = true;
        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        Playtime = 0f;
        var particleTimer = 0f;
        while(IsPlaying && (Playtime < duration || looping))
        {
            Playtime += Time.deltaTime;
            particleTimer += Time.deltaTime;
            while(particleTimer > 1f / emissionsPerSecond)
            {
                particleTimer -= 1f / emissionsPerSecond;
                ParticlePoolPointer = (ParticlePoolPointer + 1) % ParticlePool.Length;
                if(!ParticlePool[ParticlePool.Length - 1 - ParticlePoolPointer].gameObject.activeSelf)
                    StartCoroutine(CoParticleFly(ParticlePool[ParticlePool.Length - 1 - ParticlePoolPointer]));
            }

            yield return new WaitForEndOfFrame();
        }
        IsPlaying = false;
    }

    private IEnumerator CoParticleFly(Image particle)
    {
        particle.gameObject.SetActive(true);
        particle.transform.localPosition = Vector3.zero;
        var particleLifetime = 0f;

        //get default velocity
        var emissonAngle = new Vector3(emissionDirection.x, emissionDirection.y, 0f);
        //apply angle
        emissonAngle = Quaternion.AngleAxis(Random.Range(-emissionAngle / 2f, emissionAngle / 2f), Vector3.forward) * emissonAngle;
        //normalize
        emissonAngle.Normalize();

        var gravityForce = Vector3.zero;

        while(particleLifetime < lifetime)
        {
            particleLifetime += Time.deltaTime;

            //apply gravity
            gravityForce = Vector3.up * gravity * particleLifetime;

            //set position
            particle.transform.position += emissonAngle * speedOverLifetime.Evaluate(particleLifetime / lifetime) * Speed + gravityForce;

            //set scale
            particle.transform.localScale = Vector3.one * sizeOverLifetime.Evaluate(particleLifetime / lifetime) * size;

            //set rortaion
            particle.transform.localRotation = Quaternion.AngleAxis(rotation * particleLifetime, Vector3.forward);

            //set color
            particle.color = colorOverLifetime.Evaluate(particleLifetime / lifetime);

            yield return new WaitForEndOfFrame();
        }

        particle.gameObject.SetActive(false);
    }
}
