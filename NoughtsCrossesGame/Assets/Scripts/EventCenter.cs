using System;
using System.Collections.Generic;

public class EventCenter
{
    static EventCenter instance;
    public static EventCenter Instance 
    {
        get
        {
            if (instance == null) instance = new EventCenter();
            return instance;
        }
    }

    Dictionary<GameEvent, Action<object[]>> eventDict = new Dictionary<GameEvent, Action<object[]>>();

    public void AddListener(GameEvent evt, Action<object[]> callback)
    {
        if (!eventDict.ContainsKey(evt))
            eventDict[evt] = null;
        eventDict[evt] += callback;
    }

    public void RemoveListener(GameEvent evt, Action<object[]> callback)
    {
        if (eventDict.ContainsKey(evt))
        {
            eventDict[evt] -= callback;
            if (eventDict[evt] == null)
                eventDict.Remove(evt);
        }
    }

    public void Broadcast(GameEvent evt, params object[] args)
    {
        if (eventDict.ContainsKey(evt))
            eventDict[evt]?.Invoke(args);
    }
}