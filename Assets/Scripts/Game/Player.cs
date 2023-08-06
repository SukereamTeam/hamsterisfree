using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;

    [SerializeField]
    private LineManager lineManager = null;


    private readonly float dragDistance = 0.3f;

    private float mouseDownTime = 0f;
    private Vector3 mouseDownPos = Vector3.zero;

    private int lineLayer = -1;



    // TODO
    // 몬스터타일 선택 하면 게임 오버

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
            var result = RaycastGameScreen(Input.mousePosition);

            if (result.Item1.collider.IsNull())
            {
                // GameScreen 영역을 벗어나면
                Debug.Log("### GameScreen 영역을 벗어나면 ###");
                GameManager.Instance.MapManager.IsFade.Value = false;
                this.lineManager.EndDraw();

                return;
            }

            GameManager.Instance.MapManager.IsFade.Value = true;

            this.mouseDownTime = Time.time;
            this.mouseDownPos = Input.mousePosition;

            this.lineManager.BeginDraw();
        }

        if (Input.GetMouseButton(0))
        {
            var result = RaycastGameScreen(Input.mousePosition);

            if (result.Item1.collider.IsNull())
            {
                // GameScreen 영역을 벗어나면
                Debug.Log("### GameScreen 영역을 벗어나면 ###");
                GameManager.Instance.MapManager.IsFade.Value = false;
                this.lineManager.EndDraw();

                return;
            }

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


            GameManager.Instance.MapManager.Mask.transform.position = new Vector3(result.Item2.x, result.Item2.y, GameManager.Instance.MapManager.Mask.transform.position.z);

            if (this.lineManager.CurrentLine != null)
                this.lineManager.DrawLine(result.Item2);
        }

        if (Input.GetMouseButtonUp(0))
        {
            GameManager.Instance.MapManager.IsFade.Value = false;

            this.lineManager.EndDraw();
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

    private (RaycastHit2D, Vector2) RaycastGameScreen(Vector3 _MousePosition)
    {
        Vector2 mousePosition = gameCamera.ScreenToWorldPoint(_MousePosition);

        var raycastResult = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, this.lineLayer);

        return (raycastResult, mousePosition);
    }
}
