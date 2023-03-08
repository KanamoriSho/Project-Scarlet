using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Create PlayerMoveData")]
public class PlayerMoveData : ScriptableObject
{
    [Label("キャラクター名")]
    public string _playerName;

    [Label("HP")]
    public int _maxHp;

    [Label("ショット時のSE")]
    public AudioClip _shotSoundEffect = default;

    [Label("移動スピード")]
    public int _speed;

    [Label("横の移動範囲")]
    public float _xLimitOfMoving = 5.0f;

    [Label("縦の移動範囲")]
    public float _yLimitOfMoving = 10.0f;

    [Label("弾")]
    public Sprite _sprite;

    [Label("秒間発射数"), Range(1, 100)]
    public int _shotPerSecond = default;

    [Label("プール用初期生成弾数")]
    public int _initiallyGeneratedShots = 50;
}
