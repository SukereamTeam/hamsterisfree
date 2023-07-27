using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class UnityExtension
{
    public static void SetActive(this Component component, bool isActive)
    {
        if (null == component) return;
        if (null == component.gameObject) return;

        component.gameObject.SetActive(isActive);
    }

    /// <summary> Converts given bitmask to layer number </summary>
    /// <returns> layer number </returns>
    public static int ToLayer(this LayerMask layerMask)
    {
        var bitMask = layerMask.value;
        int result = bitMask > 0 ? 0 : 31;
        while (bitMask > 1)
        {
            bitMask = bitMask >> 1;
            result++;
        }
        return result;
    }

    public static void SetLayer(this GameObject gameObject, int layer, bool recursively)
    {
        gameObject.layer = layer;

        if (recursively == false)
        {
            return;
        }

        var colliders = gameObject.transform.GetComponentsInChildren<Collider>();
        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                collider.gameObject.layer = layer;
            }
        }
        var renderers = gameObject.transform.GetComponentsInChildren<Renderer>();
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = layer;
            }
        }
    }

    public static void WaitForSeconds(this MonoBehaviour behaviour, float waitingTime, Action onComplete)
    {
        if (waitingTime <= 0f)
        {
            onComplete?.Invoke();
        }
        else
        {
            behaviour.StartCoroutine(WaitForSecondsCoroutine(waitingTime, () =>
            {
                if (behaviour.IsNull()) { return; }
                onComplete?.Invoke();
            }));
        }
    }

    private static IEnumerator WaitForSecondsCoroutine(float waitingTime, Action onComplete)
    {
        yield return new WaitForSeconds(waitingTime);

        onComplete?.Invoke();
    }

    public static void WaitForEndOfFrame(this MonoBehaviour behaviour, Action onComplete)
    {
        behaviour.StartCoroutine(WaitForEndOfFrameCoroutine(() =>
        {
            if (behaviour.IsNull()) { return; }
            onComplete?.Invoke();
        }));
    }

    private static IEnumerator WaitForEndOfFrameCoroutine(Action onComplete)
    {
        yield return new WaitForEndOfFrame();

        onComplete?.Invoke();
    }

    public static void WaitForNextFrame(this MonoBehaviour behaviour, Action onComplete)
    {
        behaviour.StartCoroutine(WaitForNextFrameCoroutine(() =>
        {
            if (behaviour.IsNull()) { return; }
            onComplete?.Invoke();
        }));
    }

    private static IEnumerator WaitForNextFrameCoroutine(Action onComplete)
    {
        yield return null;

        onComplete?.Invoke();
    }

    public static void WaitUntil(this MonoBehaviour behaviour, Func<bool> predicate, Action onComplete)
    {
        behaviour.StartCoroutine(WaitUntilCoroutine(predicate, () =>
        {
            if (behaviour.IsNull()) { return; }
            onComplete?.Invoke();
        }));
    }

    private static IEnumerator WaitUntilCoroutine(Func<bool> predicate, Action onComplete)
    {
        yield return new WaitUntil(predicate);

        onComplete?.Invoke();
    }

    public static void WaitFor(this MonoBehaviour behaviour, IEnumerator routine, Action onComplete)
    {
        behaviour.StartCoroutine(WaitForCoroutine(routine, () =>
        {
            if (behaviour.IsNull()) { return; }
            onComplete?.Invoke();
        }));
    }

    private static IEnumerator WaitForCoroutine(IEnumerator routine, Action onComplete)
    {
        yield return routine;

        onComplete?.Invoke();
    }

    public static void Activate(this GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.gameObject.SetActive(true);
        }
    }

    public static void Deactivate(this GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.gameObject.SetActive(false);
        }
    }

    public static void Activate(this Component component)
    {
        if (component != null)
        {
            component.gameObject.SetActive(true);
        }
    }

    public static void Deactivate(this Component component)
    {
        if (component != null)
        {
            component.gameObject.SetActive(false);
        }
    }

    public static bool IsNull(this UnityEngine.Object value)
    {
        return (value == null);
    }

    public static bool IsNotNull(this UnityEngine.Object value)
    {
        return (value != null);
    }

    public static T GetRandom<T>(this System.Collections.Generic.List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static void SetLossyScale(this Transform transform, Vector3 scale)
    {
        var prevParent = transform.parent;
        transform.parent = null;
        transform.localScale = scale;
        transform.parent = prevParent;
    }

    public static IEnumerator Move(this Transform transform, Vector3 destination, float duration)
    {
        if (duration < float.Epsilon
            || (destination - transform.position).sqrMagnitude < Vector3.kEpsilonNormalSqrt)
        {
            transform.position = destination;
            yield break;
        }

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public static void AddAnimationEvent(this Animator animator, string clipName, float time)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                var animationEvent = new AnimationEvent();
                animationEvent.functionName = "CallSendMessage";
                animationEvent.time = time;
                clip.AddEvent(animationEvent);

                return;
            }
        }
    }

    public static void AddAnimationEventFrame(this Animator animator, string clipName, float frame)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                var animationEvent = new AnimationEvent();
                animationEvent.functionName = "CallSendMessage";
                animationEvent.time = System.Convert.ToSingle(frame) / clip.frameRate;
                clip.AddEvent(animationEvent);

                return;
            }
        }
    }

    public static void ClearAnimationEvent(this Animator animator, string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                clip.events = null;
                return;
            }
        }
    }

    public static void SetTrigger(this Animator animator, string name, MonoBehaviour behaviour, Action onComplete = null)
    {
        if (onComplete.IsNull() || behaviour.IsNull())
        {
            return;
        }

        animator.SetTrigger(name);
        behaviour.WaitForNextFrame(() =>
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.IsNullOrEmpty())
            {
                onComplete?.Invoke();
            }
            else
            {
                behaviour?.WaitForSeconds(clipInfo[0].clip.length, onComplete);
            }
        });
    }

    public static void SetTrigger(this Animator animator, int id, MonoBehaviour behaviour, Action onComplete = null)
    {
        if (onComplete.IsNull() || behaviour.IsNull())
        {
            return;
        }

        animator.SetTrigger(id);
        behaviour.WaitForNextFrame(() =>
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.IsNullOrEmpty())
            {
                onComplete?.Invoke();
            }
            else
            {
                behaviour?.WaitForSeconds(clipInfo[0].clip.length, onComplete);
            }
        });
    }

    public static Vector2 WorldToCanvas(this Canvas canvas, Vector3 world_position, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        var viewport_position = camera.WorldToViewportPoint(world_position);
        var canvas_rect = canvas.GetComponent<RectTransform>();

        return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                           (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
    }

    public static Vector3 CanvasToWorld(this Camera camera, Vector3 canvasPosition)
    {
        var pos = camera.ViewportToWorldPoint(canvasPosition);
        return camera.WorldToViewportPoint(pos);
    }

    public static Transform FindTrasnform(this Transform transform, string name)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            if (child.name == name)
            {
                return child;
            }
            else
            {
                child = FindTrasnform(child, name);
                if (child != null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    public static Transform[] FindContainsAll(this Transform transform, string name)
    {
        List<Transform> list = new List<Transform>();
        var transforms = transform.GetComponentsInChildren<Transform>(true);
        if (transforms == null)
        {
            return null;
        }

        foreach (var tr in transforms)
        {
            if (tr.name.Contains(name))
            {
                list.Add(tr);
            }
        }

        return list.ToArray();
    }

    public static void GetComponentIfNull<T>(this Component that, ref T cachedT) where T : Component
    {
        if (cachedT == null)
        {
            if (null == that)
            {
                Debug.LogWarning("GetComponent of Component failed on Component");
                return;
            }

            cachedT = (T)that.GetComponent(typeof(T));
            if (cachedT == null)
            {
                Debug.LogWarning("GetComponent of type " + typeof(T) + " failed on " + that.name, that);
                return;
            }
        }
    }

    public static void ResetTransform(this Transform transform, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        transform.localPosition = position;
        transform.localRotation = Quaternion.Euler(rotation);
        transform.localScale = scale;
    }

    public static void ResetTransform(this Transform transform)
    {
        ResetTransform(transform, Vector3.zero, Vector3.zero, Vector3.one);
    }

    public static void CopyLocalTransform(this Transform transform, Transform target)
    {
        transform.localPosition = target.localPosition;
        transform.localRotation = target.localRotation;
        transform.localScale = target.localScale;
    }

    public static void CopyTransform(this Transform transform, Transform target)
    {
        transform.SetPositionAndRotation(target.position, target.rotation);
        transform.localScale = target.localScale;
    }

    public static bool IsApproximate(this Vector3 a, Vector3 b, float epsilon = float.Epsilon)
    {
        return a.x - b.x < epsilon
            & a.y - b.y < epsilon
            & a.z - b.z < epsilon;
    }

    public static void ChangeSprite(this UnityEngine.UI.Image image, Sprite sprite)
    {
        image.sprite = null;
        image.sprite = sprite;
    }

    public static AudioClip LoadAudipClip(this string audioPath, string bundleName)
    {
        string path = string.Format("{0}/{1}", bundleName, audioPath);
        if (!path.IsNullOrEmpty())
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                return clip;
            }
            else
            {
                Debug.LogErrorFormat("not load Resources : {0}", path);
            }
        }

        return null;
    }

    public static void PlaySound(this string audioPath, string bundleName, bool loop = false, int index = 0, System.Action onComplete = null)
    {
        audioPath.PlaySound(bundleName, 1f, loop, index, onComplete);
    }

    public static void PlaySound(this string audioPath, string bundleName, float volume, bool loop = false, int index = 0, System.Action onComplete = null)
    {
        var clip = LoadAudipClip(audioPath, bundleName);

        if (clip != null)
        {
            SoundManager.Stop(index);
            SoundManager.Play(clip, loop, volume, index);

            if (onComplete != null)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.WaitForSeconds(clip.length, onComplete);
                }
                else
                {
                    onComplete();
                }
            }
        }
        else
        {
            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public static void PlaySound(this string audioPath, bool loop = false, int index = 0, System.Action onComplete = null)
    {
        if (audioPath.IsValidText())
        {
            PlaySound(audioPath, "Sounds", loop, index, onComplete);
        }
        else
        {
            Debug.LogWarning("audioPath is Null Or Empty");

            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public static void PlaySound(this AudioClip clip, bool loop = false, int index = 0, System.Action onComplete = null)
    {
        if (clip != null)
        {
            SoundManager.Stop(index);
            SoundManager.Play(clip, loop, 1, index);

            if (onComplete != null)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.WaitForSeconds(clip.length, onComplete);
                }
                else
                {
                    onComplete();
                }
            }
        }
    }

    public static void PlayOneShotSound(this string audioPath, string bundleName, int index = 0, System.Action onComplete = null)
    {
        audioPath.PlayOneShotSound(bundleName, 1f, index, onComplete);
    }

    public static void PlayOneShotSound(this string audioPath, string bundleName, float volume, int index = 0, System.Action onComplete = null)
    {
        string path = string.Empty;
        if (bundleName.IsNullOrEmpty())
        {
            path = audioPath;
        }
        else
        {
            path = string.Format("{0}/{1}", bundleName, audioPath);
        }

        if (!path.IsNullOrEmpty())
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                SoundManager.PlayOneShot(clip, volume, index);

                if (onComplete != null)
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.WaitForSeconds(clip.length, onComplete);
                    }
                    else
                    {
                        onComplete();
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("not load Resources : {0}", path);

                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }
    }

    public static void PlayOneShotSound(this string audioPath, int index = 0, System.Action onComplete = null)
    {
        if (audioPath.IsValidText())
        {
            PlayOneShotSound(audioPath, "Sounds", index, onComplete);
        }
        else
        {
            Debug.LogWarning("audioPath is Null Or Empty");

            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public static void PlayOneShotSound(this AudioClip clip, int index = 0, System.Action onComplete = null)
    {
        if (clip != null)
        {
            SoundManager.PlayOneShot(clip, 1, index);

            if (onComplete != null)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.WaitForSeconds(clip.length, onComplete);
                }
                else
                {
                    onComplete();
                }
            }
        }
    }

    public static Sprite LoadSprite(this string spritePath)
    {
        var sprite = Resources.Load<Sprite>(spritePath);

        if (sprite != null)
        {
            return sprite;
        }
        else
        {
            Debug.LogErrorFormat("not found Sprite : {0}", spritePath);
        }

        return null;
    }

    public static void LoadSprite(this string spritePath, ref SpriteRenderer sprite)
    {
        if (spritePath.IsNullOrEmpty()) return;
        if (null == sprite) return;

        sprite.sprite = Resources.Load<Sprite>(spritePath);
    }

    public static void LoadImage(this string imagePath, ref Image img)
    {
        if (imagePath.IsNullOrEmpty()) return;
        if (null == img) return;

        img.sprite = Resources.Load<Sprite>(imagePath);
    }

    public static void SetText(this string textValue, ref Text text)
    {
        if (null == text) return;

        text.text = textValue;
    }

    public static Vector2 ScreenToCanvasPosition(this RectTransform canvasRect, Vector2 pos)
    {
        Vector2 resultPos;
        int wid = Screen.width;
        int hei = Screen.height;

        resultPos.x = (pos.x - (wid / 2)) / wid;
        resultPos.y = (pos.y - (hei / 2)) / hei;

        resultPos.x *= canvasRect.rect.width;
        resultPos.y *= canvasRect.rect.height;

        return resultPos;
    }

    public static T DeepCopy<T>(this T obj) where T : class
    {
        if (typeof(T).IsSerializable == false || typeof(ISerializable).IsAssignableFrom(typeof(T)))
            return null;

        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }
}