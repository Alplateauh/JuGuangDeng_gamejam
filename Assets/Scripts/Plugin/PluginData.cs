using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PluginData", menuName = "Plugin/PluginData")]
public class PluginData : ScriptableObject
{
    public string pluginName;
    public int number;
    public string description;
    public InteractionType interactionType;
    public Sprite icon;
    public Color color;
}
