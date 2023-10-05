using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ScrollIndexCallback2 : MonoBehaviour 
{
    public TextMeshProUGUI text;
    public LayoutElement element;
    public static float[] randomWidths = new float[3] { 100, 150, 100 };
    public static float[] setScales = new float[3] { 1f, 2f, 1f };

    private int index = -1;
    
    void ScrollCellIndex(int idx)
    {
        string name = "Cell " + idx.ToString();
        if (text != null)
        {
            text.text = name;
        }
        element.preferredWidth = randomWidths[Mathf.Abs(idx) % 3];
        element.preferredHeight = randomWidths[Mathf.Abs(idx) % 3];
        
        gameObject.name = name;

        this.index = idx;
    }

    public void OnClick_Log()
    {
        Debug.Log($"# index : {this.index} #");
    }
}
