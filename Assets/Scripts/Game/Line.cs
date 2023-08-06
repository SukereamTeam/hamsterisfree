using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer = null;



    private List<Vector2> points = new List<Vector2>();
    public List<Vector2> Points => this.points;

    public int PointCount { get; private set; }

    private float pointsMinDistance = 0.1f;



    public void AddPoint(Vector2 _NewPoint)
    {
        if (PointCount >= 1 && Vector2.Distance(_NewPoint, GetLastPoint()) < pointsMinDistance)
        {
            return;
        }

        points.Add(_NewPoint);
        PointCount++;

        this.lineRenderer.positionCount = PointCount;
        this.lineRenderer.SetPosition(PointCount - 1, _NewPoint);
    }

    public Vector2 GetLastPoint()
    {
        return (Vector2)lineRenderer.GetPosition(PointCount - 1);
    }

    public void SetLineColor(Gradient _ColorGradient)
    {
        this.lineRenderer.colorGradient = _ColorGradient;
    }

    public void SetPointMinDistance(float _Distance)
    {
        this.pointsMinDistance = _Distance;
    }

    public void SetLineWidth(float _Width)
    {
        this.lineRenderer.startWidth = _Width;
        this.lineRenderer.endWidth = _Width;
    }

    public void Clear()
    {
        this.lineRenderer.positionCount = 0;
        this.points.Clear();

        PointCount = 0;
    }
}
