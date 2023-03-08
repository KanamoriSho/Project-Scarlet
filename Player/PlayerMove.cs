using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Label("プレイヤームーブデータ")]
    private PlayerMoveData _playerMoveData = default;

    [SerializeField, Label("ショット")]
    private GameObject _shot = default;

    [SerializeField, Label("ショット用プール")]
    private GameObject _shotPool = default;

    #region 移動系パラメータ定数
    private int LEFT_MOVE = -1;
    private int RIGHT_MOVE = 1;
    private int DOWN_MOVE = -1;
    private int UP_MOVE = 1;
    private float SLOW_MOVE_RATE = 0.5f;
    #endregion

    private Transform _playerTransform = default;           //自身のTransform格納用

    private SpriteRenderer _spriteRenderer = default;       //自身のSpriteRenderer格納用

    private Animator _anim = default;                       //自身のAnimator格納用

    private WaitForSeconds _interval = default;             //コルーチンのキャッシュ

    private float _currentInterval = default;               //コルーチンの待機時間設定用

    private bool _isInterval = false;                       //インターバル中判定フラグ

    private AudioSource audioSource = default;              //自身のAnimtor格納用

    private bool _isAuto = false;                           //オートモード判定フラグ

    private float SECOND = 1.0f;                            //一秒の定数

    private void Awake()
    {
        _playerTransform = this.GetComponent<Transform>();          //自身のトランスフォームを取得

        _spriteRenderer = this.GetComponent<SpriteRenderer>();      //自身のスプライトレンダラー取得

        _anim = this.GetComponent<Animator>();                      //自身のアニメーター取得

        _currentInterval = SECOND / _playerMoveData._shotPerSecond; //秒間ショット数を求める

        _interval = new WaitForSeconds(_currentInterval);           //ショット間のインターバルをキャッシュ

        audioSource = this.GetComponent<AudioSource>();             //AudioSourceを取得
    }

    void Start()
    {
        /*弾をプールに生成する
         * _playerMoveData._initiallyGeneratedShots : 初期生成弾数(スクリプタブルオブジェクトから受け取り)
         */
        for (int count = 0; count < _playerMoveData._initiallyGeneratedShots; count++)
        {
            GameObject newShot = Instantiate(_shot, _shotPool.transform);       //弾の生成

            newShot.SetActive(false);                   //生成した弾をfalseにする
        }
    }

    void Update()
    {
        if(_currentInterval != SECOND / _playerMoveData._shotPerSecond)     //秒間発射数が変更されたら
        {
            _currentInterval = SECOND / _playerMoveData._shotPerSecond;     //再計算して再代入

            _interval = new WaitForSeconds(_currentInterval);               //WaitForSecondsのキャッシュを書き換え
        }

        

        float horizontalInput = default;            //左右入力値
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            _anim.SetBool("LeftMoving", true);      //左移動true
            _anim.SetBool("RightMoving", false);    //右移動false
            _anim.SetBool("SideMoving", true);      //横移動中true

            horizontalInput = LEFT_MOVE;            //左右入力値に左移動値の定数を入れる
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            _anim.SetBool("RightMoving", true);     //右移動true
            _anim.SetBool("LeftMoving", false);     //左移動false
            _anim.SetBool("SideMoving", true);      //横移動true

            horizontalInput = RIGHT_MOVE;           //左右入力値に右移動値の定数を入れる
        }
        
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _anim.SetBool("LeftMoving", false);     //左移動false
            _anim.SetBool("SideMoving", false);     //横移動false
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _anim.SetBool("RightMoving", false);    //右移動false
            _anim.SetBool("SideMoving", false);     //横移動false
        }

        float verticalInput = default;              //上下入力値
        if(Input.GetKey(KeyCode.UpArrow))
        {
            verticalInput = UP_MOVE;                //上下入力値に上移動値の定数を入れる
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalInput = DOWN_MOVE;              //上下入力値に下移動値の定数を入れる
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            horizontalInput = horizontalInput * SLOW_MOVE_RATE;     //左右入力値に減速率をかける
            verticalInput = verticalInput * SLOW_MOVE_RATE;         //上下入力値に減速率をかける
        }

        Move(horizontalInput, verticalInput);       //移動

        if(Input.GetKey(KeyCode.Z))
        {
            Shot();     //弾発射処理
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(!_isAuto)            //falseなら
            {
                _isAuto = true;     //trueに
            }
            else　                  //trueなら
            {
                _isAuto = false;    //falseに
            }
        }

        if (_isAuto)
        {
            Shot();     //弾発射処理
        }
    }

    /// <summary>
    /// <para>Shot</para>
    /// <para>弾の発射処理</para>
    /// </summary>
    private void Shot()
    {
        if (_isInterval)    //インターバル中
        {
            return;     //何もしない
        }

        foreach (Transform shot in _shotPool.transform)     //オブジェクトプール内に未使用オブジェクトが無いか捜索
        {
            if(!shot.gameObject.activeSelf)             //未使用オブジェクトを見つけたら
            {
                shot.gameObject.SetActive(true);            //trueにする

                shot.position = this.transform.position;    //trueにした弾をプレイヤーの位置に移動

                StartCoroutine(RateOfShot());           //インターバル処理
                return;
            }
        }

        //以下未使用オブジェクトが無かった場合

        GameObject newShot = Instantiate(_shot, _shotPool.transform);       //新しく弾を生成

        newShot.transform.position = _playerTransform.position;             //生成した弾をプレイヤーの位置に移動

        StartCoroutine(RateOfShot());                   //インターバル処理

    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    /// <returns>_interval : インターバル時間</returns>
    IEnumerator RateOfShot()
    {
        if(!_playerMoveData._shotSoundEffect)
        {
            audioSource.PlayOneShot(_playerMoveData._shotSoundEffect);
        }


        _isInterval = true;

        yield return _interval;

        _isInterval = false;

    }

    /// <summary>
    /// 移動処理
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

        /*現在位置に移動入力と移動スピードを加算する
         * 
         * _playerMoveData._speed : プレイヤーの移動速度(スクリプタブルオブジェクトから受け取り)
         */
        playerPosition = new Vector2(playerPosition.x + horizontalInput * _playerMoveData._speed * Time.deltaTime,
                                        playerPosition.y + verticalInput * _playerMoveData._speed * Time.deltaTime);

        /*
         *  if(右 or 上の移動制限を超えたか)
         *      右 or 上の移動制限内に戻す
         *  else if(左 or 下の移動制限を超えたか)
         *      左 or 下の移動制限内に戻す
         */

        if (playerPosition.x > _playerMoveData._xLimitOfMoving)
        {
            playerPosition.x = _playerMoveData._xLimitOfMoving;
        }
        else if (playerPosition.x < -_playerMoveData._xLimitOfMoving)
        {
            playerPosition.x = -_playerMoveData._xLimitOfMoving;
        }

        if (playerPosition.y > _playerMoveData._yLimitOfMoving)
        {
            playerPosition.y = _playerMoveData._yLimitOfMoving;
        }
        else if (playerPosition.y < -_playerMoveData._yLimitOfMoving)
        {
            playerPosition.y = -_playerMoveData._yLimitOfMoving;
        }

        _playerTransform.position = playerPosition;
    }
}
