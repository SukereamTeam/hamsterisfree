using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera = null;

    [SerializeField]
    private LineManager lineManager = null;


    private readonly float dragDistance = 0.5f;

    private float mouseDownTime = 0f;
    private Vector3 mouseDownPos = Vector3.zero;

    private int lineLayer = -1;



    private void Start()
    {
        this.lineLayer = (1 << LayerMask.NameToLayer("GameScreen"));
    }

    private void Update()
    {
        if (GameManager.IsInstance == false)
        {
            return;
        }

        // 게임이 종료되었거나, 아직 시작한 상태가 아니라면 return
        if (GameManager.Instance.IsGame.Value == false)
        {
            return;
        }
        
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var (hit2D, _) = RaycastGameScreen(Input.mousePosition);

            if (hit2D.collider == null)
            {
                // GameScreen 영역을 벗어나면
                Debug.Log("### GameScreen 영역을 벗어나면 ###");
                GameManager.Instance.MapManager.IsFade.Value = false;
                this.lineManager.EndDraw();

                return;
            }

            GameManager.Instance.MapManager.IsFade.Value = true;

            this.mouseDownTime = Time.time;
            this.mouseDownPos = this.gameCamera.ScreenToWorldPoint(Input.mousePosition);//Input.mousePosition;

            this.lineManager.BeginDraw();
        }

        if (Input.GetMouseButton(0))
        {
            var result = RaycastGameScreen(Input.mousePosition);


            if (result.hit2D.collider == null)
            {
                // GameScreen 영역을 벗어나면
                Debug.Log("### GameScreen 영역을 벗어나면 ###");
                GameManager.Instance.MapManager.IsFade.Value = false;
                this.lineManager.EndDraw();

                return;
            }

            var curMousePos = this.gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 distance = curMousePos - this.mouseDownPos;
            float sqrLen = distance.sqrMagnitude;

            if (sqrLen > (dragDistance * dragDistance) &&
                (Time.time - this.mouseDownTime) < GameManager.Instance.MapManager.FadeTime)
            {
                Debug.Log($"아직 시간 안됨, 움직이지 마! 움직인 거리 : {sqrLen}");

                GameManager.Instance.MapManager.IsFade.Value = false;
            }
            else
            {
                var tile = RaycastTile(Input.mousePosition);

                if (tile != null)
                {
                    tile.TileTrigger();
                }
            }


            GameManager.Instance.MapManager.Mask.transform.position = new Vector3(result.Item2.x, result.Item2.y, GameManager.Instance.MapManager.BlockRenderer.transform.position.z);

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
        Ray ray = this.gameCamera.ScreenPointToRay(_MousePosition);

        var raycastResult = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, Physics.AllLayers);

        if (raycastResult.collider != null)
        {
            var tile = raycastResult.transform.GetComponent<TileBase>();

            return tile;
        }

        return null;
    }

    private (RaycastHit2D hit2D, Vector2 mousePos) RaycastGameScreen(Vector3 _MousePosition)
    {
        Vector2 mousePosition = this.gameCamera.ScreenToWorldPoint(_MousePosition);

        var raycastResult = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, this.lineLayer);

        return (raycastResult, mousePosition);
    }
}
