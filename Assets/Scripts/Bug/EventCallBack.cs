using System.Collections.Generic;
using UnityEngine;

public class EventCallBack : MonoBehaviour
{
    public static EventCallBack _instance;

    public delegate void BugEventHandler<T>(out bool flag,float time,T data);
    
    private Dictionary<BugType, object> PlayerBugEvent = new Dictionary<BugType, object>();
    
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void AddBugEvent<T>(BugType type, BugEventHandler<T> action)
    {
        if (PlayerBugEvent.ContainsKey(type))
        {

            if (PlayerBugEvent[type] is BugEventHandler<T> existingAction)
            {
                PlayerBugEvent[type] = (BugEventHandler<T>)existingAction + action;
            }
        }
        else
        {
            PlayerBugEvent[type] = action;
        }
    }

    public void CalBackEvent<T>(BugType type, out bool fl,float time, T Data)
    {
        bool Fl = false;
        
        if (PlayerBugEvent.ContainsKey(type) && PlayerBugEvent[type] is BugEventHandler<T> handler)
        {
            handler(out Fl,time, Data);
        }
        
        fl = Fl;
    }
    
    public void SubscribeToBug<T>(BugType type, BugEventHandler<T> action)
    {
        AddBugEvent(type, action);
    }

    public void UnsubscribeFromBug<T>(BugType type, BugEventHandler<T> action)
    {
        if (PlayerBugEvent.ContainsKey(type))
        {
            if (PlayerBugEvent[type] is BugEventHandler<T> existingAction)
            {
                var newAction = (BugEventHandler<T>)existingAction - action;
                if (newAction != null)
                {
                    PlayerBugEvent[type] = newAction;
                }
                else
                {
                    PlayerBugEvent.Remove(type);
                }
            }
        }
    }
}