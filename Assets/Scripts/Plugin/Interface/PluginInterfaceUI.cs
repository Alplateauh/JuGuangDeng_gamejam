using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PluginInterfaceUI : MonoBehaviour
{
    private PluginManager pluginManager;
    public List<Button> activeButton = new List<Button>();
    public List<Button> inactiveButton = new List<Button>();
    private List<Button> exchangeButton = new List<Button>();
    
    public Dictionary<Button,BasePlugin> buttonOwnedPlugin = new Dictionary<Button, BasePlugin>();
    
    public void SetManager(PluginManager pluginManager)
    {
        this.pluginManager = pluginManager;
    }

    void ShowPlugin()
    {
        int active = 0;
        int inactive = 0;
        foreach (BasePlugin plugin in pluginManager.pickedPlugins)
        {
            if (!pluginManager.activePlugins.Contains(plugin))
            {
                inactiveButton[inactive].interactable = true;
                inactiveButton[inactive].image.sprite = plugin.pluginData.icon;
                buttonOwnedPlugin[inactiveButton[inactive]] = plugin;
                inactive++;
            }
        }

        foreach (BasePlugin plugin in pluginManager.activePlugins)
        {
            activeButton[active].interactable = true;
            activeButton[active].image.sprite = plugin.pluginData.icon;
            buttonOwnedPlugin[activeButton[active]] = plugin;
            active++;
        }
        
        if (active > 2)
        {
            Debug.LogError("激活插件过多");
        }
    }

    void ExchangePlugin(Button button)
    {
        if (exchangeButton.Count == 0)
        {
            exchangeButton.Add(button);
        }
        else if (exchangeButton.Contains(button))
        {
            exchangeButton.Remove(button);
        }
        else if (exchangeButton.Count == 1)
        {
            BasePlugin plugin1 = buttonOwnedPlugin[exchangeButton[0]];
            BasePlugin plugin2 = buttonOwnedPlugin[button];
            pluginManager.ExchangePlugin(plugin1, plugin2);
            exchangeButton.Clear();
        }
        ShowPlugin();
    }
}
