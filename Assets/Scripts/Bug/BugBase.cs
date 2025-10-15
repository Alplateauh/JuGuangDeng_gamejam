using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    [Range(1, 10)]
    public int rayCount = 3;
    
    protected float tileWidth;

    protected virtual void OnPlayerTrigger() { }
    protected virtual void OnPlayerMove() { }
    
    protected IEnumerator RaycastCheck()
    {
        while (true)
        {
            Vector2[] rayOrigins = CalculateRayOrigins();
            bool playerClicked = false;
            

            foreach (Vector2 origin in rayOrigins)
            {

                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, RAY_DISTANCE, playerLayer);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    playerClicked = true;
                    break;
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
        
        for (int i = 0; i < rayCount; i++)
        {
            float xOffset = ((i + 1) * spacing) - (tileWidth / 2);

            origins[i] = (Vector2)transform.position + new Vector2(xOffset, 0);
        }

        origins[origins.Length - 1] = (Vector2)transform.position + new Vector2(-tileWidth / 2, 0);
        origins[origins.Length - 2] = (Vector2)transform.position + new Vector2(tileWidth / 2, 0);
        
        return origins;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2[] origins = CalculateRayOrigins();
        
        foreach (Vector2 origin in origins)
        {

            Gizmos.DrawRay(origin, Vector2.up * RAY_DISTANCE);
            Gizmos.DrawWireCube(origin, new Vector3(0.05f, 0.05f, 0.05f));
        }
    }
}