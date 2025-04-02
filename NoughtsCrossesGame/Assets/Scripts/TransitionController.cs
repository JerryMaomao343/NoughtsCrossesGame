using UnityEngine;
using DG.Tweening;

public class TransitionController : MonoBehaviour
{
    public GameObject startBoard;
    public GameObject chessBoard;
    public GameObject pauseBoard;

    private Vector3 _startBoardStartRotate;
    private Vector3 _startBoardStartPos;
    private Vector3 _chessBoardStartPos;
    private Vector3 _pauseBoardStartPos;
    private Vector3 _pauseBoardStartRotate;
    private Vector3 _startBoardEndPos = new Vector3(12.4f, 4.2f, -4.72f);
    private Vector3 _startBoardEndRotate = new Vector3(0, 0, -90f);
    private Vector3 _pauseBoardEndRotate = new Vector3(-35, 0, 0);
    private Vector3 _chessBoardEndPos = new Vector3(0, 0.07f, 0);
    private Vector3 _pauseBoardEndPos = new Vector3(-0.3f,-0.22f, -5.3f);
    private void OnEnable()
    {
        _startBoardStartPos = startBoard.transform.position;
        _startBoardStartRotate = startBoard.transform.localRotation.eulerAngles;
        _pauseBoardStartRotate = pauseBoard.transform.localRotation.eulerAngles;
        _chessBoardStartPos = chessBoard.transform.position;
        _pauseBoardStartPos = pauseBoard.transform.position;
        
        EventCenter.Instance.AddListener(GameEvent.OnStartGame, OnGameStart);
        EventCenter.Instance.AddListener(GameEvent.OnEndGame, OnGameEnd);
        EventCenter.Instance.AddListener(GameEvent.EnterPause, OnPauseShow);
        EventCenter.Instance.AddListener(GameEvent.ExitPause, OnPauseHide);
        //EventCenter.Instance.AddListener(GameEvent.ReturnToMainMenu, OnPauseHide);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener(GameEvent.OnStartGame, OnGameStart);
        EventCenter.Instance.RemoveListener(GameEvent.OnEndGame, OnGameEnd);
        EventCenter.Instance.RemoveListener(GameEvent.EnterPause, OnPauseShow);
        EventCenter.Instance.RemoveListener(GameEvent.ExitPause, OnPauseHide);
        //EventCenter.Instance.RemoveListener(GameEvent.ReturnToMainMenu, OnPauseHide);
    }

    /// <summary>
    /// 进入游戏动画脚本
    /// </summary>
    private void OnGameStart(object[] args)
    {
        Debug.Log("[TransitionController] 收到 GameStart，播放对局进入动画");
        Sequence enterSequence = DOTween.Sequence();
        enterSequence.AppendInterval(0.2f);
        //界面黑板移出
        enterSequence.Append(startBoard.transform.DOMove(_startBoardEndPos, 0.5f));
        enterSequence.Join(startBoard.transform.DOLocalRotate(_startBoardEndRotate, 0.5f));
        //棋盘抬升
        enterSequence.Join(chessBoard.transform.DOMove(_chessBoardEndPos,0.3f));
        
        enterSequence.AppendCallback(() =>
        {
            Debug.Log("[TransitionController] 对局进入动画播放完毕");
            EventCenter.Instance.Broadcast(GameEvent.OnFinishEnterAni);
        });
    }

    /// <summary>
    /// 当游戏结束或玩家点击退出，对局结束动画
    /// </summary>
    private void OnGameEnd(object[] args)
    {
        Debug.Log("[TransitionController] 收到 GameEnd，播放对局退出动画");
        GameManager.Instance.isOnGame = false;
        Sequence exitSequence = DOTween.Sequence();

        if (!GameManager.Instance.isOnGame)
        {
            exitSequence.Append(pauseBoard.transform.DOMove(_pauseBoardStartPos, 0.5f));
            exitSequence.Join(pauseBoard.transform.DOLocalRotate(_pauseBoardStartRotate, 0.5f));
        }
        
        //界面黑板移入
        exitSequence.Append(startBoard.transform.DOMove(_startBoardStartPos, 0.5f));
        exitSequence.Join(startBoard.transform.DOLocalRotate(_startBoardStartRotate, 0.5f));
        exitSequence.AppendCallback(() => { EventCenter.Instance.Broadcast(GameEvent.OnFinishExitAni); });
        //棋盘下降
        exitSequence.Join(chessBoard.transform.DOMove(_chessBoardStartPos,0.3f));
        exitSequence.AppendCallback(() =>
        {
            Debug.Log("[TransitionController] 对局退出动画播放完毕");
            // 退出动画结束后，广播 OnFinishExitAni
        });
        
    }
    
    private void OnPauseHide(object[] args)
    {
        Debug.Log("[TransitionController] 结束暂停");
        GameManager.Instance.isOnGame = true;
        Sequence exitSequence = DOTween.Sequence();
        //界面黑板归位
        exitSequence.Append(pauseBoard.transform.DOMove(_pauseBoardStartPos, 0.5f));
        exitSequence.Join(pauseBoard.transform.DOLocalRotate(_pauseBoardStartRotate, 0.5f));
        
    }
    
    private void OnPauseShow(object[] args)
    {
        Debug.Log("[TransitionController] 开始暂停");

        Sequence exitSequence = DOTween.Sequence();
        //界面黑板归位
        exitSequence.Append(pauseBoard.transform.DOMove(_pauseBoardEndPos, 0.5f));
        exitSequence.Join(pauseBoard.transform.DOLocalRotate(_pauseBoardEndRotate, 0.5f)).OnComplete(() =>
        {
            GameManager.Instance.isOnGame = false;
        });
        
    }
}