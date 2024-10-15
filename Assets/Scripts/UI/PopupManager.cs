using Cysharp.Threading.Tasks;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    private GameObject _popupRoot;
    
    private const string POPUP_PATH = "Prefabs/";

    public async UniTask InitializeAsync()
    {
        var prefab = await Resources.LoadAsync(POPUP_PATH + "PopupRoot") as GameObject;
        _popupRoot = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(_popupRoot);
    }
    
    public async UniTask<T> CreateAsync<T>() where T : PopupBase
    {
        var prefab = await Resources.LoadAsync(POPUP_PATH + typeof(T).Name) as GameObject;

        if (prefab == null)
        {
            Debug.LogError("Prefab not found : " + POPUP_PATH + typeof(T).Name);
            return null;
        }

        prefab.SetActive(false);
        var go = Object.Instantiate(prefab);
        var popup = go.GetComponent<T>();

        if (_popupRoot != null) go.transform.SetParent(_popupRoot.transform);

        prefab.SetActive(true);

        var rect = go.transform as RectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
        rect.SetAsLastSibling();

        popup.OnHideComplete += () => { RemovePopup(popup); };
        
        return popup;
    }
    
    public void RemovePopup(PopupBase popup)
    {
        Object.DestroyImmediate(popup.gameObject);
    }
}
