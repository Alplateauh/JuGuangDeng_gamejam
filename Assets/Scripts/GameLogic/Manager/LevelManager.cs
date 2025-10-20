using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonAutoMono<LevelManager>
{
    public Player player { get; private set; }
}
