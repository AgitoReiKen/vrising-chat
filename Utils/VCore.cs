using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using Unity.Entities;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using UnityEngine;

namespace Chat.Utils;

public class VCore
{
    public static ComponentType[] ConnectedUserComponents =  [
        ComponentType.ReadOnly(Il2CppType.Of<ConnectedUser>()),
        ComponentType.ReadOnly(Il2CppType.Of<User>())
    ];  
    public static ComponentType[] UserComponents =  [
        ComponentType.ReadOnly(Il2CppType.Of<User>())
    ];

    public static EntityManager EntityManager => Server.EntityManager;

    public static EntityQuery? _ConnectedUserQuery;

    public static EntityQuery ConnectedUserQuery =>
        _ConnectedUserQuery ??= Server.EntityManager.CreateEntityQuery(ConnectedUserComponents);
    
    public static EntityQuery? _UserQuery;
    public static EntityQuery UserQuery => _UserQuery ??= Server.EntityManager.CreateEntityQuery(UserComponents);
    
    private static World? _server;
    public static World Server => _server ??= GetWorld(nameof(Server));

    private static World GetWorld(string name)
    {
        foreach (World sAllWorld in World.s_AllWorlds)
        {
            if (sAllWorld.Name == name)
            {
                return sAllWorld;
            }
        }

        throw new Exception($"Couldn't find world with name \"{name}\"");
    } 
    public static List<Entity> GetConnectedPlayerEntities()
    {
        var users = ConnectedUserQuery.ToEntityArray(Allocator.Temp);
        var userList = new List<Entity>();
        for (int i = 0; i < users.Length; i++)
        {
            userList.Add(users[i]);
        }

        return userList;
    }
}