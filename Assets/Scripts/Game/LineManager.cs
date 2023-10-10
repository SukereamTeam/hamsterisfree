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

    // TODO
    // Queue 로 만들어서 인덱스로 접근해 CurrentLine 삭제 관리?


    public void BeginDraw()
    {
        CurrentLine = Instantiate(linePrefab, this.transform).GetComponent<Line>();

        CurrentLine.SetLineColor(this.lineColor);
        CurrentLine.SetPointMinDistance(this.linePointMinDistance);
        CurrentLine.SetLineWidth(this.lineWidth);
    }

    public void DrawLine(Vector2 _Point)
    {
        if (CurrentLine != null)
            CurrentLine.AddPoint(_Point);
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
                else
                {
                    CurrentLine = null;
                }
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
}
