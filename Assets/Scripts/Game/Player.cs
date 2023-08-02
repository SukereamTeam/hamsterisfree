using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;

    // TODO
    // Line Renderer로 선 그리기
    // 씨앗타일 선택 시작하면 페이드처리, 페이드 완료 되어야 게임 시작
    // 몬스터타일 선택 하면 게임 오버

    private void Update()
    {
        var mousePosition = Input.mousePosition;

        int layerMask = 0; //(1 << LayerMask.NameToLayer(""));
        Ray ray = gameCamera.ScreenPointToRay(mousePosition);

        var raycastResult = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, Physics.AllLayers);

        if (raycastResult.IsNull())
            return;


        var tile = raycastResult.transform.GetComponent<TileBase>();
        

        if (Input.GetMouseButtonDown(0))
        {
            if (tile.TileType == TileType.Seed)
            {
                GameManager.Instance.MapManager.IsFade = true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            tile.TileTriggerEvent();
        }

        if (Input.GetMouseButtonUp(0))
        {

        }
    }


}
