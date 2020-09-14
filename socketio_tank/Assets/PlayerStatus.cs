using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public const float maxHealth = 3;
    public float currentHealth = maxHealth;
    [SerializeField, Tooltip("無敵時間")] float invincible = default;

    public Slider slider;

    float invincibleCount;
    bool invincibleOn = false;
    void Start()
    {
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (invincibleOn)
        {
            invincibleCount -= Time.deltaTime;
           
        }

        if (invincibleCount <= 0)
        {
            invincibleOn = false;
        }

        if (currentHealth <= 0)
        {
            //Destroy(this.gameObject);
        }

    }
    public void OnChangeHealth()
    {
        slider.value = currentHealth;
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet") && invincibleCount <= 0)
        {
            //パワーの値だけHPを減らす
            float power = other.gameObject.GetComponent<OffensivePower>().Power;

            currentHealth -= power;
            slider.value = currentHealth;
            //OnChangeHealth();
            NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
            //n.CommandHealthChange(gameObject, power);
            //ダメージSE
            //AudioManager.Instance.PlaySE(AUDIO.SE_DAMAGE);

            //カウントリセット
            invincibleCount = invincible;
            invincibleOn = true;
        }
    }
}
