using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;

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
        }


        // TODO
        // Raycast 호출하는 부분 따로 분리해서 drag 할 때랑 up 할 때 모두 쏴줘야 할 듯

        var mousePosition = Input.mousePosition;

        int layerMask = 0; //(1 << LayerMask.NameToLayer(""));
        Ray ray = gameCamera.ScreenPointToRay(mousePosition);

        var raycastResult = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, Physics.AllLayers);

        if (raycastResult.collider.IsNull())
        {
            if (Input.GetMouseButtonUp(0))
            {
                GameManager.Instance.MapManager.IsFade.Value = false;
            }

            return;
        }

        var tile = raycastResult.transform.GetComponent<TileBase>();
        
        if (Input.GetMouseButton(0))
        {
            if (GameManager.Instance.MapManager.IsCanStart.Value == false)
            {
                GameManager.Instance.MapManager.IsFade.Value = false;
            }
            else
            {
                tile.TileTriggerEvent();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            GameManager.Instance.MapManager.IsFade.Value = false;
        }
    }


}
