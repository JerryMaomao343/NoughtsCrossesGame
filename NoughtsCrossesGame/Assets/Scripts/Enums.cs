using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏事件
/// </summary>
public enum GameEvent
{ 
    OnStartGame, //开始游戏 
    OnEndGame, //结束游戏
    
    OnFinishEnterAni,//进入对局动画播完
    OnFinishExitAni,//退出对局动画播完
    
    OnPlayerRound, //开始玩家回合
    OnAIRound, //开始AI回合
    OnPlayerPlace, //玩家点击落子
    OnAIPlace, //AI落子
}

public enum ButtonType
{
    Start,
    Exit
}


