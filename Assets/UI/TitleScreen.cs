using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    public void StartGame()
    {
        loadingScreen.SetActive(true);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Dungeon");
        loadingScreen.SetActive(false);
    }
}
