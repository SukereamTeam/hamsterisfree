using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{

    [SerializeField]
    private GameObject linePrefab;

    [SerializeField]
    private Gradient lineColor;

    [SerializeField]
    private float linePointMinDistance;

    [SerializeField]
    private float lineWidth;


    public Line CurrentLine { get; private set; }



    public void BeginDraw()
    {
        CurrentLine = Instantiate(linePrefab, this.transform).GetComponent<Line>();

        CurrentLine.SetLineColor(this.lineColor);
        CurrentLine.SetPointMinDistance(this.linePointMinDistance);
        CurrentLine.SetLineWidth(this.lineWidth);
    }

    public void DrawLine(Vector2 _Point)
    {
        CurrentLine.AddPoint(_Point);
    }

    public void EndDraw()
    {
        if (CurrentLine != null)
        {
            if (CurrentLine.PointCount < 2)
            {
                Destroy(CurrentLine.gameObject);
            }
            else
            {
                CurrentLine = null;
            }
        }
    }
}
