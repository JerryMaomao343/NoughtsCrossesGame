using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseBoard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Trigger()
    {
        EventCenter.Instance.Broadcast(GameEvent.ReturnToMainMenu);
    }
}
