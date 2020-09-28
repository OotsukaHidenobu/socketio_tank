using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// BGMとSEの管理をするマネージャ。シングルトン。
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    //ボリューム保存用のkeyとデフォルト値
    private const string BGM_VOLUME_KEY = "BGM_VOLUME_KEY";
    private const string SE_VOLUME_KEY = "SE_VOLUME_KEY";
    private const float BGM_VOLUME_DEFULT = 0.5f;
    private const float SE_VOLUME_DEFULT = 0.3f;
    //オーディオファイルのパス
    private const string BGM_PATH = "Audio/BGM";
    private const string SE_PATH = "Audio/SE";
    private const string AudioMixer_PATH = "Audio/AudioMixer";

    //BGMがフェードするのにかかる時間
    public const float BGM_FADE_SPEED_RATE_HIGH = 0.9f;
    public const float BGM_FADE_SPEED_RATE_LOW = 0.3f;
    private float _bgmFadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH;

    //次流すBGM名、SE名
    private string _nextBGMName;
    private string _nextSEName;

    //BGMをフェードアウト中か
    private bool _isFadeOut = false;

    private bool _stopBGMCheck = false;

    private float _bgmVol;

    //BGM用、SE用に分けてオーディオソースを持つ
    private AudioSource _bgmSource;
    private List<AudioSource> _seSourceList;
    private const int SE_SOURCE_NUM = 20;

    //全AudioClipを保持
    private Dictionary<string, AudioClip> _bgmDic, _seDic;

    private AudioMixer audioMixier;


    AudioSource se3d;


    //=================================================================================
    //初期化
    //=================================================================================

    private void Awake()
    {
        if (this != Instance)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        //オーディオリスナーおよびオーディオソースをSE+1(BGMの分)作成
        //gameObject.AddComponent<AudioListener>();
        for (int i = 0; i < SE_SOURCE_NUM + 1; i++)
        {
            gameObject.AddComponent<AudioSource>();
        }

        //作成したオーディオソースを取得して各変数に設定、ボリュームも設定
        AudioSource[] audioSourceArray = GetComponents<AudioSource>();
        _seSourceList = new List<AudioSource>();

        for (int i = 0; i < audioSourceArray.Length; i++)
        {
            audioSourceArray[i].playOnAwake = false;

            if (i == 0)
            {
                audioSourceArray[i].loop = true;
                _bgmSource = audioSourceArray[i];
                _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
            }
            else
            {
                _seSourceList.Add(audioSourceArray[i]);
                audioSourceArray[i].volume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFULT);
            }

        }

        //リソースフォルダから全SE&BGMのファイルを読み込みセット
        _bgmDic = new Dictionary<string, AudioClip>();
        _seDic = new Dictionary<string, AudioClip>();

        object[] bgmList = Resources.LoadAll(BGM_PATH);
        object[] seList = Resources.LoadAll(SE_PATH);
        audioMixier = Resources.Load<AudioMixer>(AudioMixer_PATH);

        foreach (AudioClip bgm in bgmList)
        {
            _bgmDic[bgm.name] = bgm;
        }
        foreach (AudioClip se in seList)
        {
            _seDic[se.name] = se;
        }

    }

    //=================================================================================
    //SE
    //=================================================================================

    /// <summary>
    /// 指定したファイル名のSEを流す。第三引数のdelayに指定した時間だけ再生までの間隔を空ける
    /// </summary>
    /// /// /// <param name="bgmName">再生するBGM AUDIO. で指定する</param>
    /// /// /// /// <param name="delay"></param>
    public void PlaySE(string seName, float vol = SE_VOLUME_DEFULT, float delay = 0.0f,float time = 0)
    {
        if (!_seDic.ContainsKey(seName))
        {
            Debug.Log(seName + "という名前のSEがありません");
            return;
        }

        foreach (AudioSource seSource in _seSourceList)
        {
            if (!seSource.isPlaying)
            {

                seSource.volume = vol;
                seSource.outputAudioMixerGroup = audioMixier.FindMatchingGroups("SE")[0];
            }

        }
        _nextSEName = seName;
        StartCoroutine(DelayPlaySE(_nextSEName, delay, time));
    }
    IEnumerator DelayPlaySE(string nextSEName, float delay,float time)
    {
        yield return new WaitForSeconds(delay);
        foreach (AudioSource seSource in _seSourceList)
        {
            if (!seSource.isPlaying)
            {
                seSource.clip = _seDic[nextSEName] as AudioClip;
                seSource.time = time;
                seSource.Play();
                //seSource.PlayOneShot(_seDic[nextSEName] as AudioClip);
                yield break;
            }

        }
    }

    /// <summary> 3DSEを再生します。 </summary>
    /// <param name="seName"> 再生するSE </param>
    /// <param name="gameObject"> 再生したい場所のオブジェクト（this.gameObject） </param>
    /// <param name="vol"> ボリューム </param>
    /// <param name="delay"> 指定した時間だけ再生までの間隔を空ける </param>
    /// <param name="maxDistance"> 音の聞こえる最大距離 </param>
    /// <param name="loop"> ループ再生したいか </param>
    /// <param name="doChild"> 生成場所を子にするかしないか </param>
    public void PlaySE(string seName, GameObject gameObject, float vol = SE_VOLUME_DEFULT, float delay = 0.0f, float maxDistance = 50, bool loop = false, bool doChild = true)
    {
        if (!_seDic.ContainsKey(seName))
        {
            Debug.Log(seName + "という名前のSEがありません");
            return;
        }



        _nextSEName = seName;

        if (loop)
        {
            //同じ名前のSEは再生しない(重複制御)
            foreach (Transform child in gameObject.transform)
            {
                AudioSource childAudio = child.GetComponent<AudioSource>();
                if (childAudio && childAudio.clip.name == seName)
                {
                    return;
                }
            }
        }
        //オブジェクトの生成
        GameObject se_E = new GameObject(seName);

        //再生する場所のオブジェクトの子にする
        if (doChild)
        {
            se_E.transform.parent = gameObject.transform;
            se_E.transform.position = gameObject.transform.position;
        }
        else
        {
            se_E.transform.position = gameObject.transform.position;
        }
        //生成したオブジェクトにAudioSourceをつける
        se_E.AddComponent<AudioSource>();
        se3d = se_E.GetComponent<AudioSource>();
        //クリップにseNameの音をつける
        se3d.clip = _seDic[_nextSEName] as AudioClip;
        //SE用のAudioMixierをつける
        se3d.outputAudioMixerGroup = audioMixier.FindMatchingGroups("SE")[0];
        //音量の調整
        se3d.volume = vol;
        //ループ再生するかしないか
        se3d.loop = loop;
        //3D再生にする
        se3d.spatialBlend = 1;
        //ドップラー効果をなしに
        se3d.dopplerLevel = 0;
        //3Dの最小距離を０に
        se3d.minDistance = 0;
        //3Dの最大距離を調整
        se3d.maxDistance = maxDistance;
        //3Dの距離減衰をCustomに
        se3d.rolloffMode = AudioRolloffMode.Custom;
        //再生の時間を遅らせる
        se3d.PlayDelayed(delay);
        //再生
        se3d.Play();

        //ループしないなら生成した音を削除
        if (loop == false)
        {
            Destroy(se_E, se3d.clip.length + delay);
        }

    }

    /// <summary> 3DSEを再生します。 </summary>
    /// <param name="seName"> 再生するSE </param>
    /// <param name="gameObject"> 再生したい場所のオブジェクト（this.gameObject） </param>
    /// <param name="vol"> ボリューム </param>
    /// <param name="pitch"> ピッチ </param>
    /// <param name="delay"> 指定した時間だけ再生までの間隔を空ける </param>
    /// <param name="maxDistance"> 音の聞こえる最大距離 </param>
    /// <param name="loop"> ループ再生したいか </param>
    /// <param name="doChild"> 生成場所を子にするかしないか </param>
    public void PlaySEPitch(string seName, GameObject gameObject, float vol = SE_VOLUME_DEFULT, float pitch = 1f, float delay = 0.0f, float maxDistance = 50, bool loop = false, bool doChild = true)
    {
        if (!_seDic.ContainsKey(seName))
        {
            Debug.Log(seName + "という名前のSEがありません");
            return;
        }



        _nextSEName = seName;

        if (loop)
        {
            //同じ名前のSEは再生しない(重複制御)
            foreach (Transform child in gameObject.transform)
            {
                AudioSource childAudio = child.GetComponent<AudioSource>();
                if (childAudio && childAudio.clip.name == seName)
                {
                    return;
                }
            }
        }
        //オブジェクトの生成
        GameObject se_E = new GameObject(seName);

        //再生する場所のオブジェクトの子にする
        if (doChild)
        {
            se_E.transform.parent = gameObject.transform;
            se_E.transform.position = gameObject.transform.position;
        }
        else
        {
            se_E.transform.position = gameObject.transform.position;
        }
        //生成したオブジェクトにAudioSourceをつける
        se_E.AddComponent<AudioSource>();
        se3d = se_E.GetComponent<AudioSource>();
        //クリップにseNameの音をつける
        se3d.clip = _seDic[_nextSEName] as AudioClip;
        //SE用のAudioMixierをつける
        se3d.outputAudioMixerGroup = audioMixier.FindMatchingGroups("SE")[0];
        //音量の調整
        se3d.volume = vol;
        //ループ再生するかしないか
        se3d.loop = loop;
        //ピッチの変更
        se3d.pitch = pitch;
        //3D再生にする
        se3d.spatialBlend = 1;
        //ドップラー効果をなしに
        se3d.dopplerLevel = 0;
        //3Dの最小距離を０に
        se3d.minDistance = 0;
        //3Dの最大距離を調整
        se3d.maxDistance = maxDistance;
        //3Dの距離減衰をCustomに
        se3d.rolloffMode = AudioRolloffMode.Custom;
        //再生の時間を遅らせる
        se3d.PlayDelayed(delay);
        //再生
        se3d.Play();

        //ループしないなら生成した音を削除
        if (loop == false)
        {
            Destroy(se_E, se3d.clip.length + delay);
        }

    }

    /// <summary> SEを停止する。(再生が終わってから止める) </summary>
    /// <param name="seName"> 止めたいSE </param>
    /// <param name="gameObject"> 再生してるSEの場所のオブジェクト </param>
    public void StopSE(string seName, GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            AudioSource childAudio = child.GetComponent<AudioSource>();
            if (childAudio && childAudio.clip.name == seName)
            {
                childAudio.loop = false;
                Destroy(child.gameObject, childAudio.clip.length);
            }
        }
    }

    /// <summary> SEをすぐに停止する。 </summary>
    /// <param name="seName"> 止めたいSE </param>
    /// <param name="gameObject"> 再生してるSEの場所のオブジェクト </param>
    public void StopNowSE(string seName, GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            AudioSource childAudio = child.GetComponent<AudioSource>();
            if (childAudio && childAudio.clip.name == seName)
            {
                Destroy(child.gameObject);
            }
        }
    }



    //=================================================================================
    //BGM
    //=================================================================================

    /// <summary>
    /// 指定したファイル名のBGMを流す。ただし既に流れている場合は前の曲をフェードアウトさせてから。
    /// 第二引数のfadeSpeedRateに指定した割合でフェードアウトするスピードが変わる
    /// </summary>
    /// /// /// <param name="bgmName">再生するBGM AUDIO. で指定する</param>
    /// /// /// /// <param name="vol">音量</param>
    /// /// /// /// /// <param name="fadeSpeedRate">何秒後にフェードアウトするか</param>
    public void PlayBGM(string bgmName, float vol = BGM_VOLUME_DEFULT, float fadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH)
    {
        if (!_bgmDic.ContainsKey(bgmName))
        {
            Debug.Log(bgmName + "という名前のBGMがありません");
            return;
        }

        //現在BGMが流れていない時はそのまま流す
        if (!_bgmSource.isPlaying)
        {
            _nextBGMName = "";
            _bgmSource.clip = _bgmDic[bgmName] as AudioClip;
            _bgmSource.outputAudioMixerGroup = audioMixier.FindMatchingGroups("BGM")[0];
            _bgmSource.Play();
            _bgmSource.volume = vol;
            _bgmVol = vol;
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, vol);
            _stopBGMCheck = false;
        }
        //違うBGMが流れている時は、流れているBGMをフェードアウトさせてから次を流す。同じBGMが流れている時はスルー
        else if (_bgmSource.clip.name != bgmName)
        {
            _nextBGMName = bgmName;
            FadeOutBGM(fadeSpeedRate);
        }
    }

    /// <summary>
    /// BGMをすぐに止める
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    /// <summary>
    /// 現在流れている曲をフェードアウトさせる
    /// fadeSpeedRateに指定した秒数でフェードアウトする
    /// </summary>
    /// <param name="fadeSpeedRate"></param>
    public void FadeOutBGM(float fadeSpeedRate = BGM_FADE_SPEED_RATE_LOW)
    {
        _bgmFadeSpeedRate = fadeSpeedRate;
        _isFadeOut = true;
    }

    /// <summary>
    /// BGMが止まったらTrueを返す
    /// </summary>
    /// <returns></returns>
    public bool StopBGMCheck()
    {
        return _stopBGMCheck;
    }

    private void Update()
    {
        if (!_isFadeOut)
        {
            return;
        }
        //徐々にボリュームを下げていき、ボリュームが0になったらボリュームを戻し次の曲を流す
        float rate = _bgmFadeSpeedRate;
        _bgmFadeSpeedRate -= Time.unscaledDeltaTime;
        _bgmSource.volume = Mathf.Lerp(0, _bgmSource.volume, _bgmFadeSpeedRate / rate);
        if (_bgmSource.volume <= 0)
        {
            _bgmSource.Stop();
            _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
            _isFadeOut = false;
            _stopBGMCheck = true;

            if (!string.IsNullOrEmpty(_nextBGMName))
            {
                PlayBGM(_nextBGMName, _bgmVol);
            }
        }

    }

    //=================================================================================
    //音量変更
    //=================================================================================

    /// <summary>
    /// BGMとSEのボリュームを別々に変更&保存
    /// </summary>
    public void ChangeVolume(float BGMVolume, float SEVolume)
    {
        _bgmSource.volume = BGMVolume;
        foreach (AudioSource seSource in _seSourceList)
        {
            seSource.volume = SEVolume;
        }

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);
    }

}