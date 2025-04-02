using UnityEngine;
using DG.Tweening;

public class DifficultStar : MonoBehaviour
{
    [Tooltip("星星编号，从1开始（从左到右）")]
    public int starIndex = 1; 
    public Material filledMaterial; // 填满时使用的材质
    public Material emptyMaterial;  // 空白时使用的材质

    public float hoverScale = 1.2f;
    public float hoverDuration = 0.2f;
    public float clickScale = 0.9f;
    public float clickDuration = 0.1f;

    private Vector3 originalScale;
    private Renderer rend;

    private void Start()
    {
        originalScale = transform.localScale;
        rend = GetComponent<Renderer>();
        SetFilled(false); // 初始状态为空白
    }

    private void OnMouseEnter()
    {
        transform.DOScale(originalScale*hoverScale, hoverDuration);
        EventCenter.Instance.Broadcast(GameEvent.OnButtonSelect);
        // 通知菜单管理器：当前悬浮的星星编号
        if (StartMenuManager.Instance != null)
        {
            StartMenuManager.Instance.UpdateStarSelection(starIndex);
            StartMenuManager.Instance.SetDifficulty(starIndex);
        }
    }

    private void OnMouseExit()
    {
        transform.DOScale(originalScale, hoverDuration);
    }

    /// <summary>
    /// 设置当前星星的显示状态：填满或空白
    /// </summary>
    public void SetFilled(bool filled)
    {
        if (rend != null)
        {
            rend.material = filled ? filledMaterial : emptyMaterial;
        }
    }
}

