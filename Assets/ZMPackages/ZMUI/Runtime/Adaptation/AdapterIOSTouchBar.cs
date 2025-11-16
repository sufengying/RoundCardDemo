using UnityEngine;

namespace ZM.UI
{
    public class AdapterIOSTouchBar : MonoBehaviour
    {
        /// <summary>
        /// 偏移量
        /// </summary>
        public float offsetValue = 0.01f;

        private void Start()
        {
#if UNITY_IOS
            GeneratorAdaptation();
#endif
        }


        public void GeneratorAdaptation()
        {
            RectTransform rectTrans = transform.GetComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(rectTrans.anchorMin.x,offsetValue);
        }
    }
}
