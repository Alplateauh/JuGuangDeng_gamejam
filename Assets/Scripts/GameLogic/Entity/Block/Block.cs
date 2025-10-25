using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Collider2D collider;
    //private Player player;
    
    public BlockType blockType;
    public Vector2 checkArea;
    public LayerMask playerLayerMask;
    private Vector2[] cornerPos;
    
    
    public Player player;

    private void Start()
    {
        collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D playerCol)
    {
        int layerMaskOfCol = 1 << playerCol.gameObject.layer;
        if ((playerLayerMask.value & layerMaskOfCol) != 0)
        {
            player = playerCol.gameObject.GetComponent<Player>();
            if (player != null)
                player.isHitBlock = true;
        }
    }

    private void OnCollisionExit2D(Collision2D playerCol)
    {
        int layerMaskOfCol = 1 << playerCol.gameObject.layer;
        if ((playerLayerMask.value & layerMaskOfCol) != 0)
        {
            if (player != null)
            {
                player.isHitBlock = false;
                player = null;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, checkArea);
        
        if (cornerPos != null && cornerPos.Length == 4)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < cornerPos.Length; i++)
            {
                Gizmos.DrawSphere(cornerPos[i], 0.1f);
            }
        }
    }
}

public enum BlockType
{
    Rough,
    Smooth,
}