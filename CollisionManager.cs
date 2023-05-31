using System;
using System.Linq;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [SerializeField, Label("当たり判定サイズ"), Range(0.0f, 1.0f)]
    private float _colliderScale = 0.05f;

    private enum Faction
    {
        [EnumLabel("派閥", "プレイヤー")]
        Player,
        [EnumLabel("派閥", "敵")]
        Enemy,
    }

    [SerializeField, EnumElements(typeof(Faction))]
    private Faction _faction;

    private enum Type
    {
        [EnumLabel("キャラ / 弾", "キャラクター")]
        Charactor,
        [EnumLabel("キャラ / 弾", "弾")]
        Shot,
    }

    [SerializeField, EnumElements(typeof(Type))]
    private Type _type;

    [SerializeField, Label("敵弾のタグ"), TagFieldDrawer]
    private string _enemyShotTag = default;     //そのキャラにとっての敵の弾のタグを格納する変数

    private enum ColliderShape
    {
        [EnumLabel("当たり判定の形状","球")]
        Sphere,
        [EnumLabel("当たり判定の形状","正方形")]
        Square,
    }

    [SerializeField, EnumElements(typeof(ColliderShape))]
    private ColliderShape _selfColliderShape;

    private ColliderShape _shotColliderShape;

    [SerializeField,Label("四角形の頂点")]
    private Vector2[] _vertices; // 四角形の頂点

    private Vector2[] _normals; // 辺の法線ベクトル

    private PlayerMove _playerMove = default;   //プレイヤーのPlayerMoveの格納用変数

    private bool _isHit = false;                //被弾判定フラグ

    public int GetColliderShape
    {
        get { return (int)_selfColliderShape; }
    }

    public float GetColliderScale
    {
        //_colliderRadiusを返す
        get { return _colliderScale; }
    }

    public bool GetSetHitFlag
    {
        //_isHitを返す
        get { return _isHit; }

        //_isHitに受け取った値を入れる
        set { _isHit = value; }
    }

    private void Awake()
    {
        switch (_selfColliderShape)
        {
            case ColliderShape.Sphere:

                break;

            case ColliderShape.Square:


                //反時計回りに頂点座標を定義
                _vertices = new Vector2[4]
                {  
                    //左下
                    new Vector2(-_colliderScale, -_colliderScale),
                    //右下
                    new Vector2(_colliderScale, -_colliderScale),
                    //右上
                    new Vector2(_colliderScale, _colliderScale),
                    //左上
                    new Vector2(-_colliderScale, _colliderScale)
                };

                // 辺の法線ベクトルを計算する
                _normals = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    // 現在の頂点と次の頂点を取得する
                    Vector2 current = _vertices[i];
                    Vector2 next = _vertices[(i + 1) % 4];

                    // 辺のベクトルを計算する
                    Vector2 edge = next - current;

                    // 辺の法線ベクトルを計算する
                    _normals[i] = new Vector2(-edge.y, edge.x).normalized;
                }

                break;
        }

        //プレイヤーか?
        if (_faction == Faction.Player)
        {
            //プレイヤーである

            //プレイヤーのPlayerMoveコンポーネントを取得
            _playerMove = this.gameObject.GetComponent<PlayerMove>();
        }
    }

    private void Update()
    {
        //自分は弾か?
        if(_type == Type.Shot)
        {
            //弾である

            //何もしない
            return;
        }

        //以下判定処理

        //画面内の全敵弾を取得
        Transform[] enemyShotsInScene = GameObject.FindGameObjectsWithTag(_enemyShotTag).Select(enemyShot => enemyShot.transform).ToArray();

        //敵弾が一つもない
        if (enemyShotsInScene.Length == 0)
        {
            //処理をせず戻る
            return;
        }

        //取得した敵弾を距離の照準にソート
        Transform[] sortedByDistance =
                    enemyShotsInScene.OrderBy(enemyShots => Vector3.Distance(enemyShots.transform.position, transform.position)).ToArray();

        CollisionManager currentCollisionManager = sortedByDistance[0].gameObject.GetComponent<CollisionManager>();

        int shotColliderShape = currentCollisionManager.GetColliderShape;

        _shotColliderShape = (ColliderShape)Enum.ToObject(typeof(ColliderShape), shotColliderShape);

        float shotColliderScale = currentCollisionManager.GetColliderScale;

        //一番近い弾の当たり判定の半径 + 自身の当たり判定の半径を求める
        float sumOfColliderScale = shotColliderScale + this._colliderScale;


        switch (_shotColliderShape)
        {
            case ColliderShape.Sphere:

                //自身がプレイヤーかつ、無敵状態の場合
                if (_faction == Faction.Player && _playerMove.GetIsInvincible)
                {
                    //処理をしない
                    return;
                }

                //弾との距離が当たり判定の半径の合計よりも小さくなったら
                if (Vector3.Distance(sortedByDistance[0].position, transform.position) <= sumOfColliderScale && !_isHit)
                {
                    //被弾フラグをtrueに
                    _isHit = true;

                    //画面内の敵弾の配列を初期化
                    enemyShotsInScene = new Transform[0];

                    //距離順の配列を初期化
                    sortedByDistance = new Transform[0];
                }


                break;

            case ColliderShape.Square:


                //反時計回りに頂点座標を定義
                _vertices = new Vector2[4]
                {  
                    //左下
                    new Vector2(-shotColliderScale, -shotColliderScale),
                    //右下
                    new Vector2(shotColliderScale, -shotColliderScale),
                    //右上
                    new Vector2(shotColliderScale, shotColliderScale),
                    //左上
                    new Vector2(shotColliderScale, shotColliderScale)
                };

                // 辺の法線ベクトルを計算する
                _normals = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    // 現在の頂点と次の頂点を取得する
                    Vector2 current = _vertices[i];
                    Vector2 next = _vertices[(i + 1) % 4];

                    // 辺のベクトルを計算する
                    Vector2 edge = next - current;

                    // 辺の法線ベクトルを計算する
                    _normals[i] = new Vector2(-edge.y, edge.x).normalized;
                }


                // 各辺の距離を計算
                float[] distances = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    Vector2 current = _vertices[i];
                    Vector2 next = _vertices[(i + 1) % 4];
                    Vector2 edge = next - current;
                    Vector2 edgeNormal = _normals[i];
                    distances[i] = Vector2.Dot(this.transform.position - (Vector3)current, edgeNormal);
                }

                //自身がプレイヤーかつ、無敵状態の場合
                if (_faction == Faction.Player && _playerMove.GetIsInvincible)
                {
                    //処理をしない
                    return;
                }

                // 当たり判定が発生したかどうかを判断
                if (distances[0] <= 0 && distances[1] <= 0 && distances[2] <= 0 && distances[3] <= 0)
                {
                    //被弾フラグをtrueに
                    _isHit = true;

                    //画面内の敵弾の配列を初期化
                    enemyShotsInScene = new Transform[0];

                    //距離順の配列を初期化
                    sortedByDistance = new Transform[0];
                }

                break;
        }
    

    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>当たり判定のサイズを点で描画するメソッド デバッグ用</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        Gizmos.DrawSphere(this.gameObject.transform.position, _colliderScale);
#endif

    }
}
