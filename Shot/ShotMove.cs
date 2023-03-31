using UnityEngine;

public class ShotMove : MonoBehaviour
{
    [Label("ショットムーブデータ")]
    public ShotMoveData _shotMoveData = default;             //弾軌道のスクリプタブルオブジェクト

    private SpriteRenderer _spriteRenderer = default;       //自身のSpriteRenderer格納用

    private Vector2 _targetPosition = default;              //_targetのPosition格納用

    private float _initialVelocity = default;

    private float _speed = default;                 //速度格納用

    private EnemyCharactorMove _charactorMove = default;

    private PlayerMove _playerMove = default;

    private Animator _animator = default;

    private float _time = default;                  //時間格納用

    private float _colliderRadius = default;        //当たり判定の大きさを格納

    public float GetColliderRadius
    {
        get { return _colliderRadius; }
    }

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

    private float _shotAngle = default;             //発射角格納用

    private int _shotCounter = default;             //その弾を発射した数を格納する(CharactorDataから受け取り)

    private int _maxShotCount = default;            //その弾の最大発射数を格納する(CharactorDataから受け取り)

    private int _pelletCounter = default;           //現在生成された弾の数を格納する(CharactorDataから受け取り)

    private int _maxPelletCount = default;          //同時に生成する弾の数を格納する(CharactorDataから受け取り)

    private bool _isDecerationPerShot = false;

    public float GetSetShotAngle          //_targetのGet、Set用プロパティ
    {
        get { return _shotAngle; }
        set 
        {
            _shotAngle = value;
            _shotVector = new Vector3(0, 0, _shotAngle); 
        }
    }

    //以下の変数はOnDrowGizmosに受け渡すためにフィールド変数にしてますが、ローカル変数で事足りるのでいずれ消すかも

    private Vector2 _fixedRelayPoint = default;     //ベジェ曲線の中間点格納用

    private Vector2 _relayPointVector = default;    //射手 - _target間のベクトル格納用

    private Vector2 _relayPointY = default;         //_relayPointVector上の縦(Y)軸座標格納用

    private void Awake()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>();          //SpriteRenderer取得

        _spriteRenderer.sprite = _shotMoveData._shotSprite;             //スプライト変更

        _animator = this.GetComponent<Animator>();

        this._colliderRadius = _shotMoveData._colliderRadius;           //当たり判定のサイズを設定

        GetShooter();           //弾を撃つキャラを取得(プレイヤーとボスのみ)

        //生成/発射ごとの加減速をする弾か?
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;
        }

        //この弾の射手は敵か?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //敵

            _charactorMove = _shooter.GetComponent<EnemyCharactorMove>();       //射手のEnemyCharactorMoveを取得

            _isDecerationPerShot = _charactorMove.GetIsDecelerationPerShoot;    //発射ごとに減速するかを取得
        }
        else
        {
            //プレイヤー

            _playerMove = _shooter.GetComponent<PlayerMove>();                  //PlayerMoveを取得

            _isDecerationPerShot = _playerMove.GetIsDecelerationPerShoot;       //発射ごとに減速するかを取得
        }
    }

    private void OnEnable()
    {
        Reset();        //初期化処理

        //弾の回転フラグがtrueか
        if (_shotMoveData._isSpinning)
        {
            this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));     //trueならランダムで回転させる
        }

        //2色目がある弾か
        if (_shotMoveData._hasAlternativeColor)
        {
            //2色目があるなら

            _shotCounter = _charactorMove.GetCurrentShotCount;

            Sprite currentShotSprite = default;

            //奇遇判定
            if(_shotCounter % 2 == 1)
            {
                //奇数なら

                _animator.SetBool("AltColor", false);             //スプライト変更
            }
            else
            {
                //遇数なら

                _animator.SetBool("AltColor", true);
            }

            _spriteRenderer.sprite = currentShotSprite;
        }

        //生成ごとに加速/減速しない弾なら
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;     //終了
        }

        //以下生成ごとに加減速する弾の場合

        //プレイヤーでは無い?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //プレイヤーではない

            //発射ごとに加減速するか?
            if (!_isDecerationPerShot)
            {
                //しない(生成ごとに加減速)
                _pelletCounter = _charactorMove.GetCurrentPelletCount;      //_charactorMoveから現在の生成弾数を受け取る

                _maxPelletCount = _charactorMove.GetMaxPelletCount;         //_charactorMoveから同時生成弾数を受け取る
            }
            else
            {
                //する
                _shotCounter = _charactorMove.GetCurrentShotCount;      //_charactorMoveから現在の生成弾数を受け取る

                _maxShotCount = _charactorMove.GetMaxShotCount;         //_charactorMoveから最大発射弾数を受け取る
            }
        }
        else
        {
            //プレイヤーである

            //発射ごとに加減速するか?
            if (!_isDecerationPerShot)
            {
                //しない(生成ごとに加減速)
                _pelletCounter = _playerMove.GetCurrentPelletCount;      //_playerMoveから現在の生成弾数を受け取る

                _maxPelletCount = _playerMove.GetMaxPelletCount;         //_playerMoveから同時生成弾数を受け取る
            }
            else
            {
                //する
                _shotCounter = _playerMove.GetCurrentShotCount;      //_playerMoveから現在の発射弾数を受け取る

                _maxShotCount = _playerMove.GetMaxShotCount;         //_playerMoveから最大発射弾数を受け取る
            }
        }
    }

    /// <summary>
    /// <para>Reset</para>
    /// <para>変数初期化用メソッド デバッグ時にも呼び出せるようにメソッド化してます</para>
    /// </summary>
    private void Reset()
    {
        _shooterPosition = _shooter.transform.position;                 //射手の座標を再設定

        this.transform.position = _shooterPosition;                     //自身を射手の座標に移動

        _time = 0;                                                      //経過時間をリセット

        //当たり判定の大きさが_shotMoveDataのものと異なるか
        if (this._colliderRadius != _shotMoveData._colliderRadius)
        {
            //異なる

            this._colliderRadius = _shotMoveData._colliderRadius;       //当たり判定のサイズを再設定
        }
    }

    /// <summary>
    /// <para>OnBecameInvisible</para>
    /// <para>画面外に出た弾を消す処理</para>
    /// </summary>
    private void OnBecameInvisible()
    {
        ObjectDisabler();
    }

    private void FixedUpdate()
    {
        _time += Time.deltaTime;                        //時間の加算

        Vector2 currentPos = this.transform.position;   //現在の自身の座標を取得

        //初速変化処理用ローカル変数

                int shotLength = 0;         //一度に生成する弾数 or 発射する弾数 + 1格納用

                int shotCount = 0;          //生成する弾の内現在何発目かを格納する

        /*弾の初速計算処理分け
         * 
         * Nomal      : 初速計算無し(デフォルト値そのまま)
         * FastToSlow : 発射 or 生成ごとに初速が減速
         * SlowToFast : 発射 or 生成ごとに初速が加速
         */

        switch (_shotMoveData._shotVelocity)
        {
            case ShotMoveData.ShotVelocity.Nomal:   //通常弾(初速変化なし)

                this._initialVelocity = _shotMoveData._initialVelocity;     //初速にデフォルト値の初速を設定

                break;

            case ShotMoveData.ShotVelocity.FastToSlow:      //発射 or 生成ごとに初速が減速


                //放射状に飛ぶ弾か?
                if (!_isDecerationPerShot)
                {
                    //違う

                    shotLength = _maxPelletCount + 1;         //一度に生成する弾数 + 1を格納

                    shotCount = _pelletCounter;               //現在何発目の生成か
                }
                else
                {
                    //放射状に飛ぶ弾

                    shotLength = _maxShotCount + 1;           //連射する弾数 + 1を格納

                    shotCount = _shotCounter;                 //現在何発目かを格納
                }

                //発射 or 生成ごとの減速値を算出
                float decelerationValue = _shotMoveData._initialVelocity / (shotLength * _shotMoveData._shotVelocityRate);

                //発射 or 生成ごとの減速値 * 発射・生成数を計算(デフォルトの初速から引く速度)
                float subtractionValue = decelerationValue * shotCount;

                //デフォルト値から引いた速度をこの弾の初速として格納
                this._initialVelocity = _shotMoveData._initialVelocity - subtractionValue;

                break;

            case ShotMoveData.ShotVelocity.SlowToFast:      //発射 or 生成ごとに初速が加速


                //放射状に飛ぶ弾か?
                if (!_isDecerationPerShot)
                {
                    //違う

                    shotLength = _maxPelletCount + 1;         //一度に生成する弾数 + 1を格納

                    shotCount = _pelletCounter;               //現在何発目の生成かを格納
                }
                else
                {
                    //放射状に飛ぶ弾

                    shotLength = _maxShotCount + 1;           //連射する弾数 + 1を格納

                    shotCount = _shotCounter;                 //現在何発目かを格納
                }

                //発射 or 生成ごとの減速値を算出
                float accelerationValue = _shotMoveData._initialVelocity / (shotLength * _shotMoveData._shotVelocityRate);

                //発射 or 生成ごとの減速値 * 発射・生成数を計算(デフォルトの初速に加算する速度)
                float additionValue = accelerationValue * shotCount;

                //デフォルト値から加算した速度をこの弾の初速として格納
                this._initialVelocity = _shotMoveData._initialVelocity + additionValue;

                break;
        }

        /*特殊軌道    主に速度関連
         * Nomal                     : 特殊軌道無し
         * Acceleration_Deceleration : 加減速
         * Laser                     : レーザー
         */

        switch (_shotMoveData._shotSettings)
        {
            case ShotMoveData.ShotSettings.Nomal:       //速度変化なし

                _speed = this._initialVelocity;       //速度に初速値を格納

                break;

            case ShotMoveData.ShotSettings.Acceleration_Deceleration:       //発射後に加減速

                //アニメーションカーブの値 * 初速値で算出した速度を格納
                _speed = _shotMoveData._speedCurve.Evaluate((float)_time / _shotMoveData._timeToSpeedChange) * this._initialVelocity;

                break;

            case ShotMoveData.ShotSettings.Laser:       //レーザー弾

                break;
        }

        /*ショットの飛び方
         * Straight       : 直進
         * Curve          : カーブ
         */

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                float radian = _shotAngle * (Mathf.PI / 180);
                _shotVector = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
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
    /// <returns>direction = 発射地点～ターゲット地点間のベクトル</returns>
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

        /*ベクトル上の0.0～1.0でオフセットした中間点を算出
         * 0.0 : 発射地点
         * 0.5 : 発射地点とターゲットの中間
         * 1.0 : ターゲットの座標
         */
        _relayPointY = Vector2.Lerp(_shooterPosition, _targetPosition, _shotMoveData._curveShotVerticalOffset);

        /*弾軌道の左右値に応じて計算するベクトルの向きを変更する
         * 
         * 左に飛ばす場合は_relayPointVectorに対して左向きの垂直ベクトルに対して左右値をかける
         * 右に飛ばす場合は_relayPointVectorに対して右向きの垂直ベクトルに対して左右値をかける
         * 
         * _relayPointYで求めたベクトル上の中間地点をもとに垂直ベクトルを出す
         */

        float horizontalAxisOffset = _shotMoveData._curveShotHorizontalOffset;      //ベクトルに対する横軸オフセット値を設定

        //左向き
        if (_shotMoveData._curveShotHorizontalOffset <= 0)
        {
            //発射地点～ターゲット間ベクトルに対する左向きに垂直なベクトルを求める
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            //ベクトル上の中間点を支点にベジェ曲線の中間点の座標を左にずらす
            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        //右向き
        else if (_shotMoveData._curveShotHorizontalOffset > 0)
        {
            //発射地点～ターゲット間ベクトルに対する右向きに垂直なベクトルを求める
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            //ベクトル上の中間点を支点にベジェ曲線の中間点の座標を右にずらす
            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        //発射地点～中間地点間でLerp移動
        Vector2 firstVec = Vector2.Lerp(_shooterPosition, _fixedRelayPoint, _time);

        //中間地点間～ターゲット座標間でLerp移動
        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _targetPosition, _time);

        //Lerp移動中の座標2つ間をLerp移動
        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _time);

        /*ターゲット地点を通り過ぎても真っすぐ飛ぶために中間地点～ターゲット地点間のベクトルを設定
         * 
         * ターゲット地点を通り過ぎた時点でここで設定したベクトルで飛んでいく
         */
        _shotVector = StraightShotCalculateVector(_fixedRelayPoint, _targetPosition);


        return currentCurvePos;     //ベジェ曲線上の現在の座標を返す
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
    /// <para>AnimEvent_Disable</para>
    /// <para>無効化処理(アニメーションイベント用)</para>
    /// </summary>
    public void AnimEvent_Disable()
    {
        this.gameObject.SetActive(false);       //自身を無効化
    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>ベジェ曲線の始点 - 中間点間、中間点 - 終点間を線で描画するメソッド デバッグ用</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        Gizmos.DrawSphere(this.transform.position,_colliderRadius);

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

        }

#endif

    }
}



