using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAssetsPath 
{
    public static string text="Text/";
    public static string ui_text = text + "UI/";
    public static string ui_text_GameTitle = ui_text + "GameTitle/GameTitle";
    public static string MainGameInterface = "Text/UI/MainGameInterface/MainGameInterface";

    public static string playerFrefabs = "prefabs/player/";
    public static string enemyFrefabs = "prefabs/enemy/";
    public static string NPCFrefabs = "prefabs/NPC/";

    public static string[] AllUiPath = { "Text/UI/GameTitle/GameTitle",
                                        "Text/UI/MainGameInterface/MainGameInterface "};

    public static string battleWorld = "World/BattleWorld/";

    public static string battleUI = "Text/UI/BattleWorld/BattleUIWindow";
    public static string playerBattleUI = "Text/UI/BattleWorld/CharacterInfo";

    public static string chacaterHeadUI = "Text/UI/BattleWorld/Chacater";

    public static string StateSlider="Text/UI/BattleWorld/StateSlider";

#region ¼¼ÄÜÂ·¾¶

    public static string SkillPerfab="prefabs/SkillAnim/SkillPrefab/";
    public static string SkillConfig="prefabs/SkillAnim/SkillConfig/";

#endregion

    public static string playerConfig="prefabs/CharacterConfig/Player/";
    public static string enemyConfig="prefabs/CharacterConfig/Enemy/";
}
