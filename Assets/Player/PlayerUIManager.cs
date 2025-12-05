using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _healthBarMask;
    [SerializeField] private Image _healthBar;



    void Start()
    {
        PlayerStats.Instance.onPlayerHurt += () =>
        {
            _healthBar.color = Color.red;
        };
    }

    void Update()
    {
        HealthBar();

        _healthBar.color = Color.Lerp(_healthBar.color, Color.white, 2f * Time.deltaTime);
    }

    void HealthBar()
    {
        float hpPercent = PlayerStats.Instance.health / PlayerStats.Instance.maxHealth;
        Vector2 newSize = new Vector2(hpPercent * 755f, _healthBarMask.sizeDelta.y);

        _healthBarMask.sizeDelta = newSize;
    }
}
