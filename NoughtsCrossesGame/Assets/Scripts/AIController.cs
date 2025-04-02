using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AIController : MonoBehaviour
{
    public CellOccupant aiOccupant = CellOccupant.O;
    public CellOccupant opponentOccupant = CellOccupant.X;
    
    
    [FormerlySerializedAs("_maxDepth")] [Range(1, 9)]
    public int maxDepth = 1; 

    [FormerlySerializedAs("_randomFactor")] [Range(0f, 1f)]
    public float randomFactor = 0.1f;

    public void ChangeDifficult(int diff)
    {
        maxDepth = diff;
        randomFactor = 1.0f - 0.2f * diff;
    }

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

        // 如果有一定 randomFactor，我们可能直接在少量几率下纯随机落子
        if (Random.value < randomFactor && emptyCells.Count > 0)
        {
            int r = Random.Range(0, emptyCells.Count);
            GridCell randomCell = emptyCells[r];

            return randomCell;
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

        // 2. 如果超出最大搜索深度，就用局面评估函数
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

    /// <summary>分数评估</summary>
    private int EvaluateBoard(List<GridCell> allCells)
    {
        int score = 0;
        int[,] lines = new int[,]
        {
            {0,0, 0,1, 0,2},
            {1,0, 1,1, 1,2},
            {2,0, 2,1, 2,2},
            {0,0, 1,0, 2,0},
            {0,1, 1,1, 2,1},
            {0,2, 1,2, 2,2},
            {0,0, 1,1, 2,2},
            {0,2, 1,1, 2,0},
        };

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

            int aiCount = 0, oppCount = 0;
            if (o1 == aiOccupant) aiCount++;
            if (o2 == aiOccupant) aiCount++;
            if (o3 == aiOccupant) aiCount++;
            if (o1 == opponentOccupant) oppCount++;
            if (o2 == opponentOccupant) oppCount++;
            if (o3 == opponentOccupant) oppCount++;

            // 如果一条线上同时含有AI和对手的子，不加分
            if (aiCount > 0 && oppCount > 0) continue;

            // AI计分
            if (aiCount == 2) score += 3;
            else if (aiCount == 1) score += 1;

            // 对手计分
            if (oppCount == 2) score -= 3;
            else if (oppCount == 1) score -= 1;
        }

        return score;
    }

    private bool CheckWin(List<GridCell> allCells, CellOccupant occupant)
    {
        for (int i = 0; i < 3; i++)
        {
            if (GetOcc(allCells, i,0) == occupant && 
                GetOcc(allCells, i,1) == occupant && 
                GetOcc(allCells, i,2) == occupant) return true;

            if (GetOcc(allCells, 0,i) == occupant &&
                GetOcc(allCells, 1,i) == occupant &&
                GetOcc(allCells, 2,i) == occupant) return true;
        }
        if (GetOcc(allCells, 0,0) == occupant &&
            GetOcc(allCells, 1,1) == occupant &&
            GetOcc(allCells, 2,2) == occupant) return true;
        if (GetOcc(allCells, 0,2) == occupant &&
            GetOcc(allCells, 1,1) == occupant &&
            GetOcc(allCells, 2,0) == occupant) return true;

        return false;
    }

    private bool CheckDraw(List<GridCell> allCells)
    {
        foreach (var cell in allCells)
        {
            if (cell.occupant == CellOccupant.None)
                return false;
        }
        return true;
    }

    private CellOccupant GetOcc(List<GridCell> allCells, int row, int col)
    {
        var cell = allCells.Find(x => x.cellIndex.x == row && x.cellIndex.y == col);
        return (cell != null) ? cell.occupant : CellOccupant.None;
    }
}
