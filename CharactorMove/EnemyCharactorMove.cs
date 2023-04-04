using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharactorMove : MonoBehaviour
{
    [SerializeField, Label("キャラムーブデータ")]
    private CharactorMoveData _charactorMoveData = default;

    private int _waveCount = 0;         //ウェーブ数を格納する変数

    [SerializeField]
    private int _currentShotNumber = 0;         //現在の発射する弾の番号を格納する変数

    [SerializeField, Label("ショット用プール")]
    private GameObject[] _shotPools = default;

    [SerializeField]
    private int _currentShotCount = 0;          //何発その弾を撃ったかを格納する変数

    private int _maxShotCount = 0;              //その弾を何発撃つかを格納する変数

    private int _currentPelletCount = 0;        //発射する弾の現在の生成数を格納する変数

    private int _maxPelletCount = 0;        //発射する弾の同時生成数を格納する変数

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

    public bool GetIsDecelerationPerShoot
    {
        //CharactorMoveDataの発射ごとに初速を加減速させるかのフラグを返す(ShotMoveに受け渡す)
        get { return _charactorMoveData._movementAndShootingPaterns[_waveCount]._isDecelerationPerShoot[_currentShotNumber]; }
    }


    CharactorShootingData.ShotPatern _currentShotPatern = default;       //弾の撃ち方を格納するEnum

    private float _timer = 0;                           //時間計測用変数

    private float _multiShotOffsetAngle = default;      //複数方向に発射する場合の発射角を格納する変数

    private float _swingShotOffsetAngle = default;      //回転撃ちをする際の加算角を格納する変数

    private int _checkpointCounter = 0;                 //現在の移動チェックポイントの番号を格納する

    private int _nextCheckPointNumber = 0;              //次に向かうチェックポイントの番号を格納する

    [SerializeField, Label("チェックポイント")]
    private List<Vector2> _moveCheckPoints = new List<Vector2>();       //移動用チェックポイントの座標を格納する

    private Vector2 _movingOffset = new Vector2(0, 0);                  //チェックポイントからどれだけずらして移動させるか(隊列移動時用)

    [SerializeField]
    private List<float> _intervalBetweenMoves = new List<float>();      //移動～移動間待機時間を格納するリスト
    [SerializeField]
    private bool _isMovingInterval = false;

    private WaitForSeconds _movingInterval = default;       //移動時のコルーチンのキャッシュ

    private WaitForSeconds _shotInterval = default;         //弾の連射速度を管理するコルーチンのキャッシュ

    private float SECOND = 1.0f;                            //一秒の定数

    private bool _isShotInterval = false;                   //発射インターバル中判定フラグ
    [SerializeField]
    private bool _isIgnoreThisCheckPoint = false;           //そのチェックポイントで発射処理を無視するか

    private bool _isShotInSameTime = false;

    private bool hasArrived = false;

    private GameObject _player = default;                   //プレイヤー格納用

    private Vector2 _targetingPosition = default;           //狙っている座標格納用(発射角計算用)

    private string PLAYER_TAG = "Player";                   //プレイヤーのタグを格納する定数

    private Animator _animator = default;           //自身のAnimtor格納用

    private AudioSource audioSource = default;      //自身のAudioSource格納用


    //以下の変数はOnDrowGizmosに受け渡すためにフィールド変数にしてますが、計算自体はローカル変数で事足りるので最終的に消す予定です。

    private Vector2 _fixedRelayPoint = default;     //ベジェ曲線の中間点格納用

    private Vector2 _relayPointVector = default;    //初期位置 - 目標位置間のベクトル格納用

    private Vector2 _relayPointY = default;         //_relayPointVector上の縦(Y)軸座標格納用

    private void Awake()
    {
        _animator = this.gameObject.GetComponent<Animator>();                   //Animatorの取得

        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);                 //プレイヤーキャラの取得

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

                    newShot.SetActive(false);                   //生成した弾をfalseにする
                }
            }
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;            //時間を加算

        //次の移動先チェックポイントが指定できているか
        if (_nextCheckPointNumber != _checkpointCounter + 1 || _nextCheckPointNumber != 0)
        {
            //できていない

            _nextCheckPointNumber = _checkpointCounter + 1;       //現在のチェックポイント + 1を次のチェックポイント番号として格納

            //チェックポイント格納配列の要素数を超えた?
            if (_checkpointCounter + 1 >= _moveCheckPoints.Count)
            {
                //超えた

                _nextCheckPointNumber = 0;      //0を次チェックポイント番号として格納
            }
        }

        //移動中に弾を撃つか否かのフラグを格納
        bool isShotOnTheMove = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig[_currentShotNumber];

        int nextShotNumber = _currentShotNumber + 1;

        if(nextShotNumber >= _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig.Length)
        {
            nextShotNumber = 0;
        }

        //次の弾が移動中に撃つか否かのフラグを格納
        bool isNextShotOnTheMove = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig[nextShotNumber];

        //現在の座標が次チェックポイントと同じか
        if ((Vector2)this.transform.position == _moveCheckPoints[_nextCheckPointNumber] && !hasArrived)
        {
            hasArrived = true;

            if (isShotOnTheMove != isNextShotOnTheMove)
            {
                _isIgnoreThisCheckPoint = true;
            }

            _movingInterval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);    //移動間のインターバルをキャッシュ

            _checkpointCounter = _nextCheckPointNumber;     //現在のチェックポイントを書き換え

            StartCoroutine(MovementInterval());             //移動～移動間の待機コルーチン

            SetShotNumber();
        }

        //移動間の待機中なら
        if (_isMovingInterval)
        {
            if(_isIgnoreThisCheckPoint)
            {
                return;
            }

            //移動中に撃つフラグがfalse?
            if (!isShotOnTheMove)
            {
                //falseだった

                SettingShotPrameters();     //弾に受け渡すパラメータの設定・発射

            }

            return;
        }

        //移動中に撃つフラグがtrue?
        if (isShotOnTheMove)
        {
            //trueだった

            SettingShotPrameters();     //弾に受け渡すパラメータの設定・発射
        }

        //移動時に曲線的に飛ぶ?
        if (!_charactorMoveData._isCurveMoving)
        {
            //false

            Vector2 currentPosition = _moveCheckPoints[_checkpointCounter];         //現在位置

            Vector2 nextPosition = _moveCheckPoints[_nextCheckPointNumber];         //移動先の目標座標

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

            this.transform.position = CalcuateBezierCurve() + _movingOffset;
        }
    }

    /// <summary>
    /// <para>SetShotNumber</para>
    /// <para>発射する弾の弾番号の変更、発射数の初期化を行う</para>
    /// </summary>
    private void SetShotNumber()
    {
        StopCoroutine(RateOfShot());        //発射～発射間のコルーチンを終了

        _currentShotCount = 0;              //発射回数を0に初期化

        _currentShotNumber++;               //発射する弾の配列参照番号を変更

        //配列参照番号が配列の要素数を超えていないか
        if (_currentShotNumber >= _charactorMoveData._movementAndShootingPaterns[_waveCount]._shots.Length)
        {
            //超えた

            _currentShotNumber = 0;     //配列参照番号を0に戻す
        }
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    /// <returns>_interval : インターバル時間</returns>
    IEnumerator MovementInterval()
    {
        _isMovingInterval = true;       //移動待機中フラグtrue

        yield return _movingInterval;   //待機時間分待機

        _isMovingInterval = false;      //移動待機中フラグfalse

        _isIgnoreThisCheckPoint = false;

        hasArrived = false;

        _timer = 0;                     //時間計測用タイマリセット
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>カーブ弾のベジェ曲線を生成・現在地点を算出するスクリプト</para>
    /// </summary>
    /// <returns>currentCurvePos = 算出されたベジェ曲線上の座標</returns>
    private Vector2 CalcuateBezierCurve()
    {
        Vector2 currentMoveCheckpoint = _moveCheckPoints[_checkpointCounter];   //現在のチェックポイントの座標

        Vector2 nextMoveCheckpoint = _moveCheckPoints[_nextCheckPointNumber];   //次のチェックポイントの座標

        _relayPointVector = currentMoveCheckpoint - nextMoveCheckpoint;         //現在地 - 次チェックポイント間のベクトルを算出


        /*ベクトル上の0.0～1.0でオフセットした中間点を算出
         * 0.0 : 発射地点
         * 0.5 : 発射地点とターゲットの中間
         * 1.0 : ターゲットの座標
         */
        _relayPointY = Vector2.Lerp(currentMoveCheckpoint, nextMoveCheckpoint, _charactorMoveData._curveMoveVerticalOffset);

        /*弾軌道の左右値に応じて計算するベクトルの向きを変更する
         * 
         * 左に飛ばす場合は_relayPointVectorに対して左向きの垂直ベクトルに対して左右値をかける
         * 右に飛ばす場合は_relayPointVectorに対して右向きの垂直ベクトルに対して左右値をかける
         * 
         * _relayPointYで求めたベクトル上の中間地点をもとに垂直ベクトルを出す
         */

        float horizontalAxisOffset = _charactorMoveData._curveMoveHorizontalOffset;      //ベクトルに対する横軸オフセット値を設定

        if (horizontalAxisOffset <= 0)
        {
            //発射地点～ターゲット間ベクトルに対する左向きに垂直なベクトルを求める
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        else if (horizontalAxisOffset > 0)
        {
            //発射地点～ターゲット間ベクトルに対する右向きに垂直なベクトルを求める
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        Vector2 firstVec = Vector2.Lerp(currentMoveCheckpoint, _fixedRelayPoint, _timer);

        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, nextMoveCheckpoint, _timer);

        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _timer);


        return currentCurvePos;
    }

    /// <summary>
    /// <para>SettingShotPrameters</para>
    /// <para>発射する弾のパラメータをもとに連射速度や発射数を参照して発射の停止を行う</para>
    /// </summary>
    private void SettingShotPrameters()
    {
        //弾の最大発射数を格納
        _maxShotCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotCounts[_currentShotNumber];
        
        //現在の発射数が最大発射数を超えていないか
        if (_currentShotCount <= _maxShotCount)
        {
            //越えていない

            //秒間に何発撃つかを格納
            int shotPerSeconds = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotPerSeconds[_currentShotNumber] + 1;

            _shotInterval = new WaitForSeconds(SECOND / shotPerSeconds);      //ショット～ショット間の待機時間を設定

            Shot();         //弾発射処理
        }
        else
        {
            //越えた

            _isShotInSameTime = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isShotInSameTime[_currentShotNumber];

            if (_isShotInSameTime)
            {
                _isIgnoreThisCheckPoint = false;

                SetShotNumber();

                SettingShotPrameters();     //弾に受け渡すパラメータの設定・発射
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

            return;     //何もしない
        }

        _multiShotOffsetAngle = 0;   //発射角の初期化

        //回転撃ちをするかのフラグを取得
        bool isSwingShot = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isSwingShots[_currentShotNumber];

        //回転撃ちする?
        if (isSwingShot)
        {
            //する

            //回転撃ち時に回す角度の取得
            float centralAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._swingShotFormedAngles[_currentShotNumber];

            //回転撃ち時の初弾の角度の取得
            float firstAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._swingShotFirstAngles[_currentShotNumber];

            //単位角を算出
            float radian = centralAngle / _maxShotCount;


            //初弾か?
            if (_currentShotCount <= 0)
            {
                //初弾

                _swingShotOffsetAngle = firstAngle;     //発射角に初弾の角度を設定
            }
            else
            {
                _swingShotOffsetAngle += radian;        //発射角に単位角を加算
            }
        }
        else
        {
            //しない

            _swingShotOffsetAngle = 0;  //角度を初期化
        }

        //現在の弾の撃ち方を格納(enum)
        _currentShotPatern = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotPaterns[_currentShotNumber];

        //格納した撃ち方をもとに処理分け

        switch (_currentShotPatern)           //弾の撃ち方
        {
            case CharactorShootingData.ShotPatern.OneShot:              //単発発射

                EnableShot();                           //弾の有効化 or 生成
                StartCoroutine(RateOfShot());           //インターバル処理

                break;

            case CharactorShootingData.ShotPatern.AllAtOnce:

                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount <= _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    EnableShot();               //弾の有効化 or 生成
                }

                _currentPelletCount = 0;        //同時生成弾数を初期化

                StartCoroutine(RateOfShot());   //インターバル処理

                break;


            case CharactorShootingData.ShotPatern.MultipleShots:        //複数発同時発射

                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                float maxOffset = 0;        //最大発射角

                float currentAngle = 0;     //現在の発射角

                //一度に生成する弾数分回るループ
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    float formedAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._multiShotFormedAngles[_currentShotNumber];

                    //初弾か?
                    if (pelletCount == 0)
                    {
                        //初弾

                        maxOffset = formedAngle / 2;       //最大発射角を算出

                        _multiShotOffsetAngle = -maxOffset;         //最大発射角を代入

                        currentAngle = formedAngle / (_maxPelletCount - 1);     //弾と弾の間の角度を算出
                    }
                    else
                    {
                        //2発目以降

                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentAngle;   //最初に設定した発射角に加算
                    }

                    EnableShot();               //弾の有効化 or 生成
                }

                _currentPelletCount = 0;        //同時生成弾数を初期化

                StartCoroutine(RateOfShot());   //インターバル処理

                break;

            case CharactorShootingData.ShotPatern.RadialShots:      //放射状発射

                float currentRadialAngle = 0;   //ショット～ショット間の角度格納用

                //同時生成弾数
                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                //同時生成弾数分ループ
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;      //現在の生成弾数を格納(初速変更用にShotMoveへ受け渡すため)

                    if (pelletCount == 0)       //初弾の場合
                    {
                        _multiShotOffsetAngle = 0;                      //ずらし角の初期化
                        currentRadialAngle = 360 / _maxPelletCount;     //弾と弾の間の角度を算出
                    }
                    else
                    {
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentRadialAngle;   //最初に設定した発射角に加算
                    }

                    EnableShot();               //弾の有効化 or 生成
                }

                _currentPelletCount = 0;

                StartCoroutine(RateOfShot());           //インターバル処理

                break;
        }

        _currentShotCount++;                    //撃った弾数を加算

    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>発射する弾に対応したプールを探索し、未使用の弾があればその弾を有効化。無ければ新たにプール内に生成するメソッド</para>
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

                shot.gameObject.SetActive(true);            //見つけた弾を有効化

                CheckShotType(shot);                        //弾種の判定

                shot.position = this.transform.position;    //trueにした弾をプレイヤーの位置に移動

                return;     //処理を終了
            }
        }

        //以下未使用オブジェクトが無かった場合新しく弾を生成
        
        //新たに生成する弾の弾番号を取得(弾番号と配列要素数の差を修正するため取得値 -1を格納)
        int shotNumber = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shots[_currentShotNumber] - 1;

        //新たに発射する弾のオブジェクトを取得
        GameObject shotObject = _charactorMoveData._shots[shotNumber];

        //取得した弾オブジェクトを対応するプールの子オブジェクトとして生成
        GameObject newShot = Instantiate(shotObject, _shotPools[_currentShotNumber].transform);

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
        //発射SEがあるか
        if (_charactorMoveData._shotSoundEffect != null)
        {
            //ある

            audioSource.PlayOneShot(_charactorMoveData._shotSoundEffect);       //発射SEを再生
        }

        _isShotInterval = true;         //発射インターバル中フラグをtrueに

        yield return _shotInterval;     //発射間インターバル処理

        _isShotInterval = false;        //発射インターバル中フラグをfalseに

    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>弾の種類を判定する。自機狙いフラグが立っている場合に発射角にプレイヤーとのベクトル角を加算する</para>
    /// </summary>
    /// <param name="shot">Shotメソッドで有効化/生成された弾。オブジェクトプール探索の際にTransform型で取得するためTransform型</param>
    private void CheckShotType(Transform shot)
    {
        //自機狙い弾か否かを格納するフラグ
        bool isTargetingPlayer = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isTargetingPlayer[_currentShotNumber];

        //自機狙い弾か
        if (!isTargetingPlayer)
        {
            //通常弾

            _targetingPosition = Vector2.down;      //真下をターゲット座標に
        }
        else
        {
            //自機狙い

            //同時生成弾かつその初弾か
            if (_currentShotPatern != CharactorShootingData.ShotPatern.OneShot && _currentPelletCount <= 0)
            {
                _targetingPosition = _player.transform.position;    //ターゲットとしてその瞬間のプレイヤーの座標を格納

                return;
            }

            //同時生成弾ではない場合は常にプレイヤーの座標をターゲット座標として格納する

            _targetingPosition = _player.transform.position;    //ターゲットとしてその瞬間のプレイヤーの座標を格納
        }

        /*
         * _targetingPosition は自機狙いか否かで入るものが変わります
         * 
         * 通常弾     : キャラクターの正面(Vector2.down)
         * 
         * 自機狙い弾 : 初弾発射時のプレイヤーの座標
         */

        Vector2 degree = _targetingPosition - (Vector2)this.transform.position; //現在の座標とターゲットとして格納する座標間のベクトルを求める


        float radian = Mathf.Atan2(degree.y, degree.x);                         //ベクトルから角度に変換

        ShotMove shotMove = shot.GetComponent<ShotMove>();

        /* 発射方向に同時発射時の加算角(放射状、扇状の場合)と回し角(回し撃ちの場合)を加算して弾に発射角として受け渡し
         * 
         * _multiShotOffsetAngle  : 扇形・円形に撃つ際に弾～弾間の弧度を格納
         * _swingShotOffsetAngle  : 回し撃ちの際に弾～弾間の孤度を格納
         */
        shotMove.GetSetShotAngle = radian * Mathf.Rad2Deg + _multiShotOffsetAngle + _swingShotOffsetAngle;

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
            Gizmos.DrawSphere(_moveCheckPoints[_checkpointCounter], 0.1f);
            Gizmos.DrawSphere(_moveCheckPoints[_nextCheckPointNumber], 0.1f);

            Gizmos.DrawLine(_moveCheckPoints[_checkpointCounter], _moveCheckPoints[_nextCheckPointNumber]);
        }
        else
        {

            Gizmos.DrawSphere(_moveCheckPoints[_checkpointCounter], 0.1f);
            Gizmos.DrawSphere(_moveCheckPoints[_nextCheckPointNumber], 0.1f);

            Gizmos.DrawSphere(_fixedRelayPoint, 0.2f);

            Gizmos.DrawSphere(_relayPointY, 0.1f);

            Gizmos.DrawLine(_moveCheckPoints[_checkpointCounter], _fixedRelayPoint);

            Gizmos.DrawLine(_fixedRelayPoint, _moveCheckPoints[_nextCheckPointNumber]);

        }
#endif

    }


}
