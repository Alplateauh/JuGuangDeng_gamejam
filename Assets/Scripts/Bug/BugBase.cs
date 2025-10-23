using System.Collections;
using System.Collections.Generic;
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
    
    [Min(1)]
    public int rayCount = 1;
    
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

    [Min(1)]
    public float RayScale;
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
        
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, RAY_DISTANCE * RayScale, playerLayer);
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

                    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, RAY_DISTANCE* RayScale, playerLayer);
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
    
       protected List<Vector3Int> GetAllTiles()
        {
            List<Vector3Int> positions = new List<Vector3Int>();
            
            if (tilemap != null)
            {
                BoundsInt bounds = tilemap.cellBounds;
                foreach (var position in bounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(position))
                    {
                        positions.Add(position);
                    }
                }
                positions.Sort((a, b) => a.x.CompareTo(b.x));
            }
            
            return positions;
        }
        
        
        protected void ChangeTileColor(Vector3Int position, float saturationDelta)
        {
            if (tilemap == null) return;
            
            Color currentColor = tilemap.GetColor(position);
    
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);
    
            s = Mathf.Clamp01(s + saturationDelta / 100f); // 注意：除以100，因为你的滑块是0-100
    
            Color newColor = Color.HSVToRGB(h, s, v);
            newColor.a = currentColor.a;
            
            tilemap.RemoveTileFlags(position, TileFlags.LockColor);
            tilemap.SetColor(position, newColor);
            
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
                
                Gizmos.DrawRay(rayOrigin, direction * RAY_DISTANCE * RayScale);

                Gizmos.DrawWireSphere(rayOrigin, 0.02f);
                
                Vector2 rayEnd = rayOrigin + direction * RAY_DISTANCE * RayScale;
                Gizmos.DrawWireSphere(rayEnd, 0.01f);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, RAY_DISTANCE * RayScale);
        }
        else
        {
            Vector2[] origins = CalculateRayOrigins();
        
            foreach (Vector2 origin in origins)
            {
                Gizmos.DrawRay(origin, Vector2.up * RAY_DISTANCE * RayScale);
                Gizmos.DrawWireCube(origin, new Vector3(0.05f, 0.05f, 0.05f));
            }
        }
    }
}