using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPos :MonoBehaviour
{
    public Transform[] playerPosArray;
    public Transform[] enemyPosArray;
    public Transform[] NPCPosArray;

    public Transform mapMid;
    public Transform playerMid;
    public Transform enemyMid;


    public GameObject getSkillParent(EffectRendered skillTarget)
    {
        
        switch(skillTarget)
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

    public void OnDestroy()
    {
  

        // 清理数组引用
        if (playerPosArray != null)
        {
            for (int i = 0; i < playerPosArray.Length; i++)
            {
                playerPosArray[i] = null;
            }
            playerPosArray = null;
        }

        if (enemyPosArray != null)
        {
            for (int i = 0; i < enemyPosArray.Length; i++)
            {
                enemyPosArray[i] = null;
            }
            enemyPosArray = null;
        }

        if (NPCPosArray != null)
        {
            for (int i = 0; i < NPCPosArray.Length; i++)
            {
                NPCPosArray[i] = null;
            }
            NPCPosArray = null;
        }

        // 清理其他 Transform 引用
        mapMid = null;
        playerMid = null;
        enemyMid = null;
    }

}
