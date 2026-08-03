using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    private BossHealth bossHealth;

    private void Start()
    {
        // Trouve automatiquement le boss dans la scène à son apparition
        bossHealth = FindFirstObjectByType<BossHealth>();

        if (bossHealth != null)
        {
            // S'abonne à l'événement de changement de vie
            bossHealth.OnHealthChanged += UpdateHealthBar;
            bossHealth.OnDeath += HideHealthBar;
        }
        else
        {
            gameObject.SetActive(false); // Cache la barre si le boss n'est pas là
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void HideHealthBar()
    {
        gameObject.SetActive(false); // Cache la barre quand le boss meurt
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
            bossHealth.OnDeath -= HideHealthBar;
        }
    }
}