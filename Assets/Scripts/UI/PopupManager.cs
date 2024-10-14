using Cysharp.Threading.Tasks;
using UnityEngine;

public class PopupManager
{
    private const string POPUP_PATH = "Prefabs/";
    
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

        Object.DontDestroyOnLoad(go.gameObject);

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
