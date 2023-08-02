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
        if (Input.GetMouseButtonDown(0))
        {

        }

        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;

            int layerMask = 0; //(1 << LayerMask.NameToLayer(""));
            Ray ray = gameCamera.ScreenPointToRay(mousePosition);

            var raycastResult = Physics.RaycastAll(ray, Mathf.Infinity, Physics.AllLayers);

            foreach(var obj in raycastResult)
            {
                Debug.Log($"{obj.transform.name}");

                var tile = obj.transform.GetComponent<TileBase>();

                if (tile.TileType == TileType.Monster)
                {

                }
                else if (tile.TileType == TileType.Seed)
                {

                }
                else if (tile.TileType == TileType.Exit)
                {
                    Debug.Log("Game End");
                }
            }
        }
    }


}
