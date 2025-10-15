using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EventCallBack : MonoBehaviour
{

    public static EventCallBack _instance;

    private  Dictionary<BugType, Action> BugEvent = new Dictionary<BugType, Action>();
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            
        }else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);   
        }
    }

    
    private void AddBugEvent(BugType type, Action action)
    {
        if (BugEvent.ContainsKey(type))
        {
            BugEvent[type] += action;
        }
        else
        {
            BugEvent[type] = action;
        }
    }

    public void CalBackEvent(BugType type)
    {
        BugEvent[type]?.Invoke();
    }
    
    public void SubscribeToBug(BugType type, Action action)
    {
        AddBugEvent(type, action);
    }

    public void UnsubscribeFromBug(BugType type, Action action)
    {
        if (BugEvent.ContainsKey(type))
        {
            BugEvent[type] -= action;
        }
    }
}
