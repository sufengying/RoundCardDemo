using UnityEngine;
using System.Collections.Generic;

public class BoneRenderer : MonoBehaviour
{
    public Color boneColor = Color.green; // 骨骼线条的颜色
    public float boneSize = 0.01f; // 骨骼线条的粗细
    public bool showBones = true; // 是否显示骨骼
    public bool showNames = false; // 是否显示骨骼名称

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Transform[] bones;
    private Dictionary<Transform, LineRenderer> boneLines = new Dictionary<Transform, LineRenderer>();

    void Start()
    {
        // 获取SkinnedMeshRenderer组件
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found on this object!");
            return;
        }

        // 获取所有骨骼
        bones = skinnedMeshRenderer.bones;
        
        // 为每个骨骼创建LineRenderer
        foreach (Transform bone in bones)
        {
            if (bone != null)
            {
                CreateBoneLine(bone);
            }
        }
    }

    void CreateBoneLine(Transform bone)
    {
        // 创建LineRenderer对象
        GameObject lineObj = new GameObject("BoneLine_" + bone.name);
        lineObj.transform.SetParent(transform);
        
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = boneColor;
        line.endColor = boneColor;
        line.startWidth = boneSize;
        line.endWidth = boneSize;
        line.positionCount = 2;
        
        // 存储LineRenderer引用
        boneLines[bone] = line;

        // 如果显示骨骼名称，创建TextMesh
        if (showNames)
        {
            GameObject textObj = new GameObject("BoneName_" + bone.name);
            textObj.transform.SetParent(lineObj.transform);
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = bone.name;
            textMesh.fontSize = 20;
            textMesh.color = boneColor;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
        }
    }

    void Update()
    {
        if (!showBones || skinnedMeshRenderer == null) return;

        // 更新每个骨骼的线条位置
        foreach (Transform bone in bones)
        {
            if (bone != null && boneLines.ContainsKey(bone))
            {
                LineRenderer line = boneLines[bone];
                
                // 设置线条起点（当前骨骼位置）
                line.SetPosition(0, bone.position);
                
                // 设置线条终点（父骨骼位置，如果有的话）
                if (bone.parent != null)
                {
                    line.SetPosition(1, bone.parent.position);
                }
                else
                {
                    // 如果没有父骨骼，将终点设置为起点
                    line.SetPosition(1, bone.position);
                }

                // 更新骨骼名称位置
                if (showNames)
                {
                    Transform textTransform = line.transform.Find("BoneName_" + bone.name);
                    if (textTransform != null)
                    {
                        textTransform.position = bone.position;
                        textTransform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        // 清理所有创建的LineRenderer对象
        foreach (var line in boneLines.Values)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        boneLines.Clear();
    }
} 