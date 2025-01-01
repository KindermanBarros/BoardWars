using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private BoardGame boardGame;
    [SerializeField] private CameraController cameraController;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        if (boardGame == null)
            boardGame = FindObjectOfType<BoardGame>();
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
    }
}
