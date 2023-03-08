using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorMove : MonoBehaviour
{
    [SerializeField, Label("キャラムーブデータ")]
    private CharactorMoveData _charactorMoveData = default;

    private int _waveCount = 0;

    private int _shotNumber = 0;

    [SerializeField, Label("ショット用プール")]
    private GameObject _shotPool = default;

    private int _currentShotCount = 0;

    private int _maxShotCount = 0;

    [SerializeField]
    private int _checkpointCounter = 0;

    private int _nextCheckPoint = 0;

    [SerializeField, Label("チェックポイント")]
    private List<Vector2> _moveCheckPoints = new List<Vector2>();

    private Vector2 _movingOffset = new Vector2(0,0);

    private float _time = 0;

    private bool _isFinalCheckpoint = false;

    private Animator _animator = default;

    private AudioSource audioSource = default;              //自身のAnimtor格納用

    [SerializeField]
    private List<float> _intervalBetweenMoves = new List<float>();

    private bool _isMovingInterval = false;

    private WaitForSeconds _interval = default;             //コルーチンのキャッシュ

    private WaitForSeconds _shotInterval = default;

    private float SECOND = 1.0f;                            //一秒の定数

    private float _currentInterval = default;               //コルーチンの待機時間設定用

    [SerializeField]
    private bool _isInterval = false;                       //インターバル中判定フラグ

    private GameObject _player = default;

    private Vector2 _targetingPosition = default;

    private string PLAYER_TAG = "Player";

    private string BOSS_TAG = "Boss";

    //以下の変数はOnDrowGizmosに受け渡すためにフィールド変数にしてますが、ローカル変数で事足りるのでいずれ消すかも

    private Vector2 _fixedRelayPoint = default;     //ベジェ曲線の中間点格納用

    private Vector2 _relayPointVector = default;    //初期位置 - 目標位置間のベクトル格納用

    private Vector2 _relayPointY = default;         //_relayPointVector上の縦(Y)軸座標格納用

    private Vector2 _relayPointX = default;         //_relayPointVector上の横(X)軸座標格納用

    private void Awake()
    {
        _animator = this.gameObject.GetComponent<Animator>();

        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
    }

    void Start()
    {
        /*弾をプールに生成する
         * _charactorMoveData._waves                   : ウェーブ数(ボスキャラ以外は1)
         * _charactorMoveData._initiallyGeneratedShots : 初期生成弾数(スクリプタブルオブジェクトから受け取り)
         */
        for (int waveCount = 0; waveCount < _charactorMoveData._waveCount; waveCount++)
        {
            for (int count = 0; count < _charactorMoveData._initiallyGeneratedShots; count++)
            {
                GameObject newShot = Instantiate(_charactorMoveData._waves[waveCount]._shots[count], _shotPool.transform);       //弾の生成

                newShot.SetActive(false);                   //生成した弾をfalseにする
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _time += Time.deltaTime;

        if (_isMovingInterval)
        {
            if (_currentShotCount < _charactorMoveData._shotCounts[_waveCount]._shotCounts[_shotNumber])
            {
                _shotInterval = new WaitForSeconds(SECOND / _charactorMoveData._shotPerSeconds[_waveCount]);




                Shot();
            }

            return;
        }

        if (_nextCheckPoint != _checkpointCounter + 1)
        {
            _nextCheckPoint = _checkpointCounter + 1;
        }


        if (_nextCheckPoint < _moveCheckPoints.Count)
        {


            if (!_charactorMoveData._isCurve)
            {
                this.transform.position = Vector2.Lerp(_moveCheckPoints[_checkpointCounter], _moveCheckPoints[_nextCheckPoint],
                                                        _charactorMoveData._speed * _charactorMoveData._speedCurve.Evaluate(_time));
            }
            else
            {
                this.transform.position = CalcuateBezierCurve();
            }
        }
        else
        {
            _checkpointCounter = 0;

            _nextCheckPoint = _checkpointCounter + 1;
        }

        if ((Vector2)this.transform.position == _moveCheckPoints[_nextCheckPoint])
        {

            _interval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);           //ショット間のインターバルをキャッシュ

            StartCoroutine(MovementInterval());

            _checkpointCounter++;

            _currentShotCount = 0;

        }

    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    /// <returns>_interval : インターバル時間</returns>
    IEnumerator MovementInterval()
    {

        _isMovingInterval = true;

        yield return _interval;

        _isMovingInterval = false;

        _time = 0;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>カーブ弾のベジェ曲線を生成・現在地点を算出するスクリプト</para>
    /// </summary>
    /// <returns>currentCurvePos = 算出されたベジェ曲線上の座標</returns>
    private Vector2 CalcuateBezierCurve()
    {

        //現在地 - 次チェックポイント間のベクトルを算出
        _relayPointVector = _moveCheckPoints[_checkpointCounter] - _moveCheckPoints[_nextCheckPoint];

        //ベクトル上のオフセットした中間点を算出
        _relayPointY = Vector2.Lerp(_moveCheckPoints[_checkpointCounter], _moveCheckPoints[_nextCheckPoint],
                                                                                _charactorMoveData._verticalOffset);

        /*弾軌道の左右値に応じて計算するベクトルの向きを変更する
         * 
         * 左に飛ばす場合は_relayPointVectorに対して左向きの垂直ベクトルに対して左右値をかける
         * 右に飛ばす場合は_relayPointVectorに対して右向きの垂直ベクトルに対して左右値をかける
         * 
         * _relayPointYで求めたベクトル上の中間地点をもとに垂直ベクトルを出す
         */

        if (_charactorMoveData._horizontalOffset <= 0)
        {
            _fixedRelayPoint = new Vector2(-_relayPointVector.y, _relayPointVector.x) * _charactorMoveData._horizontalOffset + _relayPointY;
        }
        else if (_charactorMoveData._horizontalOffset > 0)
        {
            _fixedRelayPoint = new Vector2(_relayPointVector.y, -_relayPointVector.x) * -_charactorMoveData._horizontalOffset + _relayPointY;
        }

        Vector2 firstVec = Vector2.Lerp(_moveCheckPoints[_checkpointCounter], _fixedRelayPoint, _time);

        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _moveCheckPoints[_nextCheckPoint], _time);

        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _time);


        return currentCurvePos;
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
            if (!shot.gameObject.activeSelf)             //未使用オブジェクトを見つけたら
            {
                shot.gameObject.SetActive(true);            //trueにする

                CheckShotType(shot);

                shot.position = this.transform.position;    //trueにした弾をプレイヤーの位置に移動

                _currentShotCount++;

                StartCoroutine(RateOfShot());           //インターバル処理
                return;
            }
        }

        //以下未使用オブジェクトが無かった場合

        GameObject newShot = Instantiate(_charactorMoveData._waves[_waveCount]._shots[_shotNumber], _shotPool.transform);       //新しく弾を生成

        CheckShotType(newShot.transform);

        newShot.transform.position = this.transform.position;             //生成した弾をキャラクターの位置に移動

        _currentShotCount++;

        StartCoroutine(RateOfShot());                   //インターバル処理
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>弾発射のインターバル処理を行う</para>
    /// </summary>
    /// <returns>_shotInterval : インターバル時間</returns>
    IEnumerator RateOfShot()
    {
        if (_charactorMoveData._shotSoundEffect != null)
        {
            audioSource.PlayOneShot(_charactorMoveData._shotSoundEffect);
        }


        _isInterval = true;

        yield return _shotInterval;

        _isInterval = false;

    }

    private void CheckShotType(Transform shot)
    {
        ShotMove shotMove = shot.GetComponent<ShotMove>();

        switch(_charactorMoveData._shotVelocity[_shotNumber])
        {
            case CharactorMoveData.ShotVelocity.Nomal:

            break;

            case CharactorMoveData.ShotVelocity.FastToSlow:

                float currentShot = shotMove.GetSetSpeed;

                shotMove.GetSetSpeed = currentShot - (currentShot * 0.1f * _currentShotCount);

                break;
        }

        ShotMoveData smd = shotMove._shotMoveData;

        switch (smd._shotType)
        {
            case ShotMoveData.ShotType.TargetToPlayer:

                if (_currentShotCount <= 0)
                {
                    _targetingPosition = _player.transform.position;
                }

                shot.GetComponent<ShotMove>().GetSetTargetPosition = _targetingPosition;

                break;

        }
    }
}
