using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/Create ShotMoveData")]
public class ShotMoveData : ScriptableObject
{

    [Label("弾")]
    public Sprite _shotSprite = default;

    [Label("誘導弾")]
    public bool _isLockOnShot = false;

    [Label("回転")]
    public bool _isSpinning = false;

    [Label("色違い")]
    public bool _hasAlternativeColor = false;

    [Label("2色目スプライト")]
    public Sprite _alternativeColorSprite = default;

    [Label("当たり判定大きさ"), Range(0.00f, 1.00f)]
    public float _colliderRadius = 0.05f;

    public enum ShooterType
    {
        Player,             //プレイヤー
        Boss,               //ボス
        Common,             //雑魚敵
    }

    [Label("弾を撃つキャラ")]
    public ShooterType _shooterType;

    [Label("初速")]
    public float _initialVelocity = 3;

    public enum ShotType
    {
        [EnumLabel("弾の種類", "直進")]
        Straight,           //直進
        [EnumLabel("弾の種類", "カーブ")]
        Curve,              //カーブ
        [EnumLabel("弾の種類", "追尾")]
        Homing,             //追尾
    }

    [Label("弾の種類"), EnumElements(typeof(ShotType))]
    public ShotType _shotType;

    public enum ShotSettings
    {
        [EnumLabel("特殊軌道", "無し")]
        Nomal,                           //特殊軌道無し
        [EnumLabel("特殊軌道", "加減速")]
        Acceleration_Deceleration,       //加減速
        [EnumLabel("特殊軌道", "レーザー")]
        Laser,                           //レーザー
    }

    [Label("特殊軌道"), EnumElements(typeof(ShotSettings))]
    public ShotSettings _shotSettings;

    [Label("加減速の時間")]
    public int _timeToSpeedChange = default;

    public enum ShotVelocity
    {
        [EnumLabel("弾の初速", "等速")]
        Nomal,
        [EnumLabel("弾の初速", "生成ごとに減速")]
        FastToSlow,
        [EnumLabel("弾の初速", "生成ごとに加速")]
        SlowToFast,
    }

    [EnumElements(typeof(ShotVelocity))]
    public ShotVelocity _shotVelocity;

    [Label("生成時の加減速具合"), Range(0.1f, 2.0f)]
    public float _shotVelocityRate = 1.5f;

    [Label("加減速カーブ"), HideInInspector]
    public AnimationCurve _speedCurve;          //加減速カーブ

    [Label("カーブ用縦軸オフセット"), Range(0.0f, 1.0f), HideInInspector]
    public float _curveShotVerticalOffset = default;

    [Label("カーブ用横軸オフセット"), Range(-1.0f, 1.0f), HideInInspector]
    public float _curveShotHorizontalOffset = default;

    [Label("デバッグモード")]
    public bool _isDebug = false;

}
