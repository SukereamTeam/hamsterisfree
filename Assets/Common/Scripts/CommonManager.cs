using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : MonoSingleton<CommonManager>
{
    [SerializeField]
    private DataContainer dataContainer = null;
    public DataContainer DataContainer => this.dataContainer;

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        this.dataContainer = this.transform.gameObject.AddComponent<DataContainer>();

        // Table Load
        this.dataContainer.Initialize();
    }
}
