using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void OnClick_Next()
    {
        SceneController.LoadScene("Game");
    }
}
