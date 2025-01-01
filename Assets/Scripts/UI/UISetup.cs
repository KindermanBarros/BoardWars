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
        // Get or add Canvas component
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        // Configure Canvas
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Get or add CanvasScaler
        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        // Configure CanvasScaler
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        // Get or add GraphicRaycaster
        GraphicRaycaster raycaster = gameObject.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            raycaster = gameObject.AddComponent<GraphicRaycaster>();

        // Make sure there's an EventSystem in the scene
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("Canvas setup completed!");
    }

#if UNITY_EDITOR
    // Add a menu item to quickly set up the Canvas
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
