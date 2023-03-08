using System.Collections;
using System.Collections.Generic;
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

    public enum ShooterType
    {
        Player,             //プレイヤー
        Boss,               //ボス
        Common,             //雑魚敵
    }

    [Label("弾を撃つキャラ")]
    public ShooterType _shooterType;

    [Label("プレイヤー固有の弾か"), HideInInspector]
    public bool _isPlayersShot = false;

    [Label("ボス固有の弾か"), HideInInspector]
    public bool _isBossShot = false;

    [Label("弾の速度")]
    public float _speed = 3;

    public enum ShotType
    {
        [EnumLabel("弾の種類", "直進")]
        Straight,           //直進
        [EnumLabel("弾の種類", "カーブ")]
        Curve,              //カーブ
        [EnumLabel("弾の種類", "自機狙い")]
        TargetToPlayer,     //自機狙い
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
    public int _timeToSprrdChange = default;

    [Label("加減速カーブ"), HideInInspector]
    public AnimationCurve _speedCurve;          //加減速カーブ

    [Label("カーブ用縦軸オフセット"), Range(0.0f, 1.0f), HideInInspector]
    public float _verticalOffset = default;

    [Label("カーブ用横軸オフセット"), Range(-1.0f, 1.0f), HideInInspector]
    public float _horizontalOffset = default;

    [Label("デバッグモード")]
    public bool _isDebug = false;

}
