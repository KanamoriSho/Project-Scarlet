using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharactorMove : MonoBehaviour
{
    [SerializeField, Label("�L�������[�u�f�[�^")]
    private CharactorMoveData _charactorMoveData = default;

    private int _waveCount = 0;         //�E�F�[�u�����i�[����ϐ�

    [SerializeField]
    private int _currentShotNumber = 0;         //���݂̔��˂���e�̔ԍ����i�[����ϐ�

    [SerializeField, Label("�V���b�g�p�v�[��")]
    private GameObject[] _shotPools = default;

    [SerializeField]
    private int _currentShotCount = 0;          //�������̒e�������������i�[����ϐ�

    private int _maxShotCount = 0;              //���̒e�������������i�[����ϐ�

    private int _currentPelletCount = 0;        //���˂���e�̌��݂̐��������i�[����ϐ�

    private int _maxPelletCount = 0;        //���˂���e�̓������������i�[����ϐ�

    public int GetCurrentShotCount
    {
        //_currentPelletCount��Ԃ�
        get { return _currentShotCount; }
    }

    public int GetMaxShotCount
    {
        //_maxShotCount��Ԃ�
        get { return _maxShotCount; }
    }

    public int GetCurrentPelletCount
    {
        //_currentPelletCount��Ԃ�
        get { return _currentPelletCount; }
    }

    public int GetMaxPelletCount
    {
        //_maxPelletCount��Ԃ�
        get { return _maxPelletCount; }
    }

    public bool GetIsDecelerationPerShoot
    {
        //CharactorMoveData�̔��˂��Ƃɏ����������������邩�̃t���O��Ԃ�(ShotMove�Ɏ󂯓n��)
        get { return _charactorMoveData._movementAndShootingPaterns[_waveCount]._isDecelerationPerShoot[_currentShotNumber]; }
    }


    CharactorShootingData.ShotPatern _currentShotPatern = default;       //�e�̌��������i�[����Enum

    private float _timer = 0;                           //���Ԍv���p�ϐ�

    private float _multiShotOffsetAngle = default;      //���������ɔ��˂���ꍇ�̔��ˊp���i�[����ϐ�

    private float _swingShotOffsetAngle = default;      //��]����������ۂ̉��Z�p���i�[����ϐ�

    private int _checkpointCounter = 0;                 //���݂̈ړ��`�F�b�N�|�C���g�̔ԍ����i�[����

    private int _nextCheckPointNumber = 0;              //���Ɍ������`�F�b�N�|�C���g�̔ԍ����i�[����

    [SerializeField, Label("�`�F�b�N�|�C���g")]
    private List<Vector2> _moveCheckPoints = new List<Vector2>();       //�ړ��p�`�F�b�N�|�C���g�̍��W���i�[����

    private Vector2 _movingOffset = new Vector2(0, 0);                  //�`�F�b�N�|�C���g����ǂꂾ�����炵�Ĉړ������邩(����ړ����p)

    [SerializeField]
    private List<float> _intervalBetweenMoves = new List<float>();      //�ړ��`�ړ��ԑҋ@���Ԃ��i�[���郊�X�g
    [SerializeField]
    private bool _isMovingInterval = false;

    private WaitForSeconds _movingInterval = default;       //�ړ����̃R���[�`���̃L���b�V��

    private WaitForSeconds _shotInterval = default;         //�e�̘A�ˑ��x���Ǘ�����R���[�`���̃L���b�V��

    private float SECOND = 1.0f;                            //��b�̒萔

    private bool _isShotInterval = false;                   //���˃C���^�[�o��������t���O
    [SerializeField]
    private bool _isIgnoreThisCheckPoint = false;           //���̃`�F�b�N�|�C���g�Ŕ��ˏ����𖳎����邩

    private bool _isShotInSameTime = false;

    private bool hasArrived = false;

    private GameObject _player = default;                   //�v���C���[�i�[�p

    private Vector2 _targetingPosition = default;           //�_���Ă�����W�i�[�p(���ˊp�v�Z�p)

    private string PLAYER_TAG = "Player";                   //�v���C���[�̃^�O���i�[����萔

    private Animator _animator = default;           //���g��Animtor�i�[�p

    private AudioSource audioSource = default;      //���g��AudioSource�i�[�p


    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A�v�Z���̂̓��[�J���ϐ��Ŏ������̂ōŏI�I�ɏ����\��ł��B

    private Vector2 _fixedRelayPoint = default;     //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;    //�����ʒu - �ڕW�ʒu�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;         //_relayPointVector��̏c(Y)�����W�i�[�p

    private void Awake()
    {
        _animator = this.gameObject.GetComponent<Animator>();                   //Animator�̎擾

        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);                 //�v���C���[�L�����̎擾

        /*�e���v�[���ɐ�������
         * _charactorMoveData._waves                   : �E�F�[�u��(�{�X�L�����ȊO��1)
         * _charactorMoveData._initiallyGeneratedShots : ���������e��(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)
         */

        //�E�F�[�u�������[�v
        for (int waveCount = 0; waveCount < _charactorMoveData._waveCount; waveCount++)
        {
            //���̃E�F�[�u�Ŏg�p����e��̐����i�[
            int _currentShotNumber = _charactorMoveData._movementAndShootingPaterns[waveCount]._shots.Length;

            //�E�F�[�u���g�p�e�̎�ޕ����[�v
            for (int shotNumber = 0; shotNumber < _currentShotNumber; shotNumber++)
            {

                //�E�F�[�u���Ŏg�p�����e�𐶐����郋�[�v
                for (int shotLength = 0; shotLength < _charactorMoveData._initiallyGeneratedShots; shotLength++)
                {
                    //�g�p����e��z�񂩂���o���i�[
                    GameObject currentShotObject = 
                                _charactorMoveData._shots[_charactorMoveData._movementAndShootingPaterns[waveCount]._shots[shotNumber] - 1];

                    //�e�̐���
                    GameObject newShot = Instantiate(currentShotObject, _shotPools[shotNumber].transform);

                    newShot.SetActive(false);                   //���������e��false�ɂ���
                }
            }
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;            //���Ԃ����Z

        //���̈ړ���`�F�b�N�|�C���g���w��ł��Ă��邩
        if (_nextCheckPointNumber != _checkpointCounter + 1 || _nextCheckPointNumber != 0)
        {
            //�ł��Ă��Ȃ�

            _nextCheckPointNumber = _checkpointCounter + 1;       //���݂̃`�F�b�N�|�C���g + 1�����̃`�F�b�N�|�C���g�ԍ��Ƃ��Ċi�[

            //�`�F�b�N�|�C���g�i�[�z��̗v�f���𒴂���?
            if (_checkpointCounter + 1 >= _moveCheckPoints.Count)
            {
                //������

                _nextCheckPointNumber = 0;      //0�����`�F�b�N�|�C���g�ԍ��Ƃ��Ċi�[
            }
        }

        //�ړ����ɒe�������ۂ��̃t���O���i�[
        bool isShotOnTheMove = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig[_currentShotNumber];

        int nextShotNumber = _currentShotNumber + 1;

        if(nextShotNumber >= _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig.Length)
        {
            nextShotNumber = 0;
        }

        //���̒e���ړ����Ɍ����ۂ��̃t���O���i�[
        bool isNextShotOnTheMove = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isMoveingShootig[nextShotNumber];

        //���݂̍��W�����`�F�b�N�|�C���g�Ɠ�����
        if ((Vector2)this.transform.position == _moveCheckPoints[_nextCheckPointNumber] && !hasArrived)
        {
            hasArrived = true;

            if (isShotOnTheMove != isNextShotOnTheMove)
            {
                _isIgnoreThisCheckPoint = true;
            }

            _movingInterval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);    //�ړ��Ԃ̃C���^�[�o�����L���b�V��

            _checkpointCounter = _nextCheckPointNumber;     //���݂̃`�F�b�N�|�C���g����������

            StartCoroutine(MovementInterval());             //�ړ��`�ړ��Ԃ̑ҋ@�R���[�`��

            SetShotNumber();
        }

        //�ړ��Ԃ̑ҋ@���Ȃ�
        if (_isMovingInterval)
        {
            if(_isIgnoreThisCheckPoint)
            {
                return;
            }

            //�ړ����Ɍ��t���O��false?
            if (!isShotOnTheMove)
            {
                //false������

                SettingShotPrameters();     //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����

            }

            return;
        }

        //�ړ����Ɍ��t���O��true?
        if (isShotOnTheMove)
        {
            //true������

            SettingShotPrameters();     //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����
        }

        //�ړ����ɋȐ��I�ɔ��?
        if (!_charactorMoveData._isCurveMoving)
        {
            //false

            Vector2 currentPosition = _moveCheckPoints[_checkpointCounter];         //���݈ʒu

            Vector2 nextPosition = _moveCheckPoints[_nextCheckPointNumber];         //�ړ���̖ڕW���W

            /* �ړ����x�̌v�Z
             * �ړ����x * �ړ����x�p�A�j���[�V�����J�[�u�̒l
             */
            float movingSpeed = _charactorMoveData._speed * _charactorMoveData._speedCurve.Evaluate(_timer);

            /* Lerp�Ń`�F�b�N�|�C���g�Ԃ��ړ�
             * �ґ��ړ��p�I�t�Z�b�g�l�����Z����(�P�̔�s�̏ꍇ��+-0)
             */

            this.transform.position = Vector2.Lerp(currentPosition, nextPosition, movingSpeed) + _movingOffset;
        }
        else
        {
            //true

            /* �x�W�F�Ȑ�����o�������W�����ݒn�_��
             * �ґ��ړ��p�I�t�Z�b�g�l�����Z����(�P�̔�s�̏ꍇ��+-0)
             */

            this.transform.position = CalcuateBezierCurve() + _movingOffset;
        }
    }

    /// <summary>
    /// <para>SetShotNumber</para>
    /// <para>���˂���e�̒e�ԍ��̕ύX�A���ː��̏��������s��</para>
    /// </summary>
    private void SetShotNumber()
    {
        StopCoroutine(RateOfShot());        //���ˁ`���ˊԂ̃R���[�`�����I��

        _currentShotCount = 0;              //���ˉ񐔂�0�ɏ�����

        _currentShotNumber++;               //���˂���e�̔z��Q�Ɣԍ���ύX

        //�z��Q�Ɣԍ����z��̗v�f���𒴂��Ă��Ȃ���
        if (_currentShotNumber >= _charactorMoveData._movementAndShootingPaterns[_waveCount]._shots.Length)
        {
            //������

            _currentShotNumber = 0;     //�z��Q�Ɣԍ���0�ɖ߂�
        }
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    /// <returns>_interval : �C���^�[�o������</returns>
    IEnumerator MovementInterval()
    {
        _isMovingInterval = true;       //�ړ��ҋ@���t���Otrue

        yield return _movingInterval;   //�ҋ@���ԕ��ҋ@

        _isMovingInterval = false;      //�ړ��ҋ@���t���Ofalse

        _isIgnoreThisCheckPoint = false;

        hasArrived = false;

        _timer = 0;                     //���Ԍv���p�^�C�}���Z�b�g
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>�J�[�u�e�̃x�W�F�Ȑ��𐶐��E���ݒn�_���Z�o����X�N���v�g</para>
    /// </summary>
    /// <returns>currentCurvePos = �Z�o���ꂽ�x�W�F�Ȑ���̍��W</returns>
    private Vector2 CalcuateBezierCurve()
    {
        Vector2 currentMoveCheckpoint = _moveCheckPoints[_checkpointCounter];   //���݂̃`�F�b�N�|�C���g�̍��W

        Vector2 nextMoveCheckpoint = _moveCheckPoints[_nextCheckPointNumber];   //���̃`�F�b�N�|�C���g�̍��W

        _relayPointVector = currentMoveCheckpoint - nextMoveCheckpoint;         //���ݒn - ���`�F�b�N�|�C���g�Ԃ̃x�N�g�����Z�o


        /*�x�N�g�����0.0�`1.0�ŃI�t�Z�b�g�������ԓ_���Z�o
         * 0.0 : ���˒n�_
         * 0.5 : ���˒n�_�ƃ^�[�Q�b�g�̒���
         * 1.0 : �^�[�Q�b�g�̍��W
         */
        _relayPointY = Vector2.Lerp(currentMoveCheckpoint, nextMoveCheckpoint, _charactorMoveData._curveMoveVerticalOffset);

        /*�e�O���̍��E�l�ɉ����Čv�Z����x�N�g���̌�����ύX����
         * 
         * ���ɔ�΂��ꍇ��_relayPointVector�ɑ΂��č������̐����x�N�g���ɑ΂��č��E�l��������
         * �E�ɔ�΂��ꍇ��_relayPointVector�ɑ΂��ĉE�����̐����x�N�g���ɑ΂��č��E�l��������
         * 
         * _relayPointY�ŋ��߂��x�N�g����̒��Ԓn�_�����Ƃɐ����x�N�g�����o��
         */

        float horizontalAxisOffset = _charactorMoveData._curveMoveHorizontalOffset;      //�x�N�g���ɑ΂��鉡���I�t�Z�b�g�l��ݒ�

        if (horizontalAxisOffset <= 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂��鍶�����ɐ����ȃx�N�g�������߂�
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        else if (horizontalAxisOffset > 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂���E�����ɐ����ȃx�N�g�������߂�
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
    /// <para>���˂���e�̃p�����[�^�����ƂɘA�ˑ��x�┭�ː����Q�Ƃ��Ĕ��˂̒�~���s��</para>
    /// </summary>
    private void SettingShotPrameters()
    {
        //�e�̍ő唭�ː����i�[
        _maxShotCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotCounts[_currentShotNumber];
        
        //���݂̔��ː����ő唭�ː��𒴂��Ă��Ȃ���
        if (_currentShotCount <= _maxShotCount)
        {
            //�z���Ă��Ȃ�

            //�b�Ԃɉ����������i�[
            int shotPerSeconds = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotPerSeconds[_currentShotNumber] + 1;

            _shotInterval = new WaitForSeconds(SECOND / shotPerSeconds);      //�V���b�g�`�V���b�g�Ԃ̑ҋ@���Ԃ�ݒ�

            Shot();         //�e���ˏ���
        }
        else
        {
            //�z����

            _isShotInSameTime = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isShotInSameTime[_currentShotNumber];

            if (_isShotInSameTime)
            {
                _isIgnoreThisCheckPoint = false;

                SetShotNumber();

                SettingShotPrameters();     //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����
            }
        }
    }

    /// <summary>
    /// <para>Shot</para>
    /// <para>�e�̔��ˏ����B ��ѕ��A�p�x����ݒ肷��</para>
    /// </summary>
    private void Shot()
    {
        //�C���^�[�o������
        if (_isShotInterval)
        {
            //�C���^�[�o����

            return;     //�������Ȃ�
        }

        _multiShotOffsetAngle = 0;   //���ˊp�̏�����

        //��]���������邩�̃t���O���擾
        bool isSwingShot = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isSwingShots[_currentShotNumber];

        //��]��������?
        if (isSwingShot)
        {
            //����

            //��]�������ɉ񂷊p�x�̎擾
            float centralAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._swingShotFormedAngles[_currentShotNumber];

            //��]�������̏��e�̊p�x�̎擾
            float firstAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._swingShotFirstAngles[_currentShotNumber];

            //�P�ʊp���Z�o
            float radian = centralAngle / _maxShotCount;


            //���e��?
            if (_currentShotCount <= 0)
            {
                //���e

                _swingShotOffsetAngle = firstAngle;     //���ˊp�ɏ��e�̊p�x��ݒ�
            }
            else
            {
                _swingShotOffsetAngle += radian;        //���ˊp�ɒP�ʊp�����Z
            }
        }
        else
        {
            //���Ȃ�

            _swingShotOffsetAngle = 0;  //�p�x��������
        }

        //���݂̒e�̌��������i�[(enum)
        _currentShotPatern = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shotPaterns[_currentShotNumber];

        //�i�[���������������Ƃɏ�������

        switch (_currentShotPatern)           //�e�̌�����
        {
            case CharactorShootingData.ShotPatern.OneShot:              //�P������

                EnableShot();                           //�e�̗L���� or ����
                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;

            case CharactorShootingData.ShotPatern.AllAtOnce:

                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount <= _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    EnableShot();               //�e�̗L���� or ����
                }

                _currentPelletCount = 0;        //���������e����������

                StartCoroutine(RateOfShot());   //�C���^�[�o������

                break;


            case CharactorShootingData.ShotPatern.MultipleShots:        //��������������

                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                float maxOffset = 0;        //�ő唭�ˊp

                float currentAngle = 0;     //���݂̔��ˊp

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    float formedAngle = _charactorMoveData._movementAndShootingPaterns[_waveCount]._multiShotFormedAngles[_currentShotNumber];

                    //���e��?
                    if (pelletCount == 0)
                    {
                        //���e

                        maxOffset = formedAngle / 2;       //�ő唭�ˊp���Z�o

                        _multiShotOffsetAngle = -maxOffset;         //�ő唭�ˊp����

                        currentAngle = formedAngle / (_maxPelletCount - 1);     //�e�ƒe�̊Ԃ̊p�x���Z�o
                    }
                    else
                    {
                        //2���ڈȍ~

                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentAngle;   //�ŏ��ɐݒ肵�����ˊp�ɉ��Z
                    }

                    EnableShot();               //�e�̗L���� or ����
                }

                _currentPelletCount = 0;        //���������e����������

                StartCoroutine(RateOfShot());   //�C���^�[�o������

                break;

            case CharactorShootingData.ShotPatern.RadialShots:      //���ˏ󔭎�

                float currentRadialAngle = 0;   //�V���b�g�`�V���b�g�Ԃ̊p�x�i�[�p

                //���������e��
                _maxPelletCount = _charactorMoveData._movementAndShootingPaterns[_waveCount]._pelletCountInShots[_currentShotNumber];

                //���������e�������[�v
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    _currentPelletCount = pelletCount;      //���݂̐����e�����i�[(�����ύX�p��ShotMove�֎󂯓n������)

                    if (pelletCount == 0)       //���e�̏ꍇ
                    {
                        _multiShotOffsetAngle = 0;                      //���炵�p�̏�����
                        currentRadialAngle = 360 / _maxPelletCount;     //�e�ƒe�̊Ԃ̊p�x���Z�o
                    }
                    else
                    {
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentRadialAngle;   //�ŏ��ɐݒ肵�����ˊp�ɉ��Z
                    }

                    EnableShot();               //�e�̗L���� or ����
                }

                _currentPelletCount = 0;

                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;
        }

        _currentShotCount++;                    //�������e�������Z

    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>���˂���e�ɑΉ������v�[����T�����A���g�p�̒e������΂��̒e��L�����B������ΐV���Ƀv�[�����ɐ������郁�\�b�h</para>
    /// </summary>
    private void EnableShot()
    {
        //�I�u�W�F�N�g�v�[�����ɖ��g�p�I�u�W�F�N�g���������{��
        foreach (Transform shot in _shotPools[_currentShotNumber].transform)
        {
            //���g�p�I�u�W�F�N�g����������
            if (!shot.gameObject.activeSelf)
            {
                //���g�p�I�u�W�F�N�g��������

                shot.gameObject.SetActive(true);            //�������e��L����

                CheckShotType(shot);                        //�e��̔���

                shot.position = this.transform.position;    //true�ɂ����e���v���C���[�̈ʒu�Ɉړ�

                return;     //�������I��
            }
        }

        //�ȉ����g�p�I�u�W�F�N�g�����������ꍇ�V�����e�𐶐�
        
        //�V���ɐ�������e�̒e�ԍ����擾(�e�ԍ��Ɣz��v�f���̍����C�����邽�ߎ擾�l -1���i�[)
        int shotNumber = _charactorMoveData._movementAndShootingPaterns[_waveCount]._shots[_currentShotNumber] - 1;

        //�V���ɔ��˂���e�̃I�u�W�F�N�g���擾
        GameObject shotObject = _charactorMoveData._shots[shotNumber];

        //�擾�����e�I�u�W�F�N�g��Ή�����v�[���̎q�I�u�W�F�N�g�Ƃ��Đ���
        GameObject newShot = Instantiate(shotObject, _shotPools[_currentShotNumber].transform);

        CheckShotType(newShot.transform);                           //�e��̔���

        newShot.transform.position = this.transform.position;       //���������e���L�����N�^�[�̈ʒu�Ɉړ�


    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    /// <returns>_shotInterval : �C���^�[�o������</returns>
    IEnumerator RateOfShot()
    {
        //����SE�����邩
        if (_charactorMoveData._shotSoundEffect != null)
        {
            //����

            audioSource.PlayOneShot(_charactorMoveData._shotSoundEffect);       //����SE���Đ�
        }

        _isShotInterval = true;         //���˃C���^�[�o�����t���O��true��

        yield return _shotInterval;     //���ˊԃC���^�[�o������

        _isShotInterval = false;        //���˃C���^�[�o�����t���O��false��

    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>�e�̎�ނ𔻒肷��B���@�_���t���O�������Ă���ꍇ�ɔ��ˊp�Ƀv���C���[�Ƃ̃x�N�g���p�����Z����</para>
    /// </summary>
    /// <param name="shot">Shot���\�b�h�ŗL����/�������ꂽ�e�B�I�u�W�F�N�g�v�[���T���̍ۂ�Transform�^�Ŏ擾���邽��Transform�^</param>
    private void CheckShotType(Transform shot)
    {
        //���@�_���e���ۂ����i�[����t���O
        bool isTargetingPlayer = _charactorMoveData._movementAndShootingPaterns[_waveCount]._isTargetingPlayer[_currentShotNumber];

        //���@�_���e��
        if (!isTargetingPlayer)
        {
            //�ʏ�e

            _targetingPosition = Vector2.down;      //�^�����^�[�Q�b�g���W��
        }
        else
        {
            //���@�_��

            //���������e�����̏��e��
            if (_currentShotPatern != CharactorShootingData.ShotPatern.OneShot && _currentPelletCount <= 0)
            {
                _targetingPosition = _player.transform.position;    //�^�[�Q�b�g�Ƃ��Ă��̏u�Ԃ̃v���C���[�̍��W���i�[

                return;
            }

            //���������e�ł͂Ȃ��ꍇ�͏�Ƀv���C���[�̍��W���^�[�Q�b�g���W�Ƃ��Ċi�[����

            _targetingPosition = _player.transform.position;    //�^�[�Q�b�g�Ƃ��Ă��̏u�Ԃ̃v���C���[�̍��W���i�[
        }

        /*
         * _targetingPosition �͎��@�_�����ۂ��œ�����̂��ς��܂�
         * 
         * �ʏ�e     : �L�����N�^�[�̐���(Vector2.down)
         * 
         * ���@�_���e : ���e���ˎ��̃v���C���[�̍��W
         */

        Vector2 degree = _targetingPosition - (Vector2)this.transform.position; //���݂̍��W�ƃ^�[�Q�b�g�Ƃ��Ċi�[������W�Ԃ̃x�N�g�������߂�


        float radian = Mathf.Atan2(degree.y, degree.x);                         //�x�N�g������p�x�ɕϊ�

        ShotMove shotMove = shot.GetComponent<ShotMove>();

        /* ���˕����ɓ������ˎ��̉��Z�p(���ˏ�A���̏ꍇ)�Ɖ񂵊p(�񂵌����̏ꍇ)�����Z���Ēe�ɔ��ˊp�Ƃ��Ď󂯓n��
         * 
         * _multiShotOffsetAngle  : ��`�E�~�`�Ɍ��ۂɒe�`�e�Ԃ̌ʓx���i�[
         * _swingShotOffsetAngle  : �񂵌����̍ۂɒe�`�e�Ԃ̌Ǔx���i�[
         */
        shotMove.GetSetShotAngle = radian * Mathf.Rad2Deg + _multiShotOffsetAngle + _swingShotOffsetAngle;

    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>�x�W�F�Ȑ��̎n�_ - ���ԓ_�ԁA���ԓ_ - �I�_�Ԃ���ŕ`�悷�郁�\�b�h �f�o�b�O�p</para>
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
