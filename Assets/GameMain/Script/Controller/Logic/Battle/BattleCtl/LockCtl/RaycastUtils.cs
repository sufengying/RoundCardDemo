using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastUtils : SingletionMono<RaycastUtils>
{
    public bool RaycastFromMouse(out RaycastHit hitInfo, float maxDistance = Mathf.Infinity)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitInfo, maxDistance);
    }

    // 射线检测，传入层掩码(忽略某些层),其他层选中，返回是否命中物体
    public bool RaycastFromMouseIgnore(out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, string[] ignorelayerNames = null)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~0; // 默认所有层

        if(ignorelayerNames != null && ignorelayerNames.Length > 0)
        {
            int ignoreLayerMask = 0;
            foreach(string layerName in ignorelayerNames)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if(layer != -1)
                {
                    ignoreLayerMask |= (1 << layer);
                }
            }
            layerMask &= ~ignoreLayerMask; // 取反，排除这些层
        }
        
        return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
    }

    // 射线检测，传入层掩码(指定某些层),其他层忽略，返回是否命中物体
    public bool RaycastFromMouseSelect(out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, string[] SelectlayerNames = null)
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 0; // 初始化为0

        if(SelectlayerNames != null && SelectlayerNames.Length > 0)
        {
            foreach(string layerName in SelectlayerNames)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if(layer != -1) // 检查层名是否有效
                {
                    layerMask |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"RaycastUtils: Invalid layer name: {layerName}");
                }
            }
        }
        
        return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
    }

    public void DrawDebugRay(float maxDistance = Mathf.Infinity)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 1f);
    }
}
