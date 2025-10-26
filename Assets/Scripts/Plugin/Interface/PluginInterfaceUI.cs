using System;
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

    void Start()
    {
        foreach (Button button in activeButton)
        {
            button.onClick.AddListener(() => { ExchangePlugin(button);});
        }

        foreach (Button button in inactiveButton)
        {
            button.onClick.AddListener(() => { ExchangePlugin(button);});
        }
    }

    private void OnEnable()
    {
        if (pluginManager)
        {
            Debug.Log("禁用跳跃");
            pluginManager.gameObject.GetComponent<Player>().OpenOrCloseJumpInputWindow(false);
        }
    }

    private void OnDisable()
    {
        if (pluginManager)
        {
            Debug.Log("启动跳跃");
            pluginManager.gameObject.GetComponent<Player>().OpenOrCloseJumpInputWindow(true);
        }
    }

    public void SetManager(PluginManager pluginManager)
    {
        this.pluginManager = pluginManager;
        Debug.Log("禁用跳跃");
        pluginManager.gameObject.GetComponent<Player>().OpenOrCloseJumpInputWindow(false);
        ShowPlugin();
    }

    void ShowPlugin()
    {
        int active = 0;
        int inactive = 0;
        Debug.Log(pluginManager.pickedPlugins.Count);
        foreach (BasePlugin plugin in pluginManager.pickedPlugins)
        {
            if (!pluginManager.activePlugins.Contains(plugin))
            {
                inactiveButton[inactive].gameObject.SetActive(true);
                inactiveButton[inactive].interactable = true;
                buttonOwnedPlugin[inactiveButton[inactive]] = plugin;
                Image image = inactiveButton[inactive].GetComponent<Image>();
                //image.sprite = plugin.pluginData.icon;
                image.color = plugin.pluginData.color;
                Debug.Log("Color:"+image.color);
                inactive++;
            }
        }

        foreach (BasePlugin plugin in pluginManager.activePlugins)
        {
            inactiveButton[active].gameObject.SetActive(true);
            activeButton[active].interactable = true;
            buttonOwnedPlugin[activeButton[active]] = plugin;
            Image image = activeButton[active].GetComponent<Image>();
            //image.sprite = plugin.pluginData.icon;
            image.color = plugin.pluginData.color;
            Debug.Log("Color:"+image.color);
            active++;
        }
        
        if (active > 3)
        {
            Debug.LogError("激活插件过多");
        }

        for (; inactive < 6; inactive++)
        {
            inactiveButton[inactive].gameObject.SetActive(false);
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
            if (buttonOwnedPlugin.ContainsKey(exchangeButton[0]) && buttonOwnedPlugin.ContainsKey(button))
            {
                BasePlugin plugin1 = buttonOwnedPlugin[exchangeButton[0]];
                BasePlugin plugin2 = buttonOwnedPlugin[button];
                pluginManager.ExchangePlugin(plugin1, plugin2);
            }
            else if (buttonOwnedPlugin.ContainsKey(button)&&activeButton.Contains(exchangeButton[0]))
            {
                BasePlugin plugin1 = buttonOwnedPlugin[button];
                pluginManager.ActivePlugin(plugin1);
            }
            else if (buttonOwnedPlugin.ContainsKey(exchangeButton[0]) &&activeButton.Contains(button))
            {
                BasePlugin plugin1 = buttonOwnedPlugin[exchangeButton[0]];
                pluginManager.ActivePlugin(plugin1);
            }
            exchangeButton.Clear();
        }
        ShowPlugin();
    }
}
