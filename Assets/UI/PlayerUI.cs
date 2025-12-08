using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private RectTransform _healthBarMask;
    [SerializeField] private Image _healthBar;

    private float _lerpedHealth;

    void Start()
    {
        PlayerStats.Instance.onPlayerHurt += () =>
        {
            _healthBar.color = Color.red;
        };

        PlayerInputController.Instance.onPause += () => { SetPauseMenu(true); };
    }

    void Update()
    {
        _healthBar.color = Color.Lerp(_healthBar.color, Color.white, 2f * Time.deltaTime);
        _lerpedHealth = Mathf.Lerp(_lerpedHealth, PlayerStats.Instance.health, 2f * Time.deltaTime);

        HealthBar();
    }

    void HealthBar()
    {
        float hpPercent = _lerpedHealth / PlayerStats.Instance.maxHealth;
        Vector2 newSize = new Vector2(hpPercent * 755f, _healthBarMask.sizeDelta.y);

        _healthBarMask.sizeDelta = newSize;
    }

    public void SetPauseMenu(bool state)
    {
        _pauseMenu.SetActive(state);

        if (_pauseMenu.activeSelf)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
