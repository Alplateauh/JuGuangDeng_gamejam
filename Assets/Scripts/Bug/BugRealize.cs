using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class BugRealize : BugBase
{
    float timer;
    public float time;
    
    private Coroutine countdownCoroutine = null;
    
    public BugType type;

    [Min(0)]
    public float distance;
    public delegate void BugEventHandler(out bool flag);

    private Dictionary<BugType, BugEventHandler> BugEvent;

    [Range(0,100)]
    public float ColorSChange;

    private List<Vector3Int> ColorTile = new List<Vector3Int>();
    
    public int ColorCount = 0;

    public Transform EndPosition;
    void Start()
    {

        tileCount = ColorCount;
        
        Renderer tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            tileWidth = tileRenderer.bounds.size.x;
        }
        else
        {
            Collider tileCollider = GetComponent<Collider>();
            tileWidth = tileCollider != null ? tileCollider.bounds.size.x : 1f;
        }
        

        if (type == BugType.ArgumentOut)
        {
            List<Vector3Int> allTiles = GetAllTiles();
            
            int Count = Mathf.Min(Mathf.Abs(tileCount), allTiles.Count);
            bool Right = tileCount > 0;
            
            for (int i = 0; i < Count; i++)
            {
                int index = Right ? i : allTiles.Count - 1 - i;
                Vector3Int tile = allTiles[index];
                
                ColorTile.Add(tile);
                ChangeTileColor(tile, ColorSChange);
            }

            tileWidth = ColorCount;

        }
        else
        {

            tileCount = 0;
        }

        if (type == BugType.Undefined)
        {
            isCircle = true;
        }
        else
        {
            isCircle = false;
        }

        StartCoroutine(base.RaycastCheck());
    }

    protected override void OnPlayerTrigger()
    {

        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(Countdown());
        }
    }

    protected override void OnPlayerMove()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        timer = 0;
    }

    IEnumerator Countdown()
    {
        while (timer < time)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (BugEvent == null)
        {
            BugEvent = new Dictionary<BugType, BugEventHandler>
            {
                { BugType.Stack, BugStackEvent },
                { BugType.Heap, BugHeapEvent },
                { BugType.Null, BugNullEvent },
                { BugType.Undefined, BugUndefinedEvent },
                { BugType.ArgumentOut, BugArgumentOutEvent },
                { BugType.Missing, BugMissingEvent }
            };
        }


        bool BugFl = false;
        
        BugEvent[type]?.Invoke(out BugFl);

        if (BugFl)
        {
            timer = 0;
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            yield break;
        }
    }
   
    
    //半完成,缺少shader
    void BugStackEvent(out bool Fl)
    {
        //Todo : 制作对应Shader
        //
        
        Vector3 EndPosition = transform.position + Random.insideUnitSphere * distance;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,EndPosition);
        
        
        transform.DOMove(EndPosition,AnimTime)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => 
            {
                //
            });
        
            Fl = CalkBackFl;
       
    }

    //完成
    void BugHeapEvent(out bool Fl)
    {
        _analogGlitch.active = true;
        
        Vector3 EndPosition = transform.position + Random.insideUnitSphere * distance;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,EndPosition);
        
        DOVirtual.DelayedCall(AnimTime, () => 
        {
            _analogGlitch.active = false;
        });
        
        Fl = CalkBackFl;
        
    }

    //完成
    void BugNullEvent(out bool Fl)
    {
        _digitalGlitch.active = true;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,true);
        
        gameObject.SetActive(false);
        
        DOVirtual.DelayedCall(AnimTime, () => 
        {
            gameObject.SetActive(true);
            _digitalGlitch.active = false;
        });
        
        Fl = CalkBackFl;
    }
    
    //完成
    void BugUndefinedEvent(out bool Fl)
    {
        _vignette.active = true;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,true);
        
        DOVirtual.DelayedCall(AnimTime, () => 
        {
            _vignette.active = false;
            
        });
        
        Fl = CalkBackFl;
    }

    //完成
    void BugArgumentOutEvent(out bool Fl)
    {
        _snowGlitch.active = true;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,EndPosition.position);
        
        foreach (var tile in  ColorTile)
        {
            tilemap.SetTile(tile, null);
        }
        
        transform.DORotate(new Vector3(0, 0, 180), AnimTime, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => 
            {
                _snowGlitch.active = false;
            });
        
        Fl = CalkBackFl;
    }
    
    //完成
    void BugMissingEvent(out bool Fl)
    {
        _twistGlitch.active = true;
        
        bool CalkBackFl = false;
        
        EventCallBack._instance.CalBackEvent(type,out CalkBackFl,time,true);
        
        Fl = CalkBackFl;
    }
    
    
    List<Vector3Int> GetAllTiles()
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
    
    
    void ChangeTileColor(Vector3Int position, float saturationDelta)
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
    

}