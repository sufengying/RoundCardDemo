using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BattleWorldConfig", menuName = "BattleWorldConfig")]
public class BattleWorldConfig : ScriptableObject
{
    [LabelText("世界预设")]
    public GameObject worldPrefab;

    [LabelText("玩家配置")]
    [ListDrawerSettings(DraggableItems = false, HideAddButton = true)]
    public CharacterConfig[] playerConfigList=new CharacterConfig[3];

    [LabelText("敌人配置")]
    [ListDrawerSettings(DraggableItems = false, HideAddButton = true)]
    public List<CharacterConfig> enemyConfigList=new List<CharacterConfig>(3);


  

}
