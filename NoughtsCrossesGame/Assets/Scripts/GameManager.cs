using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Board board;
    
    public enum CellState { Empty, X, O }
    private CellState[,] boardState = new CellState[3,3];

    public GameObject prefabX;
    public GameObject prefabO;

    private bool isXTurn = true;

    private void OnEnable()
    {
        if (board != null)
        {
            // 改成订阅 OnCellClicked(GridCell cell)
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
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                boardState[i, j] = CellState.Empty;
            }
        }
    }

    private void HandleCellClicked(GridCell cell)
    {
        // 拿到坐标
        int row = cell.cellIndex.x;
        int col = cell.cellIndex.y;

        // 1. 检查是否可落子
        if (boardState[row, col] != CellState.Empty)
        {
            Debug.Log("该格子已经被占用！");
            return;
        }

        // 2. 更新棋盘状态
        boardState[row, col] = isXTurn ? CellState.X : CellState.O;

        // 3. 根据格子的 transform.position 放置对应的 X / O 预制体
        Vector3 spawnPos = cell.transform.position;
        Instantiate(isXTurn ? prefabX : prefabO, spawnPos, Quaternion.identity);

        // 4. 检查胜负
        if (CheckWin(isXTurn ? CellState.X : CellState.O))
        {
            Debug.Log(isXTurn ? "X 胜利！" : "O 胜利！");
            // TODO: 处理游戏结束
        }
        else if (CheckDraw())
        {
            Debug.Log("平局！");
            // TODO: 处理游戏结束
        }

        // 5. 切换回合
        isXTurn = !isXTurn;
    }

    private bool CheckWin(CellState currentPlayer)
    {
        // 行、列
        for (int i = 0; i < 3; i++)
        {
            // 行
            if (boardState[i,0] == currentPlayer &&
                boardState[i,1] == currentPlayer &&
                boardState[i,2] == currentPlayer)
                return true;

            // 列
            if (boardState[0,i] == currentPlayer &&
                boardState[1,i] == currentPlayer &&
                boardState[2,i] == currentPlayer)
                return true;
        }

        // 对角
        if (boardState[0,0] == currentPlayer &&
            boardState[1,1] == currentPlayer &&
            boardState[2,2] == currentPlayer)
            return true;
        if (boardState[0,2] == currentPlayer &&
            boardState[1,1] == currentPlayer &&
            boardState[2,0] == currentPlayer)
            return true;

        return false;
    }

    private bool CheckDraw()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (boardState[i,j] == CellState.Empty)
                    return false;
        return true;
    }
}
