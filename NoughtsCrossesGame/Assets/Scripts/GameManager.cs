using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Board board;                  
    public GameObject prefabX;           
    public GameObject prefabO;           
    public AIController aiController;    

    //玩家使用 X, AI使用 O, 并且 X先手
    private bool isPlayerTurn = true;

    private void OnEnable()
    {
        if (board != null)
            board.OnCellClicked += HandleCellClicked;
    }

    private void OnDisable()
    {
        if (board != null)
            board.OnCellClicked -= HandleCellClicked;
    }

    private void Start()
    {
        // 初始化所有格子
        foreach (var cell in board.allCells)
        {
            cell.occupant = CellOccupant.None;
        }

        // 设置AI的棋子
        if (aiController != null)
        {
            aiController.aiOccupant = CellOccupant.O;
            aiController.opponentOccupant = CellOccupant.X;
        }
    }

    /// <summary>
    /// 玩家点击某个格子时触发
    /// </summary>
    private void HandleCellClicked(GridCell cell)
    {
        if (!isPlayerTurn)
        {
            // 如果现在是AI回合，忽略玩家点击
            return;
        }

        // 如果该格子已被占用
        if (cell.occupant != CellOccupant.None)
        {
            Debug.Log("这个格子已经有子了");
            return;
        }

        // 玩家有效落子，广播并生成模型
        cell.occupant = CellOccupant.X;
        EventCenter.Instance.Broadcast(GameEvent.OnPlayerPlace);
        Instantiate(prefabX, cell.transform.position, Quaternion.identity);

        // 检查玩家对局结果
        if (CheckWin(CellOccupant.X))
        {
            Debug.Log("玩家(X)赢了");
            // TODO: 结束游戏
            return;
        }
        else if (CheckDraw())
        {
            Debug.Log("平局");
            // TODO: 结束游戏
            return;
        }

        // 切换到AI回合并广播
        isPlayerTurn = false;
        EventCenter.Instance.Broadcast(GameEvent.OnAIRound);
        Invoke(nameof(HandleAIMove), 0.5f); 
    }

    private void HandleAIMove()
    {
        // 获取 AI 要下的格子
        if (aiController == null) return;

        GridCell bestCell = aiController.GetBestMove(board.allCells);
        if (bestCell != null)
        {
            // AI 落子 (O)
            bestCell.occupant = CellOccupant.O;
            Instantiate(prefabO, bestCell.transform.position, Quaternion.identity);

            // 检查AI是否赢
            if (CheckWin(CellOccupant.O))
            {
                Debug.Log("AI(O)赢了");
                // TODO: 结束游戏
                return;
            }
            else if (CheckDraw())
            {
                Debug.Log("平局");
                // TODO: 结束游戏
                return;
            }
        }
        else
        {
            // 如果 bestCell == null, 说明棋盘满了或者其他异常
            Debug.Log("AI 没有可下的格子, 平局?");
        }

        // 切回玩家回合
        isPlayerTurn = true;
    }

    // 下面2个判定逻辑跟AIController的 CheckWin/CheckDraw 类似, 
    // 只要保持一致即可 (或者你可以直接复用AIController那套函数)

    private bool CheckWin(CellOccupant occupant)
    {
        CellOccupant GetOcc(int r, int c)
        {
            var cell = board.allCells.Find(x => x.cellIndex.x == r && x.cellIndex.y == c);
            return (cell != null) ? cell.occupant : CellOccupant.None;
        }

        // 行列
        for (int i = 0; i < 3; i++)
        {
            // 行 i
            if (GetOcc(i, 0) == occupant && GetOcc(i, 1) == occupant && GetOcc(i, 2) == occupant)
                return true;
            // 列 i
            if (GetOcc(0, i) == occupant && GetOcc(1, i) == occupant && GetOcc(2, i) == occupant)
                return true;
        }
        // 对角
        if (GetOcc(0, 0) == occupant && GetOcc(1, 1) == occupant && GetOcc(2, 2) == occupant)
            return true;
        if (GetOcc(0, 2) == occupant && GetOcc(1, 1) == occupant && GetOcc(2, 0) == occupant)
            return true;

        return false;
    }

    private bool CheckDraw()
    {
        foreach (var cell in board.allCells)
        {
            if (cell.occupant == CellOccupant.None)
                return false;
        }
        return true;
    }
}
