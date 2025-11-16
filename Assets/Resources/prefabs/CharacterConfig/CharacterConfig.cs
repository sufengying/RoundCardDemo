using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "CharacterConfig", order = 0)]
public class CharacterConfig : ScriptableObject
{
    [HideInInspector]
    public bool hideIcon = false;

    [LabelText("角色阵营"), OnValueChanged("OnChangedTeam")]
    public Team team;

    [LabelText("角色头像"), LabelWidth(0.1f), PreviewField(70, ObjectFieldAlignment.Left), SuffixLabel("角色头像")]
    public Sprite characterIcon;
    [LabelText("角色头像"), LabelWidth(0.1f), PreviewField(70, ObjectFieldAlignment.Left), SuffixLabel("角色头像")]
    public Sprite characterPainting;

    [LabelText("终极技能图标"), HideIf("hideIcon"), LabelWidth(0.1f), PreviewField(70, ObjectFieldAlignment.Left), SuffixLabel("终极技能图标")]
    public Sprite UlitmateIcon;

    [LabelText("角色预制体")]
    public GameObject characterPrefab;

    [LabelText("角色名字")]
    public string characterName;

    [LabelText("角色最大血量"), MinValue(0)]
    public float maxHp;

    [LabelText("角色攻击力"), MinValue(0)]
    public float atk;

    [LabelText("角色物理防御"), MinValue(0)]
    public float def;

    [LabelText("角色魔法防御"), MinValue(0)]
    public float res;
    [LabelText("角色速度"), MinValue(0)]
    public int speed;

    [LabelText("角色初始技能点"), MinValue(0)]
    public int firstSkillPoints;
    //角色技能点最大值
    [LabelText("角色技能点最大值"), MinValue(4)]
    public int maxSkillPoints;
    [LabelText("角色在地图上的位置")]
    public MapPosID posID;

    //角色技能列表
    [LabelText("角色技能列表")]
    public List<SkillConfig> skillList;


    private void OnChangedTeam(Team team)
    {
        switch (team)
        {
            case Team.Player:
                hideIcon = false;
                break;
            case Team.Enemy:
                UlitmateIcon = null;
                hideIcon = true;
                break;
            case Team.NPC:
                break;
        }
    }

}

public enum MapPosID
{
    [LabelText("1号位置")]
    Position1 = 0,
    [LabelText("2号位置")]
    Position2 = 1,
    [LabelText("3号位置")]
    Position3 = 2,


}

public enum Team
{
    [LabelText("玩家")]
    Player,
    [LabelText("敌人")]
    Enemy,
    [LabelText("NPC")]
    NPC,
}