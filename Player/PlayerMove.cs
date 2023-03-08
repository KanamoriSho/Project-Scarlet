using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Label("�v���C���[���[�u�f�[�^")]
    private PlayerMoveData _playerMoveData = default;

    [SerializeField, Label("�V���b�g")]
    private GameObject _shot = default;

    [SerializeField, Label("�V���b�g�p�v�[��")]
    private GameObject _shotPool = default;

    #region �ړ��n�p�����[�^�萔
    private int LEFT_MOVE = -1;
    private int RIGHT_MOVE = 1;
    private int DOWN_MOVE = -1;
    private int UP_MOVE = 1;
    private float SLOW_MOVE_RATE = 0.5f;
    #endregion

    private Transform _playerTransform = default;           //���g��Transform�i�[�p

    private SpriteRenderer _spriteRenderer = default;       //���g��SpriteRenderer�i�[�p

    private Animator _anim = default;                       //���g��Animator�i�[�p

    private WaitForSeconds _interval = default;             //�R���[�`���̃L���b�V��

    private float _currentInterval = default;               //�R���[�`���̑ҋ@���Ԑݒ�p

    private bool _isInterval = false;                       //�C���^�[�o��������t���O

    private AudioSource audioSource = default;              //���g��Animtor�i�[�p

    private bool _isAuto = false;                           //�I�[�g���[�h����t���O

    private float SECOND = 1.0f;                            //��b�̒萔

    private void Awake()
    {
        _playerTransform = this.GetComponent<Transform>();          //���g�̃g�����X�t�H�[�����擾

        _spriteRenderer = this.GetComponent<SpriteRenderer>();      //���g�̃X�v���C�g�����_���[�擾

        _anim = this.GetComponent<Animator>();                      //���g�̃A�j���[�^�[�擾

        _currentInterval = SECOND / _playerMoveData._shotPerSecond; //�b�ԃV���b�g�������߂�

        _interval = new WaitForSeconds(_currentInterval);           //�V���b�g�Ԃ̃C���^�[�o�����L���b�V��

        audioSource = this.GetComponent<AudioSource>();             //AudioSource���擾
    }

    void Start()
    {
        /*�e���v�[���ɐ�������
         * _playerMoveData._initiallyGeneratedShots : ���������e��(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)
         */
        for (int count = 0; count < _playerMoveData._initiallyGeneratedShots; count++)
        {
            GameObject newShot = Instantiate(_shot, _shotPool.transform);       //�e�̐���

            newShot.SetActive(false);                   //���������e��false�ɂ���
        }
    }

    void Update()
    {
        if(_currentInterval != SECOND / _playerMoveData._shotPerSecond)     //�b�Ԕ��ː����ύX���ꂽ��
        {
            _currentInterval = SECOND / _playerMoveData._shotPerSecond;     //�Čv�Z���čđ��

            _interval = new WaitForSeconds(_currentInterval);               //WaitForSeconds�̃L���b�V������������
        }

        

        float horizontalInput = default;            //���E���͒l
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            _anim.SetBool("LeftMoving", true);      //���ړ�true
            _anim.SetBool("RightMoving", false);    //�E�ړ�false
            _anim.SetBool("SideMoving", true);      //���ړ���true

            horizontalInput = LEFT_MOVE;            //���E���͒l�ɍ��ړ��l�̒萔������
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            _anim.SetBool("RightMoving", true);     //�E�ړ�true
            _anim.SetBool("LeftMoving", false);     //���ړ�false
            _anim.SetBool("SideMoving", true);      //���ړ�true

            horizontalInput = RIGHT_MOVE;           //���E���͒l�ɉE�ړ��l�̒萔������
        }
        
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _anim.SetBool("LeftMoving", false);     //���ړ�false
            _anim.SetBool("SideMoving", false);     //���ړ�false
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _anim.SetBool("RightMoving", false);    //�E�ړ�false
            _anim.SetBool("SideMoving", false);     //���ړ�false
        }

        float verticalInput = default;              //�㉺���͒l
        if(Input.GetKey(KeyCode.UpArrow))
        {
            verticalInput = UP_MOVE;                //�㉺���͒l�ɏ�ړ��l�̒萔������
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalInput = DOWN_MOVE;              //�㉺���͒l�ɉ��ړ��l�̒萔������
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            horizontalInput = horizontalInput * SLOW_MOVE_RATE;     //���E���͒l�Ɍ�������������
            verticalInput = verticalInput * SLOW_MOVE_RATE;         //�㉺���͒l�Ɍ�������������
        }

        Move(horizontalInput, verticalInput);       //�ړ�

        if(Input.GetKey(KeyCode.Z))
        {
            Shot();     //�e���ˏ���
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(!_isAuto)            //false�Ȃ�
            {
                _isAuto = true;     //true��
            }
            else�@                  //true�Ȃ�
            {
                _isAuto = false;    //false��
            }
        }

        if (_isAuto)
        {
            Shot();     //�e���ˏ���
        }
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
            if(!shot.gameObject.activeSelf)             //���g�p�I�u�W�F�N�g����������
            {
                shot.gameObject.SetActive(true);            //true�ɂ���

                shot.position = this.transform.position;    //true�ɂ����e���v���C���[�̈ʒu�Ɉړ�

                StartCoroutine(RateOfShot());           //�C���^�[�o������
                return;
            }
        }

        //�ȉ����g�p�I�u�W�F�N�g�����������ꍇ

        GameObject newShot = Instantiate(_shot, _shotPool.transform);       //�V�����e�𐶐�

        newShot.transform.position = _playerTransform.position;             //���������e���v���C���[�̈ʒu�Ɉړ�

        StartCoroutine(RateOfShot());                   //�C���^�[�o������

    }

    /// <summary>
    /// <para>RateOfShot</para>
    /// <para>�e���˂̃C���^�[�o���������s��</para>
    /// </summary>
    /// <returns>_interval : �C���^�[�o������</returns>
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
    /// �ړ�����
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

        /*���݈ʒu�Ɉړ����͂ƈړ��X�s�[�h�����Z����
         * 
         * _playerMoveData._speed : �v���C���[�̈ړ����x(�X�N���v�^�u���I�u�W�F�N�g����󂯎��)
         */
        playerPosition = new Vector2(playerPosition.x + horizontalInput * _playerMoveData._speed * Time.deltaTime,
                                        playerPosition.y + verticalInput * _playerMoveData._speed * Time.deltaTime);

        /*
         *  if(�E or ��̈ړ������𒴂�����)
         *      �E or ��̈ړ��������ɖ߂�
         *  else if(�� or ���̈ړ������𒴂�����)
         *      �� or ���̈ړ��������ɖ߂�
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
