using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;         

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Board board; 
    public AIController aiController; 
    public GameObject prefabX;
    public GameObject prefabO;

    [Header("Animation / Timing")]
    public float afterPlayerPlaceDelay = 1.0f; 
    public float afterAIPlaceDelay     = 1.0f; 

    private Sequence _turnSequence;
    public bool isPlayerTurn =true;
    private bool _gameOver = false;
    
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    private void OnEnable()
    {
        if (board != null) 
            board.OnCellClicked += OnPlayerClickCell;
    }

    private void OnDisable()
    {
        if (board != null) 
            board.OnCellClicked -= OnPlayerClickCell;
    }

    private void Start()
    {
        // 初始化棋盘状态
        foreach (var cell in board.allCells)
        {
            cell.occupant = CellOccupant.None;
        }
        // AI设置 (玩家= X, AI= O)
        aiController.aiOccupant = CellOccupant.O;
        aiController.opponentOccupant = CellOccupant.X;

        // 构造并启动回合队列流程
        BuildTurnSequence();
        _turnSequence.Play();
    }

    /// <summary>
    /// 构造游戏回合队列
    /// </summary>
    private void BuildTurnSequence()
    {
        // 清空
        if (_turnSequence != null && _turnSequence.IsActive())
            _turnSequence.Kill();
        
        _turnSequence = DOTween.Sequence()
            .SetAutoKill(false) 
            .SetUpdate(UpdateType.Normal, false)
            .Pause();

        // ----------- 玩家回合 -----------
        _turnSequence.AppendCallback(() => 
        {
            if (_gameOver) return;
            EventCenter.Instance.Broadcast(GameEvent.OnPlayerRound);
            isPlayerTurn = true;
            _turnSequence.Pause();
        });
        //--------- 玩家回合后等待 ---------
        _turnSequence.AppendInterval(afterPlayerPlaceDelay);

        // ------------ AI回合 -----------
        _turnSequence.AppendCallback(() =>
        {
            if (_gameOver) return;
            EventCenter.Instance.Broadcast(GameEvent.OnAIRound);
            isPlayerTurn = false;
            _turnSequence.Pause();
            Invoke(nameof(HandleAIPlace), 0.5f);
        });

        // --------- AI回合后等待 --------
        _turnSequence.AppendInterval(afterAIPlaceDelay);

        // -------决定是否结束或循环 -------
        _turnSequence.AppendCallback(() =>
        {
            if (!_gameOver)
            {
                _turnSequence.Goto(0, true);
            }
            else
            {
                Debug.Log("【GameOver】游戏队列停止");
            }
        });
    }

    /// <summary>
    /// 当玩家点击某格子
    /// </summary>
    private void OnPlayerClickCell(GridCell cell)
    {
        //无效
        if (!isPlayerTurn || _gameOver) return;
        if (cell.occupant != CellOccupant.None)
        {
            Debug.Log("该格子已被占用");
            return;
        }

        // 有效，应用落子
        cell.occupant = CellOccupant.X;
        Instantiate(prefabX, cell.transform.position, Quaternion.identity);

        //结束回合
        isPlayerTurn = false;

        // 检查胜负
        if (CheckWin(CellOccupant.X))
        {
            Debug.Log("玩家(X)赢了");
            _gameOver = true;
        }
        else if (CheckDraw())
        {
            Debug.Log("平局");
            _gameOver = true;
        }
        
        Debug.Log("继续游戏队列");
        _turnSequence.Play();
    }


    /// <summary>
    /// AI落子
    /// </summary>
    private void HandleAIPlace()
    {
        if (_gameOver) 
        {
            // 若已经结束，不再下棋
            _turnSequence.Play(); 
            return;
        }

        GridCell bestCell = aiController.GetBestMove(board.allCells);
        if (bestCell != null)
        {
            bestCell.occupant = CellOccupant.O;
            EventCenter.Instance.Broadcast(GameEvent.OnAIPlace);
            Instantiate(prefabO, bestCell.transform.position, Quaternion.identity);

            // 检查AI是否赢
            if (CheckWin(CellOccupant.O))
            {
                Debug.Log("AI(O)赢了");
                _gameOver = true;
            }
            else if (CheckDraw())
            {
                Debug.Log("平局");
                _gameOver = true;
            }
        }
        else
        {
            Debug.Log("AI没有可下位置，可能平局");
            _gameOver = true;
        }
        
        Debug.Log("继续游戏队列");
        _turnSequence.Play();
    }

    // ===== 胜负/平局判定示例 =====
    private bool CheckWin(CellOccupant occupant)
    {
        // 先用个获取 occupant 的助手
        CellOccupant GetOcc(int r, int c)
        {
            var cell = board.allCells.Find(x => x.cellIndex.x == r && x.cellIndex.y == c);
            return (cell != null) ? cell.occupant : CellOccupant.None;
        }

        for (int i = 0; i < 3; i++)
        {
            // 行 i
            if (GetOcc(i,0) == occupant && GetOcc(i,1) == occupant && GetOcc(i,2) == occupant)
                return true;
            // 列 i
            if (GetOcc(0,i) == occupant && GetOcc(1,i) == occupant && GetOcc(2,i) == occupant)
                return true;
        }
        // 对角
        if (GetOcc(0,0) == occupant && GetOcc(1,1) == occupant && GetOcc(2,2) == occupant)
            return true;
        if (GetOcc(0,2) == occupant && GetOcc(1,1) == occupant && GetOcc(2,0) == occupant)
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
