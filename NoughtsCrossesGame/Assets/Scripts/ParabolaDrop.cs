using DG.Tweening;
using UnityEngine;

public class ParabolaDrop : MonoBehaviour
{
    public float arcHeight = 2.0f; // 弧线顶点比中点高多少
    public float dropTime = 0.5f;  // 动画时长

    public ItemType itemType;
    
    public void DoParabolaDrop(Vector3 startPos, Vector3 endPos)
    {
        transform.position = startPos;
        
        Vector3 midPos = (startPos + endPos) * 0.5f; 
        midPos.y += arcHeight;
        
        Vector3[] path = new Vector3[] { startPos, midPos, endPos };

        switch (itemType)
        {
            case ItemType.Piece:
                EventCenter.Instance.Broadcast(GameEvent.OnPieceLift);
                break;
            case ItemType.Coin:
                EventCenter.Instance.Broadcast(GameEvent.OnCoinLift);
                break;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOPath(path, dropTime, PathType.CatmullRom, PathMode.Full3D)
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                switch (itemType)
                {
                    case ItemType.Piece:
                        EventCenter.Instance.Broadcast(GameEvent.OnPieceHit);
                        break;
                    case ItemType.Coin:
                        EventCenter.Instance.Broadcast(GameEvent.OnCoinHit);
                        break;
                }
            })
        );
        if (itemType==ItemType.Coin)
        {
            sequence.Join(transform.DORotate(new Vector3(0, 0, 360), dropTime, RotateMode.FastBeyond360));

        }
    }
}