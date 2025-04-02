using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextPanel : UIBase
{
    public TMP_Text messageText;
    private void Start()
    {
        messageText.color = new Color(1,1,1,0);
        GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public override void Init(object param)
    {
        messageText.DOFade(1, 0.3f);
        GetComponent<RectTransform>().DOScale(1, 0.3f);
        if (param is string)
        {
            messageText.text = (string)param;
        }
    }

    public override void DestroySelf()
    {
        messageText.DOFade(0, 0.2f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}