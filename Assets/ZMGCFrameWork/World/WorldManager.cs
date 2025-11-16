using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager  
{
    private static List<World> mWorldList = new List<World>();
    /// <summary>
    /// 默认游戏世界
    /// </summary>
    public static World DefaultGameWorld { get; private set; }
    /// <summary>
    /// 构建一个游戏世界
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void CreateWorld<T>() where T: World,new ()
    {
        T world = new T();
        DefaultGameWorld = world;

        //初始化当前游戏世界的程序集脚本
        TypeManager.InitlizateWorldAssemblies(world, GetBehaviourExecution(world));
        world.OnCreate();
        mWorldList.Add(world);

    }
    
    public static void UpdateWorld() 
    {
        DefaultGameWorld.OnUpdate();
 
    }

    public static  IBehaviourExecution GetBehaviourExecution(World world)
    {
        if (world.GetType().Name=="BattleWorld")
        {
            return new BattleWorldScriptExecutionOrder();
        }

        return null;
    }

    /// <summary>
    /// 销毁指定游戏世界
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="world"></param>
    public static void DestroyWorld<T>()where T:World
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            if (mWorldList[i].GetType().Name == typeof(T).Name)
            {
                mWorldList[i].DestoryWorld(typeof(T).Namespace);
                mWorldList.Remove(mWorldList[i]);
                break;
            }
        }
    }
}
