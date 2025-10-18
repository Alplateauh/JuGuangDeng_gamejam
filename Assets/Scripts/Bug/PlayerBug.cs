using UnityEngine;
using DG.Tweening;
public class PlayerBug : MonoBehaviour
{
    private Rigidbody rb;
    public float ForceMag;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        RegisterEvent();
    }

    private void OnDestroy()
    {
        DeleteEvent();
    }

     void RegisterEvent()
    {

        EventCallBack._instance.SubscribeToBug<Vector2>(BugType.Stack, StackEvent);
        EventCallBack._instance.SubscribeToBug<Vector2>(BugType.Heap, HeaderEvent);
        EventCallBack._instance.SubscribeToBug<bool>(BugType.Null, NullEvent);
        EventCallBack._instance.SubscribeToBug<bool>(BugType.Undefined, UndefinedEvent);
        EventCallBack._instance.SubscribeToBug<Vector2>(BugType.ArgumentOut, ArgumentOutEvent);
        EventCallBack._instance.SubscribeToBug<bool>(BugType.Missing, MissingEvent);
    }

     void DeleteEvent()
    {

        EventCallBack._instance.UnsubscribeFromBug<Vector2>(BugType.Stack, StackEvent);
        EventCallBack._instance.UnsubscribeFromBug<Vector2>(BugType.Heap, HeaderEvent);
        EventCallBack._instance.UnsubscribeFromBug<bool>(BugType.Null, NullEvent);
        EventCallBack._instance.UnsubscribeFromBug<bool>(BugType.Undefined, UndefinedEvent);
        EventCallBack._instance.UnsubscribeFromBug<Vector2>(BugType.ArgumentOut, ArgumentOutEvent);
        EventCallBack._instance.UnsubscribeFromBug<bool>(BugType.Missing, MissingEvent);
    }

    void StackEvent( out bool fl,float time,Vector2 forward)
    {
        Debug.Log("Stack is Trigger");
        
        //禁用移动
        transform.DOMove(forward,time)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => 
            {
                //解除禁用移动
            });
        
        fl = true;
    }   
    
    void HeaderEvent(out bool fl,float time,Vector2 forward)
    {
        Debug.Log("Header is Trigger");
        
        //玩家弹飞
        rb.AddForce(Vector3.forward * ForceMag);
        
        fl = true;
    }    
    
    void NullEvent(out bool fl,float time,bool Air)
    {
        Debug.Log("Null is Trigger");
        
        //进入空中移动
        
        fl = true;
    }   
    
    void UndefinedEvent(out bool fl,float time,bool mess)
    {
        Debug.Log("Undefined is Trigger");
        
        //打乱wasd
        
        fl = true;
    }   
    
    void ArgumentOutEvent(out bool fl,float time,Vector2 Endpo)
    {
        Debug.Log("ArgumentOut is Trigger");
        
        //玩家瞬移
        transform.position = Endpo;
        
        fl = true;
    }    
    
    void MissingEvent(out bool fl,float time,bool SceneStop)
    {
        Debug.Log("Missing is Trigger");
        
        fl = true;
    }
    
}
