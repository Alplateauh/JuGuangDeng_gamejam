using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public enum BugType
{
    Stack,
    Heap,
    Null,
    Undefined,
    ArgumentOut,
    Missing,
}


public class BugBase : MonoBehaviour
{
    private const float RAY_DISTANCE = 0.3f;
    private const float CHECK_INTERVAL = 0.1f;
    public LayerMask playerLayer;

    protected Tilemap tilemap;
    
    [Min(0)]
    public float AnimTime;
    
    [Range(1, 10)]
    public int rayCount;
    
    protected float tileWidth;

    protected virtual void OnPlayerTrigger() { }
    protected virtual void OnPlayerMove() { }

    protected Volume _Volume;

    protected Vignette _vignette;
    protected AnalogGlitchVolume _analogGlitch;
    protected DigitalGlitchVolume _digitalGlitch;
    protected SnowGlitchVolume _snowGlitch;
    protected TwistGlitchVolume _twistGlitch;
    
    protected int tileCount;

    protected bool isCircle;
    private void Awake()
    {
        _Volume = FindObjectOfType<Volume>();
        if (_Volume != null)
        {
            _Volume.profile.TryGet(out _vignette);
            _Volume.profile.TryGet(out _analogGlitch);
            _Volume.profile.TryGet(out _digitalGlitch);
            _Volume.profile.TryGet(out _snowGlitch);
            _Volume.profile.TryGet(out _twistGlitch);
            
        }
        
        tilemap = GetComponent<Tilemap>();
    }

    protected IEnumerator RaycastCheck()
    {
        while (true)
        {
            Vector2[] rayOrigins = CalculateRayOrigins();
            bool playerClicked = false;

            if (isCircle)
            {
                float angleStep = 360f / rayCount;
    
                for (int i = 0; i < rayCount; i++)
                {
                    float angle = i * angleStep;
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, RAY_DISTANCE * 2, playerLayer);
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        playerClicked = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (Vector2 origin in rayOrigins)
                {

                    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, RAY_DISTANCE, playerLayer);
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        playerClicked = true;
                        break;
                    }
                }
            }
            

            if (playerClicked)
            {
                OnPlayerTrigger();
            }
            else
            {
                OnPlayerMove();
            }
            
            yield return new WaitForSeconds(CHECK_INTERVAL);
        }
    }

    protected Vector2[] CalculateRayOrigins()
    {
        Vector2[] origins = new Vector2[rayCount + 2];
        float spacing = tileWidth / (rayCount + 1);

        Vector2 CenterPo = transform.position;

        if (tileCount != 0)
        {
            CenterPo = CenterPo + new Vector2(-tileCount,0);
        }

        for (int i = 0; i < rayCount; i++)
        {
            float xOffset = ((i + 1) * spacing) - (tileWidth / 2);

            origins[i] = CenterPo + new Vector2(xOffset, 0);
        }

        origins[origins.Length - 1] = CenterPo + new Vector2(-tileWidth / 2, 0);
        origins[origins.Length - 2] = CenterPo + new Vector2(tileWidth / 2, 0);
        
        return origins;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    
        if (isCircle)
        {
            float angleStep = 360f / rayCount;
        
            for (int i = 0; i < rayCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 rayOrigin = transform.position;
                
                Gizmos.DrawRay(rayOrigin, direction * RAY_DISTANCE * 2);

                Gizmos.DrawWireSphere(rayOrigin, 0.02f);
                
                Vector2 rayEnd = rayOrigin + direction * RAY_DISTANCE * 2;
                Gizmos.DrawWireSphere(rayEnd, 0.01f);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, RAY_DISTANCE * 2);
        }
        else
        {
            Vector2[] origins = CalculateRayOrigins();
        
            foreach (Vector2 origin in origins)
            {
                Gizmos.DrawRay(origin, Vector2.up * RAY_DISTANCE);
                Gizmos.DrawWireCube(origin, new Vector3(0.05f, 0.05f, 0.05f));
            }
        }
    }
}