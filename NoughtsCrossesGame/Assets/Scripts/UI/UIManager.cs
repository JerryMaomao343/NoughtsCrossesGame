using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
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

    [Header("UI Parent")]
    // UI生成时的父对象，例如Canvas
    public Transform uiParent;

    /// <summary>
    /// 实例化指定的UI prefab，并传递参数给其实现 IUIHandler 的脚本
    /// </summary>
    /// <typeparam name="T">UI面板对应的脚本类型，该脚本应实现 IUIHandler 接口</typeparam>
    /// <param name="prefab">要生成的UI prefab</param>
    /// <param name="param">初始化参数，可装箱传递</param>
    /// <returns>返回实例化后的UI脚本组件</returns>
    public T ShowUI<T>(GameObject prefab, object param = null) where T : MonoBehaviour
    {
        GameObject uiObj = Instantiate(prefab, uiParent);
        T uiScript = uiObj.GetComponent<T>();
        if (uiScript != null)
        {
            IUIHandler handler = uiScript as IUIHandler;
            if (handler != null)
            {
                handler.Init(param);
            }
        }
        return uiScript;
    }

    /// <summary>
    /// 销毁指定的UI对象
    /// </summary>
    /// <param name="uiObj">要关闭的UI对象</param>
    public void CloseUI(GameObject uiObj)
    {
        Destroy(uiObj);
    }
}

