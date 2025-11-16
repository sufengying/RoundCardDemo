using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PooledPrefabControlPlayable : PrefabControlPlayable
{
    private GameObject m_Instance;
    private bool m_IsFromPool;
    private ParticleSystem[] m_ParticleSystems;
    
    // 控制属性
    public bool updateParticle = true;
    public float particleSpeed = 1f;
    public bool pauseOnClipEnd = true;

    //父类由同名的ScriptPlayable方法,使用new关键字表示隐藏掉父类的方法
    public new static ScriptPlayable<PooledPrefabControlPlayable> Create(PlayableGraph graph, GameObject prefabGameObject, 
                                                                                                Transform parentTransform)
    {
        // 1. 检查预制体是否为空
        if (prefabGameObject == null)
            return ScriptPlayable<PooledPrefabControlPlayable>.Null;

        // 2. 创建 Playable
        var handle = ScriptPlayable<PooledPrefabControlPlayable>.Create(graph);
        // 3. 初始化 Playable
        // behaviour 包含：
        // - m_Instance: 存储实例化的预制体
        // - m_IsFromPool: 标记是否来自对象池
        // - Initialize 方法
        // - OnPlayableDestroy 方法
        // - 其他 PlayableBehaviour 的方法
        handle.GetBehaviour().Initialize(prefabGameObject, parentTransform);
        // 4. 返回 Playable
        return handle;
    }
    //重写Initialize方法，在PrefabControlPlayable源码修改
    public override GameObject Initialize(GameObject prefabGameObject, Transform parentTransform)
    {
        if (prefabGameObject == null)
            throw new System.ArgumentNullException("Prefab cannot be null");

        if (m_Instance != null)
        {
            Debug.LogWarningFormat("Prefab Control Playable ({0}) has already been initialized with a Prefab ({1}).", prefabGameObject.name, m_Instance.name);
            return m_Instance;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 在编辑器中预览时，直接实例化预制体
            m_Instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefabGameObject);
            if (m_Instance != null)
            {
                
                
                // 设置父物体和位置
                if (parentTransform != null)
                {
                    m_Instance.transform.SetParent(parentTransform, false);
                    
                }
                else
                {
                    // 如果没有父物体，将预制体放在场景根节点下
                    m_Instance.transform.SetParent(null);
                    
                }

                // 设置变换
                m_Instance.transform.localPosition = Vector3.zero;
                m_Instance.transform.localRotation = Quaternion.identity;
                m_Instance.transform.localScale = Vector3.one;
                
                // 获取粒子系统组件
                m_ParticleSystems = m_Instance.GetComponentsInChildren<ParticleSystem>(true);
                
                
                // 设置预览时的初始状态
                m_Instance.SetActive(false);
                

            }
        
        }
        else
#endif
        {
            // 运行时代码保持不变
            m_Instance = ObjectPool.Instance.Get(prefabGameObject);
            if (m_Instance != null)
            {
                m_IsFromPool = true;
                if (parentTransform != null)
                {
                    m_Instance.transform.SetParent(parentTransform, false);
                }
                m_Instance.transform.localPosition = Vector3.zero;
                m_Instance.transform.localRotation = Quaternion.identity;
                m_Instance.transform.localScale = Vector3.one;
                
                // 获取粒子系统组件
                m_ParticleSystems = m_Instance.GetComponentsInChildren<ParticleSystem>(true);
            }
            else
            {
                m_Instance = Object.Instantiate(prefabGameObject, parentTransform, false);
                m_IsFromPool = false;
                m_Instance.transform.localPosition = Vector3.zero;
                m_Instance.transform.localRotation = Quaternion.identity;
                m_Instance.transform.localScale = Vector3.one;
                
                // 获取粒子系统组件
                m_ParticleSystems = m_Instance.GetComponentsInChildren<ParticleSystem>(true);
            }
        }

        if (m_Instance != null)
        {
            m_Instance.name = prefabGameObject.name + " [Timeline]";
            SetHideFlagsRecursive(m_Instance);
        }

        return m_Instance;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (m_Instance != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                
            }
#endif
            m_Instance.SetActive(true);
            
            // 确保位置正确
            if (m_Instance.transform.parent == null)
            {
                
            }
            
            m_Instance.transform.localPosition = Vector3.zero;
            m_Instance.transform.localRotation = Quaternion.Euler(Vector3.zero);
            m_Instance.transform.localScale = Vector3.one;
            
            // 控制粒子系统
            if (updateParticle && m_ParticleSystems != null)
            {
                foreach (var ps in m_ParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Play();
                        var main = ps.main;
                        main.simulationSpeed = particleSpeed;
                    }
                }
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (m_Instance != null && pauseOnClipEnd)
        {

            // 停用预制体
            m_Instance.SetActive(false);
            
            // 暂停粒子系统
            if (updateParticle && m_ParticleSystems != null)
            {
                foreach (var ps in m_ParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Pause();
                        ps.Clear();
                        ps.Stop();
                    }
                }
            }

            // 重置预制体状态
            ResetPrefabState();
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (m_Instance != null)
        {
            // 停用预制体
            m_Instance.SetActive(false);
            
            // 清理粒子系统
            if (m_ParticleSystems != null)
            {
                foreach (var ps in m_ParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop();
                        ps.Clear();
                    }
                }
            }

            // 重置预制体状态
            ResetPrefabState();
            
            if (Application.isPlaying)
            {
                if (m_IsFromPool)
                {
                    ObjectPool.Instance.Return(m_Instance);
                }
                else
                {
                    Object.Destroy(m_Instance);
                }
            }
            else
            {
                // 在编辑器中预览时，使用 DestroyImmediate
                Object.DestroyImmediate(m_Instance);
            }
            m_Instance = null;
        }

    }

    private void ResetPrefabState()
    {
        if (m_Instance != null)
        {
            // 只重置主物体的位置，保持子物体的原始位置
            m_Instance.transform.localPosition = Vector3.zero;
            m_Instance.transform.localRotation = Quaternion.Euler(Vector3.zero);
            m_Instance.transform.localScale = Vector3.one;

            // 重置所有粒子系统
            if (m_ParticleSystems != null)
            {
                foreach (var ps in m_ParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop();
                        ps.Clear();
                        var main = ps.main;
                        main.simulationSpeed = 1f;
                    }
                }
            }
        }
    }

    public GameObject Initializel(GameObject prefabGameObject, Transform parentTransform)
    {
        var instance = base.Initialize(prefabGameObject, parentTransform);
        
        // 获取所有粒子系统组件
        if (instance != null)
        {
            m_ParticleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        }
        
        return instance;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (m_Instance != null && updateParticle && m_ParticleSystems != null)
        {
            // 更新粒子系统速度
            foreach (var ps in m_ParticleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    main.simulationSpeed = particleSpeed * info.effectiveSpeed;
                }
            }
        }
    }
} 