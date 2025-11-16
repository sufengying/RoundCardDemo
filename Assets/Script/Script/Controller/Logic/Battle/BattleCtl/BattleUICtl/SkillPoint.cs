using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class SkillPoint : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
        }

        public void SetActive(bool active)
        {
            canvasGroup.alpha = active ? 1 : 0;
        }

        public void OnDestroy()
        {
            canvasGroup = null;
        }
    }
}
