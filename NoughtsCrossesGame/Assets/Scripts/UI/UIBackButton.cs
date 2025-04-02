using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackButton : MonoBehaviour
{
    public Button btn;

    private void Start()
    {
        btn.onClick.AddListener(Trigger);
    }

    private void Trigger()
    {
        EventCenter.Instance.Broadcast(GameEvent.ReturnToMainMenu);
    }
}
