using System.Linq;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [SerializeField, Label("当たり判定サイズ"), Range(0.0f, 1.0f)]
    private float _colliderRadius = 0.05f;

    public float GetColliderRadius
    {
        get { return _colliderRadius; }
    }

    [SerializeField, Label("プレイヤー")]
    private bool _isPlayer = false;

    private PlayerMove _playerMove = default;

    [SerializeField, Label("敵弾のタグ"), TagFieldDrawer]
    private string _enemyShotTag = default;

    private bool _isHit = false;

    private bool _isInvincible = false;

    public bool GetSetHitFlag
    {
        get { return _isHit; }
        set { _isHit = value; }
    }

    private void Awake()
    {
        if(_isPlayer)
        {
            _playerMove = this.gameObject.GetComponent<PlayerMove>();
        }
    }

    private void Update()
    {

        Transform[] enemyShotsInScene = GameObject.FindGameObjectsWithTag(_enemyShotTag).Select(enemyShot => enemyShot.transform).ToArray();

        if(enemyShotsInScene.Length == 0)
        {
            return;
        }

        Transform[] sortedByDistance = enemyShotsInScene.OrderBy(enemyShots => Vector3.Distance(enemyShots.transform.position, transform.position)).ToArray();

        float SumOfColliderRadius = sortedByDistance[0].gameObject.GetComponent<ShotMove>().GetColliderRadius + this._colliderRadius;

        if(_isPlayer && _playerMove.GetIsInvincible)
        {
            return;
        }

        if (Vector3.Distance(sortedByDistance[0].position, transform.position) <= SumOfColliderRadius)
        {
            _isHit = true;

            enemyShotsInScene = new Transform[0];

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
