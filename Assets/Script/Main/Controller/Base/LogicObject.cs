using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicObject : LogicBehavior
{
    public RenderObject renderObject { get; protected set; }
    //…Ë÷√‰÷»æ∂‘œÛ
    public void SetRenderObiect(RenderObject renderObject)
    {
        this.renderObject = renderObject;
    }

}
