using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public CellOccupant aiOccupant = CellOccupant.O;
    public CellOccupant opponentOccupant = CellOccupant.X;

    [Range(1, 9)]
    public int maxDepth = 9; // 难度因子: 越小 -> 越容易失误 (因为只看浅层)

    [Range(0f, 1f)]
    public float randomFactor = 0f; // 难度因子: 越大 -> 越可能随机选择非最优解

    /// <summary>
    /// 获取 AI 的最佳落子(返回要下的格子)
    /// </summary>
    public GridCell GetBestMove(List<GridCell> allCells)
    {
        int bestScore = int.MinValue;
        GridCell bestCell = null;

        // 收集所有“空”格子
        List<GridCell> emptyCells = new List<GridCell>();
        foreach (var cell in allCells)
        {
            if (cell.occupant == CellOccupant.None)
                emptyCells.Add(cell);
        }

        // 如果有一定 randomFactor，我们可能直接抛弃 Minimax，在少量几率下纯随机落子
        // 例如 30% 概率随机，让 AI 更“失误”
        if (Random.value < randomFactor)
        {
            // 直接在空格子中任选一个
            int r = Random.Range(0, emptyCells.Count);
            return emptyCells[r];
        }

        // 否则执行 Minimax
        for (int i = 0; i < emptyCells.Count; i++)
        {
            var cell = emptyCells[i];
            // 试着在这里下子
            cell.occupant = aiOccupant;

            int score = Minimax(allCells, 0, false);

            // 撤销
            cell.occupant = CellOccupant.None;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }
        return bestCell;
    }

    private int Minimax(List<GridCell> allCells, int depth, bool isAiTurn)
    {
        // 1. 检查胜负或平局
        if (CheckWin(allCells, aiOccupant)) return +10 - depth;
        if (CheckWin(allCells, opponentOccupant)) return -10 + depth;
        if (CheckDraw(allCells)) return 0;

        // 2. 如果超出最大搜索深度，就用“局面评估函数(Evaluation)”来返回大概分数
        if (depth >= maxDepth)
        {
            return EvaluateBoard(allCells);
        }

        // 3. 继续搜索
        if (isAiTurn)
        {
            int bestScore = int.MinValue;
            foreach (var cell in allCells)
            {
                if (cell.occupant == CellOccupant.None)
                {
                    cell.occupant = aiOccupant;
                    int score = Minimax(allCells, depth + 1, false);
                    cell.occupant = CellOccupant.None;

                    bestScore = Mathf.Max(bestScore, score);
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            foreach (var cell in allCells)
            {
                if (cell.occupant == CellOccupant.None)
                {
                    cell.occupant = opponentOccupant;
                    int score = Minimax(allCells, depth + 1, true);
                    cell.occupant = CellOccupant.None;

                    bestScore = Mathf.Min(bestScore, score);
                }
            }
            return bestScore;
        }
    }

    /// <summary>
    /// 分数评估方法
    /// </summary>
    /// <param name="allCells">所有落子</param>
    /// <returns>决策得分</returns>
    private int EvaluateBoard(List<GridCell> allCells)
    {
        // +3 分: AI 有两连且第三格为空
        // +1 分: AI 有1个子
        // -3 分: 对手有两连且第三格为空
        // -1 分: 对手有1个子

        int score = 0;

        // 把 3行+3列+2对角都检查 看双方的子数
        int[,] lines = new int[,]
        {
            {0,0, 0,1, 0,2}, // row0
            {1,0, 1,1, 1,2}, // row1
            {2,0, 2,1, 2,2}, // row2
            {0,0, 1,0, 2,0}, // col0
            {0,1, 1,1, 2,1}, // col1
            {0,2, 1,2, 2,2}, // col2
            {0,0, 1,1, 2,2}, // diag1
            {0,2, 1,1, 2,0}, // diag2
        };

        // 辅助函数
        CellOccupant GetOcc(int r, int c)
        {
            GridCell cell = allCells.Find(x => x.cellIndex.x == r && x.cellIndex.y == c);
            return cell != null ? cell.occupant : CellOccupant.None;
        }

        for (int i = 0; i < lines.GetLength(0); i++)
        {
            int r1 = lines[i, 0], c1 = lines[i, 1];
            int r2 = lines[i, 2], c2 = lines[i, 3];
            int r3 = lines[i, 4], c3 = lines[i, 5];

            CellOccupant o1 = GetOcc(r1, c1);
            CellOccupant o2 = GetOcc(r2, c2);
            CellOccupant o3 = GetOcc(r3, c3);

            // 统计这一条线 AI 的子数, 对手的子数
            int aiCount = 0, oppCount = 0;
            if (o1 == aiOccupant) aiCount++;
            if (o2 == aiOccupant) aiCount++;
            if (o3 == aiOccupant) aiCount++;
            if (o1 == opponentOccupant) oppCount++;
            if (o2 == opponentOccupant) oppCount++;
            if (o3 == opponentOccupant) oppCount++;

            
            
            //决策得分评估
            // 2 连 + 1 空 = +3 分；
            // 1 连 = +1 分
            // 如果一条线上同时包含AI和对手的子, 不加分
            if (aiCount > 0 && oppCount > 0) continue;
            
            //AI计分
            if (aiCount == 2) score += 3;
            else if (aiCount == 1) score += 1;
            
            //对手计分
            if (oppCount == 2) score -= 3;
            else if (oppCount == 1) score -= 1;
        }

        return score;
    }

    /// <summary>
    /// 检查是否胜利
    /// </summary>
    /// <param name="allCells">所有落子</param>
    /// <param name="occupant">玩家对手</param>
    /// <returns></returns>
    private bool CheckWin(List<GridCell> allCells, CellOccupant occupant)
    {
        // 行列
        for (int i = 0; i < 3; i++)
        {
            if (GetOcc(allCells, i, 0) == occupant && 
                GetOcc(allCells, i, 1) == occupant && 
                GetOcc(allCells, i, 2) == occupant)
                return true;

            if (GetOcc(allCells, 0, i) == occupant &&
                GetOcc(allCells, 1, i) == occupant &&
                GetOcc(allCells, 2, i) == occupant)
                return true;
        }
        // 对角
        if (GetOcc(allCells, 0, 0) == occupant &&
            GetOcc(allCells, 1, 1) == occupant &&
            GetOcc(allCells, 2, 2) == occupant)
            return true;
        if (GetOcc(allCells, 0, 2) == occupant &&
            GetOcc(allCells, 1, 1) == occupant &&
            GetOcc(allCells, 2, 0) == occupant)
            return true;

        return false;
    }

    private bool CheckDraw(List<GridCell> allCells)
    {
        foreach (var cell in allCells)
        {
            if (cell.occupant == CellOccupant.None) return false;
        }
        return true;
    }

    private CellOccupant GetOcc(List<GridCell> allCells, int row, int col)
    {
        var cell = allCells.Find(x => x.cellIndex.x == row && x.cellIndex.y == col);
        return (cell != null) ? cell.occupant : CellOccupant.None;
    }
}
