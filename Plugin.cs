using System;
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Chat.API;
using Chat.Core;
using HarmonyLib;
using Newtonsoft.Json.Linq;
 
namespace Chat;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.agitoreiken.database", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    public static Plugin Instance = null!;
    public IChatAPI API = null!;
    public override void Load()
    {
        Instance = this;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loading...");

        API = new ChatAPI();
    }

    public override bool Unload()
    {
        return true;
    } 
}
