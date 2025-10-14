using UnityEngine;

public abstract class BasePlugin : MonoBehaviour
{
    public PluginData pluginData;
    public abstract void Effect();
}

[CreateAssetMenu(fileName = "PluginData", menuName = "Plugin/PluginData")]
public class PluginData : ScriptableObject
{
    public string objectName;
    public int number;
    public string description;
}