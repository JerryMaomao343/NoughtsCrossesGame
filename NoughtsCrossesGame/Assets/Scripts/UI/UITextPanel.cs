using UnityEngine;
using UnityEngine.UI;

public class UITextPanel : MonoBehaviour, IUIHandler
{
    public Text messageText;

    // 通过 Init 接收初始化参数，例如一个字符串
    public void Init(object param)
    {
        if (param is string)
        {
            messageText.text = (string)param;
        }
    }

    // 你可以增加关闭按钮的事件，调用 UIManager.Instance.CloseUI(gameObject);
}