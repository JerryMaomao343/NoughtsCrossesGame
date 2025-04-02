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
        
    }

    // 事件回调方法，根据不同事件播放对应声音
    private void OnStartGame(object[] args)
    {
        PlaySound(startGameClip);
    }

    private void OnEndGame(object[] args)
    {
        PlaySound(endGameClip);
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
        PlaySound(clickBoard);
    }

    private void OnShowText(object[] args)
    {
        PlaySound(showText);
    }
    

    /// <summary>
    /// 根据传入的 AudioClip 播放声音（使用 PlayOneShot）
    /// </summary>
    /// <param name="clip">要播放的 AudioClip</param>
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

