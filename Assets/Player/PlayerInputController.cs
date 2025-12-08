using System;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public static PlayerInputController Instance;
    public event Action onPause;

    void Awake()
    {
        Instance = this;
    }

    public void OnPauseMenu()
    {
        onPause.Invoke();
    }
}