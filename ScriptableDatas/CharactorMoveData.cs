using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotsList
{
    [Label("ショット")]
    public GameObject[] _shots;
}

[System.Serializable]
public class ShotCountList
{
    [Label("ショット数")]
    public int[] _shotCounts;
}

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

    [Label("ウェーブ")]
    public ShotsList[] _waves = default;

    [Label("ウェーブ")]
    public ShotCountList[] _shotCounts = default;

    public enum MovementPatern
    {
        [EnumLabel("移動の仕方", "移動のみ")]
        OnlyMove,
        [EnumLabel("移動の仕方", "撃ちながら")]
        ShootingWhileMoving,
    }

    [Label("移動の仕方"), EnumElements(typeof(MovementPatern))]
    public List<MovementPatern> _movementPaterns = new List<MovementPatern>();

    public bool _isCurve = false;

    [Label("移動用加速度カーブ")]
    public AnimationCurve _speedCurve = default;

    [Label("横の移動範囲")]
    public float _xLimitOfMoving = 5.0f;

    [Label("縦の移動範囲")]
    public float _yLimitOfMoving = 10.0f;

    [Label("弾")]
    public Sprite _sprite;

    [Label("秒間発射数"), Range(1, 100)]
    public int[] _shotPerSeconds = default;

    [Label("プール用初期生成弾数")]
    public int _initiallyGeneratedShots = 50;

    [Label("カーブ用縦軸オフセット"), Range(0.0f, 1.0f)]
    public float _verticalOffset = default;

    [Label("カーブ用横軸オフセット"), Range(-1.0f, 1.0f)]
    public float _horizontalOffset = default;

    public enum ShotPatern
    {
        [EnumLabel("弾の撃ち方", "単発")]
        OneShot,
        [EnumLabel("弾の撃ち方", "複数発")]
        MultipleShots,
        [EnumLabel("弾の撃ち方", "放射状")]
        RadialShots,
    }

    [Label("弾の撃ち方"), EnumElements(typeof(ShotPatern))]
    public List<ShotPatern> _shotPaterns = new List<ShotPatern>();

    public enum ShotVelocity
    {
        [EnumLabel("弾の速度", "等速")]
        Nomal,
        [EnumLabel("弾の速度", "徐々に減速")]
        FastToSlow,
        [EnumLabel("弾の速度", "徐々に加速")]
        SlowToFast,

    }

    [Label("弾の撃ち方"), EnumElements(typeof(ShotVelocity))]
    public List<ShotVelocity> _shotVelocity = new List<ShotVelocity>();

    [HideInInspector]
    public int _pelletCountInShot = 2;

    [HideInInspector]
    public int _formedAngle = 30;
}
