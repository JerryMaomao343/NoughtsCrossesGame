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

    private Vector3[] xOriginalPos;   
    private Vector3[] oOriginalPos;
    private int xIndex = 0;
    private int oIndex = 0;

    [Header("Animation / Timing")]
    public float afterPlayerPlaceDelay = 1; 
    public float afterAIPlaceDelay     = 1;
    public float resetOnePieceTime = 0.3f;  
    public float resetPieceDelay   = 0.1f; 

    // 记录X/O放置顺序
    private List<GameObject> placedPieces = new List<GameObject>();
    
    // 金币相关：每局赢就拿，不立刻归还 
    [Header("Coins on Table")]
    // 场景中桌子上预先放好的金币
    public List<GameObject> goldCoins;  
    private Vector3[] goldCoinsOriginalPos;
    private int goldCoinIndex = 0; // 下次要取哪枚金币
    private Vector3 coinOffsetPerCoin = new Vector3(0f, 0f, 0.8f); 

    // 记录本场被拿走的金币整场结束后一次性回收
    private List<GameObject> takenGoldCoins = new List<GameObject>();

    // 赢家放金币的奖杯位(玩家/AI)
    public Transform playerTrophyPos;
    public Transform aiTrophyPos;

    // 五局三胜
    private int playerWins = 0;
    private int aiWins    = 0;
    private int currentRound = 1;
    private const int MAX_ROUNDS = 5;

    private bool _gameOver  = false; // 当前局是否结束
    private bool _matchOver = false; // 整场是否结束

    // 游戏队列
    private Sequence turnSequence;
    public bool isPlayerTurn = false; 

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
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region 初始化/事件
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
        // 记录X/O初始pos
        xOriginalPos = new Vector3[xPieces.Count];
        for (int i = 0; i < xPieces.Count; i++)
            xOriginalPos[i] = xPieces[i].transform.position;

        oOriginalPos = new Vector3[oPieces.Count];
        for (int i = 0; i < oPieces.Count; i++)
            oOriginalPos[i] = oPieces[i].transform.position;

        // occupant=none
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // AI
        aiController.aiOccupant = CellOccupant.O;
        aiController.opponentOccupant = CellOccupant.X;

        // 记录金币初始pos
        goldCoinsOriginalPos = new Vector3[goldCoins.Count];
        for (int i = 0; i < goldCoins.Count; i++)
            goldCoinsOriginalPos[i] = goldCoins[i].transform.position;

        BuildTurnSequence();
    }

    public void NewMatch()
    {
        playerWins = 0;
        aiWins = 0;
        currentRound = 1;
        _gameOver = false;
        _matchOver = false;
        isPlayerTurn = false;

        // 棋盘 occupant
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // 清空X/O 记录
        placedPieces.Clear();
        xIndex = 0;
        oIndex = 0;
        // 重置并还原X/O坐标
        for (int i = 0; i < xPieces.Count; i++)
            xPieces[i].transform.position = xOriginalPos[i];
        for (int i = 0; i < oPieces.Count; i++)
            oPieces[i].transform.position = oOriginalPos[i];

        // 金币重置
        takenGoldCoins.Clear();
        goldCoinIndex = 0;
        for (int i = 0; i < goldCoins.Count; i++)
            goldCoins[i].transform.position = goldCoinsOriginalPos[i];

        BuildTurnSequence();
    }

    private void OnFinishEnterAni(object[] args)
    {
        // 进入动画播完，正式开始
        turnSequence.Play();
    }

    private void OnFinishExitAni(object[] args)
    {
        // 退出动画播完
        Debug.Log("对局彻底结束，可返回主菜单");
    }
    #endregion

    #region 回合队列
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
            isPlayerTurn = false;
            turnSequence.Pause();
            Invoke(nameof(HandleAIPlace), 0.5f);
        });
        turnSequence.AppendInterval(afterAIPlaceDelay);

        // 若没结束，循环
        turnSequence.AppendCallback(() =>
        {
            if (_matchOver) return;
            if (!_gameOver)
            {
                turnSequence.Goto(0, true);
            }
            else
            {
                // 单局结束 => 检查五局三胜
                CheckMatchState();
            }
        });
    }
    #endregion

    #region 五局三胜
    private void CheckMatchState()
    {
        if (playerWins >= 3 || aiWins >= 3 || currentRound >= MAX_ROUNDS)
        {
            _matchOver = true;
            Debug.Log($"整场结束: playerWins={playerWins}, aiWins={aiWins}");
            // 此时收回(棋子+金币)
            ReverseReturnAllPieces(() =>
            {
                // 收回后广播GameEnd
                EventCenter.Instance.Broadcast(GameEvent.OnEndGame);
            });
        }
        else
        {
            // 下局
            currentRound++;
            Debug.Log($"第{currentRound}局开始");

            ResetForNextGame();
        }
    }

    private void ResetForNextGame()
    {
        _gameOver = false;
        isPlayerTurn = false;

        // 清空棋盘 occupant
        foreach (var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // 仅收回X/O，不收金币(因为每小局赢的金币要留在奖杯位)
        ReverseReturnPieces(() =>
        {
            placedPieces.Clear();
            xIndex = 0;
            oIndex = 0;
            BuildTurnSequence();
            turnSequence.Play();
        });
    }
    #endregion

    #region 收回相关
    /// <summary>
    /// 单局结束后，只收X/O到起始位置，但金币不管（继续留在奖杯处）
    /// </summary>
    private void ReverseReturnPieces(TweenCallback onComplete)
    {
        Sequence seq = DOTween.Sequence();
        float delay = 0f;

        // 逆序收回 X/O
        for (int i = placedPieces.Count - 1; i >= 0; i--)
        {
            GameObject piece = placedPieces[i];
            int idxX = xPieces.IndexOf(piece);
            if (idxX >= 0)
            {
                seq.Insert(delay, piece.transform.DOMove(xOriginalPos[idxX], resetOnePieceTime));
            }
            else
            {
                int idxO = oPieces.IndexOf(piece);
                if (idxO >= 0)
                {
                    seq.Insert(delay, piece.transform.DOMove(oOriginalPos[idxO], resetOnePieceTime));
                }
            }
            delay += resetPieceDelay;
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 整场结束后再收回(棋子+金币)
    /// </summary>
    private void ReverseReturnAllPieces(TweenCallback onComplete)
    {
        Sequence seq = DOTween.Sequence();
        float delay = 0f;

        // 先收 X/O
        for (int i = placedPieces.Count - 1; i >= 0; i--)
        {
            GameObject piece = placedPieces[i];
            int idxX = xPieces.IndexOf(piece);
            if (idxX >= 0)
            {
                seq.Insert(delay, piece.transform.DOMove(xOriginalPos[idxX], resetOnePieceTime));
            }
            else
            {
                int idxO = oPieces.IndexOf(piece);
                if (idxO >= 0)
                {
                    seq.Insert(delay, piece.transform.DOMove(oOriginalPos[idxO], resetOnePieceTime));
                }
            }
            delay += resetPieceDelay;
        }

        // 再收金币
        for (int i = takenGoldCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = takenGoldCoins[i];
            int idx = goldCoins.IndexOf(coin);
            if (idx >= 0)
            {
                seq.Insert(delay, coin.transform.DOMove(goldCoinsOriginalPos[idx], resetOnePieceTime));
            }
            delay += resetPieceDelay;
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }
    #endregion

    #region 落子

    private void OnPlayerClickCell(GridCell cell)
    {
        if (!isPlayerTurn || _gameOver || _matchOver) return;
        if (cell.occupant != CellOccupant.None)
        {
            Debug.Log("格子被占用");
            return;
        }

        cell.occupant = CellOccupant.X;
        if (xIndex < xPieces.Count)
        {
            var piece = xPieces[xIndex];
            xIndex++;
            piece.SetActive(true);

            ParabolaDrop drop = piece.GetComponent<ParabolaDrop>();
            if (drop != null)
                drop.DoParabolaDrop(piece.transform.position, cell.transform.position);
            else
                piece.transform.position = cell.transform.position;

            placedPieces.Add(piece);
        }

        if (CheckWin(CellOccupant.X))
        {
            Debug.Log($"玩家赢第{currentRound}局");
            playerWins++;
            // 当场拿金币给玩家
            TakeGoldCoin(true);
            _gameOver = true;
        }
        else if (CheckDraw())
        {
            Debug.Log("平局");
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

        var bestCell = aiController.GetBestMove(board.allCells);
        if (bestCell != null)
        {
            bestCell.occupant = CellOccupant.O;

            if (oIndex < oPieces.Count)
            {
                var piece = oPieces[oIndex];
                oIndex++;
                piece.SetActive(true);

                ParabolaDrop drop = piece.GetComponent<ParabolaDrop>();
                if (drop != null)
                    drop.DoParabolaDrop(piece.transform.position, bestCell.transform.position);
                else
                    piece.transform.position = bestCell.transform.position;

                placedPieces.Add(piece);
            }

            if (CheckWin(CellOccupant.O))
            {
                Debug.Log($"AI赢第{currentRound}局");
                aiWins++;
                // AI拿金币
                TakeGoldCoin(false);
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
            Debug.Log("AI无可下之处, 平局?");
            _gameOver = true;
        }

        turnSequence.Play();
    }

    #endregion

    #region 小局胜出时拿金币
    /// <summary>
    /// 取下一枚金币给胜利方，把它抛到对应的奖杯位置，并根据胜利局数做位置偏移
    /// </summary>
    /// <param name="isPlayer">true = 玩家，false = AI</param>
    private void TakeGoldCoin(bool isPlayer)
    {
        //如果金币都拿完了，就退出
        if (goldCoinIndex >= goldCoins.Count)
        {
            Debug.LogWarning("金币不足，无法再拿金币！");
            return;
        }

        // 拿下一枚金币
        GameObject coin = goldCoins[goldCoinIndex];
        goldCoinIndex++;  // 下标递增
        coin.SetActive(true);  // 显示

        int winsCount = isPlayer ? playerWins : aiWins;
        // 对应奖杯点
        Transform trophyPos = isPlayer ? playerTrophyPos : aiTrophyPos;
        
        int offsetIndex = winsCount - 1;

        // 计算“奖杯位置 + 偏移”
        Vector3 finalPos = trophyPos.position + coinOffsetPerCoin * offsetIndex;

        // 做抛物线动画
        ParabolaDrop drop = coin.GetComponent<ParabolaDrop>();
        if (drop != null)
        {

            drop.DoParabolaDrop(coin.transform.position, finalPos);
        }
        else
        {
            // 如果没抛物线脚本，就直线Tween
            coin.transform.DOMove(finalPos, 1f).SetEase(Ease.OutQuad);
        }

        Debug.Log($"给{(isPlayer ? "玩家" : "AI")}放第 {winsCount} 枚金币 (索引:{offsetIndex}) 到 {finalPos}");
    }


    #endregion

    #region 胜负判定
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
