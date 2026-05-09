using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsoleHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void LaunchHiddenDevConsole()
    {
#if UNITY_EDITOR
#else
        Debug.Log("Dev Console Instatiated");
        Object.Instantiate(Resources.Load("Developer Console/DevConsole") as GameObject);
#endif

    }
}
