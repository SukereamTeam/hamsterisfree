using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : MonoSingleton<CommonManager>
{

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;


        // Table Load
        DataContainer.Initialize();
    }
}
