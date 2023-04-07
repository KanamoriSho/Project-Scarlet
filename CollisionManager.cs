using System.Linq;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [SerializeField, Label("当たり判定サイズ"), Range(0.0f, 1.0f)]
    private float _colliderRadius = 0.05f;

    public float GetColliderRadius
    {
        //_colliderRadiusを返す
        get { return _colliderRadius; }
    }

    [SerializeField, Label("プレイヤー")]
    private bool _isPlayer = false;             //プレイヤーか否かのフラグ

    private PlayerMove _playerMove = default;   //プレイヤーのPlayerMoveの格納用変数

    [SerializeField, Label("敵弾のタグ"), TagFieldDrawer]
    private string _enemyShotTag = default;     //そのキャラにとっての敵の弾のタグを格納する変数

    private bool _isHit = false;                //被弾判定フラグ

    public bool GetSetHitFlag
    {
        //_isHitを返す
        get { return _isHit; }

        //_isHitに受け取った値を入れる
        set { _isHit = value; }
    }

    private void Awake()
    {
        //プレイヤーか?
        if(_isPlayer)
        {
            //プレイヤーである

            //プレイヤーのPlayerMoveコンポーネントを取得
            _playerMove = this.gameObject.GetComponent<PlayerMove>();
        }
    }

    private void Update()
    {
        //画面内の全敵弾を取得
        Transform[] enemyShotsInScene = GameObject.FindGameObjectsWithTag(_enemyShotTag).Select(enemyShot => enemyShot.transform).ToArray();

        //敵弾が一つもない
        if(enemyShotsInScene.Length == 0)
        {
            //処理をせず戻る
            return;
        }

        //取得した敵弾を距離の照準にソート
        Transform[] sortedByDistance = 
                    enemyShotsInScene.OrderBy(enemyShots => Vector3.Distance(enemyShots.transform.position, transform.position)).ToArray();

        //一番近い弾の当たり判定の半径 + 自身の当たり判定の半径を求める
        float SumOfColliderRadius = sortedByDistance[0].gameObject.GetComponent<ShotMove>().GetColliderRadius + this._colliderRadius;

        //自身がプレイヤーかつ、無敵状態の場合
        if(_isPlayer && _playerMove.GetIsInvincible)
        {
            //処理をしない
            return;
        }

        //弾との距離が当たり判定の半径の合計よりも小さくなったら
        if (Vector3.Distance(sortedByDistance[0].position, transform.position) <= SumOfColliderRadius && !_isHit)
        {
            //被弾フラグをtrueに
            _isHit = true;

            //画面内の敵弾の配列を初期化
            enemyShotsInScene = new Transform[0];

            //距離順の配列を初期化
            sortedByDistance = new Transform[0];
        }

    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>当たり判定のサイズを点で描画するメソッド デバッグ用</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        Gizmos.DrawSphere(this.gameObject.transform.position, _colliderRadius);
#endif

    }
}
