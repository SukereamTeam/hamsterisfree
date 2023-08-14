using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO", menuName = "Create SO/SO")]
public class TestScriptable : ScriptableObject
{
    public int value;
    public int counter = 0;

    public void UpdateCounter()
    {
        counter += value; // make sure you assign the reference to counter variable
    }
}
