using System.Collections;
using UnityEngine;

public class BugRealize : BugBase
{
    float timer;
    public float time;
    
    public BugType type;
    
    private Coroutine countdownCoroutine = null;
    
    void Start()
    {
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
        
        EventCallBack._instance.CalBackEvent(type);
        timer = 0;
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        yield break;
    }
}