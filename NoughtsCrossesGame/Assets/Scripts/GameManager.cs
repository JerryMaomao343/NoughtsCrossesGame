using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Board board;
    public AIController aiController;

    [Header("Pieces (Scene)")]
    public List<GameObject> xPieces;  
    public List<GameObject> oPieces;

    // 每个棋子的原始坐标
    private Vector3[] xOriginalPos;   
    private Vector3[] oOriginalPos;

    // 下一次要用哪枚 X / O
    private int xIndex = 0;
    private int oIndex = 0;

    [Header("Animation / Timing")]
    public float afterPlayerPlaceDelay = 1; 
    public float afterAIPlaceDelay     = 1;

    // 逆序归还相关
    public float resetOnePieceTime = 0.3f;  
    public float resetPieceDelay   = 0.1f; 

    // 记录已放置棋子的顺序
    private List<GameObject> placedPieces = new List<GameObject>();

    // 五局三胜
    private int playerWins = 0;
    private int aiWins    = 0;
    private int currentRound = 1;
    private const int MAX_ROUNDS = 5;

    private bool _gameOver  = false; // 当前局结束
    private bool _matchOver = false; // 整场结束

    // 回合队列
    private Sequence turnSequence;
    public bool isPlayerTurn = false; // 等进入动画

    #region 单例
    
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
                    var go = new GameObject("GameManager_ReverseReturn_EndClear");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    
    #endregion

    #region 初始化
    
    private void OnEnable()
    {
        if (board != null)
            board.OnCellClicked += OnPlayerClickCell;

        EventCenter.Instance.AddListener(GameEvent.OnFinishEnterAni, OnFinishEnterAni);
        EventCenter.Instance.AddListener(GameEvent.OnFinishExitAni, OnFinishExitAni);
    }

    private void OnDisable()
    {
        if (board != null)
            board.OnCellClicked -= OnPlayerClickCell;

        EventCenter.Instance.RemoveListener(GameEvent.OnFinishEnterAni, OnFinishEnterAni);
        EventCenter.Instance.RemoveListener(GameEvent.OnFinishExitAni, OnFinishExitAni);
    }

    private void Start()
    {
        // 记录初始坐标
        xOriginalPos = new Vector3[xPieces.Count];
        for (int i = 0; i < xPieces.Count; i++)
        {
            xOriginalPos[i] = xPieces[i].transform.position;
        }
        oOriginalPos = new Vector3[oPieces.Count];
        for (int i = 0; i < oPieces.Count; i++)
        {
            oOriginalPos[i] = oPieces[i].transform.position;
        }

        // occupant=none
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // AI设置
        aiController.aiOccupant = CellOccupant.O;
        aiController.opponentOccupant = CellOccupant.X;

        // 回合队列
        BuildTurnSequence();
    }
    
    /// <summary>
    /// 初始化比赛
    /// </summary>
    public void NewMatch()
    {
        //重置所有状态
        playerWins = 0;
        aiWins = 0;
        currentRound = 1;
        _gameOver = false;
        _matchOver = false;
        isPlayerTurn = false;

        //清空棋盘 occupant
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // 清空本轮放置顺序
        placedPieces.Clear();

        //隐藏并还原每个棋子到它的初始位置
        for (int i = 0; i < xPieces.Count; i++)
        {
            xPieces[i].transform.position = xOriginalPos[i];
        }
        for (int i = 0; i < oPieces.Count; i++)
        {
            oPieces[i].transform.position = oOriginalPos[i];
        }
        xIndex = 0;
        oIndex = 0;

        //重新构建回合队列
        BuildTurnSequence();
    }
    
    #endregion

    #region 开局动画完成

    private void OnFinishEnterAni(object[] args)
    {
        Debug.Log("[GameManager] OnFinishEnterAni -> Start turn sequence");
        turnSequence.Play();
    }

    private void OnFinishExitAni(object[] args)
    {
        Debug.Log("[GameManager] OnFinishExitAni -> Done exit. Could do final cleanup or main menu...");
    }

    #endregion

    #region 游戏回合队列
    
    private void BuildTurnSequence()
    {
        if (turnSequence != null && turnSequence.IsActive())
            turnSequence.Kill();

        turnSequence = DOTween.Sequence().SetAutoKill(false).Pause();

        // 玩家回合
        turnSequence.AppendCallback(() =>
        {
            if (_matchOver) return;
            if (_gameOver) return;

            Debug.Log("【玩家回合】");
            EventCenter.Instance.Broadcast(GameEvent.OnPlayerRound);
            isPlayerTurn = true;
            turnSequence.Pause();
        });
        turnSequence.AppendInterval(afterPlayerPlaceDelay);

        // AI回合
        turnSequence.AppendCallback(() =>
        {
            if (_matchOver) return;
            if (_gameOver) return;

            Debug.Log("【AI回合】");
            EventCenter.Instance.Broadcast(GameEvent.OnAIRound);
            isPlayerTurn = false;
            turnSequence.Pause();
            Invoke(nameof(HandleAIPlace), 0.5f);
        });
        turnSequence.AppendInterval(afterAIPlaceDelay);

        // 循环或结束
        turnSequence.AppendCallback(() =>
        {
            if (_matchOver) return;
            if (!_gameOver)
            {
                turnSequence.Goto(0, true);
            }
            else
            {
                // 本局结束 -> Check
                CheckMatchState();
            }
        });
    }

    #endregion

    #region 五局三胜

    private void CheckMatchState()
    {
        // 若3胜或打满5局 -> 整场结束
        if (playerWins >= 3 || aiWins >= 3 || currentRound >= MAX_ROUNDS)
        {
            _matchOver = true;
            Debug.Log($"[GameManager] 全场结束: playerWins={playerWins}, aiWins={aiWins}");
            // 最终也要收回棋子
            ReverseReturnAllPieces(() =>
            {
                // 收回后再广播GameEnd
                EventCenter.Instance.Broadcast(GameEvent.OnEndGame);
            });
        }
        else
        {
            // 还没结束 -> 下一局
            currentRound++;
            Debug.Log($"[GameManager] 第{currentRound}局开始");
            ResetForNextGame();
        }
    }

    /// <summary>
    /// 当只是“单局结束但比赛未结束”时，收回棋子后再开始下一局
    /// </summary>
    private void ResetForNextGame()
    {
        _gameOver = false;
        isPlayerTurn = false;
        // occupant=none
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // 逆序收回棋子
        ReverseReturnAllPieces(() =>
        {
            // 全部收回后，清空 placedPieces
            placedPieces.Clear();
            // 重置下标
            xIndex = 0;
            oIndex = 0;

            BuildTurnSequence();
            turnSequence.Play();
        });
    }

    /// <summary>
    /// 逆序回收棋子
    /// </summary>
    private void ReverseReturnAllPieces(TweenCallback onComplete)
    {
        Sequence seq = DOTween.Sequence();
        float delay = 0f;

        // 逆序收回棋子
        for (int i = placedPieces.Count - 1; i >= 0; i--)
        {
            GameObject piece = placedPieces[i];
            int xIdx = xPieces.IndexOf(piece);
            if (xIdx >= 0)
            {
                // X
                seq.Insert(delay,
                    piece.transform.DOMove(xOriginalPos[xIdx], resetOnePieceTime));
            }
            else
            {
                // O
                int oIdx = oPieces.IndexOf(piece);
                if (oIdx >= 0)
                {
                    seq.Insert(delay,
                        piece.transform.DOMove(oOriginalPos[oIdx], resetOnePieceTime));
                }
            }
            delay += resetPieceDelay;
        }
        
        seq.OnComplete(() =>
        {

            onComplete?.Invoke();
        });
    }
    
    #endregion

    #region 双方落子

    private void OnPlayerClickCell(GridCell cell)
    {
        if (!isPlayerTurn || _gameOver || _matchOver) return;
        if (cell.occupant != CellOccupant.None)
        {
            Debug.Log("格子被占用");
            return;
        }

        cell.occupant = CellOccupant.X;
        EventCenter.Instance.Broadcast(GameEvent.OnPlayerPlace);

        if (xIndex < xPieces.Count)
        {
            GameObject piece = xPieces[xIndex];
            xIndex++;
            ParabolaDrop drop = piece.GetComponent<ParabolaDrop>();
            drop.DoParabolaDrop(piece.transform.position, cell.transform.position);

            // 记录顺序
            placedPieces.Add(piece);
        }
        else
        {
            Debug.LogWarning("X耗尽");
        }

        if (CheckWin(CellOccupant.X))
        {
            Debug.Log($"第{currentRound}局：玩家X赢");
            playerWins++;
            _gameOver = true;
        }
        else if (CheckDraw())
        {
            Debug.Log($"第{currentRound}局：平局");
            _gameOver = true;
        }

        isPlayerTurn = false;
        turnSequence.Play();
    }

    private void HandleAIPlace()
    {
        if (_gameOver || _matchOver)
        {
            turnSequence.Play();
            return;
        }

        GridCell bestCell = aiController.GetBestMove(board.allCells);
        if (bestCell != null)
        {
            bestCell.occupant = CellOccupant.O;
            EventCenter.Instance.Broadcast(GameEvent.OnAIPlace);

            if (oIndex < oPieces.Count)
            {
                GameObject piece = oPieces[oIndex];
                oIndex++;
                ParabolaDrop drop = piece.GetComponent<ParabolaDrop>();
                drop.DoParabolaDrop(piece.transform.position, bestCell.transform.position);

                placedPieces.Add(piece);
            }
            else
            {
                Debug.LogWarning("O耗尽");
            }

            if (CheckWin(CellOccupant.O))
            {
                Debug.Log($"第{currentRound}局：AI(O)赢");
                aiWins++;
                _gameOver = true;
            }
            else if (CheckDraw())
            {
                Debug.Log($"第{currentRound}局：平局");
                _gameOver = true;
            }
        }
        else
        {
            Debug.Log("AI无可下之处，平局?");
            _gameOver = true;
        }

        turnSequence.Play();
    }
    
    #endregion

    #region 单局判定

    private bool CheckWin(CellOccupant occupant)
    {
        CellOccupant GetOcc(int r, int c)
        {
            var cell = board.allCells.Find(x => x.cellIndex.x == r && x.cellIndex.y == c);
            return cell != null ? cell.occupant : CellOccupant.None;
        }

        for (int i = 0; i < 3; i++)
        {
            if (GetOcc(i,0) == occupant && GetOcc(i,1) == occupant && GetOcc(i,2) == occupant) return true;
            if (GetOcc(0,i) == occupant && GetOcc(1,i) == occupant && GetOcc(2,i) == occupant) return true;
        }
        if (GetOcc(0,0) == occupant && GetOcc(1,1) == occupant && GetOcc(2,2) == occupant) return true;
        if (GetOcc(0,2) == occupant && GetOcc(1,1) == occupant && GetOcc(2,0) == occupant) return true;
        return false;
    }

    private bool CheckDraw()
    {
        foreach (var cell in board.allCells)
        {
            if (cell.occupant == CellOccupant.None) return false;
        }
        return true;
    }

    #endregion

}
