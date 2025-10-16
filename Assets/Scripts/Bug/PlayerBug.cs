using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBug : MonoBehaviour
{
    void Start()
    {
        RegisterEvent();
    }

    private void OnDestroy()
    {
        DeleteEvent();
    }

     void RegisterEvent()
    {

        EventCallBack._instance.SubscribeToBug(BugType.Stack, StackEvent);
        EventCallBack._instance.SubscribeToBug(BugType.Heap, HeaderEvent);
        EventCallBack._instance.SubscribeToBug(BugType.Null, NullEvent);
        EventCallBack._instance.SubscribeToBug(BugType.Undefined, UndefinedEvent);
        EventCallBack._instance.SubscribeToBug(BugType.ArgumentOut, ArgumentOutEvent);
        EventCallBack._instance.SubscribeToBug(BugType.Missing, MissingEvent);
    }

     void DeleteEvent()
    {

        EventCallBack._instance.UnsubscribeFromBug(BugType.Stack, StackEvent);
        EventCallBack._instance.UnsubscribeFromBug(BugType.Heap, HeaderEvent);
        EventCallBack._instance.UnsubscribeFromBug(BugType.Null, NullEvent);
        EventCallBack._instance.UnsubscribeFromBug(BugType.Undefined, UndefinedEvent);
        EventCallBack._instance.UnsubscribeFromBug(BugType.ArgumentOut, ArgumentOutEvent);
        EventCallBack._instance.UnsubscribeFromBug(BugType.Missing, MissingEvent);
    }

    void StackEvent()
    {
        Debug.Log("Stack is Trigger");
    }   
    
    void HeaderEvent()
    {
        Debug.Log("Header is Trigger");
    }    
    
    void NullEvent()
    {
        Debug.Log("Null is Trigger");
    }   
    
    void UndefinedEvent()
    {
        Debug.Log("Undefined is Trigger");
    }   
    
    void ArgumentOutEvent()
    {
        Debug.Log("ArgumentOut is Trigger");
    }    
    
    void MissingEvent()
    {
        Debug.Log("Missing is Trigger");
    }
}
