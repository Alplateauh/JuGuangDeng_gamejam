using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class TimeMgr : BaseManager<TimeMgr>
{
    //private Tweener tweener;
    private float defaultFixedDeltaTime;
    private TimeState timeState = TimeState.Normal;

    public TimeMgr()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }
    public void StopTime()
    {
        //tweener = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.3f)
        //     .OnComplete(() => Time.timeScale = 0.1f);
        //Debug.Log("时间变化");
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        timeState = TimeState.Pause;
        EventCenter.GetInstance().EventTrigger("时间变化");
        //Debug.Log("时间变化");
    }

    public void RecoveryTime()
    {
        //tweener = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.3f)
        //                 .OnComplete(() => Time.timeScale = 1f);
        //Debug.Log("时间变化");
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        timeState = TimeState.Normal;
        EventCenter.GetInstance().EventTrigger("时间变化");
        //Debug.Log("时间变化");
    }

    public bool IsStop()
    {
        //Debug.Log(timeState == TimeState.Pause);
        return timeState == TimeState.Pause;
    }
}

public enum TimeState
{
    Normal,
    Pause
}
