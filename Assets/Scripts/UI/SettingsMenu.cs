using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private Slider audioSlider;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
        }

        if (audioSlider != null)
        {
            audioSlider.value = AudioManager.Instance.GetVolume();
            audioSlider.onValueChanged.AddListener(SetAudioVolume);
        }
    }

    private void OnEnable()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void OnDisable()
    {
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }

    public void BackToMain()
    {
        settingsCanvas.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    private void SetAudioVolume(float volume)
    {
        AudioManager.Instance.SetVolume(volume);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}