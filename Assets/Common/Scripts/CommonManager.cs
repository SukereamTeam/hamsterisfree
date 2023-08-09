using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : MonoSingleton<CommonManager>
{

    //TODO : DELETE (Test용)
    public int CurStageIndex { get; private set; }

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        CurStageIndex = 0;

        // Table Load
        DataContainer.Initialize();
    }
}
