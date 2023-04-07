using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharactorMove : MonoBehaviour
{
    [SerializeField, Label("�L�������[�u�f�[�^")]
    private CharactorMoveData _charactorMoveData = default;

    private CharactorShootingData _currentMovementAndShootingPaterns = default;

    [SerializeField, Label("�V���b�g�p�v�[��")]
    private GameObject[] _shotPools = default;

    [SerializeField, Label("�`�F�b�N�|�C���g")]
    private List<Vector2> _moveCheckpoints = new List<Vector2>();       //�ړ��p�`�F�b�N�|�C���g�̍��W���i�[����

    [SerializeField, Label("�ҋ@����")]
    private List<float> _intervalBetweenMoves = new List<float>();      //�ړ��`�ړ��ԑҋ@���Ԃ��i�[���郊�X�g

    private float _timer = 0;                   //���Ԍv���p�ϐ�

    private int _waveCount = 0;                 //�E�F�[�u�����i�[����ϐ�
    [SerializeField]
    private int _currentShotNumber = 0;         //���݂̔��˂���e�̔ԍ����i�[����ϐ�

    private int _currentShotCount = 0;          //�������̒e�������������i�[����ϐ�

    private int _maxShotCount = 0;              //���̒e�������������i�[����ϐ�

    private int _currentPelletCount = 0;        //���˂���e�̌��݂̐��������i�[����ϐ�

    private int _maxPelletCount = 0;            //���˂���e�̓������������i�[����ϐ�

    CharactorShootingData.ShotPatern _currentShotPatern = default;       //�e�̌��������i�[����Enum

    private Vector2 _targetingPosition = default;       //�_���Ă�����W�i�[�p(���ˊp�v�Z�p)

    private float _multiShotOffsetAngle = default;      //���������ɔ��˂���ꍇ�̔��ˊp���i�[����ϐ�

    private float _swingShotOffsetAngle = default;      //��]����������ۂ̉��Z�p���i�[����ϐ�

    private int _checkpointCounter = 0;                 //���݂̈ړ��`�F�b�N�|�C���g�̔ԍ����i�[����

    private int _nextCheckpointNumber = 0;              //���Ɍ������`�F�b�N�|�C���g�̔ԍ����i�[����

    private Vector2 _movingOffset = new Vector2(0, 0);  //�`�F�b�N�|�C���g����ǂꂾ�����炵�Ĉړ������邩(����ړ����p)

    private WaitForSeconds _movingInterval = default;   //�ړ����̃R���[�`���̃L���b�V��

    private WaitForSeconds _shotInterval = default;     //�e�̘A�ˑ��x���Ǘ�����R���[�`���̃L���b�V��

    private const float SECOND = 1.0f;                  //��b�̒萔
    [SerializeField]
    private bool _isMovingInterval = false;             //�ړ��ҋ@������t���O
    [SerializeField]
    private bool _isShotInterval = false;               //���˃C���^�[�o��������t���O
    [SerializeField]
    private bool _isNotShotInThisCheckpoint = false;    //���̃`�F�b�N�|�C���g�Ŕ��ˏ����𖳎����邩

    private const string PLAYER_TAG = "Player";         //�v���C���[�̃^�O���i�[����萔

    private GameObject _player = default;               //�v���C���[�i�[�p

    private Animator _animator = default;               //���g��Animtor�i�[�p

    private AudioSource audioSource = default;          //���g��AudioSource�i�[�p

    #region Getter

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

    public bool GetIsDecelerationPerShot
    {
        //CharactorMoveData�̔��˂��Ƃɏ����������������邩�̃t���O��Ԃ�(ShotMove�Ɏ󂯓n��)
        get { return _currentMovementAndShootingPaterns._isDecelerationPerShoot[_currentShotNumber]; }
    }

    #endregion

    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A�v�Z���̂̓��[�J���ϐ��Ŏ������̂ōŏI�I�ɏ����\��ł��B

    private Vector2 _fixedRelayPoint = default;         //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;        //�����ʒu - �ڕW�ʒu�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;             //_relayPointVector��̏c(Y)�����W�i�[�p

    private void Awake()
    {
        //Animator�̎擾
        _animator = this.gameObject.GetComponent<Animator>();

        //�v���C���[�L�����̎擾
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG);

        //���݂̍s���p�^�[�����擾
        _currentMovementAndShootingPaterns = _charactorMoveData._movementAndShootingPaterns[_waveCount];

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

                    //���������e��false�ɂ���
                    newShot.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        //���Ԃ����Z
        _timer += Time.deltaTime;

        //���̈ړ���`�F�b�N�|�C���g���w��ł��Ă��邩
        if (_nextCheckpointNumber != _checkpointCounter + 1 || _nextCheckpointNumber != 0)
        {
            //�ł��Ă��Ȃ�

            //���݂̃`�F�b�N�|�C���g + 1�����̃`�F�b�N�|�C���g�ԍ��Ƃ��Ċi�[
            _nextCheckpointNumber = _checkpointCounter + 1;

            //�`�F�b�N�|�C���g�i�[�z��̗v�f�����z����?
            if (_checkpointCounter + 1 >= _moveCheckpoints.Count)
            {
                //�z����

                //0�����`�F�b�N�|�C���g�ԍ��Ƃ��Ċi�[
                _nextCheckpointNumber = 0;
            }

            //�s���p�^�[���̍X�V
            _currentMovementAndShootingPaterns = _charactorMoveData._movementAndShootingPaterns[_waveCount];
        }

        //�ړ����ɒe�������ۂ��̃t���O���i�[
        bool isShotOnTheMove = _currentMovementAndShootingPaterns._isMovingShooting[_currentShotNumber];

        /*���݂̒e�̌��^�C�~���O�Ǝ��̒e�̌��^�C�~���O���قȂ邩�̃t���O���i�[����B
         * 
         * ���� : �ړ����Ȃ��猂��    �� : �~�܂��Č���
         * 
         * �̂悤�ȏ�Ԃ���true�ɂȂ�B
         */
        bool isCurrentShotMach = CheckCurrentAndNextShotType(isShotOnTheMove);

        Debug.Log("���݂̒e�̌��^�C�~���O�Ǝ��̒e�̌��^�C�~���O���قȂ邩" + isCurrentShotMach);

        //���݂̍��W�����`�F�b�N�|�C���g�Ɠ�����
        if (this.transform.position == (Vector3)_moveCheckpoints[_nextCheckpointNumber])
        {
            //���݂̒e�̌��^�C�~���O�Ǝ��̒e�̌��^�C�~���O���قȂ邩
            if (isCurrentShotMach)
            {
                //�قȂ�

                //���̃`�F�b�N�|�C���g�ł͔��ˏ��������Ȃ�
                _isNotShotInThisCheckpoint = true;

                Debug.Log("�^�C�~���O���قȂ�");
            }
            else
            {

                Debug.Log("���^�C�~���O������");
            }

            //�ړ��Ԃ̃C���^�[�o�����L���b�V��
            _movingInterval = new WaitForSeconds(_intervalBetweenMoves[_checkpointCounter]);

            //���݂̃`�F�b�N�|�C���g����������
            _checkpointCounter = _nextCheckpointNumber;

            //�ړ��`�ړ��Ԃ̑ҋ@�R���[�`��

            //�ҋ@���ԕ��ҋ@
            StartCoroutine(MovementInterval());

            //���˂���e�̒e�ԍ��̕ύX�A���ː��̏�����
            SetShotNumber();
        }

        //�ړ��Ԃ̑ҋ@���Ȃ�
        if (_isMovingInterval)
        {
            //���̃`�F�b�N�|�C���g�ł͔��ˏ��������Ȃ���
            if (_isNotShotInThisCheckpoint)
            {
                //���Ȃ�

                return;
            }

            //�ړ����Ɍ��t���O��false?
            if (!isShotOnTheMove)
            {
                //false

                //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����
                SettingShotPrameters();
            }

            return;
        }

        //�ړ����Ɍ��t���O��true?
        if (isShotOnTheMove)
        {
            //true

            //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����
            SettingShotPrameters();
        }

        //�ړ����ɋȐ��I�ɔ��?
        if (!_charactorMoveData._isCurveMoving)
        {
            //false

            //���݈ʒu
            Vector2 currentPosition = _moveCheckpoints[_checkpointCounter];

            //�ړ���̖ڕW���W
            Vector2 nextPosition = _moveCheckpoints[_nextCheckpointNumber];

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
            this.transform.position = CalculateBezierCurve() + _movingOffset;
        }
    }

    #region �ړ��֘A���\�b�h

    /// <summary>
    /// <para>CheckCurrentAndNextShotType</para>
    /// <para>���݂̒e�̌��^�C�~���O(�ړ����Ȃ��炩�~�܂��Ă�)�Ǝ��̒e�̌��^�C�~���O���r���A�قȂ��true��Ԃ�����</para>
    /// </summary>
    /// <param name="isShotOnTheMove">���݂̒e�̌��^�C�~���O (true : �ړ����Ȃ��猂�� false : �~�܂��Č���)</param>
    /// <returns>if(isShotOnTheMove != isNextShotOnTheMove) �̌��ʂ�Ԃ�</returns>
    private bool CheckCurrentAndNextShotType(bool isShotOnTheMove)
    {
        //���ʗp�t���O���`
        bool isChangeMoveShotToNextShot = default;

        //���Ɍ��e�̒e�ԍ����`
        int nextShotNumber = _currentShotNumber + 1;

        //���̔ԍ��͒e��z��̗v�f�����z���Ă��Ȃ���
        if (nextShotNumber >= _currentMovementAndShootingPaterns._isMovingShooting.Length)
        {
            //�z���Ă���

            //0�ɏ�����
            nextShotNumber = 0;
        }

        //���̒e���ړ����Ɍ����ۂ��̃t���O���i�[
        bool isNextShotOnTheMove = _currentMovementAndShootingPaterns._isMovingShooting[nextShotNumber];

        //���݂̒e�Ǝ��̒e�̌��^�C�~���O���قȂ邩
        if (isShotOnTheMove != isNextShotOnTheMove)
        {
            //�قȂ�

            //���ʗp�t���O��true���i�[
            isChangeMoveShotToNextShot = true;
        }
        else
        {
            //����

            //���ʗp�t���O��false���i�[
            isChangeMoveShotToNextShot = false;
        }

        //���ʂ�Ԃ�
        return isChangeMoveShotToNextShot;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>�J�[�u�e�̃x�W�F�Ȑ��𐶐��E���ݒn�_���Z�o����X�N���v�g</para>
    /// </summary>
    /// <returns>currentCurvePos = �Z�o���ꂽ�x�W�F�Ȑ���̍��W</returns>
    private Vector2 CalculateBezierCurve()
    {
        //���݂̃`�F�b�N�|�C���g�̍��W
        Vector2 currentMoveCheckpoint = _moveCheckpoints[_checkpointCounter];

        //���̃`�F�b�N�|�C���g�̍��W
        Vector2 nextMoveCheckpoint = _moveCheckpoints[_nextCheckpointNumber];

        //���ݒn - ���`�F�b�N�|�C���g�Ԃ̃x�N�g�����Z�o
        _relayPointVector = currentMoveCheckpoint - nextMoveCheckpoint;


        /*�x�N�g�����0.0�`1.0�ŃI�t�Z�b�g�������ԓ_���Z�o
         * 0.0 : ���݂̃`�F�b�N�|�C���g
         * 0.5 : ���݂̃`�F�b�N�|�C���g�Ǝ��̃`�F�b�N�|�C���g�̒���
         * 1.0 : ���̃`�F�b�N�|�C���g�̍��W
         */
        _relayPointY = Vector2.Lerp(currentMoveCheckpoint, nextMoveCheckpoint, _charactorMoveData._curveMoveVerticalOffset);

        /*�ړ��O���̍��E�l�ɉ����Čv�Z����x�N�g���̌�����ύX����
         * 
         * ���ɔ�΂��ꍇ��_relayPointVector�ɑ΂��č������̐����x�N�g���ɑ΂��č��E�l��������
         * �E�ɔ�΂��ꍇ��_relayPointVector�ɑ΂��ĉE�����̐����x�N�g���ɑ΂��č��E�l��������
         * 
         * _relayPointY�ŋ��߂��x�N�g����̒��Ԓn�_�����Ƃɐ����x�N�g�����o��
         */

        //�x�N�g���ɑ΂��鉡���I�t�Z�b�g�l��ݒ�
        float horizontalAxisOffset = _charactorMoveData._curveMoveHorizontalOffset;

        //���E�l���}�C�i�X(�������ł��邩)
        if (horizontalAxisOffset < 0)
        {
            //�������ł���

            //���݂̃`�F�b�N�|�C���g�`���̃`�F�b�N�|�C���g�ԃx�N�g���ɑ΂��鍶�����ɐ����ȃx�N�g�������߂�
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            //�Z�o�����x�N�g���ɑ΂��Ē��Ԓn�_��Y���I�t�Z�b�g��X���I�t�Z�b�g�l�𑫂��A���Ԓn�_�̍��W�����߂�
            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        else
        {
            //�E�����ł���

            //���݂̃`�F�b�N�|�C���g�`���̃`�F�b�N�|�C���g�ԃx�N�g���ɑ΂���E�����ɐ����ȃx�N�g�������߂�
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            //�Z�o�����x�N�g���ɑ΂��Ē��Ԓn�_��Y���I�t�Z�b�g��X���I�t�Z�b�g�l�𑫂��A���Ԓn�_�̍��W�����߂�
            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        /* ���݂̃`�F�b�N�|�C���g�`���ԓ_�A���ԓ_�`���̃`�F�b�N�|�C���g���q���������Lerp�ړ������AfirstVector�AsecodVector2�̈ړ�������W�����߂�B
         * 
         * firstVector�AsecodVector2�̍��W�Ԃ�Lerp�ړ������A�Ȑ���̍��WcurrentCurvePos�����߂�B
         * 
         * �Z�o����currentCurvePos��Ԃ�
         */

        //���݂̃`�F�b�N�|�C���g�`���ԓ_�Ԃ̃x�N�g����𓙑������^��������
        Vector2 firstVector = Vector2.Lerp(currentMoveCheckpoint, _fixedRelayPoint, _timer);

        //���ԓ_�`���̃`�F�b�N�|�C���g�Ԃ̃x�N�g����𓙑������^��������
        Vector2 secondtVector = Vector2.Lerp(_fixedRelayPoint, nextMoveCheckpoint, _timer);

        //firstVector�`secondVector�Ԃ̃x�N�g����𓙑������^��������W�����߂�
        Vector2 currentCurvePos = Vector2.Lerp(firstVector, secondtVector, _timer);

        //�Z�o�������W��Ԃ��l�Ƃ��ĕԂ�
        return currentCurvePos;
    }

    #endregion

    #region �e�֘A���\�b�h

    /// <summary>
    /// <para>SetShotNumber</para>
    /// <para>���˂���e�̒e�ԍ��̕ύX�A���ː��̏��������s��</para>
    /// </summary>
    private void SetShotNumber()
    {

        //���˃C���^�[�o�����t���O��false��
        StopCoroutine(RateOfShot());
        _isShotInterval = false;

        //���ˉ񐔂�0�ɏ�����
        _currentShotCount = 0;

        //���˂���e�̔z��Q�Ɣԍ���ύX
        _currentShotNumber++;

        //�z��Q�Ɣԍ����z��̗v�f�����z���Ă��Ȃ���
        if (_currentShotNumber >= _currentMovementAndShootingPaterns._shots.Length)
        {
            //�z����

            //�z��Q�Ɣԍ���0�ɖ߂�
            _currentShotNumber = 0;
        }
    }

    /// <summary>
    /// <para>SettingShotPrameters</para>
    /// <para>���˂���e�̃p�����[�^�����ƂɘA�ˑ��x�┭�ː����Q�Ƃ��Ĕ��ˏ����Ƃ��̒�~���s��</para>
    /// </summary>
    private void SettingShotPrameters()
    {
        //�e�̍ő唭�ː����i�[
        _maxShotCount = _currentMovementAndShootingPaterns._shotCounts[_currentShotNumber];

        //���݂̔��ː����ő唭�ː����z���Ă��Ȃ���
        if (_currentShotCount <= _maxShotCount)
        {
            //�z���Ă��Ȃ�

            //�b�Ԃɉ����������i�[
            int shotPerSeconds = _currentMovementAndShootingPaterns._shotPerSeconds[_currentShotNumber] + 1;

            //�V���b�g�`�V���b�g�Ԃ̑ҋ@���Ԃ�ݒ�
            _shotInterval = new WaitForSeconds(SECOND / shotPerSeconds);

            //�e���ˏ���
            Shot();
        }
        else
        {
            //�z����

            //���̒e�Ɠ����Ɍ����̃t���O���擾
            bool isShotInSameTime = _currentMovementAndShootingPaterns._isShotInSameTime[_currentShotNumber];

            //�����Ɍ���?
            if (isShotInSameTime)
            {
                //�����Ɍ���

                //���˃C���^�[�o�����t���O��false��
                StopCoroutine(RateOfShot());

                //���̃`�F�b�N�|�C���g�Ō����̔��ʃt���O��������
                _isNotShotInThisCheckpoint = false;

                //���˂���e�̒e�ԍ��̕ύX�A���ː��̏�����
                SetShotNumber();

                //�e�Ɏ󂯓n���p�����[�^�̐ݒ�E����
                SettingShotPrameters();
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

            //�������Ȃ�
            return;
        }

        //���ˊp�̏�����
        _multiShotOffsetAngle = 0;

        //��]�����̗L���̔���Ɗp�x�v�Z
        SwingShotCheck();

        //���݂̒e�̌��������i�[(enum)
        _currentShotPatern = _currentMovementAndShootingPaterns._shotPaterns[_currentShotNumber];

        //�i�[���������������Ƃɏ�������
        switch (_currentShotPatern)           //�e�̌�����
        {
            //�P������
            case CharactorShootingData.ShotPatern.OneShot:

                #region �P������
                //�e�̗L���� or ����
                EnableShot();

                #endregion

                break;

            //�P������������
            case CharactorShootingData.ShotPatern.AllAtOnce:

                #region ��������
                //���������e�����擾
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount <= _maxPelletCount; pelletCount++)
                {
                    //���[�v�������݂̐����e���Ƃ��ēn��
                    _currentPelletCount = pelletCount;

                    //�e�̗L���� or ����
                    EnableShot();
                }

                #endregion

                break;

            //��`��������
            case CharactorShootingData.ShotPatern.MultipleShots:

                #region ��`��������

                //���������e�����擾
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

                //�ő唭�ˊp
                float maxOffset = 0;

                //���݂̔��ˊp
                float currentAngle = 0;

                //�e�̎U�z�p���擾
                float formedAngle = _currentMovementAndShootingPaterns._multiShotFormedAngles[_currentShotNumber];

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    //���[�v�������݂̐����e���Ƃ��ēn��
                    _currentPelletCount = pelletCount;

                    //���e��?
                    if (pelletCount == 0)
                    {
                        //���e

                        //�U�z�p���琳�ʂ���ɂ����ő唭�ˊp���Z�o
                        maxOffset = formedAngle / 2;

                        //�ő唭�ˊp����
                        _multiShotOffsetAngle = -maxOffset;

                        //�e�ƒe�̊Ԃ̊p�x���Z�o
                        currentAngle = formedAngle / (_maxPelletCount - 1);
                    }
                    else
                    {
                        //2���ڈȍ~

                        //���e�Őݒ肵�����ˊp�ɉ��Z
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentAngle;
                    }

                    //�e�̗L���� or ����
                    EnableShot();
                }

                #endregion

                break;

            //���ˏ󔭎�
            case CharactorShootingData.ShotPatern.RadialShots:

                #region ���ˏ󔭎�

                //�V���b�g�`�V���b�g�Ԃ̊p�x�i�[�p
                float currentRadialAngle = 0;

                //���������e�����擾
                _maxPelletCount = _currentMovementAndShootingPaterns._pelletCountInShots[_currentShotNumber];

                //���������e�������[�v
                for (int pelletCount = 0; pelletCount < _maxPelletCount; pelletCount++)
                {
                    //���[�v�������݂̐����e���Ƃ��ēn��
                    _currentPelletCount = pelletCount;

                    if (pelletCount == 0)       //���e�̏ꍇ
                    {
                        //���炵�p�̏�����
                        _multiShotOffsetAngle = 0;

                        //�e�ƒe�̊Ԃ̊p�x���Z�o
                        currentRadialAngle = 360 / _maxPelletCount;
                    }
                    else
                    {
                        //�ŏ��ɐݒ肵�����ˊp�ɉ��Z
                        _multiShotOffsetAngle = _multiShotOffsetAngle + currentRadialAngle;
                    }

                    //�e�̗L���� or ����
                    EnableShot();
                }

                #endregion

                break;
        }

        //���݂̐����e���̏�����
        _currentPelletCount = 0;

        //�C���^�[�o������
        StartCoroutine(RateOfShot());

        //�������e�������Z
        _currentShotCount++;

    }

    /// <summary>
    /// <para>SwingShotCheck</para>
    /// <para>��]����(��������?)���s�����̔���ƁA�s���ꍇ�̊p�x�v�Z���s��</para>
    /// </summary>
    private void SwingShotCheck()
    {
        //��]���������邩�̃t���O���擾
        bool isSwingShot = _currentMovementAndShootingPaterns._isSwingShots[_currentShotNumber];

        //��]��������?
        if (isSwingShot)
        {
            //����

            //��]�������ɉ񂷊p�x�̎擾
            float centralAngle = _currentMovementAndShootingPaterns._swingShotFormedAngles[_currentShotNumber];

            //��]�������̏��e�̊p�x�̎擾
            float firstAngle = _currentMovementAndShootingPaterns._swingShotFirstAngles[_currentShotNumber];

            //�P�ʊp���Z�o
            float radian = centralAngle / _maxShotCount;


            //���e��?
            if (_currentShotCount <= 0)
            {
                //���e

                //���ˊp�ɏ��e�̊p�x��ݒ�
                _swingShotOffsetAngle = firstAngle;
            }
            else
            {
                //���ˊp�ɒP�ʊp�����Z
                _swingShotOffsetAngle += radian;
            }
        }
        else
        {
            //���Ȃ�

            //�p�x��������
            _swingShotOffsetAngle = 0;
        }
    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>���˂���e�ɑΉ������v�[����T�����A���g�p�̒e������΂��̒e��L�����B������ΐV���Ƀv�[�����ɐ�������</para>
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

                //�������e��L����
                shot.gameObject.SetActive(true);

                //�e��̔���
                CheckShotType(shot);

                //true�ɂ����e���v���C���[�̈ʒu�Ɉړ�
                shot.position = this.transform.position;

                //�������I��
                return;
            }
        }

        //�ȉ����g�p�I�u�W�F�N�g�����������ꍇ�V�����e�𐶐�

        //�V���ɐ�������e�̒e�ԍ����擾(�e�ԍ��Ɣz��v�f���̍����C�����邽�ߎ擾�l -1���i�[)
        int shotNumber = _currentMovementAndShootingPaterns._shots[_currentShotNumber] - 1;

        //�V���ɔ��˂���e�̃I�u�W�F�N�g���擾
        GameObject shotObject = _charactorMoveData._shots[shotNumber];

        //�擾�����e�I�u�W�F�N�g��Ή�����v�[���̎q�I�u�W�F�N�g�Ƃ��Đ���
        GameObject newShot = Instantiate(shotObject, _shotPools[_currentShotNumber].transform);

        //�e��̔���
        CheckShotType(newShot.transform);

        //���������e���L�����N�^�[�̈ʒu�Ɉړ�
        newShot.transform.position = this.transform.position;
    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    private IEnumerator RateOfShot()
    {
        //����SE�����邩
        if (_charactorMoveData._shotSoundEffect != null)
        {
            //����

            //����SE���Đ�
            audioSource.PlayOneShot(_charactorMoveData._shotSoundEffect);
        }

        //���˃C���^�[�o�����t���O��true��
        _isShotInterval = true;

        //���ˊԃC���^�[�o������
        yield return _shotInterval;

        //���˃C���^�[�o�����t���O��false��
        _isShotInterval = false;
    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>�e�̎�ނ𔻒肷��B���@�_���t���O�������Ă���ꍇ�ɔ��ˊp�Ƀv���C���[�Ƃ̃x�N�g���p�����Z����</para>
    /// </summary>
    /// <param name="shot">Shot���\�b�h�ŗL����/�������ꂽ�e�B�I�u�W�F�N�g�v�[���T���̍ۂ�Transform�^�Ŏ擾���邽��Transform�^</param>
    private void CheckShotType(Transform shot)
    {
        //���@�_���e���ۂ����i�[����t���O
        bool isTargetingPlayer = _currentMovementAndShootingPaterns._isTargetingPlayer[_currentShotNumber];

        //���@�_���e��
        if (!isTargetingPlayer)
        {
            //�ʏ�e

            //�^�����^�[�Q�b�g���W��
            _targetingPosition = Vector2.down;
        }
        else
        {
            //���@�_��

            //���������e�����̏��e��
            if (_currentShotPatern != CharactorShootingData.ShotPatern.OneShot && _currentPelletCount <= 0)
            {
                //�^�[�Q�b�g�Ƃ��Ă��̏u�Ԃ̃v���C���[�̍��W���i�[
                _targetingPosition = _player.transform.position;

                return;
            }

            //���������e�ł͂Ȃ��ꍇ�͏�Ƀv���C���[�̍��W���^�[�Q�b�g���W�Ƃ��Ċi�[����

            //�^�[�Q�b�g�Ƃ��Ă��̏u�Ԃ̃v���C���[�̍��W���i�[
            _targetingPosition = _player.transform.position;
        }

        /*
         * _targetingPosition �͎��@�_�����ۂ��œ�����̂��ς��܂�
         * 
         * �ʏ�e     : �L�����N�^�[�̐���(Vector2.down)
         * 
         * ���@�_���e : ���e���ˎ��̃v���C���[�̍��W
         */

        //���݂̍��W�ƃ^�[�Q�b�g�Ƃ��Ċi�[������W�Ԃ̃x�N�g�������߂�
        Vector2 degree = _targetingPosition - (Vector2)this.transform.position;

        //�x�N�g������p�x�ɕϊ�
        float radian = Mathf.Atan2(degree.y, degree.x);

        //���˂���e��ShotMove�R���|�[�l���g���擾
        ShotMove shotMove = shot.GetComponent<ShotMove>();

        /* ���˕����ɓ������ˎ��̉��Z�p(���ˏ�A���̏ꍇ)�Ɖ񂵊p(�񂵌����̏ꍇ)�����Z���Ēe�ɔ��ˊp�Ƃ��Ď󂯓n��
         * 
         * _multiShotOffsetAngle  : ��`�E�~�`�Ɍ��ۂɒe�`�e�Ԃ̌ʓx���i�[
         * _swingShotOffsetAngle  : �񂵌����̍ۂɒe�`�e�Ԃ̌Ǔx���i�[
         */
        shotMove.GetSetShotAngle = radian * Mathf.Rad2Deg + _multiShotOffsetAngle + _swingShotOffsetAngle;
    }

    #endregion

    /// <summary>
    /// <para>MovementInterval</para>
    /// <para>�ړ��̃C���^�[�o���������s��</para>
    /// </summary>
    private IEnumerator MovementInterval()
    {
        //�ړ��ҋ@���t���Otrue
        _isMovingInterval = true;

        //�ҋ@���ԕ��ҋ@
        yield return _movingInterval;

        //�ړ��ҋ@���t���Ofalse
        _isMovingInterval = false;

        //���̃`�F�b�N�|�C���g�Ō����̔��ʃt���O��������
        _isNotShotInThisCheckpoint = false;

        _timer = 0;
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