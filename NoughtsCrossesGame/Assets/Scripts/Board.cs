using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public event Action<GridCell> OnCellClicked;
    public List<GridCell> allCells = new List<GridCell>();  // 9个格子
    public LayerMask boardLayer;       // 只检测格子所在的 Layer
    public float raycastDistance = 100f;
    public Material highlightEmpty;  
    public Material highlightOccupied;

    Material lastOriginalMat;
    // 用于记录上一次被高亮的格子对象
    private GameObject lastHighlightedObj = null;

    private bool _isPaused;

    void Awake()
    {
        if (allCells.Count == 0)
            allCells.AddRange(GetComponentsInChildren<GridCell>());
    }

    void Update()
    {
        Debug.Log(GameManager.Instance.isOnGame);
        if (GameManager.Instance.isOnGame && Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused)
            {
                EventCenter.Instance.Broadcast(GameEvent.EnterPause);
                _isPaused = true;
            }
            else
            {
                EventCenter.Instance.Broadcast(GameEvent.ExitPause);
                _isPaused = false;
            }
        }

        if (!GameManager.Instance.isPlayerTurn && lastHighlightedObj != null)
        {
            RestoreOriginalMaterial(lastHighlightedObj);
            return;
        }

        if (lastHighlightedObj != null)
        {
            RestoreOriginalMaterial(lastHighlightedObj);
            //lastHighlightedObj = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, boardLayer))
        {
            GameObject hitObj = hit.collider.gameObject;
            GridCell cell = hitObj.GetComponent<GridCell>();
            if (cell != null)
            {
                HighlightCell(hitObj, cell);
                // 更新记录，只在当前高亮对象改变时触发事件
                lastHighlightedObj = hitObj;

                if (Input.GetMouseButtonDown(0))
                {
                    EventCenter.Instance.Broadcast(GameEvent.ClickBoard);
                    OnCellClicked?.Invoke(cell);
                }
            }
        }
    }

    void HighlightCell(GameObject cellObj, GridCell cell)
    {
        // 如果上一次记录的对象为空，或者不同于当前对象，则触发选中事件
        if (lastHighlightedObj == null || lastHighlightedObj != cellObj)
        {
            Debug.Log("select");
            EventCenter.Instance.Broadcast(GameEvent.SelectBoard);
        }
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null)
        {
            // 记录原材质以便恢复
            lastOriginalMat = rend.material;
            
            // 根据格子的占用情况设置高亮材质
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

    void RestoreOriginalMaterial(GameObject cellObj)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null && lastOriginalMat != null)
        {
            rend.material = lastOriginalMat;
        }
    }
}
