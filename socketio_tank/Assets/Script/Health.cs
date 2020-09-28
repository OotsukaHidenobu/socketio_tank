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

    Vector3 startPos;

    GameObject resultUI;
    void Start()
    {
        PlayerController pc = GetComponent<PlayerController>();
        isLocalPlayer = pc.isLocaPlayer;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;

        resultUI = GameObject.Find("ResultUI");

        startPos = gameObject.transform.position;
    }

    public void TakeDamage(GameObject playerFrom, int amount)
    {
        currentHealth -= amount;
        AudioManager.Instance.PlaySE(AUDIO.SE_BEING_BOMBED);
        NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
        n.CommandHealthChange(playerFrom, this.gameObject, amount,false);
    }
    public void OnChangeHealth()
    {
        slider.value = currentHealth;
        if (currentHealth <= 0)
        {
            foreach(Transform child in resultUI.transform)
            {
                child.gameObject.SetActive(true);
            }
            if(startPos.x > 0)
            {
                resultUI.transform.GetChild(0).localPosition = new Vector3(-285, -1, 0);
                resultUI.transform.GetChild(1).localPosition = new Vector3(258, -1, 0);
                print("loselose"+startPos.x);
            }
            else
            {
                resultUI.transform.GetChild(0).localPosition = new Vector3(258, -1, 0);
                resultUI.transform.GetChild(1).localPosition = new Vector3(-285, -1, 0);
                print("fffasdf");
            }
            Destroy(gameObject);
            AudioManager.Instance.PlaySE(AUDIO.SE_EXPLOSION3);
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
