using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UISetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCanvas();
    }

    private void SetupCanvas()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("Canvas setup completed!");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/UI/Setup Game Canvas", false, 0)]
    static void CreateGameCanvas()
    {
        GameObject canvasObject = new GameObject("GameCanvas");
        canvasObject.AddComponent<UISetup>();
        UnityEditor.Selection.activeGameObject = canvasObject;
        Debug.Log("Created new GameCanvas with proper setup!");
    }
#endif
}
