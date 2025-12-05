using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private GameObject settings;
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
    }

    public void SettingsMenu()
    {
        settings.SetActive(!settings.activeSelf);
    }
}
