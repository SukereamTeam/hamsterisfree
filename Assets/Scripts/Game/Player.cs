using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;


    [Space(30)]
    [SerializeField]
    private GameObject linePrefab;

    [SerializeField]
    private Gradient lineColor;

    [SerializeField]
    private float linePointMinDistance;

    [SerializeField]
    private float lineWidth;


    private readonly float dragDistance = 0.3f;

    private float mouseDownTime = 0f;
    private Vector3 mouseDownPos = Vector3.zero;

    private Line currentLine;
    private int lineLayer = -1;



    // TODO
    // Line Renderer로 선 그리기
    // 페이드 완료 되어야 게임 시작
    // 몬스터타일 선택 하면 게임 오버

    // TODO
    // background bounds 밖으로 input 나가면 return;

    private void Start()
    {
        this.lineLayer = (1 << LayerMask.NameToLayer("GameScreen"));
    }

    private void Update()
    {
        if (GameManager.Instance.IsGame.Value == false)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.MapManager.IsFade.Value = true;

            this.mouseDownTime = Time.time;
            this.mouseDownPos = Input.mousePosition;

            //BlockMouseOutBounds(Input.mousePosition);

            BeginDraw();
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 offset = Input.mousePosition - this.mouseDownPos;
            float sqrLen = offset.sqrMagnitude;

            if (sqrLen > (dragDistance * dragDistance) && (Time.time - this.mouseDownTime) <
                GameManager.Instance.MapManager.FadeTime)
            {
                Debug.Log("아직 시간 안됨");

                GameManager.Instance.MapManager.IsFade.Value = false;
            }
            else
            {
                var tile = RaycastTile(Input.mousePosition);

                if (tile.IsNotNull())
                {
                    tile.TileTriggerEvent();
                }
            }

            if (this.currentLine != null)
                DrawLine();
        }

        if (Input.GetMouseButtonUp(0))
        {
            GameManager.Instance.MapManager.IsFade.Value = false;

            EndDraw();
        }
    }

    private TileBase RaycastTile(Vector3 _MousePosition)
    {
        int layerMask = 0; //(1 << LayerMask.NameToLayer(""));
        Ray ray = gameCamera.ScreenPointToRay(_MousePosition);

        var raycastResult = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, Physics.AllLayers);

        if (raycastResult.collider.IsNotNull())
        {
            var tile = raycastResult.transform.GetComponent<TileBase>();

            return tile;
        }

        return null;
    }

    private void BlockMouseOutBounds(Vector3 _MousePosition)
    {
        var backPos = GameManager.Instance.MapManager.Background.transform.position;

        var screenPos = gameCamera.WorldToScreenPoint(backPos);

        var top = gameCamera.WorldToViewportPoint(GameManager.Instance.MapManager.Background.bounds.max);
    }



    //----- Line Render
    private void BeginDraw()
    {
        this.currentLine = Instantiate(linePrefab, this.transform).GetComponent<Line>();

        this.currentLine.SetLineColor(this.lineColor);
        this.currentLine.SetPointMinDistance(this.linePointMinDistance);
        this.currentLine.SetLineWidth(this.lineWidth);


    }

    private void DrawLine()
    {
        //Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        //var raycastResult = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, this.lineLayer);


        Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);

        var raycastResult = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, this.lineLayer);

        if (raycastResult)
        {

            this.currentLine.AddPoint(mousePosition);
        }
        else
        {
            //EndDraw();
        }


    }

    private void EndDraw()
    {
        if (this.currentLine != null)
        {
            if (this.currentLine.PointCount < 2)
            {
                Destroy(this.currentLine.gameObject);
            }
            else
            {
                this.currentLine = null;
            }
        }
    }
}
