using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotBase : MonoBehaviour
{
    // 弾丸発射点
    [SerializeField, Tooltip("弾のプレハブ")] protected GameObject bullet = default;
    [SerializeField] Transform muzzle = default;


    [SerializeField, Tooltip("弾丸の速度")] float speed = 100;

    [SerializeField, Tooltip("射撃間隔(秒)")] float firingInterval = 0.3f;
    [SerializeField, Tooltip("リロード時間(秒)")] float reloadtime = 0.8f;
    [SerializeField, Tooltip("マガジン弾数(何発でリロードしなかければいけないか)")] int magazineAmmo = 6;

    [SerializeField] float power = 1;
    



    //カウントダウン用変数
    float firingIntervalCountdown;
    float reloadTimeCountdown;
    float magazinAmmoCountdown;

    //リロードボタンが押されたか
    bool reloadPush = false;

    //リロード用のトリガー
    bool reloadSound = true;

    //レイヤーマスク用のint
    int layerInt = 1 << 0 | 1 << 18 | 1 << 20 | 1 << 21;


    void Start()
    {

        firingIntervalCountdown = 0;
        reloadTimeCountdown = reloadtime;
        magazinAmmoCountdown = magazineAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        //マガジンが最大じゃないときにリロードボタンを押したら、リロードがオンになる
        if (Input.GetKeyDown(KeyCode.Y) && magazinAmmoCountdown != magazineAmmo)
        {
            reloadPush = true;
        }
        //射撃間隔のカウントダウン
        firingIntervalCountdown -= Time.deltaTime;
        //射撃間隔が0になるかつ、マガジン弾数が0じゃないかつ、合計弾薬が0以上かつ、リロードされていなかったら
        if (firingIntervalCountdown <= 0 && magazinAmmoCountdown > 0 && reloadPush == false)
        {
            //射撃ボタンを押したら弾を撃つ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
                n.CommandShoot();
                //HandGunShot();
                //撃つ音を鳴らす
                //AudioManager.Instance.PlaySE(AUDIO.SE_FIREARM_SUB_MACHINE_GUN, gameObject);
                magazinAmmoCountdown--;
                firingIntervalCountdown = firingInterval;
            }
        }
        //マガジン弾数が0になるかリロードをすると
        else if (magazinAmmoCountdown <= 0 || reloadPush)
        {
            //リロードボタン用のマガジン弾数を0
            magazinAmmoCountdown = 0;
            //リロード時間のカウントダウン
            reloadTimeCountdown -= Time.deltaTime;

            if (reloadSound)
            {
                //リロードの音を鳴らす
                //AudioManager.Instance.PlaySE(AUDIO.SE_RELOAD, gameObject);
                reloadSound = false;
            }

            //リロード時間経ったら
            if (reloadTimeCountdown <= 0)
            {
                //マガジン弾薬のリセット
                magazinAmmoCountdown = magazineAmmo;
                //リロード時間のリセット
                reloadTimeCountdown = reloadtime;
                //リロードのOFF
                reloadPush = false;
                //リロード時のサウンドトリガーをON
                reloadSound = true;

            }
        }
        
    }

    public void HandGunShot()
    {
;
        GameObject instBullet = Instantiate(bullet, muzzle.position, Quaternion.identity) as GameObject;
        Bullet b = bullet.GetComponent<Bullet>();
        b.playerFrom = this.gameObject;
        instBullet.GetComponent<Rigidbody>().velocity = transform.forward * speed;
        Destroy(instBullet, 5);
        //OffensivePower powers1 = instBullet.GetComponent<OffensivePower>();
        //powers1.power = power;
    }
}
