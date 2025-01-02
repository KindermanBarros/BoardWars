using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button backToMenuButton;

    private void Start()
    {
        replayButton.onClick.AddListener(OnReplayButtonClicked);
        backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
    }

    public void Show(string winnerName)
    {
        winnerText.text = $"{winnerName} wins!";
        gameObject.SetActive(true);
    }

    private void OnReplayButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnBackToMenuButtonClicked()
    {
        GameManager.Instance.BackToMainMenu();
    }
}
