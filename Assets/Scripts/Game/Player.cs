using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;


    private readonly float dragDistance = 0.2f;

    private float mouseDownTime = 0f;
    private Vector3 mouseDownPos = Vector3.zero;

    // TODO
    // Line Renderer로 선 그리기
    // 페이드 완료 되어야 게임 시작
    // 몬스터타일 선택 하면 게임 오버

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
        }

        if (Input.GetMouseButtonUp(0))
        {

            GameManager.Instance.MapManager.IsFade.Value = false;
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

    
}
