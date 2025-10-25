using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginObject : MonoBehaviour
{
    public InteractionType interactionType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PluginManager pluginManager = other.GetComponent<PluginManager>();
        if (pluginManager != null)
        {
            pluginManager.AddPlugin(interactionType);
            Destroy(this.gameObject);
        }
    }
}
