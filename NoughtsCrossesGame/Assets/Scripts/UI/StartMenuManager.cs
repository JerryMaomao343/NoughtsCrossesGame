using UnityEngine;
using System.Collections.Generic;

public class StartMenuManager : MonoBehaviour
{
    public static StartMenuManager Instance;

    [Header("Difficulty Stars")]
    public List<DifficultStar> stars;

    [Header("Game Settings")]
    public int currentDifficulty = 1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 当鼠标悬浮到某个星星时调用，更新所有星星的填充状态
    /// </summary>
    /// <param name="hoveredIndex">当前悬浮的星星编号</param>
    public void UpdateStarSelection(int hoveredIndex)
    {
        foreach (DifficultStar star in stars)
        {
            if (star.starIndex <= hoveredIndex)
                star.SetFilled(true);
            else
                star.SetFilled(false);
        }
    }

    /// <summary>
    /// 当点击星星时调用，确定当前难度
    /// </summary>
    /// <param name="difficulty">点击的星星编号</param>
    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
        GameManager.Instance.transform.GetComponent<AIController>().ChangeDifficult(currentDifficulty);
        Debug.Log("选中难度星星数量：" + currentDifficulty);
    }

    /// <summary>
    /// 开始游戏接口
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance.NewMatch();
        EventCenter.Instance.Broadcast(GameEvent.OnStartGame);
    }

    /// <summary>
    /// 退出游戏接口
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
    
    public void PauseExitGame()
    {
        Debug.Log("返回主界面");
        EventCenter.Instance.Broadcast(GameEvent.ReturnToMainMenu);
    }
}