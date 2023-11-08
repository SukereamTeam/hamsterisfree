using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using UniRx;

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

        

        

        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.IsMonsterTrigger == true)
            {
                // Monster에 닿아서 Trigger 연출 중이라면
                return;
            }

            var (hit2D, _) = RaycastGameScreen(Input.mousePosition);

            if (hit2D.collider == null)
            {
                //GameScreen 영역을 벗어나면
                DragEnd();

                RewindAndDecreaseStage(true);

                return;
            }

            GameManager.Instance.MapManager.IsFade.Value = true;

            GameManager.Instance.EnablePressScreen(false);

            StartTileAct();

            this.mouseDownTime = Time.time;
            this.mouseDownPos = this.gameCamera.ScreenToWorldPoint(Input.mousePosition);

            this.lineManager.BeginDraw();

            PlayDragSound(true);
        }


        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }


        if (Input.GetMouseButton(0))
        {
            if (GameManager.Instance.IsReset == true || GameManager.Instance.IsMonsterTrigger == true)
            {
                // 몬스터에 닿았거나, 게임화면 밖으로 나갔거나 하는 이유로
                // RewindStage 가 일어났는데 아직 손가락을 떼지 않았을 때
                // 드래그 처리 X

                DragEnd();

                return;
            }

            var result = RaycastGameScreen(Input.mousePosition);


            if (result.hit2D.collider == null)
            {
                // GameScreen 영역을 벗어나면
                DragEnd();

                RewindAndDecreaseStage();

                return;
            }

            var curMousePos = this.gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 distance = curMousePos - this.mouseDownPos;
            float sqrLen = distance.sqrMagnitude;

            if (sqrLen > (dragDistance * dragDistance) &&
                (Time.time - this.mouseDownTime) < GameManager.Instance.MapManager.FadeTime)
            {
                // 화면이 아직 가려지지 않았는데 움직였을 경우 reset stage
                DragEnd();

                RewindAndDecreaseStage();
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
            // Drag 뗐을 때도 Reset 되어야 하니까 / if 게임이 끝나지 않았다면 reset stage
            if (GameManager.Instance.IsGame.Value == true)
            {
                DragEnd();

                RewindAndDecreaseStage();
            }
        }
    }

    private void DragEnd()
    {
        GameManager.Instance.MapManager.IsFade.Value = false;

        this.lineManager.EndDraw();

        PlayDragSound(false);
    }

    private void RewindAndDecreaseStage(bool _IsDown = false)
    {
        if (GameManager.Instance.IsMonsterTrigger == false)
        {
            GameManager.Instance.RewindStage();

            // _IsDown 이 true라면 드래그 시작하려고 GetMouseButtonDown 한 상태인데,
            // 시작하려고 했을 때 Rewind 된 건 Try횟수 깎지 말고 봐주려고
            if (_IsDown == false)
            {
                // LimitTry Stage인 경우 드래그를 끝내도 Try 횟수 1 감소
                if (GameManager.Instance.StageManager.StageInfo.Type == Define.StageType.LimitTry)
                {
                    GameManager.Instance.StageManager.ChangeStageValue(-1);
                }
            }
        }
    }

    private TileBase RaycastTile(Vector3 _MousePosition)
    {
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

    

    private void PlayDragSound(bool _IsPlay)
    {
        if (_IsPlay == false)
        {
            if (SoundManager.Instance.IsPlaying(GameManager.Instance.DragPath) == true)
            {
                SoundManager.Instance.Stop(GameManager.Instance.DragPath);
            }
        }
        else
        {
            if (SoundManager.Instance.IsPlaying(GameManager.Instance.DragPath) == false)
            {
                SoundManager.Instance.Play(GameManager.Instance.DragPath, _Loop: true).Forget();
            }
        }
    }

    private void StartTileAct()
    {
        GameManager.Instance.IsReset = false;

        foreach (var seed in GameManager.Instance.MapManager.SeedTiles)
        {
            if (seed.IsFuncStart == false)
            {
                seed.TileFuncStart();
            }
        }

        foreach (var monster in GameManager.Instance.MapManager.MonsterTiles)
        {
            if (monster.IsFuncStart == false)
            {
                monster.TileFuncStart();
            }
        }
    }

    
}
