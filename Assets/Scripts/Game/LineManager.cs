using UnityEngine;

public class LineManager : MonoBehaviour
{

    [SerializeField]
    private GameObject linePrefab;

    [SerializeField]
    private Gradient[] lineColorArray;

    [SerializeField]
    private float linePointMinDistance;

    [SerializeField]
    private float lineWidth;


    public Line CurrentLine { get; private set; }


    public void BeginDraw()
    {
        CurrentLine = Instantiate(linePrefab, this.transform).GetComponent<Line>();

        var idx = GetRandomColorIdx();
        CurrentLine.SetLineColor(this.lineColorArray[idx]);
        CurrentLine.SetPointMinDistance(this.linePointMinDistance);
        CurrentLine.SetLineWidth(this.lineWidth);
    }

    public void DrawLine(Vector2 point)
    {
        if (CurrentLine != null)
            CurrentLine.AddPoint(point);
    }

    public void EndDraw()
    {
        if (CurrentLine != null)
        {
            if (GameManager.Instance.IsGame.Value != false)
            {
                CurrentLine.Clear();

                if (CurrentLine.PointCount < 2)
                {
                    Destroy(CurrentLine.gameObject, 0.5f);
                }

                CurrentLine = null;
            }

            //var ob = Observable.Timer(TimeSpan.FromSeconds(0.5f))
            //    .Subscribe(_ => 
            //    {
            //        Debug.Log("Done");

            //        CurrentLine.Clear();
            //        Destroy(CurrentLine.gameObject);
            //        CurrentLine = null;
            //    }).AddTo(this);

        }
    }

    private int GetRandomColorIdx()
    {
        return Random.Range(0, this.lineColorArray.Length);
    }
}
