using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Label("�L�������[�u�f�[�^")]
    private PlayerMoveData _playerMoveData = default;

    private int _currentShotNumber = 0;         //���݂̔��˂���e�̔ԍ����i�[����ϐ�

    [SerializeField, Label("�V���b�g�p�v�[��")]
    private GameObject[] _shotPools = default;

    [SerializeField, Label("�{���G�t�F�N�g�̃I�u�W�F�N�g")]
    private GameObject _bombShockWave = default;

    private Animator _bombAnimator = default;

    private int _currentShotCount = 0;          //���񂻂̒e�������������i�[����ϐ�

    private float _offsetAngle = default;   //���������ɔ��˂���ꍇ�̔��ˊp���i�[����ϐ�

    private WaitForSeconds _shotInterval = default;                 //�e�̘A�ˑ��x���Ǘ�����R���[�`���̃L���b�V��

    private WaitForSeconds _invincibleTime = default;               //���G���Ԃ��Ǘ�����R���[�`���̃L���b�V��

    private WaitForSeconds _disableEnemyShotsInterval = default;    //�{���g�p����e��������܂ł̃C���^�[�o��

    private bool _isShotInterval = false;                           //�ˌ��C���^�[�o��������t���O

    private bool _isInvincible = false;                             //���G�t���O

    private Transform _playerTransform = default;                   //�v���C���[��Transform�i�[�p

    private Vector2 _startPosition = new Vector2(0, -3);            //�������W

    private Vector2 _targetingPosition = default;                   //�_���Ă�����W�i�[�p(���ˊp�v�Z�p)

    #region Getter Setter

    public int GetCurrentShotCount
    {
        //_currentShotCount��Ԃ�
        get { return _currentShotCount; }
    }

    private int _maxShotCount = 0;              //���̒e�������������i�[����ϐ�

    public int GetMaxShotCount
    {
        //_maxShotCount��Ԃ�
        get { return _maxShotCount; }
    }

    private int _currentPelletCount = 0;        //

    public int GetCurrentPelletCount
    {
        //_currentPelletCount��Ԃ�
        get { return _currentPelletCount; }
    }

    private int _maxPelletCount = 0;        //

    public int GetMaxPelletCount
    {
        //_maxPelletCount��Ԃ�
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



    private const float SECOND = 1.0f;                          //��b�̒萔

    private const float BOMB_SHOT_DISABLE_TIME = 0.1f;

    private const float SLOW_MOVING_RATE = 0.5f;

    private const string ANIMATION_BOOL_LEFT_MOVE = "LeftMoving";

    private const string ANIMATION_BOOL_RIGHT_MOVE = "RightMoving";

    private const string ANIMATION_BOOL_SIDE_MOVING = "SideMoving";

    private Animator _animator = default;                   //���g��Animtor�i�[�p

    private AudioSource _audioSource = default;             //���g��AudioSource�i�[�p

    private CollisionManager _collisionManger = default;    //���g��CollisionManager�i�[�p

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();                          //Animator�̎擾

        _playerTransform = this.transform;                                  //���g��Transform���擾

        _audioSource = this.GetComponent<AudioSource>();                    //AudioSource�̎擾

        _collisionManger = this.GetComponent<CollisionManager>();           //CollisionManager�R���|�[�l���g�̎擾

        _bombShockWave = GameObject.FindGameObjectWithTag("BombEffect");    //�{���p�G�t�F�N�g�I�u�W�F�N�g�̎擾

        _bombAnimator = _bombShockWave.GetComponent<Animator>();

        _bombShockWave.SetActive(false);

        //�e���ˁ`���ˊԂ̑҂����Ԃ��L���b�V��
        _shotInterval = new WaitForSeconds(SECOND / (float)_playerMoveData._shotPerSeconds[_currentShotNumber]);

        //���G���Ԃ��L���b�V��
        _invincibleTime = new WaitForSeconds(_playerMoveData._afterHitInvincibleTime);


        _disableEnemyShotsInterval = new WaitForSeconds(BOMB_SHOT_DISABLE_TIME);

        /*�e���v�[���ɐ�������
         * _charactorMoveData._waves                   : �E�F�[�u��(�{�X�L�����ȊO��1)
         * _charactorMoveData._initiallyGeneratedShots : ���������e��(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)
         */

        //�g�p�e�̎�ޕ����[�v
        for (int shotNumber = 0; shotNumber < _playerMoveData._shots.Count; shotNumber++)
        {
            //�g�p�����e�𐶐����郋�[�v
            for (int shotCounter = 0; shotCounter < _playerMoveData._initiallyGeneratedShots; shotCounter++)
            {
                GameObject newShot = Instantiate(_playerMoveData._shots[shotNumber], _shotPools[shotNumber].transform);     //�e�̐���

                newShot.SetActive(false);                   //���������e��false�ɂ���
            }
        }
    }

    void FixedUpdate()
    {
        float movingXInput = default;       //X���̓��͒l�i�[�p
        float movingYInput = default;       //Y���̓��͒l�i�[�p

        //_playerMoveData._speed : �v���C���[�̈ړ����x(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)

        //�E����
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movingXInput = _playerMoveData._speed;                  //X���̓��͒l�Ƀv���C���[�̑��x������             

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);     //Animator�́u���ړ����vbool��false��

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, true);     //Animator�́u�E�ړ����vbool��true��

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);    //Animator�́u���ړ����vbool��true��
        }
        //������
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            movingXInput = -_playerMoveData._speed;                 //X���̓��͒l��(�v���C���[�̑��x * -1)������

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, true);      //Animator�́u���ړ����vbool��true��

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);    //Animator�́u���ړ����vbool��false��

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, true);    //Animator�́u���ړ����vbool��true��
        }
        //���E���͖���
        else
        {
            movingXInput = 0;                                       //X���̓��͒l��0��

            _animator.SetBool(ANIMATION_BOOL_LEFT_MOVE, false);     //Animator�́u���ړ����vbool��false��

            _animator.SetBool(ANIMATION_BOOL_RIGHT_MOVE, false);    //Animator�́u���ړ����vbool��false��

            _animator.SetBool(ANIMATION_BOOL_SIDE_MOVING, false);   //Animator�́u���ړ����vbool��false��
        }

        //�����
        if (Input.GetKey(KeyCode.UpArrow))
        {
            movingYInput = _playerMoveData._speed;                  //Y���̓��͒l�Ƀv���C���[�̑��x������   
        }
        //������
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            movingYInput = -_playerMoveData._speed;                 //Y���̓��͒l��(�v���C���[�̑��x * -1)������   
        }

        //���V�t�g����
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //�ᑬ�ړ�

            movingXInput = movingXInput * SLOW_MOVING_RATE;         //X���̈ړ����͒l��0.5�{����

            movingYInput = movingYInput * SLOW_MOVING_RATE;         //Y���̈ړ����͒l��0.5�{����
        }

        Move(movingXInput, movingYInput);       //�ŏI�I�Ȉړ����͒l���ړ������ɑ���

        //Z����
        if (Input.GetKey(KeyCode.Z))
        {
            Shot();     //�e���ˏ���
        }

        //���G��Ԃ�?
        if (_isInvincible)
        {
            //���G

            return;     //�����I��
        }

        //��e������?
        if (_collisionManger.GetSetHitFlag)
        {
            //����

            StartCoroutine(OnHit());                        //��e�R���[�`�����J�n

            _collisionManger.GetSetHitFlag = false;         //CollisionManager�̔�e�t���O��false��
        }
    }

    /// <summary>
    /// <para>Shot</para>
    /// <para>�e�̔��ˏ���</para>
    /// </summary>
    private void Shot()
    {
        if (_isShotInterval)    //�C���^�[�o����
        {
            return;     //�������Ȃ�
        }

        _offsetAngle = 0;   //���ˊp�̏�����

        switch (_playerMoveData._shotPaterns[_currentShotNumber])           //�e�̌�����
        {
            case PlayerMoveData.ShotPatern.OneShot:              //�P������

                EnableShot();                           //�e�̗L���� or ����
                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;

            case PlayerMoveData.ShotPatern.AllAtOnece:

                _maxPelletCount = _playerMoveData._pelletCountInShots[_currentShotNumber];      //�������������i�[

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount <= _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    _currentPelletCount = pelletCount;

                    EnableShot();                       //�e�̗L���� or ����
                }

                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;


            case PlayerMoveData.ShotPatern.MultipleShots:        //����������

                float maxOffset = 0;        //�ő唭�ˊp

                float currentAngle = 0;     //���݂̔��ˊp

                //��x�ɐ�������e������郋�[�v
                for (int pelletCount = 0; pelletCount < _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    if (pelletCount == 0)       //���e�̏ꍇ
                    {
                        maxOffset = _playerMoveData._formedAngles[_currentShotNumber] / 2;       //�ő唭�ˊp���Z�o
                        _offsetAngle = -maxOffset;                             //�ő唭�ˊp����

                        //�e�ƒe�̊Ԃ̊p�x���Z�o
                        currentAngle = _playerMoveData._formedAngles[_currentShotNumber] / (_playerMoveData._pelletCountInShots[_currentShotNumber] - 1);
                    }
                    else
                    {
                        _offsetAngle = _offsetAngle + currentAngle; //�ŏ��ɐݒ肵�����ˊp�ɉ��Z
                    }

                    EnableShot();                       //�e�̗L���� or ����
                }

                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;

            case PlayerMoveData.ShotPatern.RadialShots:      //���ˏ󔭎�

                float currentRadialAngle = 0;       //360 / �e���̐��l(�e�`�e�Ԃ̊p�x)���i�[����

                //�����ɐ�������e�������[�v
                for (int pelletCount = 0; pelletCount < _playerMoveData._pelletCountInShots[_currentShotNumber]; pelletCount++)
                {
                    //���e��?(1���[�v�ڂ�?)
                    if (pelletCount == 0)
                    {
                        _offsetAngle = 0;       //���Z�p�p�x��������
                        currentRadialAngle = 360 / (_playerMoveData._pelletCountInShots[_currentShotNumber]);       //�ꔭ���Ƃɉ��Z����p�x���Z�o
                    }
                    else
                    {
                        //���[�v���ڈȍ~

                        _offsetAngle = _offsetAngle + currentRadialAngle;       //�ꔭ���Ƃ̊p�x�����Z
                    }

                    EnableShot();                       //�e�̗L���� or ����
                }

                StartCoroutine(RateOfShot());           //�C���^�[�o������

                break;
        }

        _currentShotCount++;                    //�������񐔂����Z

    }

    /// <summary>
    /// <para>EnableShot</para>
    /// <para>�I�u�W�F�N�g�v�[�����Q�Ƃ��e�̗L���� or �������s��</para>
    /// </summary>
    private void EnableShot()
    {

        foreach (Transform shot in _shotPools[_currentShotNumber].transform)     //�I�u�W�F�N�g�v�[�����ɖ��g�p�I�u�W�F�N�g���������{��
        {
            if (!shot.gameObject.activeSelf)                //���g�p�I�u�W�F�N�g����������
            {
                shot.gameObject.SetActive(true);            //true�ɂ���

                CheckShotType(shot);                        //�e��̔���

                shot.position = this.transform.position;    //true�ɂ����e���v���C���[�̈ʒu�Ɉړ�


                return;
            }
        }

        //�ȉ����g�p�I�u�W�F�N�g�����������ꍇ
        //�V�����e�𐶐�
        GameObject newShot =�@Instantiate(_playerMoveData._shots[_currentShotNumber - 1],_shotPools[_currentShotNumber].transform);

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
        //�eSE�����邩?
        if (_playerMoveData._shotSoundEffect != null)
        {
            //����

            _audioSource.PlayOneShot(_playerMoveData._shotSoundEffect);     //�e����SE���Đ�
        }

        _isShotInterval = true;         //�e���ˑҋ@�t���O��true��

        yield return _shotInterval;     //�ҋ@���ԕ��ҋ@

        _isShotInterval = false;        //�e���ˑҋ@�t���O��true��

    }

    /// <summary>
    /// <para>CheckShotType</para>
    /// <para>�e�̎�ނ𔻒肷��B���@�_���t���O�������Ă���ꍇ�ɔ��ˊp�Ƀv���C���[�Ƃ̃x�N�g���p�����Z����</para>
    /// </summary>
    /// <param name="shot">Shot���\�b�h�ŗL����/�������ꂽ�e�B�I�u�W�F�N�g�v�[���T���̍ۂ�Transform�^�Ŏ擾���邽��Transform�^</param>
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
    /// <para>�ړ�����</para>
    /// </summary>
    /// <param name="horizontalInput">X������</param>
    /// <param name="verticalInput">Y������</param>
    private void Move(float horizontalInput, float verticalInput)
    {
        if (horizontalInput == 0 && verticalInput == 0)     //�ړ����͂������ꍇ
        {
            return;     //�������Ȃ�
        }

        Vector2 playerPosition = _playerTransform.position;      //�v���C���[�̌��݈ʒu���擾

        //���݈ʒu�Ɉړ����͂ƈړ��X�s�[�h�����Z����
        playerPosition = new Vector2(playerPosition.x + horizontalInput * Time.deltaTime,
                                        playerPosition.y + verticalInput * Time.deltaTime);

        /*
         *  if(�E or ��̈ړ������𒴂�����)
         *      �E or ��̈ړ��������ɖ߂�
         *  else if(�� or ���̈ړ������𒴂�����)
         *      �� or ���̈ړ��������ɖ߂�
         */

        //�E�̈ړ��͈͐����𒴂�����
        if (playerPosition.x > _playerMoveData._xLimitOfMoving)
        {
            //������

            playerPosition.x = _playerMoveData._xLimitOfMoving;     //���݂̍��W��X���̈ړ�����l�ɖ߂�
        }
        //���̈ړ��͈͐����𒴂�����
        else if (playerPosition.x < -_playerMoveData._xLimitOfMoving)
        {
            //������

            playerPosition.x = -_playerMoveData._xLimitOfMoving;     //���݂̍��W��X����(�ړ�����l * -1)�ɖ߂�
        }

        //��̈ړ��͈͐����𒴂�����
        if (playerPosition.y > _playerMoveData._yLimitOfMoving)
        {
            //������

            playerPosition.y = _playerMoveData._yLimitOfMoving;     //���݂̍��W��Y���̈ړ�����l�ɖ߂�
        }
        //���̈ړ��͈͐����𒴂�����
        else if (playerPosition.y < -_playerMoveData._yLimitOfMoving)
        {
            //������

            playerPosition.y = -_playerMoveData._yLimitOfMoving;     //���݂̍��W��Y����(�ړ�����l * -1)�ɖ߂�
        }

        _playerTransform.position = playerPosition;         //�ړ��������W���v���C���[�ɔ��f����
    }


    /// <summary>
    /// <para>Bomb</para>
    /// <para>�{���g�p������(��e���ɂ��Ă�)</para>
    /// </summary>
    /// <returns></returns>
    IEnumerator Bomb()
    {
        _bombShockWave.SetActive(true);                                     //�{���G�t�F�N�g��L����

        _bombShockWave.transform.position = _playerTransform.position;      //�{���G�t�F�N�g���v���C���[�̍��W�Ɉړ�

        _bombAnimator.SetTrigger("Enable");                                 //Animator�̋N���g���K�[���I����

        yield return _disableEnemyShotsInterval;                            //�{����������G�̒e��������܂ł̃C���^�[�o��

        GameObject[] enemyShotsInPicture = GameObject.FindGameObjectsWithTag("EnemyShot");      //���ݗL��������Ă���G�̒e��S�Ď擾

        //��قǎ擾�����G�̒e�̐��������[�v����
        for(int shotCount = 0; shotCount < enemyShotsInPicture.Length; shotCount++)
        {
            enemyShotsInPicture[shotCount].GetComponent<Animator>().SetTrigger("Disable");      //Animator�́u�������v�g���K�[���I����
        }
    }

    /// <summary>
    /// <para>OnHit</para>
    /// <para>�����蔻�� CollisionManager����ł�_isHit�t���O��true�ŌĂяo��</para>
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
