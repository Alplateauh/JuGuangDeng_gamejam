using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginInterface : MonoBehaviour
{
    public GameObject interfaceUI;

    private void Awake()
    {
        interfaceUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PluginManager pluginManager = other.gameObject.GetComponent<PluginManager>();
        if (pluginManager != null)
        {
            interfaceUI.GetComponent<PluginInterfaceUI>().SetManager(pluginManager);
            interfaceUI.SetActive(true);
        }
    }
}
