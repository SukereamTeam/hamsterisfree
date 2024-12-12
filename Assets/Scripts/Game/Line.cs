using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer = null;



    private List<Vector2> points = new List<Vector2>();
    public List<Vector2> Points => this.points;

    public int PointCount { get; private set; }

    private float _pointsMinDistance = 0.1f;



    public void AddPoint(Vector2 newPoint)
    {
        if (PointCount >= 1 && Vector2.Distance(newPoint, GetLastPoint()) < _pointsMinDistance)
        {
            return;
        }

        points.Add(newPoint);
        PointCount++;

        this.lineRenderer.positionCount = PointCount;
        this.lineRenderer.SetPosition(PointCount - 1, newPoint);
    }

    public Vector2 GetLastPoint()
    {
        return (Vector2)lineRenderer.GetPosition(PointCount - 1);
    }

    public void SetLineColor(Gradient colorGradient)
    {
        this.lineRenderer.colorGradient = colorGradient;
    }

    public void SetPointMinDistance(float distance)
    {
        this._pointsMinDistance = distance;
    }

    public void SetLineWidth(float width)
    {
        this.lineRenderer.startWidth = width;
        this.lineRenderer.endWidth = width;
    }

    public void Clear()
    {
        this.lineRenderer.positionCount = 0;
        this.points.Clear();

        PointCount = 0;
    }
}
