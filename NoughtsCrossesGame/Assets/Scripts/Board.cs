using System;
using UnityEngine;

public class Board : MonoBehaviour
{
    // 改成把整格子对象传出去
    public event Action<GridCell> OnCellClicked;

    [Header("Raycast Settings")]
    public LayerMask boardLayer;       
    public float raycastDistance = 100f;

    [Header("Highlight Settings")]
    public Material highlightMaterial;
    private Material originalMaterial;
    private Transform lastHighlighted;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 先恢复上一个格子的材质
        if (lastHighlighted != null)
        {
            RestoreOriginalMaterial(lastHighlighted.gameObject);
            lastHighlighted = null;
        }

        // 发射射线
        if (Physics.Raycast(ray, out hit, raycastDistance, boardLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            // 高亮
            HighlightCell(hitObj);
            lastHighlighted = hitObj.transform;

            // 点击
            if (Input.GetMouseButtonDown(0))
            {
                GridCell cell = hitObj.GetComponent<GridCell>();
                if (cell != null)
                {
                    OnCellClicked?.Invoke(cell);
                }
            }
        }
    }

    private void HighlightCell(GameObject cellObj)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null && highlightMaterial != null)
        {
            // 保存原材质，用于离开时恢复
            originalMaterial = rend.material;
            rend.material = highlightMaterial;
        }
    }

    private void RestoreOriginalMaterial(GameObject cellObj)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null && originalMaterial != null)
        {
            rend.material = originalMaterial;
        }
    }
}