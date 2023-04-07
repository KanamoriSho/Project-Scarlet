using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Label("キャラムーブデータ")]
    private PlayerMoveData _playerMoveData = default;

    [SerializeField, Label("ショット用プール")]
    private GameObject[] _shotPools = default;

    [SerializeField, Label("ボムエフェクトのオブジェクト")]
    private GameObject _bombShockWave = default;

    private Transform _playerTransform = default;           //自身のTransform格納用

    private Animator _animator = default;                   //自身のAnimtor格納用

    private AudioSource _audioSource = default;             //自身のAudioSource格納用

    private CollisionManager _collisionManger = default;    //自身のCollisionManager格納用

    private Animator _bombAnimator = default;               //ボムエフェクトのAnimator格納用

    private int _currentShotNumber = 0;                     //現在の発射する弾の番号を格納する変数

    private int _currentShotCount = 0;                      //何回その弾を撃ったかを格納する変数

    private int _maxShotCount = 0;                          //その弾を何発撃つかを格納する変数

    private int _currentPelletCount = 0;                    //発射する弾の現在の生成数を格納する変数

    private int _maxPelletCount = 0;                        //発射する弾の同時生成数を格納する変数

    private float _multiShotOffsetAngle = default;          //複数方向に発射する場合の発射角を格納する変数

    private float _offsetAngle = default;                   //複数方向に発射する場合の発射角を格納する変数

    private bool _isShotInterval = false;                   //射撃インターバル中判定フラグ

    PlayerMoveData.ShotPatern _currentShotPatern = default; //弾の撃ち方を格納するEnum

    private bool _isInvincible = false;                     //無敵フラグ

    private bool _isBombCoolTime = false;                   //ボム使用後クールタイム判定フラグ

    private Vector2 _startPosition = new Vector2(0, -3);            //初期座標

    private Vector2 _targetingPosition = default;                   //狙っている座標格納用(発射角計算用)

    private WaitForSeconds _shotInterval = default;                 //弾の連射速度を管理するコルーチンのインターバル

    private WaitForSeconds _invincibleTime = default;               //無敵時間を管理するコルーチンのインターバル

    private WaitForSeconds _disableEnemyShotsInterval = default;    //ボム使用から弾が消えるまでのインターバル

    private WaitForSeconds _bombInterval = default;                 //ボム使用から弾が消えるまでのインターバル

    #region Getter Setter

    public int GetCurrentShotCount
    {
        //_currentShotCountを返す
        get { return _currentShotCount; }
    }

    public int GetMaxShotCount
    {
        //_maxShotCountを返す
        get { return _maxShotCount; }
    }

    public int GetCurrentPelletCount
    {
        //_currentPelletCountを返す
        get { return _currentPelletCount; }
    }

    public int GetMaxPelletCount
    {
        //_maxPelletCountを返す
        get { return _maxPelletCount; }
    }

    public bool GetIsDecelerationPerShoot
    {
        get { return _playerMoveData._isDecelerationPerShoot[_currentShotNumber]; }
    }

    public bool GetIsInvincible
    {
        get { return _isInvincible; }
    }

    #endregion

    private const float SECOND = 1.0f;                              //一秒の定数

    private const float BOMB_SHOT_DISABLE_TIME = 0.1f;              //ボムが敵弾を無効化するまでの時間

    private const float SLOW_MOVING_RATE = 0.5f;                    //スロー移動時の減速率

    private const string ANIMATION_BOOL_LEFT_MOVE = "LeftMoving";   //左移動のAnimatorのBool名

    private const string ANIMATION_BOOL_RIGHT_MOVE = "RightMoving"; //右移動のAnimatorのBool名

    private const string ANIMATION_BOOL_SIDE_MOVING = "SideMoving"; //横移動のAnimatorのBool名

    private void Awake()
    {
        //自身のAnimatorの取得
        _animator = this.GetComponent<Animator>();

        //自身のTransformを取得
        _playerTransform = this.transform;

        //自身のAudioSourceの取得
        _audioSource = this.GetComponent<AudioSource>();

        //自身のCollisionManagerコンポーネントの取得
        _collisionManger = this.GetComponent<CollisionManager>();

        //ボム用エフェクトオブジェクトの取得
        _bombShockWave = GameObject.FindGameObjectWithTag("BombEffect");

        //ボム用エフェクトのAnimatorを取得
        _bombAnimator = _bombShockWave.GetComponent<Animator>();

        //ボム用エフェクトを無効化
        _bombShockWave.SetActive(false);

        //弾発射～発射間の待ち時間をキャッシュ
        _shotInterval = new WaitForSeconds(SECOND / (float)_playerMoveData._shotPerSeconds[_currentShotNumber]);

        //無敵時間をキャッシュ
        _invincibleTime = new WaitForSeconds(_playerMoveData._afterHitInvincibleTime);

        //敵弾無効化待機時間のキャッシュ
        _disableEnemyShotsInterval = new WaitForSeconds(BOMB_SHOT_DISABLE_TIME);

        //ボムの使用インターバルのキャッシュ
        _bombInterval = new WaitForSeconds(_playerMoveData._bombCoolTime);

        /*弾をプールに生成する
         * _charactorMoveData._waves                   : ウェーブ数(ボスキャラ以外は1)
         * _charactorMoveData._initiallyGeneratedShots : 初期生成弾数(スクリプタブルオブジェクトから受け取り)
         */

        //使用弾の種類分ループ
        for (int shotNumber = 0; shotNumber < _playerMoveData._shots.Count; shotNumber++)
        {
            //使用される弾を生成するループ
            for (int shotCounter = 0; shotCounter < _playerMoveData._initiallyGeneratedShots; shotCounter++)
            {
                //弾の生成
                GameObject newShot = Instantiate(_playerMoveData._shots[shotNumber], _shotPools[shotNumber].transform);

                //生成した弾をfalseにする
                newShot.SetActive(false);
            }
        }
    }

    void Update()
    {
        //X軸の入力値格納用
        float movingXInput = default;

        //Y軸の入力値格納用
        float movingYInput = default;

        //_playerMoveData._speed : プレイヤーの移動速度(スクリプタブルオブジェクトから受け取り)

        //右入力
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //X軸の入力値にプレイヤーの速度を入れる      
            movingXInput = _playerMoveData._speed;

            //Animatorの「左移動中」boolをfalseに
            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);

            //Animatorの「右移動中」boolをtrueに
            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, true);

            //Animatorの「横移動中」boolをtrueに
            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);
        }
        //左入力
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //X軸の入力値に(プレイヤーの速度 * -1)を入れる
            movingXInput = -_playerMoveData._speed;

            //Animatorの「左移動中」boolをtrueに
            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, true);

            //Animatorの「左移動中」boolをfalseに
            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);

            //Animatorの「横移動中」boolをtrueに
            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);
        }
        //左右入力無し
        else
        {
            //X軸の入力値を0に
            movingXInput = 0;

            //Animatorの「左移動中」boolをfalseに
            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);

            //Animatorの「左移動中」boolをfalseに
            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);

            //Animatorの「横移動中」boolをfalseに
            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, false);
        }

        //上入力
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //Y軸の入力値にプレイヤーの速度を入れる   
            movingYInput = _playerMoveData._speed;
        }
        //下入力
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //Y軸の入力値に(プレイヤーの速度 * -1)を入れる 
            movingYInput = -_playerMoveData._speed;
        }

        //左シフト入力
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //低速移動

            //X軸の移動入力値を0.5倍する
            movingXInput = movingXInput * SLOW_MOVING_RATE;

            //Y軸の移動入力値を0.5倍する
            movingYInput = movingYInput * SLOW_MOVING_RATE;
        }

        //最終的な移動入力値を移動処理に送る
        Move(movingXInput, movingYInput);

        //Z入力
        if (Input.GetKey(KeyCode.Z))
        {
            //弾発射処理
            Shot();
        }

        //無敵状態か?
        if (_isInvincible)
        {
            //無敵

            //処理終了
            return;
        }

        //被弾したか?
        if (_collisionManger.GetSetHitFlag)
        {
            //した

            //被弾コルーチンを開始
            StartCoroutine(OnHit());

            //CollisionManagerの被弾フラグをfalseに
            _collisionManger.GetSetHitFlag = false;
        }
    }

    /// <summary>
    /// <para>Shot</para>
    /// <para>弾の発射処理。 飛び方、角度等を設定する</para>
    /// </summary>
    private void Shot()
    {
        //インターバル中か
        if (_isShotInterval)
        {
            //インターバル中

            //何もしない
            return;
        }

        //発射角の初期化
        _multiShotOffsetAngle = 0;

        //現在の弾の撃ち方を格納(enum)
        _currentShotPatern = _playerMoveData._shotPaterns[_currentShotNumber];

        //格納した撃ち方をもとに処理分け
        switch (_currentShotPatern)           //弾の撃ち方
        {
            //単発発射
            case PlayerMoveData.ShotPatern.OneShot:

                #region 単発発射
                //弾の有効化 or 生成
                EnableShot();

                #endregion

                break;

            //単方向同時発射
            case PlayerMoveData.ShotPatern.AllAtOnce:

                #region 同時発射
                //同時生成弾数を取得
                _maxPelletCount = _playerMoveData._pelletCountInShots[_currentShotNumber];

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount <= _maxPelletCount; pelletCount++)
                {
                    //ループ数を現在の生成弾数として渡す
                    _currentPelletCount = pelletCount;

                    //弾の有効化 or 生成
                    EnableShot();
                }

                #endregion

                break;

            //扇形同時発射
            case PlayerMoveData.ShotPatern.MultipleShots:

                #region 扇形同時発射

                //同時生成弾数を取得
                _maxPelletCount = _playerMoveData._pelletCountInShots[_currentShotNumber];

                //最大発射角
                float maxOffset = 0;

                //現在の発射角
                float currentAngle = 0;

                //弾の散布角を取得
                float formedAngle = _playerMoveData._multiShotFormedAngles[_currentShotNumber];

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    //ループ数を現在の生成弾数として渡す
                    _currentPelletCount = pelletCount;

                    //初弾か?
                    if (pelletCount == 0)
                    {
                        //初弾

                        //散布角から正面を基準にした最大発射角を算出
                        maxOffset = formedAngle / 2;

                        //最大発射角を代入
                        _multiShotOffsetAngle = -maxOffset;

                        //弾と弾の間の角度を算出
                        currentAngle = formedAngle / (_maxPelletCount - 1);
                    }
                    else
                    {
                        //2発目以降

                        //初弾で設定した発射角に加算
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentAngle;
                    }

                    //弾の有効化 or 生成
                    EnableShot();
                }

                #endregion

                break;

            //放射状発射
            case PlayerMoveData.ShotPatern.RadialShots:

                #region 放射状発射

                //ショット～ショット間の角度格納用
                float currentRadialAngle = 0;

                //同時生成弾数を取得
                _maxPelletCount = _playerMoveData._pelletCountInShots[_currentShotNumber];

                //同時生成弾数分ループ
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    //ループ数を現在の生成弾数として渡す
                    _currentPelletCount = pelletCount;

                    if (pelletCount == 0)       //初弾の場合
                    {
                        //ずらし角の初期化
                        _multiShotOffsetAngle = 0;

                        //弾と弾の間の角度を算出
                        currentRadialAngle = 360 / _maxPelletCount;
                    }
                    else
                    {
                        //最初に設定した発射角に加算
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentRadialAngle;
                    }

                    //弾の有効化 or 生成
                    EnableShot();
                }

                #endregion

                break;
        }

        //現在の生成弾数の初期化
        _currentPelletCount = 0;

        //インターバル処理
        RateOfShot();

        //撃った弾数を加算
        _currentShotCount++;

    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>発射する弾に対応したプールを探索し、未使用の弾があればその弾を有効化。無ければ新たにプール内に生成する</para>
    /// </summary>
    private void EnableShot()
    {
        //オブジェクトプール内に未使用オブジェクトが無いか捜索
        foreach (Transform shot in _shotPools[_currentShotNumber].transform)
        {
            //未使用オブジェクトを見つけたか
            if (!shot.gameObject.activeSelf)
            {
                //未使用オブジェクトがあった

                //見つけた弾を有効化
                shot.gameObject.SetActive(true);

                //弾種の判定
                CheckShotType(shot);

                //trueにした弾をプレイヤーの位置に移動
                shot.position = this.transform.position;

                //処理を終了
                return;
            }
        }

        //以下未使用オブジェクトが無かった場合新しく弾を生成

        //取得した弾オブジェクトを対応するプールの子オブジェクトとして生成
        GameObject newShot = Instantiate(_playerMoveData._shots[_currentShotNumber], _shotPools[_currentShotNumber].transform);

        //弾種の判定
        CheckShotType(newShot.transform);

        //生成した弾をキャラクターの位置に移動
        newShot.transform.position = this.transform.position;
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    /// <returns>_shotInterval : インターバル時間</returns>
    IEnumerator RateOfShot()
    {
        //弾SEがあるか?
        if (_playerMoveData._shotSoundEffect != null)
        {
            //ある

            _audioSource.PlayOneShot(_playerMoveData._shotSoundEffect);     //弾発射SEを再生
        }

        _isShotInterval = true;         //弾発射待機フラグをtrueに

        yield return _shotInterval;     //待機時間分待機

        _isShotInterval = false;        //弾発射待機フラグをtrueに

    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>弾の種類を判定する。自機狙いフラグが立っている場合に発射角にプレイヤーとのベクトル角を加算する</para>
    /// </summary>
    /// <param name="shot">Shotメソッドで有効化/生成された弾。オブジェクトプール探索の際にTransform型で取得するためTransform型</param>
    private void CheckShotType(Transform shot)
    {
        if (!_playerMoveData._isTargetingEnemy[_currentShotNumber])
        {
            _targetingPosition = _playerTransform.position + Vector3.up;
        }
        else
        {
            //一番近くの敵を取得してターゲットとして取得する処理
        }

        Vector2 degree = _targetingPosition - (Vector2)this.transform.position;


        float radian = Mathf.Atan2(degree.y, degree.x);

        shot.GetComponent<ShotMove>().GetSetShotAngle = radian * Mathf.Rad2Deg + _offsetAngle;
    }

    /// <summary>
    /// <para>Move</para>
    /// <para>移動処理</para>
    /// </summary>
    /// <param name="horizontalInput">X軸入力</param>
    /// <param name="verticalInput">Y軸入力</param>
    private void Move(float horizontalInput, float verticalInput)
    {
        if (horizontalInput == 0 && verticalInput == 0)     //移動入力が無い場合
        {
            return;     //何もしない
        }

        Vector2 playerPosition = _playerTransform.position;      //プレイヤーの現在位置を取得

        //現在位置に移動入力と移動スピードを加算する
        playerPosition = new Vector2(playerPosition.x + horizontalInput * Time.deltaTime,
                                        playerPosition.y + verticalInput * Time.deltaTime);

        /*
         *  if(右 or 上の移動制限を超えたか)
         *      右 or 上の移動制限内に戻す
         *  else if(左 or 下の移動制限を超えたか)
         *      左 or 下の移動制限内に戻す
         */

        //右の移動範囲制限を超えたか
        if (playerPosition.x > _playerMoveData._xLimitOfMoving)
        {
            //超えた

            playerPosition.x = _playerMoveData._xLimitOfMoving;     //現在の座標をX軸の移動上限値に戻す
        }
        //左の移動範囲制限を超えたか
        else if (playerPosition.x < -_playerMoveData._xLimitOfMoving)
        {
            //超えた

            playerPosition.x = -_playerMoveData._xLimitOfMoving;     //現在の座標をX軸の(移動上限値 * -1)に戻す
        }

        //上の移動範囲制限を超えたか
        if (playerPosition.y > _playerMoveData._yLimitOfMoving)
        {
            //超えた

            playerPosition.y = _playerMoveData._yLimitOfMoving;     //現在の座標をY軸の移動上限値に戻す
        }
        //下の移動範囲制限を超えたか
        else if (playerPosition.y < -_playerMoveData._yLimitOfMoving)
        {
            //超えた

            playerPosition.y = -_playerMoveData._yLimitOfMoving;     //現在の座標をY軸の(移動上限値 * -1)に戻す
        }

        _playerTransform.position = playerPosition;         //移動した座標をプレイヤーに反映する
    }


    /// <summary>
    /// <para>Bomb</para>
    /// <para>ボム使用時処理(被弾時にも呼ぶ)</para>
    /// </summary>
    /// <returns></returns>
    IEnumerator Bomb()
    {
        if(_isBombCoolTime)
        {
            yield break;
        }

        _bombShockWave.SetActive(true);                                     //ボムエフェクトを有効化

        _bombShockWave.transform.position = _playerTransform.position;      //ボムエフェクトをプレイヤーの座標に移動

        _bombAnimator.SetTrigger("Enable");                                 //ボムエフェクトAnimatorの起動トリガーをオンに

        yield return _disableEnemyShotsInterval;                            //ボム発動から敵の弾が消えるまでのインターバル待機

        GameObject[] enemyShotsInPicture = GameObject.FindGameObjectsWithTag("EnemyShot");      //現在有効化されている敵の弾を全て取得

        //先ほど取得した敵の弾の数だけループする
        for(int shotCount = 0; shotCount < enemyShotsInPicture.Length; shotCount++)
        {
            enemyShotsInPicture[shotCount].GetComponent<Animator>().SetTrigger("Disable");      //Animatorの「無効化」トリガーをオンに
        }

        StartCoroutine(BombCoolTime());
    }

    IEnumerator BombCoolTime()
    {
        _isBombCoolTime = true;

        yield return _bombInterval;

        _isBombCoolTime = false;
    }

    /// <summary>
    /// <para>OnHit</para>
    /// <para>当たり判定 CollisionManagerからでた_isHitフラグのtrueで呼び出し</para>
    /// </summary>
    IEnumerator OnHit()
    {
        _isInvincible = true;

        _audioSource.PlayOneShot(_playerMoveData._hitSoundEffect);

        _animator.SetTrigger("Hit");

        StartCoroutine(Bomb());

        StartCoroutine(Interval(_invincibleTime));

        yield return _invincibleTime;

        _isInvincible = false;


    }

    /// <summary>
    /// <para>Interval</para>
    /// <para>インターバル処理用のコルーチン</para>
    /// </summary>
    /// <param name="intervalTime">待機時間をキャッシュしたWaitForSeconds</param>
    /// <returns>intervalTime 指定秒数待機して返す</returns>
    IEnumerator Interval(WaitForSeconds intervalTime)
    {
        yield return intervalTime;
    }

    public void PositionReset()
    {
        _playerTransform.position = _startPosition;
    }
}
