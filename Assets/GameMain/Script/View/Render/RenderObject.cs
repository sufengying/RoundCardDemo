using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderObject : MonoBehaviour
{
    public LogicObject logicObject { get; protected set; }

    //设置对应的逻辑对象
    public void  SetLogic0bject(LogicObject logicObject)
    {
        this.logicObject=logicObject;
    }

    public virtual void Update()
    {
        
    }

}
