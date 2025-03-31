using DG.Tweening;
using UnityEngine;

public class ParabolaDrop : MonoBehaviour
{
    public float arcHeight = 2.0f; // 弧线顶点比中点高多少
    public float dropTime = 0.5f;  // 动画时长

    public void DoParabolaDrop(Vector3 startPos, Vector3 endPos)
    {
        transform.position = startPos;
        
        Vector3 midPos = (startPos + endPos) * 0.5f; 
        midPos.y += arcHeight;

        // start -> mid -> end
        Vector3[] path = new Vector3[] { startPos, midPos, endPos };
        
        transform.DOPath(path, dropTime, PathType.CatmullRom, PathMode.Full3D)
            .SetEase(Ease.OutQuad); 
    }
}