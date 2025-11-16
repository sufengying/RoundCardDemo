using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZMGCFrameWork.Battle
{
    public class BlockSkill : MonoBehaviour
    {

        public Button button;
        private CharacterLogic currentCharacterLogic;
        public CircularProgressController circularProgressController;



        public void Initialize(CharacterLogic characterLogic)
        {
            currentCharacterLogic = characterLogic;
            AddListener();
        }

        public void CompleteInDuration(float duration)
        {
            circularProgressController.CompleteInDuration(duration);
        }

        public void SetCurrentCharacter(CharacterLogic characterLogic)
        {
            currentCharacterLogic = characterLogic;
        }

        public void AddListener()
        {
            button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            currentCharacterLogic.Counterattack();

        }

        public void OnDestroy()
        {
            // 移除按钮点击事件监听
            if (button != null)
            {
                button.onClick.RemoveListener(Trigger);
                button = null;
            }

            // 清理角色引用
            if (currentCharacterLogic != null)
            {
                currentCharacterLogic = null;
            }
        }
    }
}
