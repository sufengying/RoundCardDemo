using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectParentPos : MonoBehaviour
{
    [SerializeField]
    private int effectParentId;
    [SerializeField]
    private Transform EffectParentTransform;

    void Awake()
    {
        EffectParentTransform=this.GetComponent<Transform>();
    }

    public void ChangedTransform(SkillEffectInfo skillEffectInfo)
    {
        EffectParentTransform.localPosition=skillEffectInfo.localPosition;
        EffectParentTransform.localRotation=Quaternion.Euler(skillEffectInfo.localRotation);
        EffectParentTransform.localScale=skillEffectInfo.localScale;
    }

    public int GetEffectParentId()
    {
        return effectParentId;
    }

    public void OnDestroy()
    {
        
    }

}
