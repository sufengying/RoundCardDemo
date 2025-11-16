using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//继承自 ControlPlayableAsset，这是 Unity Timeline 中用于控制预制体的基类
//用于在 Timeline 中控制预制体的实例化继承自 ControlPlayableAsset，这是 Unity Timeline 中用于控制预制体的基类
//用于在 Timeline 中控制预制体的实例化
public class PooledControlPlayableAsset : ControlPlayableAsset
{
    //PlayableAsset:存储相关数据
    //PlayableBehavior: 控制Playable的行为

    //当 PlayableDirector 组件被创建或启用时,创建 PlayableGraph

    //graph: PlayableGraph 对象，用于创建和管理 Playable
    //go: 游戏对象，通常是 Timeline 所在的物体
    
    [SerializeField] public float particleSpeed = 1f;    // 粒子系统播放速度
    [SerializeField] public bool pauseOnClipEnd = true;  // 片段结束时是否暂停粒子系统
    [SerializeField] public NeedValueOnEffectInfo needValueOnEffectInfo;
    [SerializeField] public  int effectParentId;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        //prefabGameObject: 将要实例化的预制体
        if (prefabGameObject != null)
        {
            // 获取父物体的 Transform
            // sourceGameObject 是 ControlPlayableAsset 中定义的 ExposedReference<GameObject>
            // Resolve 方法用于解析引用，获取实际的 GameObject
            // ?. 是空条件运算符，如果 sourceGameObject 为空则返回 null
            //sourceGameObject: 场景上的父物体
            Transform parentTransform = sourceGameObject.Resolve(graph.GetResolver())?.transform;

            // 创建 PooledPrefabControlPlayable
            // 使用我们自定义的 PooledPrefabControlPlayable 而不是默认的 PrefabControlPlayable
            var controlPlayable = PooledPrefabControlPlayable.Create(graph, prefabGameObject, parentTransform);

            // 设置控制属性
            var behaviour = controlPlayable.GetBehaviour();
            behaviour.updateParticle = updateParticle;
            behaviour.particleSpeed = particleSpeed;
            behaviour.pauseOnClipEnd = pauseOnClipEnd;

            //返回创建的 Playable
            return controlPlayable;
        }
        //如果预制体为空。调用基类方法
        return base.CreatePlayable(graph, go);
    }

    //首先，PooledControlPlayableAsset 继承自 ControlPlayableAsset，它是一个 Timeline 中的 Playable Asset，用于控制预制体的实例化。
    //当 Timeline 播放时，会调用 PooledControlPlayableAsset 的 CreatePlayable 方法，该方法会：
    //获取父物体的 Transform
    //创建 PooledPrefabControlPlayable 实例
    //返回这个 Playable
    //这个 Playable 会被 Timeline 系统使用，具体来说：
    //它会被添加到 Timeline 的 PlayableGraph 中
    //通过 PooledControlTrack 轨道进行管理
    //在 Timeline 播放时，会控制预制体的实例化和销毁
    //在运行时：
    //当 Timeline 播放到对应片段时，会激活这个 Playable
    //Playable 会从对象池中获取预制体实例
    //当 Timeline 片段结束时，会销毁或回收预制体实例
    //在编辑器中：
    //通过 ControlPlayableAssetEditor 提供编辑器界面
    //可以设置预制体、父物体等参数
    //可以预览和控制预制体的行为
} 