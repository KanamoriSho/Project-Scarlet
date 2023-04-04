using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Create PlayerMoveData")]
public class PlayerMoveData : ScriptableObject
{
    [Label("キャラクター名")]
    public string _charactorName;

    [Label("HP")]
    public int _maxHp;

    [Label("ショット時のSE")]
    public AudioClip _shotSoundEffect = default;

    [Label("被弾時のSE")]
    public AudioClip _hitSoundEffect = default;

    [Label("移動スピード")]
    public int _speed;

    [Label("このキャラが使用する弾")]
    public List<GameObject> _shots = new List<GameObject>();

    //発射する弾数を格納するリスト
    [Label("ショット数"), Range(1, 10)]
    public int[] _shotCounts;

    //発射する弾の自機狙いの有無を格納するリスト
    [Label("自機狙い")]
    public bool[] _isTargetingEnemy;

    [Label("当たり判定のサイズ")]
    public float _colliderRadius = 0.1f;

    [Label("被弾後無敵時間")]
    public float _afterHitInvincibleTime = 5.0f;

    [Label("ボムのクールタイム")]
    public float _bombCoolTime = 5.0f;

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
        [EnumLabel("弾の撃ち方", "一度に")]
        AllAtOnece,
        [EnumLabel("弾の撃ち方", "複数発")]
        MultipleShots,
        [EnumLabel("弾の撃ち方", "放射状")]
        RadialShots,
    }

    [Label("弾の撃ち方"), EnumElements(typeof(ShotPatern))]
    public List<ShotPatern> _shotPaterns = new List<ShotPatern>();

    [Label("一度に生成する弾数")]
    public int[] _pelletCountInShots = default;

    [Label("発射角")]
    public int[] _formedAngles = default;

    [Label("発射ごとに減速するか")]
    [Tooltip("true : 同時生成をするごとに減速 連射の際に一発ずつ減速　\n false: 同時生成の際に一発ずつ減速 連射の際は一発ごとの減速はしない")]
    public bool[] _isDecelerationPerShoot = default;
}
