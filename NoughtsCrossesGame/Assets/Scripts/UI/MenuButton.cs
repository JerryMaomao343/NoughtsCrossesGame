using UnityEngine;
using DG.Tweening;

public class MenuButton : MonoBehaviour
{
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;
    public float clickScale = 0.9f;
    public float clickDuration = 0.1f;

    private Vector3 originalScale;
    
    public ButtonType buttonType; 

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        transform.DOScale( originalScale*hoverScale, hoverDuration);
        EventCenter.Instance.Broadcast(GameEvent.OnButtonSelect);
    }

    private void OnMouseExit()
    {
        transform.DOScale(originalScale, hoverDuration);
    }

    private void OnMouseDown()
    {
        EventCenter.Instance.Broadcast(GameEvent.OnButtonClick);
        transform.DOScale(originalScale * clickScale, clickDuration).OnComplete(() =>
        {
            transform.DOScale(originalScale *hoverScale, clickDuration);

            if (buttonType == ButtonType.Start)
            {
                StartMenuManager.Instance.StartGame();
            }
            else if (buttonType == ButtonType.Exit)
            {
                StartMenuManager.Instance.ExitGame();
            }
        });
    }
}
