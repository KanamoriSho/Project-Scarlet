using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorMove : MonoBehaviour
{
    [SerializeField, Label("�L�������[�u�f�[�^")]
    private CharactorMoveData _charactorMoveData = default;

    private int _waveCount = 0;

    private int _shotNumber = 0;

    [SerializeField, Label("�V���b�g�p�v�[��")]
    private GameObject _shotPool = default;

    private int _currentShotCount = 0;

    private int _maxShotCount = 0;

    [SerializeField]
    private int _checkpointCounter = 0;

    private int _nextCheckPoint = 0;

    [SerializeField, Label("�`�F�b�N�|�C���g")]
    private List<Vector2> _moveCheckPoints = new List<Vector2>();

    private Vector2 _movingOffset = new Vector2(0,0);

    private float _time = 0;

    private bool _isFinalCheckpoint = false;

    private Animator _animator = default;

    private AudioSource audioSource = default;              //���g��Animtor�i�[�p

    [SerializeField]
    private List<float> _intervalBetweenMoves = new List<float>();

    private bool _isMovingInterval = false;

    private WaitForSeconds _interval = default;             //�R���[�`���̃L���b�V��

    private WaitForSeconds _shotInterval = default;

    private float SECOND = 1.0f;                            //��b�̒萔

    private float _currentInterval = default;               //�R���[�`���̑ҋ@���Ԑݒ�p

    [SerializeField]
    private bool _isInterval = false;                       //�C���^�[�o��������t���O

    private GameObject _player = default;

    private Vector2 _targetingPosition = default;

    private string PLAYER_TAG = "Player";

    private string BOSS_TAG = "Boss";

    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A���[�J���ϐ��Ŏ������̂ł������������

    private Vector2 _fixedRelayPoint = default;     //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;    //�����ʒu - �ڕW�ʒu�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;         //_relayPointVector��̏c(Y)�����W�i�[�p

    private Vector2 _relayPointX = default;         //_relayPointVector��̉�(X)�����W�i�[�p

    private void Awake()
    {
        _animator = this.gameObject.GetComponent<Animator>();

        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
    }

    void Start()
    {
        /*�e���v�[���ɐ�������
         * _charactorMoveData._waves                   : �E�F�[�u��(�{�X�L�����ȊO��1)
         * _charactorMoveData._initiallyGeneratedShots : ���������e��(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)
         */
        for (int waveCount = 0; waveCount < _charactorMoveData._waveCount; waveCount++)
        {
            for (int count = 0; count < _charactorMoveData._initiallyGeneratedShots; count++)
            {
                GameObject newShot = Instantiate(_charactorMoveData._waves[waveCount]._shots[count], _shotPool.transform);       //�e�̐���

                newShot.SetActive(false);                   //���������e��false�ɂ���
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

            _interval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);           //�V���b�g�Ԃ̃C���^�[�o�����L���b�V��

            StartCoroutine(MovementInterval());

            _checkpointCounter++;

            _currentShotCount = 0;

        }

    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    /// <returns>_interval : �C���^�[�o������</returns>
    IEnumerator MovementInterval()
    {

        _isMovingInterval = true;

        yield return _interval;

        _isMovingInterval = false;

        _time = 0;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>�J�[�u�e�̃x�W�F�Ȑ��𐶐��E���ݒn�_���Z�o����X�N���v�g</para>
    /// </summary>
    /// <returns>currentCurvePos = �Z�o���ꂽ�x�W�F�Ȑ���̍��W</returns>
    private Vector2 CalcuateBezierCurve()
    {

        //���ݒn - ���`�F�b�N�|�C���g�Ԃ̃x�N�g�����Z�o
        _relayPointVector = _moveCheckPoints[_checkpointCounter] - _moveCheckPoints[_nextCheckPoint];

        //�x�N�g����̃I�t�Z�b�g�������ԓ_���Z�o
        _relayPointY = Vector2.Lerp(_moveCheckPoints[_checkpointCounter], _moveCheckPoints[_nextCheckPoint],
                                                                                _charactorMoveData._verticalOffset);

        /*�e�O���̍��E�l�ɉ����Čv�Z����x�N�g���̌�����ύX����
         * 
         * ���ɔ�΂��ꍇ��_relayPointVector�ɑ΂��č������̐����x�N�g���ɑ΂��č��E�l��������
         * �E�ɔ�΂��ꍇ��_relayPointVector�ɑ΂��ĉE�����̐����x�N�g���ɑ΂��č��E�l��������
         * 
         * _relayPointY�ŋ��߂��x�N�g����̒��Ԓn�_�����Ƃɐ����x�N�g�����o��
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
    /// <para>�e�̔��ˏ���</para>
    /// </summary>
    private void Shot()
    {
        if (_isInterval)    //�C���^�[�o����
        {
            return;     //�������Ȃ�
        }


        foreach (Transform shot in _shotPool.transform)     //�I�u�W�F�N�g�v�[�����ɖ��g�p�I�u�W�F�N�g���������{��
        {
            if (!shot.gameObject.activeSelf)             //���g�p�I�u�W�F�N�g����������
            {
                shot.gameObject.SetActive(true);            //true�ɂ���

                CheckShotType(shot);

                shot.position = this.transform.position;    //true�ɂ����e���v���C���[�̈ʒu�Ɉړ�

                _currentShotCount++;

                StartCoroutine(RateOfShot());           //�C���^�[�o������
                return;
            }
        }

        //�ȉ����g�p�I�u�W�F�N�g�����������ꍇ

        GameObject newShot = Instantiate(_charactorMoveData._waves[_waveCount]._shots[_shotNumber], _shotPool.transform);       //�V�����e�𐶐�

        CheckShotType(newShot.transform);

        newShot.transform.position = this.transform.position;             //���������e���L�����N�^�[�̈ʒu�Ɉړ�

        _currentShotCount++;

        StartCoroutine(RateOfShot());                   //�C���^�[�o������
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    /// <returns>_shotInterval : �C���^�[�o������</returns>
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
