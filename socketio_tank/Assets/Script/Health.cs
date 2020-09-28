using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public const float maxHealth = 100;
    public bool destroyOnDeath;

    public float currentHealth = maxHealth;
    [SerializeField, Tooltip("無敵時間")] float invincible = default;

    public Slider slider;

    bool isLocalPlayer;

    float invincibleCount;
    bool invincibleOn = false;
    void Start()
    {
        PlayerController pc = GetComponent<PlayerController>();
        isLocalPlayer = pc.isLocaPlayer;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }

    public void TakeDamage(GameObject playerFrom, int amount)
    {
        currentHealth -= amount;
        NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
        n.CommandHealthChange(playerFrom, this.gameObject, amount,false);
    }
    public void OnChangeHealth()
    {
        slider.value = currentHealth;
        if (currentHealth <= 0)
        {
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
            }
        }
    }
}
