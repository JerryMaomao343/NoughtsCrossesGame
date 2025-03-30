using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Board board;                  // 指向 Board 脚本
    public GameObject prefabX;           // X 棋子模型
    public GameObject prefabO;           // O 棋子模型

    private bool isXTurn = true;         // 是否轮到 X

    private void OnEnable()
    {
        if (board != null)
        {
            // 订阅 Board 的点击事件
            board.OnCellClicked += HandleCellClicked;
        }
    }

    private void OnDisable()
    {
        if (board != null)
        {
            board.OnCellClicked -= HandleCellClicked;
        }
    }

    private void Start()
    {
        // 如果需要初始化逻辑可以放这里
        // 比如：清空所有格子的 occupant
        if (board != null && board.allCells != null)
        {
            foreach (var cell in board.allCells)
            {
                cell.occupant = CellOccupant.None;
            }
        }
    }

    // 当某个格子被点击
    private void HandleCellClicked(GridCell cell)
    {
        // 1. 如果该格子已被占用，则无法落子
        if (cell.IsOccupied)
        {
            Debug.Log($"格子({cell.cellIndex.x},{cell.cellIndex.y}) 已有棋子，无法落子");
            return;
        }

        // 2. 根据当前轮次，给该格子记录 occupant = X 或 O
        CellOccupant occupantType = isXTurn ? CellOccupant.X : CellOccupant.O;
        cell.occupant = occupantType;

        // 3. 在格子的位置生成对应棋子
        Vector3 spawnPos = cell.transform.position;
        GameObject piecePrefab = isXTurn ? prefabX : prefabO;
        Instantiate(piecePrefab, spawnPos, Quaternion.identity);

        // 4. 检查胜负
        if (CheckWin(occupantType))
        {
            Debug.Log(occupantType + " 获胜！");
            // TODO: 这里可做游戏结束处理
            return;
        }
        // 如果没人赢，则检查平局
        else if (CheckDraw())
        {
            Debug.Log("平局！");
            // TODO: 处理平局逻辑
            return;
        }

        // 5. 切换回合
        isXTurn = !isXTurn;
    }

    // 用 occupantType 判断是否形成三连
    private bool CheckWin(CellOccupant occupantType)
    {
        // 在 board.allCells 里查找指定 (row,col) 的 occupant
        CellOccupant GetOccupantAt(int row, int col)
        {
            GridCell target = board.allCells.Find(c => 
                c.cellIndex.x == row && c.cellIndex.y == col);
            return (target != null) ? target.occupant : CellOccupant.None;
        }

        // 检查三行三列
        for (int i = 0; i < 3; i++)
        {
            // 行 i
            if (GetOccupantAt(i, 0) == occupantType &&
                GetOccupantAt(i, 1) == occupantType &&
                GetOccupantAt(i, 2) == occupantType)
            {
                return true;
            }
            // 列 i
            if (GetOccupantAt(0, i) == occupantType &&
                GetOccupantAt(1, i) == occupantType &&
                GetOccupantAt(2, i) == occupantType)
            {
                return true;
            }
        }

        // 检查两条对角
        if (GetOccupantAt(0, 0) == occupantType &&
            GetOccupantAt(1, 1) == occupantType &&
            GetOccupantAt(2, 2) == occupantType)
        {
            return true;
        }
        if (GetOccupantAt(0, 2) == occupantType &&
            GetOccupantAt(1, 1) == occupantType &&
            GetOccupantAt(2, 0) == occupantType)
        {
            return true;
        }

        // 没有三连
        return false;
    }

    private bool CheckDraw()
    {
        // 如果任意格子是 None，则还没下满
        foreach (GridCell cell in board.allCells)
        {
            if (cell.occupant == CellOccupant.None)
                return false;
        }
        // 没有三连且全部格子都不为 None -> 平局
        return true;
    }
}
