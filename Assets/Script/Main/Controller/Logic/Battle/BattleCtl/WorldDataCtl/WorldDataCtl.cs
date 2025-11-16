using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class WorldDataCtl : IDataBehaviour
    {
        public BattleWorldConfig battleWorldConfig { get; private set; }
        // 场景资源
        public GameObject worldPrefab { get; private set; }

        public Transform[] playerPositions { get; private set; }
        public Transform[] enemyPositions { get; private set; }
        public Transform[] NPCPosArray { get; private set; }

        public Transform mapMid { get; private set; }
        public Transform playerMid { get; private set; }
        public Transform enemyMid { get; private set; }

        public List<CharacterConfig> playerConfigList { get; private set; }
        public List<CharacterConfig> enemyConfigList { get; private set; }

        private Func<EffectRendered, GameObject> getSkillParent;
        private Func<List<CinemachineVirtualCamera>> getCinemachineCameraList;
        private Func<string,Transform[]> getCharacterPositions;
        private Func<string,List<CharacterConfig>> getCharacterConfigList;
        public void OnCreate()
        {
            getSkillParent = GetSkillParent;
            getCinemachineCameraList = GetCinemachineCameraList;
            getCharacterPositions = GetCharacterPositions;
            getCharacterConfigList = GetCharacterConfigList;
            RegisterEvent();

            //worldPrefab = ResourcesMgr.Instance.LoadGameObject(TestAssetsPath.battleWorld + "battleWorld01");
            battleWorldConfig=Resources.Load<BattleWorldConfig>("World/BattleWorld/BattleWorldConfig");
            worldPrefab=GameObject.Instantiate(battleWorldConfig.worldPrefab);
            if (worldPrefab == null)
            {
                Debug.LogError("Failed to load battle world prefab");
            }
            CharacterPos characterPos = worldPrefab.GetComponentInChildren<CharacterPos>();
            if (characterPos == null)
            {
                Debug.LogError("Failed to get character pos");
            }
            playerPositions = characterPos.playerPosArray;
            enemyPositions = characterPos.enemyPosArray;
            NPCPosArray = characterPos.NPCPosArray;
            mapMid = characterPos.mapMid;
            playerMid = characterPos.playerMid;
            enemyMid = characterPos.enemyMid;

            playerConfigList=EventCenter.Instance.TriggerAction<string,List<CharacterConfig>>("GameMain_GetCharacterConfigList","player");
            enemyConfigList=EventCenter.Instance.TriggerAction<string,List<CharacterConfig>>("GameMain_GetCharacterConfigList","enemy");
        }
        //注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("WorldDataCtl_GetSkillParent", getSkillParent);
            EventCenter.Instance.AddListener("WorldDataCtl_GetCinemachineCameraList", getCinemachineCameraList);
            EventCenter.Instance.AddListener("WorldDataCtl_GetCharacterPositions", getCharacterPositions);
            EventCenter.Instance.AddListener("WorldDataCtl_GetCharacterConfigList", getCharacterConfigList);
        }
        //取消事件
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("WorldDataCtl_GetSkillParent");
            EventCenter.Instance.RemoveListener("WorldDataCtl_GetCinemachineCameraList");
            EventCenter.Instance.RemoveListener("WorldDataCtl_GetCharacterPositions");
            EventCenter.Instance.RemoveListener("WorldDataCtl_GetCharacterConfigList");
        }
#region 需要注册的事件
        private GameObject GetSkillParent(EffectRendered skillRenderedTarget)
        {
            switch (skillRenderedTarget)
            {
                case EffectRendered.mapMid:
                    return mapMid.gameObject;
                case EffectRendered.playerMid:
                    return playerMid.gameObject;
                case EffectRendered.enemyMid:
                    return enemyMid.gameObject;
                default:
                    return null;
            }       
        }
        private Transform[] GetCharacterPositions(string characterTypeName)
        {
            switch (characterTypeName)
            {
                case "player":
                    return playerPositions;
                case "enemy":
                    return enemyPositions;
                case "NPC":
                    return NPCPosArray;  
            }
            Debug.LogError("GetCharacterPositions: 无效的角色类型");
            return null;
        }
        private List<CharacterConfig> GetCharacterConfigList(string characterTypeName)
        {
            switch (characterTypeName)
            {
                case "player":
                    return playerConfigList;
                case "enemy":
                    return enemyConfigList;
            }
            Debug.LogError("GetCharacterConfigList: 无效的角色类型");
            return null;
        }
        private List<CinemachineVirtualCamera> GetCinemachineCameraList()
        {
            var cinemachineCameraList = worldPrefab.GetComponentInChildren<CinemachineCameraList>();
            if (cinemachineCameraList == null)
            {
                Debug.LogError("虚拟相机列表为空");
            }
            return cinemachineCameraList.cinemachineVirtualCameraList;
        }
#endregion
        public void OnDestroy()
        {
            UnregisterEvent();
            if (worldPrefab != null)
            {
                GameObject.Destroy(worldPrefab);
                worldPrefab = null;
            }
            if (playerPositions != null)
            {
                playerPositions = null;
            }
            enemyPositions = null;
            NPCPosArray = null;
            mapMid = null;
            playerMid = null;
            enemyMid = null;
        }


    }
}
