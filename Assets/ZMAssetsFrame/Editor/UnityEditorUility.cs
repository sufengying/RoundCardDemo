
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEditorUility 
{




    public static GUIStyle GetGUIStyle(string styleName)
    {
        GUIStyle gUIStyle = null;
        foreach (var item in GUI.skin.customStyles)
        {
            if (string.Equals(item.name.ToLower(), styleName.ToLower()))
            {
                gUIStyle = item;
                break;
            }
        }
        return gUIStyle;
    }

}
