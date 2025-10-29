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

    public Dictionary<InteractionType, BasePlugin> pluginPool = new Dictionary<InteractionType, BasePlugin>();

    [SerializeField][ReadOnly]public List<BasePlugin> pickedPlugins = new List<BasePlugin>();
    [SerializeField][ReadOnly]public List<BasePlugin> activePlugins = new List<BasePlugin>();
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
        if (pickedPlugins.Any(p => p.pluginData.interactionType == interactionType))
        {
            Debug.LogWarning($"试图重复拾取插件: {interactionType}。操作忽略。");
            return;
        }

        if (pluginPool.TryGetValue(interactionType, out BasePlugin pluginToActivate))
        {
            pickedPlugins.Add(pluginToActivate);
            Debug.Log($"插件 '{pluginToActivate.pluginData.pluginName}' 已被拾取。");
            if (pluginToActivate.pluginData.autoActivate)
            {
                ActivePlugin(pluginToActivate);
                Debug.Log($"尝试自动激活插件‘{pluginToActivate.pluginData.pluginName}’");
            }
        }
        else
        {
            Debug.LogError($"在插件池中找不到类型为 {interactionType} 的插件！无法拾取。");
        }
    }

    /// <summary>
    /// 停用一个插件，并将其从激活列表中移除。
    /// </summary>
    /// <param name="interactionType">要停用的插件的类型。</param>
    public void RemovePlugin(InteractionType interactionType)
    {
        // 1. 在激活列表中查找要移除的插件
        BasePlugin pluginToRemove = pickedPlugins.FirstOrDefault(p => p.pluginData.interactionType == interactionType);

        // 2. 如果找到了，就将其从列表中移除
        if (pluginToRemove != null)
        {
            pluginToRemove.SetPlayer(null);
            pickedPlugins.Remove(pluginToRemove);
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
                pluginPool.Add(data.interactionType, newInstance);
            }
        }
        Debug.Log($"从文件夹加载并创建了 {pluginPool.Count} 个插件实例。");
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

    public void ExchangePlugin(BasePlugin plugin1, BasePlugin plugin2)
    {
        if (!pickedPlugins.Contains(plugin1) || !pickedPlugins.Contains(plugin2))
        {
            Debug.LogError("错误：试图交换一个或多个玩家未拥有的插件。");
            return;
        }

        bool isPlugin1Active = activePlugins.Contains(plugin1);
        bool isPlugin2Active = activePlugins.Contains(plugin2);

        if (isPlugin1Active ^ isPlugin2Active) 
        {
            if (isPlugin1Active)
            {
                int index = activePlugins.IndexOf(plugin1);
                activePlugins[index] = plugin2;
                Debug.Log($"插件 '{plugin1.pluginData.pluginName}' 已被替换为 '{plugin2.pluginData.pluginName}'。");
                plugin1.SetPlayer(null);
                plugin2.SetPlayer(player);
            }
            else
            {
                int index = activePlugins.IndexOf(plugin2);
                activePlugins[index] = plugin1;
                Debug.Log($"插件 '{plugin2.pluginData.pluginName}' 已被替换为 '{plugin1.pluginData.pluginName}'。");
                plugin2.SetPlayer(null);
                plugin1.SetPlayer(player);
            }
        }
        else if (isPlugin1Active && isPlugin2Active)
        {
            int index1 = activePlugins.IndexOf(plugin1);
            int index2 = activePlugins.IndexOf(plugin2);

            BasePlugin temp = activePlugins[index1];
            activePlugins[index1] = activePlugins[index2];
            activePlugins[index2] = temp;
            Debug.Log($"'{plugin1.pluginData.pluginName}' 和 '{plugin2.pluginData.pluginName}' 交换了位置。");
        }
        else
        {
            int index1 = pickedPlugins.IndexOf(plugin1);
            int index2 = pickedPlugins.IndexOf(plugin2);

            // 执行交换
            BasePlugin temp = pickedPlugins[index1];
            pickedPlugins[index1] = pickedPlugins[index2];
            pickedPlugins[index2] = temp;
            
            Debug.Log($"未激活插件 '{plugin1.pluginData.pluginName}' 和 '{plugin2.pluginData.pluginName}' 交换了它们的位置。");
        }
    }

    public void ActivePlugin(BasePlugin plugin)
    {
        if (activePlugins.Count == 3)
        {
            Debug.Log("激活插件达到上限");
            return;
        }
        if (pickedPlugins.Contains(plugin)&&!activePlugins.Contains(plugin))
        {
            activePlugins.Add(plugin);
            plugin.SetPlayer(player);
            Debug.Log($"激活未激活脚本‘{plugin.pluginData.pluginName}’");
        }
        else
        {
            Debug.LogError("未拾取插件");
        }
    }
    
    void Test()
    {
        
    }

    public void RefreshPlugins()
    {
        
    }
}