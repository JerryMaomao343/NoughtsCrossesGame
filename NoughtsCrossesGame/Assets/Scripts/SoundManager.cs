using DG.Tweening;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    // 用于播放声音的 AudioSource 组件
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip startGameClip;   // 开始游戏时播放
    public AudioClip endGameClip;     // 结束游戏时播放
    public AudioClip playerPlaceClip; // 玩家落子时播放
    public AudioClip aiPlaceClip;     // AI落子时播放
    public AudioClip buttonSelect;
    public AudioClip buttonClick;
    public AudioClip clickBoard;
    public AudioClip showText;
    public AudioClip pieceHit;
    public AudioClip pieceLift;
    public AudioClip coinHit;
    public AudioClip coinLift;
    private Camera _camera;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        _camera = Camera.main;
        
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddListener(GameEvent.OnStartGame, OnStartGame);
        EventCenter.Instance.AddListener(GameEvent.OnEndGame, OnEndGame);
        EventCenter.Instance.AddListener(GameEvent.OnPlayerPlace, OnPlayerPlace);
        EventCenter.Instance.AddListener(GameEvent.OnAIPlace, OnAIPlace);
        EventCenter.Instance.AddListener(GameEvent.OnButtonSelect, OnBtnSelect);
        EventCenter.Instance.AddListener(GameEvent.OnButtonClick, OnBtnClick);
        EventCenter.Instance.AddListener(GameEvent.SelectBoard, OnBtnSelect);
        EventCenter.Instance.AddListener(GameEvent.ClickBoard, OnClickBoard);
        EventCenter.Instance.AddListener(GameEvent.OnShowText, OnShowText);
        EventCenter.Instance.AddListener(GameEvent.OnPieceHit, OnPieceHit);
        EventCenter.Instance.AddListener(GameEvent.OnPieceLift, OnPieceLift);
        EventCenter.Instance.AddListener(GameEvent.OnCoinHit, OnCoinHit);
        EventCenter.Instance.AddListener(GameEvent.OnCoinLift, OnCoinLift);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener(GameEvent.OnStartGame, OnStartGame);
        EventCenter.Instance.RemoveListener(GameEvent.OnEndGame, OnEndGame);
        EventCenter.Instance.RemoveListener(GameEvent.OnPlayerPlace, OnPlayerPlace);
        EventCenter.Instance.RemoveListener(GameEvent.OnButtonSelect, OnBtnSelect);
        EventCenter.Instance.RemoveListener(GameEvent.OnButtonClick, OnBtnClick);
        EventCenter.Instance.RemoveListener(GameEvent.SelectBoard, OnBtnSelect);
        EventCenter.Instance.RemoveListener(GameEvent.ClickBoard, OnShowText);
        EventCenter.Instance.RemoveListener(GameEvent.OnPieceHit, OnPieceHit);
        EventCenter.Instance.RemoveListener(GameEvent.OnPieceLift, OnPieceLift);
        EventCenter.Instance.RemoveListener(GameEvent.OnCoinHit, OnCoinHit);
        EventCenter.Instance.RemoveListener(GameEvent.OnCoinLift, OnCoinLift);
        
    }

    // 事件回调方法，根据不同事件播放对应声音
    private void OnStartGame(object[] args)
    {
        PlaySound(startGameClip);
        _camera.DOShakePosition(0.1f, 0.05f);
    }

    private void OnEndGame(object[] args)
    {
        PlaySound(endGameClip);
        _camera.DOShakePosition(0.1f, 0.05f);
    }

    private void OnPlayerPlace(object[] args)
    {
        PlaySound(playerPlaceClip);
    }

    private void OnAIPlace(object[] args)
    {
        PlaySound(aiPlaceClip);
    }

    private void OnBtnSelect(object[] args)
    {
        PlaySound(buttonSelect);
    }
    
    private void OnBtnClick(object[] args)
    {
        PlaySound(buttonClick);
    }

    private void OnClickBoard(object[] args)
    {
        //PlaySound(clickBoard);
    }

    private void OnShowText(object[] args)
    {
        PlaySound(showText);
    }    
    private void OnPieceHit(object[] args)
    {
        PlaySound(pieceHit);
        
    }
    private void OnPieceLift(object[] args)
    {
        PlaySound(pieceLift);
    }
    private void OnCoinHit(object[] args)
    {
        PlaySound(coinHit);
    }
    private void OnCoinLift(object[] args)
    {
        PlaySound(coinLift);
        _camera.DOShakePosition(0.1f, 0.04f);
    }


    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

