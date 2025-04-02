using System;
using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    private Transform _uiParent;

    private void OnEnable()
    {
        _uiParent = GameObject.FindWithTag("Canvas").transform;
    }

    public static UIManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 创建 UI 对象
    /// </summary>
    public T ShowUI<T>(GameObject prefab, object param = null) where T : UIBase
    {
        return UIBase.Create<T>(prefab, _uiParent, param);
    }

    /// <summary>
    /// 关闭指定的 UI 对象，调用其 UIBase.DestroySelf() 方法来进行销毁操作。
    /// </summary>
    /// <param name="ui">需要销毁的 UI 对象</param>
    public void CloseUI(UIBase ui)
    {
        if (ui != null)
        {
            ui.DestroySelf();
        }
    }
    
    /// <summary>
    /// 延迟 n 秒后关闭指定的 UI 对象
    /// </summary>
    /// <param name="ui">需要关闭的 UI 对象</param>
    /// <param name="delaySeconds">延迟秒数</param>
    public void CloseUIAfter(UIBase ui, float delaySeconds)
    {
        if (ui != null)
        {
            DOVirtual.DelayedCall(delaySeconds, () =>
            {
                ui.DestroySelf();
            });
        }
    }
}