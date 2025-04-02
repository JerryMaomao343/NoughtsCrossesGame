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
    private int xIndex=0, oIndex=0;

    [Header("Coins (From Table)")]
    public List<GameObject> goldCoins;          // 场景中摆好的金币
    private Vector3[] goldCoinsOriginalPos;
    private int goldCoinIndex=0;
    private List<GameObject> takenGoldCoins = new List<GameObject>();

    [Header("Coin/Trophy Settings")]
    public Transform playerTrophyPos;
    public Transform aiTrophyPos;
    public Vector3 coinOffsetPerCoin = new Vector3(0,0,0.8f);

    [Header("Animation / Timing")]
    public float afterPlayerPlaceDelay = 1f;
    public float afterAIPlaceDelay     = 1f;
    public float resetOnePieceTime     = 0.3f;
    public float resetPieceDelay       = 0.1f;

    // 五局三胜
    private int playerWins=0;
    private int aiWins=0;
    private int currentRound=1;
    private const int MAX_ROUNDS=5;

    // 局 / 场 结束标记
    private bool _gameOver   = false; // 当前局结束
    private bool _matchOver  = false; // 整场结束

    // 记录本局落下的棋子(以便局末收回)
    private List<GameObject> placedPieces = new List<GameObject>();

    // 记录“谁赢了本局” (X, O, 或者 None=平局)
    private CellOccupant occupantWinner = CellOccupant.None;

    // 大队列 Sequence
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
        // 记录X/O初始坐标
        xOriginalPos = new Vector3[xPieces.Count];
        for(int i=0; i<xPieces.Count; i++)
            xOriginalPos[i] = xPieces[i].transform.position;

        oOriginalPos = new Vector3[oPieces.Count];
        for(int i=0; i<oPieces.Count; i++)
            oOriginalPos[i] = oPieces[i].transform.position;

        // occupant=none
        foreach(var c in board.allCells)
            c.occupant = CellOccupant.None;

        // AI设置
        aiController.aiOccupant = CellOccupant.O;
        aiController.opponentOccupant = CellOccupant.X;

        // 金币
        goldCoinsOriginalPos = new Vector3[goldCoins.Count];
        for(int i=0; i<goldCoins.Count; i++)
            goldCoinsOriginalPos[i] = goldCoins[i].transform.position;

        BuildTurnSequence();
    }

    public void NewMatch()
    {
        // 重置比赛数据
        playerWins = 0; 
        aiWins     = 0; 
        currentRound = 1;
        _gameOver  = false; 
        _matchOver = false; 
        occupantWinner = CellOccupant.None;
        isPlayerTurn = false;

        // 清空 occupant
        foreach(var cell in board.allCells)
            cell.occupant = CellOccupant.None;

        // 收回X/O
        placedPieces.Clear();
        xIndex=0; 
        oIndex=0;
        for(int i=0;i<xPieces.Count;i++)
            xPieces[i].transform.position = xOriginalPos[i];
        for(int i=0;i<oPieces.Count;i++)
            oPieces[i].transform.position = oOriginalPos[i];

        // 金币复位
        takenGoldCoins.Clear();
        goldCoinIndex=0;
        for(int i=0; i<goldCoins.Count; i++)
            goldCoins[i].transform.position= goldCoinsOriginalPos[i];

        BuildTurnSequence();
    }

    private void OnFinishEnterAni(object[] args)
    {
        Debug.Log("进入对局动画结束 => 开始主序列");
        UITextPanel panel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"), $"第{NumberToChinese(currentRound)}回合");
        UIManager.Instance.CloseUIAfter(panel,1f);
        turnSequence.Play();
    }
    private void OnFinishExitAni(object[] args)
    {
        Debug.Log("退出对局动画结束 => 返回主菜单或其它操作");
    }

    #region 构建回合队列
    private void BuildTurnSequence()
    {
        if(turnSequence != null && turnSequence.IsActive())
            turnSequence.Kill();

        turnSequence = DOTween.Sequence().SetAutoKill(false).Pause();

        // Step1: PlayerRound
        turnSequence.AppendCallback(() => Step_PlayerRound());
        turnSequence.AppendInterval(afterPlayerPlaceDelay);

        // Step2: AIRound
        turnSequence.AppendCallback(() => Step_AIRound());
        turnSequence.AppendInterval(afterAIPlaceDelay);

        // Step3: CheckVictory
        turnSequence.AppendCallback(() => Step_CheckVictory());
        turnSequence.AppendInterval(0.1f);

        // Step4: 循环 or NextRound
        turnSequence.AppendCallback(() =>
        {
            if(!_matchOver && !_gameOver)
            {
                // 本局还没结束 => 回到 Step1
                turnSequence.Goto(0,true);
            }
            else if(!_matchOver && _gameOver)
            {
                // 本局结束,但比赛还没结束 => Step_NextRound
                Step_NextRound();
            }
        });
    }
    #endregion

    #region 回合步骤
    private void Step_PlayerRound()
    {
        if(_matchOver || _gameOver) return; 
        Debug.Log($"[Step_PlayerRound] 回合 {currentRound}, 轮到玩家下子");
        isPlayerTurn = true;

        // 暂停Sequence => 等玩家点击
        turnSequence.Pause();
    }

    private void Step_AIRound()
    {
        if(_matchOver || _gameOver) return;
        Debug.Log($"[Step_AIRound] 回合 {currentRound}, AI回合");
        isPlayerTurn = false;

        turnSequence.Pause();
        Invoke(nameof(HandleAIPlace), 0.5f); 
        // AI思考0.5秒后再落子
    }

    private void Step_CheckVictory()
    {
        // 如果本局结束 => occupantWinner 可能= X/O/None
        if(!_gameOver) return;
        Debug.Log($"[Step_CheckVictory] 回合{currentRound}结束, 获胜者：{occupantWinner}");

        // 暂停主序列
        turnSequence.Pause();

        // 建子序列
        Sequence subSeq = DOTween.Sequence();
        UITextPanel uiTextPanel = null;

        //显示UI (可根据 occupantWinner 判断是谁赢)
        subSeq.AppendCallback(() =>
        {
            isPlayerTurn=false; // 禁止玩家点击
            if(occupantWinner == CellOccupant.X)
            {
                Debug.Log("显示UI: 玩家胜利");
                uiTextPanel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"),"您获胜");
            }
            else if(occupantWinner == CellOccupant.O)
            {
                Debug.Log("显示UI: AI胜利");
                uiTextPanel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"),"对手获胜");
            }
            else
            {
                Debug.Log("显示UI: 平局");
                uiTextPanel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"),"平局");
            }
        });
        //等2秒
        subSeq.AppendInterval(2f);

        //若有人赢 => 发金币
        subSeq.AppendCallback(() =>
        {
            if(occupantWinner == CellOccupant.X)
            {
                Debug.Log("现在才真正发金币给玩家");
                TakeGoldCoin(true);
            }
            else if(occupantWinner == CellOccupant.O)
            {
                Debug.Log("现在才真正发金币给AI");
                TakeGoldCoin(false);
            }
        });

        //关闭UI
        subSeq.AppendCallback(() =>
        {
            Debug.Log("关闭胜利UI");
            UIManager.Instance.CloseUI(uiTextPanel);
        });
        //再等1秒
        subSeq.AppendInterval(1f);

        //子序列结束 => 恢复主序列
        subSeq.OnComplete(() =>
        {
            turnSequence.Play();
        });

        // 播放子序列
        subSeq.Play();
    }

    private void Step_NextRound()
    {
        Debug.Log($"[Step_NextRound] 单局结束 => pWins={playerWins}, aiWins={aiWins}");
        if(playerWins>=3 || aiWins>=3 || currentRound>=MAX_ROUNDS)
        {
            _matchOver=true;
            Debug.Log("[Step_NextRound] 整场结束");
            string result = "";
            if (playerWins>aiWins)
            {
                result = "您获胜了";
            }
            else if (playerWins<aiWins)
            {
                result = "您失败了";
            }
            else
            {
                result = "平局";
            }
            
            
            UITextPanel panel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"), $"游戏结束，{result}");
            UIManager.Instance.CloseUIAfter(panel,2f);
            ReverseReturnAllPieces(() =>
            {
                EventCenter.Instance.Broadcast(GameEvent.OnEndGame);
            });
        }
        else
        {
            currentRound++;
            Debug.Log($"[Step_NextRound] 开始回合 {currentRound}");
            UITextPanel panel = UIManager.Instance.ShowUI<UITextPanel>(Resources.Load<GameObject>("UITextPanel"), $"第{NumberToChinese(currentRound)}回合");
            UIManager.Instance.CloseUIAfter(panel,2f);
            // 只收回X/O, 保留金币
            ReverseReturnPieces(() =>
            {
                placedPieces.Clear();
                xIndex=0; 
                oIndex=0;
                foreach(var c in board.allCells)
                    c.occupant=CellOccupant.None;
                _gameOver=false;
                occupantWinner=CellOccupant.None;

                // 回到Step1
                turnSequence.Goto(0,false);
                turnSequence.Play();
            });
        }
    }
    #endregion

    #region 双方落子(不再调用TakeGoldCoin)
    private void OnPlayerClickCell(GridCell cell)
    {
        if(!isPlayerTurn || _gameOver || _matchOver) return;
        if(cell.occupant!=CellOccupant.None)
        {
            Debug.Log("该格子已被占用");
            return;
        }

        cell.occupant=CellOccupant.X;
        if(xIndex<xPieces.Count)
        {
            var piece=xPieces[xIndex];
            xIndex++;
            piece.SetActive(true);

            var drop= piece.GetComponent<ParabolaDrop>();
            drop?.DoParabolaDrop(piece.transform.position, cell.transform.position);

            placedPieces.Add(piece);
        }

        // 判胜
        if(CheckWin(CellOccupant.X))
        {
            Debug.Log($"玩家赢 第{currentRound}局");
            occupantWinner = CellOccupant.X;  // 只记录, 不发金币
            playerWins++;
            _gameOver=true;
        }
        else if(CheckDraw())
        {
            Debug.Log("平局");
            occupantWinner = CellOccupant.None; // 表示平局
            _gameOver=true;
        }

        isPlayerTurn=false;
        // 恢复主序列
        turnSequence.Play();
    }

    private void HandleAIPlace()
    {
        if(_gameOver||_matchOver)
        {
            turnSequence.Play();
            return;
        }

        var bestCell = aiController.GetBestMove(board.allCells);
        if(bestCell!=null)
        {
            bestCell.occupant=CellOccupant.O;

            if(oIndex<oPieces.Count)
            {
                var piece=oPieces[oIndex];
                oIndex++;
                piece.SetActive(true);

                var drop= piece.GetComponent<ParabolaDrop>();
                drop?.DoParabolaDrop(piece.transform.position, bestCell.transform.position);

                placedPieces.Add(piece);
            }

            if(CheckWin(CellOccupant.O))
            {
                Debug.Log($"AI赢 第{currentRound}局");
                occupantWinner = CellOccupant.O; // 仅记录
                aiWins++;
                _gameOver=true;
            }
            else if(CheckDraw())
            {
                Debug.Log("平局");
                occupantWinner=CellOccupant.None;
                _gameOver=true;
            }
        }
        else
        {
            Debug.Log("AI无可下之处 =>平局");
            occupantWinner=CellOccupant.None;
            _gameOver=true;
        }

        turnSequence.Play();
    }
    #endregion

    #region 发金币(仅在Step_CheckVictory时)
    private void TakeGoldCoin(bool isPlayer)
    {
        if(goldCoinIndex>=goldCoins.Count)
        {
            Debug.LogWarning("金币不足!");
            return;
        }
        GameObject coin= goldCoins[goldCoinIndex];
        goldCoinIndex++;
        takenGoldCoins.Add(coin);

        // 根据该方已赢多少次 => 偏移
        int winsCount= isPlayer? playerWins: aiWins;
        int offsetIndex= winsCount-1;
        Transform trophyPos= isPlayer? playerTrophyPos: aiTrophyPos;
        Vector3 finalPos= trophyPos.position + coinOffsetPerCoin*offsetIndex;

        coin.SetActive(true);
        var drop= coin.GetComponent<ParabolaDrop>();
        if(drop!=null)
        {
            drop.DoParabolaDrop(coin.transform.position, finalPos);
        }
        else
        {
            coin.transform.DOMove(finalPos,1f).SetEase(Ease.OutQuad);
        }
    }
    #endregion

    #region 收回
    private void ReverseReturnPieces(TweenCallback onComplete)
    {
        Sequence seq= DOTween.Sequence();
        float delay=0f;
        for(int i=placedPieces.Count-1;i>=0;i--)
        {
            var piece=placedPieces[i];
            int idxX=xPieces.IndexOf(piece);
            if(idxX>=0)
            {
                seq.Insert(delay,
                    piece.transform.DOMove(xOriginalPos[idxX], resetOnePieceTime));
            }
            else
            {
                int idxO=oPieces.IndexOf(piece);
                if(idxO>=0)
                {
                    seq.Insert(delay,
                        piece.transform.DOMove(oOriginalPos[idxO], resetOnePieceTime));
                }
            }
            delay+=resetPieceDelay;
        }
        seq.OnComplete(()=> onComplete?.Invoke());
    }

    private void ReverseReturnAllPieces(TweenCallback onComplete)
    {
        Sequence seq= DOTween.Sequence();
        float delay=0f;
        // 收X/O
        for(int i=placedPieces.Count-1;i>=0;i--)
        {
            var piece=placedPieces[i];
            int idxX=xPieces.IndexOf(piece);
            if(idxX>=0)
            {
                seq.Insert(delay,
                    piece.transform.DOMove(xOriginalPos[idxX], resetOnePieceTime));
            }
            else
            {
                int idxO=oPieces.IndexOf(piece);
                if(idxO>=0)
                {
                    seq.Insert(delay,
                        piece.transform.DOMove(oOriginalPos[idxO], resetOnePieceTime));
                }
            }
            delay+=resetPieceDelay;
        }
        // 再收金币
        for(int i=takenGoldCoins.Count-1;i>=0;i--)
        {
            var coin= takenGoldCoins[i];
            int idx= goldCoins.IndexOf(coin);
            if(idx>=0)
            {
                seq.Insert(delay,
                    coin.transform.DOMove(goldCoinsOriginalPos[idx], resetOnePieceTime));
            }
            delay+=resetPieceDelay;
        }
        seq.OnComplete(()=> onComplete?.Invoke());
    }
    #endregion

    #region 判定
    private bool CheckWin(CellOccupant occupant)
    {
        CellOccupant GetOcc(int r,int c)
        {
            var cell= board.allCells.Find(x=> x.cellIndex.x==r && x.cellIndex.y==c);
            return cell!=null? cell.occupant: CellOccupant.None;
        }
        for(int i=0;i<3;i++)
        {
            if(GetOcc(i,0)== occupant && GetOcc(i,1)== occupant && GetOcc(i,2)== occupant) return true;
            if(GetOcc(0,i)== occupant && GetOcc(1,i)== occupant && GetOcc(2,i)== occupant) return true;
        }
        if(GetOcc(0,0)== occupant && GetOcc(1,1)== occupant && GetOcc(2,2)== occupant) return true;
        if(GetOcc(0,2)== occupant && GetOcc(1,1)== occupant && GetOcc(2,0)== occupant) return true;
        return false;
    }

    private bool CheckDraw()
    {
        foreach(var c in board.allCells)
        {
            if(c.occupant==CellOccupant.None) return false;
        }
        return true;
    }
    #endregion
    
    public static string NumberToChinese(int num)
    {
        switch (num)
        {
            case 1: return "一";
            case 2: return "二";
            case 3: return "三";
            case 4: return "四";
            case 5: return "五";
            default: return num.ToString();
        }
    }
}


