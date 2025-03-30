using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDebugLog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventCenter.Instance.AddListener(GameEvent.OnStartGame, OnStartGame);
        EventCenter.Instance.AddListener(GameEvent.OnEndGame, OnEndGame);
        EventCenter.Instance.AddListener(GameEvent.OnPlayerRound, OnPlayerRound);
        EventCenter.Instance.AddListener(GameEvent.OnPlayerPlace, OnPlayerPlace);
        EventCenter.Instance.AddListener(GameEvent.OnAIRound, OnAIRound);
        EventCenter.Instance.AddListener(GameEvent.OnAIPlace, OnAIPlace);
    }

    // Update is called once per frame
    private void OnStartGame(object[] args)
    {
        Debug.Log("游戏开始");
    }

    private void OnEndGame(object[] args)
    {
        Debug.Log("游戏结束");
    }

    private void OnPlayerRound(object[] args)
    {
        Debug.Log("轮到玩家回合");
    }

    private void OnPlayerPlace(object[] args)
    {
        Debug.Log("玩家落子");
    }

    private void OnAIRound(object[] args)
    {
        Debug.Log("轮到AI回合");
    }
    private void OnAIPlace(object[] args)
    {
        Debug.Log("AI落子");
    }
}
