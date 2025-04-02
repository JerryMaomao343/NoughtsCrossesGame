using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AniController : MonoBehaviour
{
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        EventCenter.Instance.AddListener(GameEvent.OnStartGame,PlayLaugh);
        EventCenter.Instance.AddListener(GameEvent.OnEndGame,PlayEnd);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener(GameEvent.OnStartGame,PlayLaugh);
        EventCenter.Instance.RemoveListener(GameEvent.OnEndGame,PlayEnd);
    }

    void PlayLaugh(object[] args)
    {
        _animator.CrossFade("Laugh",0.03f);
    }
    void PlayEnd(object[] args)
    {
        _animator.CrossFade("Lose",0.03f);
    }
}
