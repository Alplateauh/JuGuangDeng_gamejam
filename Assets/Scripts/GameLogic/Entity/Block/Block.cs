using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Collider2D collider;
    private Bounds bounds;
    private Player player;
    
    public BlockType blockType;
    public Vector2 checkArea;
    public LayerMask playerLayerMask;
    private Vector2[] cornerPos;
    
    
    public Player player;

    private void Start()
    {
        collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        CalculateCornerPositions();
        HandlePlayerLeave();
        HandlePlayerEnter();
    }
    
    
    private void CalculateCornerPositions()
    {
        bounds = collider.bounds;
        
        cornerPos = new Vector2[4];
        // 左上角
        cornerPos[0] = new Vector2(bounds.min.x, bounds.max.y);
        // 右上角
        cornerPos[1] = new Vector2(bounds.max.x, bounds.max.y);
        // 右下角
        cornerPos[2] = new Vector2(bounds.max.x, bounds.min.y);
        // 左下角
        cornerPos[3] = new Vector2(bounds.min.x, bounds.min.y);

        for (int i = 0; i < 4; i++)
        {
            Debug.Log("corner" + i + ":" + cornerPos[i].x + ", " + cornerPos[i].y);
        }
    }
    
    public Vector2[] GetCornerPos()
    {
        return cornerPos;
    }

    private void HandlePlayerEnter()
    {
        Collider2D playerCol = Physics2D.OverlapBox(this.transform.position, checkArea, 0, playerLayerMask);
        if (playerCol != null && player.hitBlock == false)
        {
            player.hitBlock = true;
            player.block = this;
        }
    }
    
    private void HandlePlayerLeave()
    {
        Collider2D playerCol = Physics2D.OverlapBox(this.transform.position, checkArea, 0, playerLayerMask);
        if (playerCol == null && player.hitBlock == true)
        {
            player.hitBlock = false;
            player.hitSide = 0;
            player.block = null;
            player.blockCornerPos = null;
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