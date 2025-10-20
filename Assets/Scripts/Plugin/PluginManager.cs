using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PluginManager : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("指定 Resources 文件夹下存放 PluginData 的子文件夹路径。")]
    [SerializeField] private string pluginDataFolderPath = "Data/PluginData"; // 默认路径

    private Dictionary<InteractionType, BasePlugin> _pluginPool = new Dictionary<InteractionType, BasePlugin>();

    [SerializeField]List<BasePlugin> _activePlugins = new List<BasePlugin>();
    
    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("PluginManager 必须挂在有 Player 组件的对象上！");
            return;
        }
        player.OpenOrCloseJumpInputWindow(false);
        InitializePluginPool();
        Debug.Log("PluginManager Initialized");
    }
    
    /// <summary>
    /// 激活一个插件，并将其添加到激活列表中。
    /// </summary>
    /// <param name="interactionType">要激活的插件的类型。</param>
    public void AddPlugin(InteractionType interactionType)
    {
        // 1. 检查插件是否已在激活列表中
        if (_activePlugins.Any(p => p.pluginData.interactionType == interactionType))
        {
            Debug.LogWarning($"试图重复激活插件: {interactionType}。操作忽略。");
            return;
        }

        // 2. 从对象池中找到要激活的插件实例
        if (_pluginPool.TryGetValue(interactionType, out BasePlugin pluginToActivate))
        {
            // 3. 将其添加到激活列表中进行追踪
            _activePlugins.Add(pluginToActivate);
            pluginToActivate.SetPlayer(player);
            Debug.Log($"插件 '{pluginToActivate.pluginData.pluginName}' 已被添加到激活列表。");
        }
        else
        {
            Debug.LogError($"在插件池中找不到类型为 {interactionType} 的插件！无法激活。");
        }
    }

    /// <summary>
    /// 停用一个插件，并将其从激活列表中移除。
    /// </summary>
    /// <param name="interactionType">要停用的插件的类型。</param>
    public void RemovePlugin(InteractionType interactionType)
    {
        // 1. 在激活列表中查找要移除的插件
        BasePlugin pluginToRemove = _activePlugins.FirstOrDefault(p => p.pluginData.interactionType == interactionType);

        // 2. 如果找到了，就将其从列表中移除
        if (pluginToRemove != null)
        {
            pluginToRemove.SetPlayer(null);
            _activePlugins.Remove(pluginToRemove);
            Debug.Log($"插件 '{pluginToRemove.pluginData.pluginName}' 已从激活列表中移除。");
        }
        else
        {
            Debug.LogWarning($"试图移除一个未被激活的插件: {interactionType}。操作被忽略。");
        }
    }

    /// <summary>
    /// 初始化插件池，根据配置的 PluginData 列表来创建所有实例
    /// </summary>
    /// <summary>
    /// 【核心改动】初始化插件池，自动从 Resources 文件夹加载所有 PluginData。
    /// </summary>
    private void InitializePluginPool()
    {
        PluginData[] allDatas = Resources.LoadAll<PluginData>(pluginDataFolderPath);

        if (allDatas.Length == 0)
        {
            Debug.LogWarning($"在 'Resources/{pluginDataFolderPath}' 路径下没有找到任何 PluginData 资源。");
        }

        foreach (PluginData data in allDatas)
        {
            if (data == null) continue;

            BasePlugin newInstance = CreatePluginInstance(data.interactionType);
            if (newInstance != null)
            {
                newInstance.pluginData = data; 
                _pluginPool.Add(data.interactionType, newInstance);
            }
        }
        Debug.Log($"从文件夹加载并创建了 {_pluginPool.Count} 个插件实例。");
    }

    private BasePlugin CreatePluginInstance(InteractionType interactionType)
    {
        switch (interactionType)
        {
            case InteractionType.X0Register:       return new X0RegisterPlugin();
            case InteractionType.StackPointer:     return new StackPointerPlugin();
            case InteractionType.GCAutoCollect:    return new GCAutoCollectPlugin();
            case InteractionType.Async:            return new AsyncPlugin();
            case InteractionType.LibraryHooking:   return new LibraryHookingPlugin();
            case InteractionType.NoNVIDIA:         return new NoNVIDIAPlugin();
            default:
                Debug.LogError($"未知的 InteractionType: {interactionType}");
                return null;
        }
    }

    void Test()
    {
        
    }
}