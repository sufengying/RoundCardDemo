using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNode : SingletionMono<GameNode>
{
    
    public  Camera mainCamera;
    public  Camera uiCamera; 

    public  Canvas mainCanvas;

    public Transform fatherTrans_Effect;

    public Transform UIRoot;

}
