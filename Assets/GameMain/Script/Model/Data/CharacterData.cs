using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterData 
{
    //角色头像
    public Sprite characterIcon;
    //终极技能图标
    public Sprite UlitmateIcon;
    //名字很重要，加载预制体根据名字来加载
    //角色名字
    public string name;
    //角色速度
    public int speed;
    //角色最大血量
    public float maxHp;
    //角色攻击力
    public float atk;
    //角色物理防御
    public float def;
    //角色魔法防御
    public float res;
    //角色初始技能点
    public int firstSkillPoints;
    //角色技能点最大值
    public int maxSkillPoints;
    //角色在地图上的位置,此属性应该在战斗配置界面配置，这个只是测试，角色数据一般不会有位置信息，位置信息一般是玩家自主设置
    public int posID;

    //角色技能列表
    public List<SkillConfig> skillList;

}
