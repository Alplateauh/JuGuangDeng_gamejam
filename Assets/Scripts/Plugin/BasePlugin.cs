using UnityEngine;
using UnityEngine.UI;

public enum InteractionType
{
    X0Register,
    StackPointer,
    GCAutoCollect,
    Async,
    LibraryHooking,
    NoNVIDIA
}

public abstract class BasePlugin : MonoBehaviour
{
    public PluginData pluginData;

    public Text InvokeUI;
    public abstract void Effect();

    public void PlayerEnter()
    {
        InvokeUI.gameObject.SetActive(true);
        InvokeUI.text = pluginData.objectName;
    }

    public void PlayerExit()
    {
        InvokeUI.gameObject.SetActive(false);
        InvokeUI.text = "";
    }
}

[CreateAssetMenu(fileName = "PluginData", menuName = "Plugin/PluginData")]
public class PluginData : ScriptableObject
{
    public string objectName;
    public int number;
    public string description;
    public InteractionType interactionType;
}