using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Label("キャラムーブデータ")]
    private PlayerMoveData _playerMoveData = default;

    private int _currentShotNumber = 0;         //現在の発射する弾の番号を格納する変数

    [SerializeField, Label("ショット用プール")]
    private GameObject[] _shotPools = default;

    [SerializeField, Label("ボムエフェクトのオブジェクト")]
    private GameObject _bombShockWave = default;

    private Animator _bombAnimator = default;

    private int _currentShotCount = 0;          //何回その弾を撃ったかを格納する変数

    private float _offsetAngle = default;   //複数方向に発射する場合の発射角を格納する変数

    private WaitForSeconds _shotInterval = default;                 //弾の連射速度を管理するコルーチンのキャッシュ

    private WaitForSeconds _invincibleTime = default;               //無敵時間を管理するコルーチンのキャッシュ

    private WaitForSeconds _disableEnemyShotsInterval = default;    //ボム使用から弾が消えるまでのインターバル

    private bool _isShotInterval = false;                           //射撃インターバル中判定フラグ

    private bool _isInvincible = false;                             //無敵フラグ

    private Transform _playerTransform = default;                   //プレイヤーのTransform格納用

    private Vector2 _startPosition = new Vector2(0, -3);            //初期座標

    private Vector2 _targetingPosition = default;                   //狙っている座標格納用(発射角計算用)

    #region Getter Setter

    public int GetCurrentShotCount
    {
        //_currentShotCountを返す
        get { return _currentShotCount; }
    }

    private int _maxShotCount = 0;              //その弾を何発撃つかを格納する変数

    public int GetMaxShotCount
    {
        //_maxShotCountを返す
        get { return _maxShotCount; }
    }

    private int _currentPelletCount = 0;        //

    public int GetCurrentPelletCount
    {
        //_currentPelletCountを返す
        get { return _currentPelletCount; }
    }

    private int _maxPelletCount = 0;        //

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



    private const float SECOND = 1.0f;                          //一秒の定数

    private const float BOMB_SHOT_DISABLE_TIME = 0.1f;

    private const float SLOW_MOVING_RATE = 0.5f;

    private const string ANIMATION_BOOL_LEFT_MOVE = "LeftMoving";

    private const string ANIMATION_BOOL_RIGHT_MOVE = "RightMoving";

    private const string ANIMATION_BOOL_SIDE_MOVING = "SideMoving";

    private Animator _animator = default;                   //自身のAnimtor格納用

    private AudioSource _audioSource = default;             //自身のAudioSource格納用

    private CollisionManager _collisionManger = default;    //自身のCollisionManager格納用

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();                          //Animatorの取得

        _playerTransform = this.transform;                                  //自身のTransformを取得

        _audioSource = this.GetComponent<AudioSource>();                    //AudioSourceの取得

        _collisionManger = this.GetComponent<CollisionManager>();           //CollisionManagerコンポーネントの取得

        _bombShockWave = GameObject.FindGameObjectWithTag("BombEffect");    //ボム用エフェクトオブジェクトの取得

        _bombAnimator = _bombShockWave.GetComponent<Animator>();

        _bombShockWave.SetActive(false);

        //弾発射～発射間の待ち時間をキャッシュ
        _shotInterval = new WaitForSeconds(SECOND / (float)_playerMoveData._shotPerSeconds[_currentShotNumber]);

        //無敵時間をキャッシュ
        _invincibleTime = new WaitForSeconds(_playerMoveData._afterHitInvincibleTime);


        _disableEnemyShotsInterval = new WaitForSeconds(BOMB_SHOT_DISABLE_TIME);

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
                GameObject newShot = Instantiate(_playerMoveData._shots[shotNumber], _shotPools[shotNumber].transform);     //弾の生成

                newShot.SetActive(false);                   //生成した弾をfalseにする
            }
        }
    }

    void FixedUpdate()
    {
        float movingXInput = default;       //X軸の入力値格納用
        float movingYInput = default;       //Y軸の入力値格納用

        //_playerMoveData._speed : プレイヤーの移動速度(スクリプタブルオブジェクトから受け取り)

        //右入力
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movingXInput = _playerMoveData._speed;                  //X軸の入力値にプレイヤーの速度を入れる             

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);     //Animatorの「左移動中」boolをfalseに

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, true);     //Animatorの「右移動中」boolをtrueに

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);    //Animatorの「横移動中」boolをtrueに
        }
        //左入力
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            movingXInput = -_playerMoveData._speed;                 //X軸の入力値に(プレイヤーの速度 * -1)を入れる

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, true);      //Animatorの「左移動中」boolをtrueに

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);    //Animatorの「左移動中」boolをfalseに

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);    //Animatorの「横移動中」boolをtrueに
        }
        //左右入力無し
        else
        {
            movingXInput = 0;                                       //X軸の入力値を0に

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);     //Animatorの「左移動中」boolをfalseに

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);    //Animatorの「左移動中」boolをfalseに

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, false);   //Animatorの「横移動中」boolをfalseに
        }

        //上入力
        if (Input.GetKey(KeyCode.UpArrow))
        {
            movingYInput = _playerMoveData._speed;                  //Y軸の入力値にプレイヤーの速度を入れる   
        }
        //下入力
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            movingYInput = -_playerMoveData._speed;                 //Y軸の入力値に(プレイヤーの速度 * -1)を入れる   
        }

        //左シフト入力
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //低速移動

            movingXInput = movingXInput * SLOW_MOVING_RATE;         //X軸の移動入力値を0.5倍する

            movingYInput = movingYInput * SLOW_MOVING_RATE;         //Y軸の移動入力値を0.5倍する
        }

        Move(movingXInput, movingYInput);       //最終的な移動入力値を移動処理に送る

        //Z入力
        if (Input.GetKey(KeyCode.Z))
        {
            Shot();     //弾発射処理
        }

        //無敵状態か?
        if (_isInvincible)
        {
            //無敵

            return;     //処理終了
        }

        //被弾したか?
        if (_collisionManger.GetSetHitFlag)
        {
            //した

            StartCoroutine(OnHit());                        //被弾コルーチンを開始

            _collisionManger.GetSetHitFlag = false;         //CollisionManagerの被弾フラグをfalseに
        }
    }

    /// <summary>
    /// <para>Shot</para>
    /// <para>弾の発射処理</para>
    /// </summary>
    private void Shot()
    {
        if (_isShotInterval)    //インターバル中
        {
            return;     //何もしない
        }

        _offsetAngle = 0;   //発射角の初期化

        switch (_playerMoveData._shotPaterns[_currentShotNumber])           //弾の撃ち方
        {
            case PlayerMoveData.ShotPatern.OneShot:              //単発発射

                EnableShot();                           //弾の有効化 or 生成
                StartCoroutine(RateOfShot());           //インターバル処理

                break;

            case PlayerMoveData.ShotPatern.AllAtOnece:

                _maxPelletCount = _playerMoveData._pelletCountInShots[_currentShotNumber];      //同時生成数を格納

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount <= _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    EnableShot();                       //弾の有効化 or 生成
                }

                StartCoroutine(RateOfShot());           //インターバル処理

                break;


            case PlayerMoveData.ShotPatern.MultipleShots:        //複数発発射

                float maxOffset = 0;        //最大発射角

                float currentAngle = 0;     //現在の発射角

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount < _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    if (pelletCount == 0)       //初弾の場合
                    {
                        maxOffset = _playerMoveData._formedAngles[_currentShotNumber] / 2;       //最大発射角を算出
                        _offsetAngle = -maxOffset;                             //最大発射角を代入

                        //弾と弾の間の角度を算出
                        currentAngle = _playerMoveData._formedAngles[_currentShotNumber] / (_playerMoveData._pelletCountInShots[_currentShotNumber] - 1);
                    }
                    else
                    {
                        _offsetAngle = _offsetAngle + currentAngle; //最初に設定した発射角に加算
                    }

                    EnableShot();                       //弾の有効化 or 生成
                }

                StartCoroutine(RateOfShot());           //インターバル処理

                break;

            case PlayerMoveData.ShotPatern.RadialShots:      //放射状発射

                float currentRadialAngle = 0;       //360 / 弾数の数値(弾～弾間の角度)を格納する

                //同時に生成する弾数分ループ
                for (int pelletCount = 0; pelletCount < _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    //初弾か?(1ループ目か?)
                    if (pelletCount == 0)
                    {
                        _offsetAngle = 0;       //加算用角度を初期化
                        currentRadialAngle = 360 / (_playerMoveData._pelletCountInShots[_currentShotNumber]);       //一発ごとに加算する角度を算出
                    }
                    else
                    {
                        //ループ二回目以降

                        _offsetAngle = _offsetAngle + currentRadialAngle;       //一発ごとの角度を加算
                    }

                    EnableShot();                       //弾の有効化 or 生成
                }

                StartCoroutine(RateOfShot());           //インターバル処理

                break;
        }

        _currentShotCount++;                    //撃った回数を加算

    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>オブジェクトプールを参照し弾の有効化 or 生成を行う</para>
    /// </summary>
    private void EnableShot()
    {

        foreach (Transform shot in _shotPools[_currentShotNumber].transform)     //オブジェクトプール内に未使用オブジェクトが無いか捜索
        {
            if (!shot.gameObject.activeSelf)                //未使用オブジェクトを見つけたら
            {
                shot.gameObject.SetActive(true);            //trueにする

                CheckShotType(shot);                        //弾種の判定

                shot.position = this.transform.position;    //trueにした弾をプレイヤーの位置に移動


                return;
            }
        }

        //以下未使用オブジェクトが無かった場合
        //新しく弾を生成
        GameObject newShot =　Instantiate(_playerMoveData._shots[_currentShotNumber - 1],_shotPools[_currentShotNumber].transform);

        CheckShotType(newShot.transform);                           //弾種の判定

        newShot.transform.position = this.transform.position;       //生成した弾をキャラクターの位置に移動


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
            //if (_currentShotCount <= 0)
            //{
            //    _targetingPosition = _player.transform.position;
            //}
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
        _bombShockWave.SetActive(true);                                     //ボムエフェクトを有効化

        _bombShockWave.transform.position = _playerTransform.position;      //ボムエフェクトをプレイヤーの座標に移動

        _bombAnimator.SetTrigger("Enable");                                 //Animatorの起動トリガーをオンに

        yield return _disableEnemyShotsInterval;                            //ボム発動から敵の弾が消えるまでのインターバル

        GameObject[] enemyShotsInPicture = GameObject.FindGameObjectsWithTag("EnemyShot");      //現在有効化されている敵の弾を全て取得

        //先ほど取得した敵の弾の数だけループする
        for(int shotCount = 0; shotCount < enemyShotsInPicture.Length; shotCount++)
        {
            enemyShotsInPicture[shotCount].GetComponent<Animator>().SetTrigger("Disable");      //Animatorの「無効化」トリガーをオンに
        }
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

        yield return _invincibleTime;

        _isInvincible = false;


    }

    public void PositionReset()
    {
        _playerTransform.position = _startPosition;
    }
}
