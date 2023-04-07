using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharactorMove : MonoBehaviour
{
    [SerializeField, Label("キャラムーブデータ")]
    private CharactorMoveData _charactorMoveData = default;

    private CharactorShootingData _currentMovementAndShootingPaterns = default;

    [SerializeField, Label("ショット用プール")]
    private GameObject[] _shotPools = default;

    [SerializeField, Label("チェックポイント")]
    private List<Vector2> _moveCheckpoints = new List<Vector2>();       //移動用チェックポイントの座標を格納する

    [SerializeField, Label("待機時間")]
    private List<float> _intervalBetweenMoves = new List<float>();      //移動～移動間待機時間を格納するリスト

    private float _timer = 0;                   //時間計測用変数

    private int _waveCount = 0;                 //ウェーブ数を格納する変数
    [SerializeField]
    private int _currentShotNumber = 0;         //現在の発射する弾の番号を格納する変数

    private int _currentShotCount = 0;          //何発その弾を撃ったかを格納する変数

    private int _maxShotCount = 0;              //その弾を何発撃つかを格納する変数

    private int _currentPelletCount = 0;        //発射する弾の現在の生成数を格納する変数

    private int _maxPelletCount = 0;            //発射する弾の同時生成数を格納する変数

    CharactorShootingData.ShotPatern _currentShotPatern = default;       //弾の撃ち方を格納するEnum

    private Vector2 _targetingPosition = default;       //狙っている座標格納用(発射角計算用)

    private float _multiShotOffsetAngle = default;      //複数方向に発射する場合の発射角を格納する変数

    private float _swingShotOffsetAngle = default;      //回転撃ちをする際の加算角を格納する変数

    private int _checkpointCounter = 0;                 //現在の移動チェックポイントの番号を格納する

    private int _nextCheckpointNumber = 0;              //次に向かうチェックポイントの番号を格納する

    private Vector2 _movingOffset = new Vector2(0, 0);  //チェックポイントからどれだけずらして移動させるか(隊列移動時用)

    private WaitForSeconds _movingInterval = default;   //移動時のコルーチンのキャッシュ

    private WaitForSeconds _shotInterval = default;     //弾の連射速度を管理するコルーチンのキャッシュ

    private const float SECOND = 1.0f;                  //一秒の定数
    [SerializeField]
    private bool _isMovingInterval = false;             //移動待機中判定フラグ
    [SerializeField]
    private bool _isShotInterval = false;               //発射インターバル中判定フラグ
    [SerializeField]
    private bool _isNotShotInThisCheckpoint = false;    //そのチェックポイントで発射処理を無視するか

    private const string PLAYER_TAG = "Player";         //プレイヤーのタグを格納する定数

    private GameObject _player = default;               //プレイヤー格納用

    private Animator _animator = default;               //自身のAnimtor格納用

    private AudioSource audioSource = default;          //自身のAudioSource格納用

    #region Getter

    public int GetCurrentShotCount
    {
        //_currentPelletCountを返す
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

    public bool GetIsDecelerationPerShot
    {
        //CharactorMoveDataの発射ごとに初速を加減速させるかのフラグを返す(ShotMoveに受け渡す)
        get { return _currentMovementAndShootingPaterns._isDecelerationPerShoot[_currentShotNumber]; }
    }

    #endregion

    //以下の変数はOnDrowGizmosに受け渡すためにフィールド変数にしてますが、計算自体はローカル変数で事足りるので最終的に消す予定です。

    private Vector2 _fixedRelayPoint = default;         //ベジェ曲線の中間点格納用

    private Vector2 _relayPointVector = default;        //初期位置 - 目標位置間のベクトル格納用

    private Vector2 _relayPointY = default;             //_relayPointVector上の縦(Y)軸座標格納用

    private void Awake()
    {
        //Animatorの取得
        _animator = this.gameObject.GetComponent<Animator>();

        //プレイヤーキャラの取得
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);

        //現在の行動パターンを取得
        _currentMovementAndShootingPaterns = _charactorMoveData._movementAndShootingPaterns[_waveCount];

        /*弾をプールに生成する
         * _charactorMoveData._waves                   : ウェーブ数(ボスキャラ以外は1)
         * _charactorMoveData._initiallyGeneratedShots : 初期生成弾数(スクリプタブルオブジェクトから受け取り)
         */

        //ウェーブ数分ループ
        for (int waveCount = 0; waveCount < _charactorMoveData._waveCount; waveCount++)
        {
            //そのウェーブで使用する弾種の数を格納
            int _currentShotNumber = _charactorMoveData._movementAndShootingPaterns[waveCount]._shots.Length;

            //ウェーブ内使用弾の種類分ループ
            for (int shotNumber = 0; shotNumber < _currentShotNumber; shotNumber++)
            {

                //ウェーブ内で使用される弾を生成するループ
                for (int shotLength = 0; shotLength < _charactorMoveData._initiallyGeneratedShots; shotLength++)
                {
                    //使用する弾を配列から取り出し格納
                    GameObject currentShotObject =
                                _charactorMoveData._shots[_charactorMoveData._movementAndShootingPaterns[waveCount]._shots[shotNumber] - 1];

                    //弾の生成
                    GameObject newShot = Instantiate(currentShotObject, _shotPools[shotNumber].transform);

                    //生成した弾をfalseにする
                    newShot.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        //時間を加算
        _timer += Time.deltaTime;

        //次の移動先チェックポイントが指定できているか
        if (_nextCheckpointNumber != _checkpointCounter + 1 || _nextCheckpointNumber != 0)
        {
            //できていない

            //現在のチェックポイント + 1を次のチェックポイント番号として格納
            _nextCheckpointNumber = _checkpointCounter + 1;

            //チェックポイント格納配列の要素数を越えた?
            if (_checkpointCounter + 1 >= _moveCheckpoints.Count)
            {
                //越えた

                //0を次チェックポイント番号として格納
                _nextCheckpointNumber = 0;
            }

            //行動パターンの更新
            _currentMovementAndShootingPaterns = _charactorMoveData._movementAndShootingPaterns[_waveCount];
        }

        //移動中に弾を撃つか否かのフラグを格納
        bool isShotOnTheMove = _currentMovementAndShootingPaterns._isMovingShooting[_currentShotNumber];

        /*現在の弾の撃つタイミングと次の弾の撃つタイミングが異なるかのフラグを格納する。
         * 
         * 現在 : 移動しながら撃つ    次 : 止まって撃つ
         * 
         * のような状態だとtrueになる。
         */
        bool isCurrentShotMach = CheckCurrentAndNextShotType(isShotOnTheMove);

        Debug.Log("現在の弾の撃つタイミングと次の弾の撃つタイミングが異なるか" + isCurrentShotMach);

        //現在の座標が次チェックポイントと同じか
        if (this.transform.position == (Vector3)_moveCheckpoints[_nextCheckpointNumber])
        {
            //現在の弾の撃つタイミングと次の弾の撃つタイミングが異なるか
            if (isCurrentShotMach)
            {
                //異なる

                //次のチェックポイントでは発射処理をしない
                _isNotShotInThisCheckpoint = true;

                Debug.Log("タイミングが異なる");
            }
            else
            {

                Debug.Log("撃つタイミングが同じ");
            }

            //移動間のインターバルをキャッシュ
            _movingInterval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);

            //現在のチェックポイントを書き換え
            _checkpointCounter = _nextCheckpointNumber;

            //移動～移動間の待機コルーチン

            //待機時間分待機
            StartCoroutine(MovementInterval());

            //発射する弾の弾番号の変更、発射数の初期化
            SetShotNumber();
        }

        //移動間の待機中なら
        if (_isMovingInterval)
        {
            //次のチェックポイントでは発射処理をしないか
            if (_isNotShotInThisCheckpoint)
            {
                //しない

                return;
            }

            //移動中に撃つフラグがfalse?
            if (!isShotOnTheMove)
            {
                //false

                //弾に受け渡すパラメータの設定・発射
                SettingShotPrameters();
            }

            return;
        }

        //移動中に撃つフラグがtrue?
        if (isShotOnTheMove)
        {
            //true

            //弾に受け渡すパラメータの設定・発射
            SettingShotPrameters();
        }

        //移動時に曲線的に飛ぶ?
        if (!_charactorMoveData._isCurveMoving)
        {
            //false

            //現在位置
            Vector2 currentPosition = _moveCheckpoints[_checkpointCounter];

            //移動先の目標座標
            Vector2 nextPosition = _moveCheckpoints[_nextCheckpointNumber];

            /* 移動速度の計算
             * 移動速度 * 移動速度用アニメーションカーブの値
             */
            float movingSpeed = _charactorMoveData._speed * _charactorMoveData._speedCurve.Evaluate(_timer);

            /* Lerpでチェックポイント間を移動
             * 編隊移動用オフセット値を加算する(単体飛行の場合は+-0)
             */
            this.transform.position = Vector2.Lerp(currentPosition, nextPosition, movingSpeed) + _movingOffset;
        }
        else
        {
            //true

            /* ベジェ曲線から出した座標を現在地点に
             * 編隊移動用オフセット値を加算する(単体飛行の場合は+-0)
             */
            this.transform.position = CalculateBezierCurve() + _movingOffset;
        }
    }

    #region 移動関連メソッド

    /// <summary>
    /// <para>CheckCurrentAndNextShotType</para>
    /// <para>現在の弾の撃つタイミング(移動しながらか止まってか)と次の弾の撃つタイミングを比較し、異なればtrueを返す処理</para>
    /// </summary>
    /// <param name="isShotOnTheMove">現在の弾の撃つタイミング (true : 移動しながら撃つ false : 止まって撃つ)</param>
    /// <returns>if(isShotOnTheMove != isNextShotOnTheMove) の結果を返す</returns>
    private bool CheckCurrentAndNextShotType(bool isShotOnTheMove)
    {
        //結果用フラグを定義
        bool isChangeMoveShotToNextShot = default;

        //次に撃つ弾の弾番号を定義
        int nextShotNumber = _currentShotNumber + 1;

        //その番号は弾種配列の要素数を越えていないか
        if (nextShotNumber >= _currentMovementAndShootingPaterns._isMovingShooting.Length)
        {
            //越えている

            //0に初期化
            nextShotNumber = 0;
        }

        //次の弾が移動中に撃つか否かのフラグを格納
        bool isNextShotOnTheMove = _currentMovementAndShootingPaterns._isMovingShooting[nextShotNumber];

        //現在の弾と次の弾の撃つタイミングが異なるか
        if (isShotOnTheMove != isNextShotOnTheMove)
        {
            //異なる

            //結果用フラグにtrueを格納
            isChangeMoveShotToNextShot = true;
        }
        else
        {
            //同じ

            //結果用フラグにfalseを格納
            isChangeMoveShotToNextShot = false;
        }

        //結果を返す
        return isChangeMoveShotToNextShot;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>カーブ弾のベジェ曲線を生成・現在地点を算出するスクリプト</para>
    /// </summary>
    /// <returns>currentCurvePos = 算出されたベジェ曲線上の座標</returns>
    private Vector2 CalculateBezierCurve()
    {
        //現在のチェックポイントの座標
        Vector2 currentMoveCheckpoint = _moveCheckpoints[_checkpointCounter];

        //次のチェックポイントの座標
        Vector2 nextMoveCheckpoint = _moveCheckpoints[_nextCheckpointNumber];

        //現在地 - 次チェックポイント間のベクトルを算出
        _relayPointVector = currentMoveCheckpoint - nextMoveCheckpoint;


        /*ベクトル上の0.0～1.0でオフセットした中間点を算出
         * 0.0 : 現在のチェックポイント
         * 0.5 : 現在のチェックポイントと次のチェックポイントの中央
         * 1.0 : 次のチェックポイントの座標
         */
        _relayPointY = Vector2.Lerp(currentMoveCheckpoint, nextMoveCheckpoint, _charactorMoveData._curveMoveVerticalOffset);

        /*移動軌道の左右値に応じて計算するベクトルの向きを変更する
         * 
         * 左に飛ばす場合は_relayPointVectorに対して左向きの垂直ベクトルに対して左右値をかける
         * 右に飛ばす場合は_relayPointVectorに対して右向きの垂直ベクトルに対して左右値をかける
         * 
         * _relayPointYで求めたベクトル上の中間地点をもとに垂直ベクトルを出す
         */

        //ベクトルに対する横軸オフセット値を設定
        float horizontalAxisOffset = _charactorMoveData._curveMoveHorizontalOffset;

        //左右値がマイナス(左向きであるか)
        if (horizontalAxisOffset < 0)
        {
            //左向きである

            //現在のチェックポイント～次のチェックポイント間ベクトルに対する左向きに垂直なベクトルを求める
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            //算出したベクトルに対して中間地点のY軸オフセットとX軸オフセット値を足し、中間地点の座標を求める
            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        else
        {
            //右向きである

            //現在のチェックポイント～次のチェックポイント間ベクトルに対する右向きに垂直なベクトルを求める
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            //算出したベクトルに対して中間地点のY軸オフセットとX軸オフセット値を足し、中間地点の座標を求める
            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        /* 現在のチェックポイント～中間点、中間点～次のチェックポイントを繋ぐ直線上をLerp移動させ、firstVector、secodVector2つの移動する座標を求める。
         * 
         * firstVector、secodVector2つの座標間をLerp移動させ、曲線上の座標currentCurvePosを求める。
         * 
         * 算出したcurrentCurvePosを返す
         */

        //現在のチェックポイント～中間点間のベクトル上を等速直線運動させる
        Vector2 firstVector = Vector2.Lerp(currentMoveCheckpoint, _fixedRelayPoint, _timer);

        //中間点～次のチェックポイント間のベクトル上を等速直線運動させる
        Vector2 secondtVector = Vector2.Lerp(_fixedRelayPoint, nextMoveCheckpoint, _timer);

        //firstVector～secondVector間のベクトル上を等速直線運動する座標を求める
        Vector2 currentCurvePos = Vector2.Lerp(firstVector, secondtVector, _timer);

        //算出した座標を返し値として返す
        return currentCurvePos;
    }

    #endregion

    #region 弾関連メソッド

    /// <summary>
    /// <para>SetShotNumber</para>
    /// <para>発射する弾の弾番号の変更、発射数の初期化を行う</para>
    /// </summary>
    private void SetShotNumber()
    {

        //発射インターバル中フラグをfalseに
        StopCoroutine(RateOfShot());
        _isShotInterval = false;

        //発射回数を0に初期化
        _currentShotCount = 0;

        //発射する弾の配列参照番号を変更
        _currentShotNumber++;

        //配列参照番号が配列の要素数を越えていないか
        if (_currentShotNumber >= _currentMovementAndShootingPaterns._shots.Length)
        {
            //越えた

            //配列参照番号を0に戻す
            _currentShotNumber = 0;
        }
    }

    /// <summary>
    /// <para>SettingShotPrameters</para>
    /// <para>発射する弾のパラメータをもとに連射速度や発射数を参照して発射処理とその停止を行う</para>
    /// </summary>
    private void SettingShotPrameters()
    {
        //弾の最大発射数を格納
        _maxShotCount = _currentMovementAndShootingPaterns._shotCounts[_currentShotNumber];

        //現在の発射数が最大発射数を越えていないか
        if (_currentShotCount <= _maxShotCount)
        {
            //越えていない

            //秒間に何発撃つかを格納
            int shotPerSeconds = _currentMovementAndShootingPaterns._shotPerSeconds[_currentShotNumber] + 1;

            //ショット～ショット間の待機時間を設定
            _shotInterval = new WaitForSeconds(SECOND / shotPerSeconds);

            //弾発射処理
            Shot();
        }
        else
        {
            //越えた

            //次の弾と同時に撃つかのフラグを取得
            bool isShotInSameTime = _currentMovementAndShootingPaterns._isShotInSameTime[_currentShotNumber];

            //同時に撃つか?
            if (isShotInSameTime)
            {
                //同時に撃つ

                //発射インターバル中フラグをfalseに
                StopCoroutine(RateOfShot());

                //次のチェックポイントで撃つかの判別フラグを初期化
                _isNotShotInThisCheckpoint = false;

                //発射する弾の弾番号の変更、発射数の初期化
                SetShotNumber();

                //弾に受け渡すパラメータの設定・発射
                SettingShotPrameters();
            }
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

        //回転撃ちの有無の判定と角度計算
        SwingShotCheck();

        //現在の弾の撃ち方を格納(enum)
        _currentShotPatern = _currentMovementAndShootingPaterns._shotPaterns[_currentShotNumber];

        //格納した撃ち方をもとに処理分け
        switch (_currentShotPatern)           //弾の撃ち方
        {
            //単発発射
            case CharactorShootingData.ShotPatern.OneShot:

                #region 単発発射
                //弾の有効化 or 生成
                EnableShot();

                #endregion

                break;

            //単方向同時発射
            case CharactorShootingData.ShotPatern.AllAtOnce:

                #region 同時発射
                //同時生成弾数を取得
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

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
            case CharactorShootingData.ShotPatern.MultipleShots:

                #region 扇形同時発射

                //同時生成弾数を取得
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

                //最大発射角
                float maxOffset = 0;

                //現在の発射角
                float currentAngle = 0;

                //弾の散布角を取得
                float formedAngle = _currentMovementAndShootingPaterns._multiShotFormedAngles[_currentShotNumber];

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
            case CharactorShootingData.ShotPatern.RadialShots:

                #region 放射状発射

                //ショット～ショット間の角度格納用
                float currentRadialAngle = 0;

                //同時生成弾数を取得
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

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
        StartCoroutine(RateOfShot());

        //撃った弾数を加算
        _currentShotCount++;

    }

    /// <summary>
    /// <para>SwingShotCheck</para>
    /// <para>回転撃ち(流し撃ち?)を行うかの判定と、行う場合の角度計算を行う</para>
    /// </summary>
    private void SwingShotCheck()
    {
        //回転撃ちをするかのフラグを取得
        bool isSwingShot = _currentMovementAndShootingPaterns._isSwingShots[_currentShotNumber];

        //回転撃ちする?
        if (isSwingShot)
        {
            //する

            //回転撃ち時に回す角度の取得
            float centralAngle = _currentMovementAndShootingPaterns._swingShotFormedAngles[_currentShotNumber];

            //回転撃ち時の初弾の角度の取得
            float firstAngle = _currentMovementAndShootingPaterns._swingShotFirstAngles[_currentShotNumber];

            //単位角を算出
            float radian = centralAngle / _maxShotCount;


            //初弾か?
            if (_currentShotCount <= 0)
            {
                //初弾

                //発射角に初弾の角度を設定
                _swingShotOffsetAngle = firstAngle;
            }
            else
            {
                //発射角に単位角を加算
                _swingShotOffsetAngle += radian;
            }
        }
        else
        {
            //しない

            //角度を初期化
            _swingShotOffsetAngle = 0;
        }
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

        //新たに生成する弾の弾番号を取得(弾番号と配列要素数の差を修正するため取得値 -1を格納)
        int shotNumber = _currentMovementAndShootingPaterns._shots[_currentShotNumber] - 1;

        //新たに発射する弾のオブジェクトを取得
        GameObject shotObject = _charactorMoveData._shots[shotNumber];

        //取得した弾オブジェクトを対応するプールの子オブジェクトとして生成
        GameObject newShot = Instantiate(shotObject, _shotPools[_currentShotNumber].transform);

        //弾種の判定
        CheckShotType(newShot.transform);

        //生成した弾をキャラクターの位置に移動
        newShot.transform.position = this.transform.position;
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    private IEnumerator RateOfShot()
    {
        //発射SEがあるか
        if (_charactorMoveData._shotSoundEffect != null)
        {
            //ある

            //発射SEを再生
            audioSource.PlayOneShot(_charactorMoveData._shotSoundEffect);
        }

        //発射インターバル中フラグをtrueに
        _isShotInterval = true;

        //発射間インターバル処理
        yield return _shotInterval;

        //発射インターバル中フラグをfalseに
        _isShotInterval = false;
    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>弾の種類を判定する。自機狙いフラグが立っている場合に発射角にプレイヤーとのベクトル角を加算する</para>
    /// </summary>
    /// <param name="shot">Shotメソッドで有効化/生成された弾。オブジェクトプール探索の際にTransform型で取得するためTransform型</param>
    private void CheckShotType(Transform shot)
    {
        //自機狙い弾か否かを格納するフラグ
        bool isTargetingPlayer = _currentMovementAndShootingPaterns._isTargetingPlayer[_currentShotNumber];

        //自機狙い弾か
        if (!isTargetingPlayer)
        {
            //通常弾

            //真下をターゲット座標に
            _targetingPosition = Vector2.down;
        }
        else
        {
            //自機狙い

            //同時生成弾かつその初弾か
            if (_currentShotPatern != CharactorShootingData.ShotPatern.OneShot && _currentPelletCount <= 0)
            {
                //ターゲットとしてその瞬間のプレイヤーの座標を格納
                _targetingPosition = _player.transform.position;

                return;
            }

            //同時生成弾ではない場合は常にプレイヤーの座標をターゲット座標として格納する

            //ターゲットとしてその瞬間のプレイヤーの座標を格納
            _targetingPosition = _player.transform.position;
        }

        /*
         * _targetingPosition は自機狙いか否かで入るものが変わります
         * 
         * 通常弾     : キャラクターの正面(Vector2.down)
         * 
         * 自機狙い弾 : 初弾発射時のプレイヤーの座標
         */

        //現在の座標とターゲットとして格納する座標間のベクトルを求める
        Vector2 degree = _targetingPosition - (Vector2)this.transform.position;

        //ベクトルから角度に変換
        float radian = Mathf.Atan2(degree.y, degree.x);

        //発射する弾のShotMoveコンポーネントを取得
        ShotMove shotMove = shot.GetComponent<ShotMove>();

        /* 発射方向に同時発射時の加算角(放射状、扇状の場合)と回し角(回し撃ちの場合)を加算して弾に発射角として受け渡し
         * 
         * _multiShotOffsetAngle  : 扇形・円形に撃つ際に弾～弾間の弧度を格納
         * _swingShotOffsetAngle  : 回し撃ちの際に弾～弾間の孤度を格納
         */
        shotMove.GetSetShotAngle = radian * Mathf.Rad2Deg + _multiShotOffsetAngle + _swingShotOffsetAngle;
    }

    #endregion

    /// <summary>
    /// <para>MovementInterval</para>
    /// <para>移動のインターバル処理を行う</para>
    /// </summary>
    private IEnumerator MovementInterval()
    {
        //移動待機中フラグtrue
        _isMovingInterval = true;

        //待機時間分待機
        yield return _movingInterval;

        //移動待機中フラグfalse
        _isMovingInterval = false;

        //次のチェックポイントで撃つかの判別フラグを初期化
        _isNotShotInThisCheckpoint = false;

        _timer = 0;
    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>ベジェ曲線の始点 - 中間点間、中間点 - 終点間を線で描画するメソッド デバッグ用</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        if (!_charactorMoveData._isCurveMoving)
        {
            Gizmos.DrawSphere(_moveCheckpoints[_checkpointCounter], 0.1f);
            Gizmos.DrawSphere(_moveCheckpoints[_nextCheckpointNumber], 0.1f);

            Gizmos.DrawLine(_moveCheckpoints[_checkpointCounter], _moveCheckpoints[_nextCheckpointNumber]);
        }
        else
        {

            Gizmos.DrawSphere(_moveCheckpoints[_checkpointCounter], 0.1f);
            Gizmos.DrawSphere(_moveCheckpoints[_nextCheckpointNumber], 0.1f);

            Gizmos.DrawSphere(_fixedRelayPoint, 0.2f);

            Gizmos.DrawSphere(_relayPointY, 0.1f);

            Gizmos.DrawLine(_moveCheckpoints[_checkpointCounter], _fixedRelayPoint);

            Gizmos.DrawLine(_fixedRelayPoint, _moveCheckpoints[_nextCheckpointNumber]);

        }
#endif

    }

}