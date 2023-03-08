using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMove : MonoBehaviour
{
    [Label("ショットムーブデータ")]
    public ShotMoveData _shotMoveData = default;             //弾軌道のスクリプタブルオブジェクト

    private SpriteRenderer _spriteRenderer = default;       //自身のSpriteRenderer格納用

    [SerializeField, Label("弾のターゲット")]
    private GameObject _target = default;

    private Vector2 _targetPosition = default;              //_targetのPosition格納用

    public Vector2 GetSetTargetPosition          //_targetのGet、Set用プロパティ
    {
        get { return _targetPosition; }
        set { _targetPosition = value; }
    }

    private float _speed = default;                 //速度格納用

    public float GetSetSpeed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    private float _time = default;                  //時間格納用

    [SerializeField]
    private GameObject _shooter = default;          //射手格納用

    public GameObject GetSetshooter         //_shooterのGet、Set用プロパティ
    {
        get { return _shooter; }
        set { _shooter = value; }
    }

    private string PLAYER_TAG = "Player";

    private string BOSS_TAG = "Boss";

    private Vector2 _shooterPosition = default;     //射手のPosition格納用

    private Vector2 _shotVector = default;          //弾の発射ベクトル格納用

    //以下の変数はOnDrowGizmosに受け渡すためにフィールド変数にしてますが、ローカル変数で事足りるのでいずれ消すかも

    private Vector2 _fixedRelayPoint = default;     //ベジェ曲線の中間点格納用

    private Vector2 _relayPointVector = default;    //射手 - _target間のベクトル格納用

    private Vector2 _relayPointY = default;         //_relayPointVector上の縦(Y)軸座標格納用

    private Vector2 _relayPointX = default;         //_relayPointVector上の横(X)軸座標格納用

    private void Awake()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>();          //SpriteRenderer取得

        _spriteRenderer.sprite = _shotMoveData._shotSprite;              //スプライト変更

        GetShooter();           //弾を撃つキャラを取得(プレイヤーとボスのみ)
    }

    private void OnEnable()
    {
        Reset();        //初期化処理

        if (_shotMoveData._isSpinning)        //弾の回転フラグがtrueか
        {
            this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));     //trueならランダムで回転させる
        }


    }

    /// <summary>
    /// <para>Reset</para>
    /// <para>変数初期化用メソッド デバッグ時にも呼び出せるようにメソッド化してます</para>
    /// </summary>
    private void Reset()
    {
        _shooterPosition = _shooter.transform.position;

        this.transform.position = _shooterPosition;

        GetTarget();
        _targetPosition = _target.transform.position;

        _time = 0;
    }

    private void GetTarget()
    {

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                GetSetTargetPosition = GetSetshooter.transform.GetChild(0).transform.GetChild(0).gameObject.transform.position;

                break;

            case ShotMoveData.ShotType.TargetToPlayer:

                _target = GameObject.FindGameObjectWithTag(PLAYER_TAG);

                break;

        }
    }

    private void OnBecameInvisible()
    {
        ObjectDisabler();
    }

    private void FixedUpdate()
    {
        _time += Time.deltaTime;

        Vector2 currentPos = this.transform.position;

        /*特殊軌道    主に速度関連
         * Nomal                     : 特殊軌道無し
         * Acceleration_Deceleration : 加減速
         * Laser                     : レーザー
         */

        switch (_shotMoveData._shotSettings)
        {
            case ShotMoveData.ShotSettings.Nomal:

                _speed = _shotMoveData._speed;       //速度をshotMoveDataから代入

                break;

            case ShotMoveData.ShotSettings.Acceleration_Deceleration:

                //アニメーションカーブの値 * shotMoveDataの_speedで算出した速度を代入
                _speed = _shotMoveData._speedCurve.Evaluate((float)_time / _shotMoveData._timeToSprrdChange) * _shotMoveData._speed;

                break;

            case ShotMoveData.ShotSettings.Laser:

                break;
        }

        /*ショットの種類
         * Straight       : 直進
         * Curve          : カーブ
         * TargetToPlayer : 自機狙い
         */

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:
                _shotVector = StraightShotCalculateVector(_shooterPosition, _targetPosition);
                currentPos += _shotVector * _speed * Time.deltaTime;

                break;

            case ShotMoveData.ShotType.Curve:

                if (_time < 1)
                {
                    currentPos = CalcuateBezierCurve();
                }
                else
                {
                    currentPos += _shotVector * _speed * Time.deltaTime;
                }


                break;

            case ShotMoveData.ShotType.TargetToPlayer:

                currentPos += StraightShotCalculateVector(_shooterPosition, _targetPosition) * _speed * Time.deltaTime;


                break;

        }

        this.transform.position = currentPos;
    }

    private void ObjectDisabler()
    {
        if (!_shotMoveData._isDebug)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            Reset();
        }
    }

    /// <summary>
    /// <para>StraightShotCalculateVector</para>
    /// <para>目標地点 - 発射地点間のベクトルを求める</para>
    /// </summary>
    /// <param name="shotPos">発射地点</param>
    /// <param name="targetPos">目標地点</param>
    /// <returns></returns>
    private Vector2 StraightShotCalculateVector(Vector2 shotPos, Vector2 targetPos)
    {

        Vector2 direction = (targetPos - shotPos).normalized;

        return direction;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>カーブ弾のベジェ曲線を生成・現在地点を算出するスクリプト</para>
    /// </summary>
    /// <returns>currentCurvePos = 算出されたベジェ曲線上の座標</returns>
    private Vector2 CalcuateBezierCurve()
    {
        //発射地点 - ターゲット間のベクトルを算出
        _relayPointVector = _shooterPosition - _targetPosition;

        //ベクトル上のオフセットした中間点を算出
        _relayPointY = Vector2.Lerp(_shooterPosition, _targetPosition, _shotMoveData._verticalOffset);

        /*弾軌道の左右値に応じて計算するベクトルの向きを変更する
         * 
         * 左に飛ばす場合は_relayPointVectorに対して左向きの垂直ベクトルに対して左右値をかける
         * 右に飛ばす場合は_relayPointVectorに対して右向きの垂直ベクトルに対して左右値をかける
         * 
         * _relayPointYで求めたベクトル上の中間地点をもとに垂直ベクトルを出す
         */

        if (_shotMoveData._horizontalOffset <= 0)        //左向き
        {
            _fixedRelayPoint = new Vector2(-_relayPointVector.y, _relayPointVector.x) * Mathf.Abs(_shotMoveData._horizontalOffset) + _relayPointY;
        }
        else if (_shotMoveData._horizontalOffset > 0)    //右向き
        {
            _fixedRelayPoint = new Vector2(_relayPointVector.y, -_relayPointVector.x) * Mathf.Abs(_shotMoveData._horizontalOffset) + _relayPointY;
        }

        Vector2 firstVec = Vector2.Lerp(_shooterPosition, _fixedRelayPoint, _time);

        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _targetPosition, _time);

        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _time);

        _shotVector = StraightShotCalculateVector(_fixedRelayPoint, _targetPosition);


        return currentCurvePos;
    }

    private void GetShooter()
    {
        switch (_shotMoveData._shooterType)
        {
            case ShotMoveData.ShooterType.Player:

                GetSetshooter = GameObject.FindGameObjectWithTag(PLAYER_TAG);

                break;

            case ShotMoveData.ShooterType.Boss:

                GetSetshooter = GameObject.FindGameObjectWithTag(BOSS_TAG);

                break;

            case ShotMoveData.ShooterType.Common:

                break;
        }


    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>ベジェ曲線の始点 - 中間点間、中間点 - 終点間を線で描画するメソッド デバッグ用</para>
    /// </summary>
    void OnDrawGizmos()
    {

#if UNITY_EDITOR


        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                Gizmos.DrawSphere(_shooterPosition, 0.1f);
                Gizmos.DrawSphere(_targetPosition, 0.1f);

                Gizmos.DrawLine(_shooterPosition, _targetPosition);

                break;

            case ShotMoveData.ShotType.Curve:

                Gizmos.DrawSphere(_shooterPosition, 0.1f);
                Gizmos.DrawSphere(_targetPosition, 0.1f);

                Gizmos.DrawSphere(_fixedRelayPoint, 0.2f);

                Gizmos.DrawSphere(_relayPointY, 0.1f);

                Gizmos.DrawLine(_shooterPosition, _fixedRelayPoint);

                Gizmos.DrawLine(_fixedRelayPoint, _targetPosition);

                break;

            case ShotMoveData.ShotType.TargetToPlayer:



                break;

        }

#endif

    }
}



