using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Settings Instance;

    public float sensSliderValue = 1.0f;
    public float volSliderValue = 1.0f;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetSensitivity(float value)
    {
        sensSliderValue = value;
    }

    public void SetVolume(float value)
    {
        volSliderValue = value;
    }
}
