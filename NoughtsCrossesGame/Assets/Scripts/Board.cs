using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // 事件：当某个格子被点击时，传递该格子给订阅者（GameManager）
    public event Action<GridCell> OnCellClicked;

    [Header("References")]
    public List<GridCell> allCells = new List<GridCell>();  // 9个格子
    public LayerMask boardLayer;       // 只检测格子所在的 Layer
    public float raycastDistance = 100f;

    [Header("Highlight Materials")]
    public Material highlightEmpty;    // 当格子 occupant == None 时，用的绿色高亮材质
    public Material highlightOccupied; // 当格子 occupant != None 时，用的黄色高亮材质

    // 如果你想在恢复原材质时，让每个格子都回到各自独特的材质，最好把“原材质”存进 GridCell。
    // 但此示例只记一个上次高亮的材质即可（简单做法）
    private Material lastOriginalMat;
    private GameObject lastHighlightedObj = null;

    private void Awake()
    {
        // 如果你没在 Inspector 手动拖拽，也可用这行自动收集
        if (allCells.Count == 0)
            allCells.AddRange(GetComponentsInChildren<GridCell>());
    }

    private void Update()
    {
        // 1. 恢复上一次高亮的格子
        if (lastHighlightedObj != null)
        {
            RestoreOriginalMaterial(lastHighlightedObj);
            lastHighlightedObj = null;
        }

        // 2. 发射射线检测鼠标悬浮
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, boardLayer))
        {
            GameObject hitObj = hit.collider.gameObject;
            GridCell cell = hitObj.GetComponent<GridCell>();
            if (cell != null)
            {
                // 悬浮到某个格子 -> 切换材质(根据occupant区分)
                HighlightCell(hitObj, cell);
                lastHighlightedObj = hitObj;

                // 3. 如果点击鼠标左键，就发事件给 GameManager
                if (Input.GetMouseButtonDown(0))
                {
                    OnCellClicked?.Invoke(cell);
                }
            }
        }
    }

    /// <summary>
    /// 根据格子的占用状态，替换成不同的高亮材质：
    /// occupant == None → highlightEmpty(绿色)
    /// occupant != None → highlightOccupied(黄色)
    /// </summary>
    private void HighlightCell(GameObject cellObj, GridCell cell)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null)
        {
            // 记录下原材质，用于恢复
            lastOriginalMat = rend.material;
            
            // 如果格子无占用，就用绿色材质，否则用黄色材质
            if (!cell.IsOccupied && highlightEmpty != null)
            {
                rend.material = highlightEmpty;
            }
            else if (cell.IsOccupied && highlightOccupied != null)
            {
                rend.material = highlightOccupied;
            }
        }
    }

    private void RestoreOriginalMaterial(GameObject cellObj)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null && lastOriginalMat != null)
        {
            rend.material = lastOriginalMat;
        }
    }
}
