using UnityEngine;

public abstract class UIBase : MonoBehaviour, IUIHandler
{
    /// <summary>
    /// 接收初始化参数
    /// </summary>
    /// <param name="param">初始化参数</param>
    public virtual void Init(object param)
    {
        
    }
    

    public static T Create<T>(GameObject prefab, Transform parent, object param = null) where T : UIBase
    {
        GameObject uiObj = Instantiate(prefab, parent);
        T uiComponent = uiObj.GetComponent<T>();
        if (uiComponent != null)
        {
            uiComponent.Init(param);
        }
        return uiComponent;
    }
    
    public virtual void DestroySelf()
    {
        Destroy(gameObject);
    }
}