using System.Collections.Generic;
using UnityEngine;

#region 二次元配列表示用クラス

[System.Serializable]
public class CharactorShootingData
{
    //発射する弾の番号を格納するリスト
    [Label("弾番号")]
    public int[] _shots;

    //発射する弾数を格納するリスト
    [Label("ショット数"), Range(1, 40)]
    public int[] _shotCounts;

    //発射する弾の自機狙いの有無を格納するリスト
    [Label("自機狙い")]
    public bool[] _isTargetingPlayer;

    //発射する弾の自機狙いの有無を格納するリスト
    [Label("撃ちながら動くか")]
    public bool[] _isMoveingShootig;

    //発射する弾の自機狙いの有無を格納するリスト
    [Label("次の弾と同時に撃つか")]
    public bool[] _isShotInSameTime;

    [Label("秒間発射数"), Range(1, 100)]
    public int[] _shotPerSeconds = default;

    public enum ShotPatern
    {
        [EnumLabel("弾の撃ち方", "単発")]
        OneShot,
        [EnumLabel("弾の撃ち方", "一度に")]
        AllAtOnce,
        [EnumLabel("弾の撃ち方", "複数発")]
        MultipleShots,
        [EnumLabel("弾の撃ち方", "放射状")]
        RadialShots,
    }

    [Label("弾の撃ち方"), EnumElements(typeof(ShotPatern))]
    public List<ShotPatern> _shotPaterns = new List<ShotPatern>();

    [Label("同時生成弾数")]
    public int[] _pelletCountInShots = default;

    [Label("回転撃ちをする?")]
    public bool[] _isSwingShots = default;

    [Label("回転撃ち時の散布角")]
    public int[] _swingShotFormedAngles = default;

    [Label("回転撃ち時の初期角")]
    public int[] _swingShotFirstAngles = default;

    [Label("同時生成時の右端～左端間の角度")]
    public int[] _multiShotFormedAngles = default;

    [Label("発射ごとに減速するか")]
    [Tooltip("true : 同時生成をするごとに減速 連射の際に一発ずつ減速　\n false: 同時生成の際に一発ずつ減速 連射の際は発射ごとの減速はしない")]
    public bool[] _isDecelerationPerShoot = default;
}

#endregion

[CreateAssetMenu(menuName = "ScriptableObject/Create CharactorMoveData")]
public class CharactorMoveData : ScriptableObject
{
    [Label("キャラクター名")]
    public string _charactorName;

    [Label("HP")]
    public int _maxHp;

    [Label("ショット時のSE")]
    public AudioClip _shotSoundEffect = default;

    [Label("移動スピード")]
    public int _speed;

    [Label("ウェーブ数")]
    public int _waveCount;

    [Label("このキャラが使用する弾")]
    public List<GameObject> _shots = new List<GameObject>();

    [Label("弾の撃ち方")]
    public List<CharactorShootingData> _movementAndShootingPaterns = new List<CharactorShootingData>();

    public bool _isCurveMoving = false;

    [Label("移動用加速度カーブ")]
    public AnimationCurve _speedCurve = default;

    [Label("横の移動範囲")]
    public float _xLimitOfMoving = 5.0f;

    [Label("縦の移動範囲")]
    public float _yLimitOfMoving = 10.0f;

    [Label("弾")]
    public Sprite _sprite;

    [Label("プール用初期生成弾数")]
    public int _initiallyGeneratedShots = 50;

    [Label("カーブ弾用縦軸オフセット"), Range(0.0f, 1.0f)]
    public float _curveMoveVerticalOffset = default;

    [Label("カーブ弾用横軸オフセット"), Range(-1.0f, 1.0f)]
    public float _curveMoveHorizontalOffset = default;
}
