using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : UIBase
{
    private Image _Image;
    private Button EnterGame;

    private void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        _Image=transform.Find("Image").gameObject.GetComponent<Image>();

        EnterGame=transform.Find("EnterGame")?.gameObject.GetComponent<Button>();

        if (EnterGame != null)
        {
           
            EnterGame.onClick.AddListener(TriggerEvent);
        }
    }

    private void TriggerEvent()
    {
        EventCenter.Instance.TriggerAction("EnterBattleWorld");
        
    }

    public override void ShowUI()
    {
        gameObject.SetActive(true);
    }
    public override void HideUI()
    {
        gameObject.SetActive(false);
    }

}
