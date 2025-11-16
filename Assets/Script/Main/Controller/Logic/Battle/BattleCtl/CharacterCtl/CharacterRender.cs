using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HighlightPlus;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;


namespace ZMGCFrameWork.Battle
{
    //角色渲染体，挂载在角色上，享受monobehavior的生命周期影响，根据角色逻辑体的指令来执行表现
    public class CharacterRender : RenderObject
    {
        //角色逻辑体
        public CharacterLogic characterLogic;
        //角色动画机
        public Animator m_animator;
        //角色数据
        public CharacterConfig m_characterConfig;

        #region 玩家阵营角色使用组件
        //角色状态UI(如果是玩家角色，则使用玩家状态UI（chacaterHeadUI）)
        public ChacaterHeadUI chacaterHeadUI;

        public ParticleSystem m_particleSystem;
        #endregion

        #region 敌方阵营角色使用组件
        //角色技能UI父节点(如果是敌方角色，则使用敌方技能UI父节点（enemyHUDParent）)
        public EnemyHUD enemyHUD;
        //敌方角色状态父节点
        public Transform enemyHUDParent;
        //敌方角色高亮效果
        public HighlightEffect m_highlightEffect;

        #endregion
        public Skills skills;
        //角色阵营
        public Team team;

        public void Initialize(CharacterConfig characterConfig)
        {
            m_animator = transform.GetComponent<Animator>();
            m_characterConfig = characterConfig;
            m_highlightEffect = transform.GetComponent<HighlightEffect>();
            skills = transform.GetComponentInChildren<Skills>();
            this.team = m_characterConfig.team;

            //如果阵营是敌人，则初始化敌人状态UI，而玩家角色UI初始化由ChacaterHeadUI组件进行
            if (team == Team.Enemy)
            {
                enemyHUD = ResourcesMgr.Instance.LoadObject<EnemyHUD>(TestAssetsPath.StateSlider, GameNode.Instance.mainCanvas.transform);
                enemyHUD.Initialize(this);
                
            }
        }
        //加载技能预制体
        public void SetSkills()
        {
            skills.LoadSkillInfo(m_characterConfig.skillList, m_animator);
            skills.InitializeSkillInfo();
        }

#region 状态切换
        public void OnExitBlock()
        {
            PlayHighlightEffect(false);
            AIStateCtl.Instance.ChangeState(AIStateEnum.waiting, characterLogic);
        }

        public void OnEnterHit()
        {
           
            AIStateCtl.Instance.ChangeState(AIStateEnum.hit, characterLogic);
        }

        public void OnExitHit()
        {
            AIStateCtl.Instance.ChangeState(AIStateEnum.waiting, characterLogic);
        }

        public void OnEnterAnimation(string animationName)
        {
            m_animator.SetTrigger(animationName);
        }

        //反击(进入运行状态，反击完成回到原来状态)
        public void OnEnterCounterattack(AIStateEnum oldState)
        {
            AIStateCtl.Instance.ChangeState(AIStateEnum.going, characterLogic);
            
        }
#endregion

        public void SetCharacterLogic()
        {
            characterLogic = (CharacterLogic)logicObject;
        }
#region 特效
        public void PlayParticleSystem()
        {
            m_particleSystem.Play();
        }

        public void PlayHighlightEffect(bool value)
        {
            m_highlightEffect.highlighted = value;
        }
#endregion

        #region 更新血条与显示血量数字
        private void UpdateEnemyHUDPos()
        {
            if (enemyHUD != null && enemyHUDParent != null && characterLogic != null && team == Team.Enemy)
            {
                enemyHUD.transform.localPosition = World3DToScreen2D(enemyHUDParent.position);
            }
        }
        /// <summary>
        /// 更新血条
        /// </summary>
        /// <param name="attackPreDelay">受到攻击的前摇时间</param>
        /// <param name="valueArray">血量值比例</param>
        /// <param name="skillDamageArray">技能伤害</param>
        public void UpdateHPSlider(int[] attackPreDelay, float[] valueArray, float[] skillDamageArray = null, GameObject dameagePrefab = null)
        {
            if (chacaterHeadUI != null && characterLogic != null && team == Team.Player)
            {
                chacaterHeadUI.UpdateHPSlider(attackPreDelay, valueArray, skillDamageArray, dameagePrefab);
            }
            if (enemyHUD != null && enemyHUDParent != null && characterLogic != null && team == Team.Enemy)
            {
                enemyHUD.UpdateHPSlider(attackPreDelay, valueArray, skillDamageArray, dameagePrefab);
            }
        }
        /// <summary>
        /// 更新血量数字，通过更新血条方法来方向调用更新血量的方法
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="dameagePrefab"></param>
        public void UpdateDamageNumber(float damage, GameObject dameagePrefab)
        {
            characterLogic.RealCalculateHP(damage);

            GameObject damageNumber = ObjectPool.Instance.Get(dameagePrefab, 6, 10, GameNode.Instance.mainCanvas.transform);

            //damageNumber.transform.SetParent(GameNode.Instance.mainCanvas.transform);
            Vector2 pos = World3DToScreen2D(transform.position);
            if(team==Team.Player)
            {
                damageNumber.transform.localPosition = new Vector2(pos.x+150, pos.y + 600);
            }
            else if(team==Team.Enemy)
            {
                damageNumber.transform.localPosition = new Vector2(pos.x, pos.y + 100);
            }
            damageNumber.GetComponent<Text>().text = (damage > 0 ? "-" : "+") + Mathf.Abs(damage);

            damageNumber.transform.DOLocalMoveY(damageNumber.transform.localPosition.y + 100, 1f);

            if (!damageNumber.TryGetComponent<CanvasGroup>(out var canvasGroup))
                canvasGroup = damageNumber.AddComponent<CanvasGroup>();
            damageNumber.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetDelay(1.2f);


            ObjectPool.Instance.Return(damageNumber, 2f);

        }
        #endregion
        private Vector3 World3DToScreen2D(Vector3 taregtPos)
        {
            Vector2 uguiPos;
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(GameNode.Instance.mainCamera, taregtPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GameNode.Instance.mainCanvas.transform as RectTransform, screenPos, GameNode.Instance.uiCamera, out uguiPos);
            uguiPos.x -= 100;
            return uguiPos;
        }
        public override void Update()
        {
            base.Update();
            UpdateEnemyHUDPos();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"{characterLogic.name} 正在{characterLogic.aiState.GetState()}状态");
                Debug.Log($"{characterLogic.name} 当前血量{characterLogic.currentHp}");
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            if(Input.GetKeyDown(KeyCode.S))
            {
                m_animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }
        public void OnDestroy()
        {
            // 清理逻辑对象
            if (characterLogic != null)
            {
                characterLogic = null;
            }
            if (logicObject != null)
            {
                logicObject = null;
            }

            // 清理配置数据
            if (m_characterConfig != null)
            {
                m_characterConfig = null;
            }

            // 清理组件引用
            if (m_animator != null)
            {
                m_animator = null;
            }
            if (m_highlightEffect != null)
            {
                m_highlightEffect = null;
            }
            if (skills != null)
            {
                skills = null;
            }

            // 清理 UI 组件
            if (chacaterHeadUI != null)
            {
                chacaterHeadUI.OnDestroy();
                chacaterHeadUI = null;
            }
            if (enemyHUD != null)
            {
                GameObject.Destroy(enemyHUD.gameObject);
                enemyHUD.OnDestroy();
              
                enemyHUD = null;
            }
            if (enemyHUDParent != null)
            {
                enemyHUDParent = null;
            }
        }
    }
}