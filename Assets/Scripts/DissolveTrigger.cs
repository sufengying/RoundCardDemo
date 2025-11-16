using UnityEngine;

public class DissolveTrigger : MonoBehaviour
{
    public DissolveController dissolveController;

    void Start()
    {
        // 如果没有手动指定DissolveController，尝试自动获取
        if (dissolveController == null)
        {
            dissolveController = GetComponent<DissolveController>();
        }
    }

    // 示例：在特定条件下触发消融
    void OnTriggerEnter(Collider other)
    {
        // 当其他物体进入触发器时触发消融
        if (other.CompareTag("Player"))
        {
            dissolveController.StartDissolve();
        }
    }

    // 示例：通过UI按钮触发
    public void OnButtonClick()
    {
        dissolveController.StartDissolve();
    }
} 