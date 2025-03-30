using UnityEngine;

public enum CellOccupant
{
    None,
    X,
    O
}

public class GridCell : MonoBehaviour
{
    public Vector2Int cellIndex;       // 用于标识本格子在棋盘中的行列
    public CellOccupant occupant = CellOccupant.None;  // 当前占用状态 (None, X, O)

    public bool IsOccupied => occupant != CellOccupant.None;
}


