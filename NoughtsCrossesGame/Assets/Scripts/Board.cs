using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public event Action<GridCell> OnCellClicked;
    public List<GridCell> allCells = new List<GridCell>();
    public LayerMask boardLayer;
    public float raycastDistance = 100f;
    public Material highlightEmpty;
    public Material highlightOccupied;

    Material lastOriginalMat;
    GameObject lastHighlightedObj;

    void Awake()
    {
        if (allCells.Count == 0)
            allCells.AddRange(GetComponentsInChildren<GridCell>());
    }

    void Update()
    {
        if (lastHighlightedObj != null)
        {
            RestoreOriginalMaterial(lastHighlightedObj);
            lastHighlightedObj = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, boardLayer))
        {
            GameObject hitObj = hit.collider.gameObject;
            GridCell cell = hitObj.GetComponent<GridCell>();
            if (cell != null)
            {
                HighlightCell(hitObj, cell);
                lastHighlightedObj = hitObj;

                if (Input.GetMouseButtonDown(0))
                {
                    OnCellClicked?.Invoke(cell);
                }
            }
        }
    }

    void HighlightCell(GameObject cellObj, GridCell cell)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null)
        {
            lastOriginalMat = rend.material;
            if (!cell.IsOccupied && highlightEmpty)
                rend.material = highlightEmpty;
            else if (cell.IsOccupied && highlightOccupied)
                rend.material = highlightOccupied;
        }
    }

    void RestoreOriginalMaterial(GameObject cellObj)
    {
        Renderer rend = cellObj.GetComponent<Renderer>();
        if (rend != null && lastOriginalMat != null)
            rend.material = lastOriginalMat;
    }
}