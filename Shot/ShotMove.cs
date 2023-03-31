using UnityEngine;

public class ShotMove : MonoBehaviour
{
    [Label("�V���b�g���[�u�f�[�^")]
    public ShotMoveData _shotMoveData = default;             //�e�O���̃X�N���v�^�u���I�u�W�F�N�g

    private SpriteRenderer _spriteRenderer = default;       //���g��SpriteRenderer�i�[�p

    private Vector2 _targetPosition = default;              //_target��Position�i�[�p

    private float _initialVelocity = default;

    private float _speed = default;                 //���x�i�[�p

    private EnemyCharactorMove _charactorMove = default;

    private PlayerMove _playerMove = default;

    private Animator _animator = default;

    private float _time = default;                  //���Ԋi�[�p

    private float _colliderRadius = default;        //�����蔻��̑傫�����i�[

    public float GetColliderRadius
    {
        get { return _colliderRadius; }
    }

    [SerializeField]
    private GameObject _shooter = default;          //�ˎ�i�[�p

    public GameObject GetSetshooter         //_shooter��Get�ASet�p�v���p�e�B
    {
        get { return _shooter; }
        set { _shooter = value; }
    }

    private string PLAYER_TAG = "Player";

    private string BOSS_TAG = "Boss";

    private Vector2 _shooterPosition = default;     //�ˎ��Position�i�[�p

    private Vector2 _shotVector = default;          //�e�̔��˃x�N�g���i�[�p

    private float _shotAngle = default;             //���ˊp�i�[�p

    private int _shotCounter = default;             //���̒e�𔭎˂��������i�[����(CharactorData����󂯎��)

    private int _maxShotCount = default;            //���̒e�̍ő唭�ː����i�[����(CharactorData����󂯎��)

    private int _pelletCounter = default;           //���ݐ������ꂽ�e�̐����i�[����(CharactorData����󂯎��)

    private int _maxPelletCount = default;          //�����ɐ�������e�̐����i�[����(CharactorData����󂯎��)

    private bool _isDecerationPerShot = false;

    public float GetSetShotAngle          //_target��Get�ASet�p�v���p�e�B
    {
        get { return _shotAngle; }
        set 
        {
            _shotAngle = value;
            _shotVector = new Vector3(0, 0, _shotAngle); 
        }
    }

    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A���[�J���ϐ��Ŏ������̂ł������������

    private Vector2 _fixedRelayPoint = default;     //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;    //�ˎ� - _target�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;         //_relayPointVector��̏c(Y)�����W�i�[�p

    private void Awake()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>();          //SpriteRenderer�擾

        _spriteRenderer.sprite = _shotMoveData._shotSprite;             //�X�v���C�g�ύX

        _animator = this.GetComponent<Animator>();

        this._colliderRadius = _shotMoveData._colliderRadius;           //�����蔻��̃T�C�Y��ݒ�

        GetShooter();           //�e�����L�������擾(�v���C���[�ƃ{�X�̂�)

        //����/���˂��Ƃ̉�����������e��?
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;
        }

        //���̒e�̎ˎ�͓G��?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //�G

            _charactorMove = _shooter.GetComponent<EnemyCharactorMove>();       //�ˎ��EnemyCharactorMove���擾

            _isDecerationPerShot = _charactorMove.GetIsDecelerationPerShoot;    //���˂��ƂɌ������邩���擾
        }
        else
        {
            //�v���C���[

            _playerMove = _shooter.GetComponent<PlayerMove>();                  //PlayerMove���擾

            _isDecerationPerShot = _playerMove.GetIsDecelerationPerShoot;       //���˂��ƂɌ������邩���擾
        }
    }

    private void OnEnable()
    {
        Reset();        //����������

        //�e�̉�]�t���O��true��
        if (_shotMoveData._isSpinning)
        {
            this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));     //true�Ȃ烉���_���ŉ�]������
        }

        //2�F�ڂ�����e��
        if (_shotMoveData._hasAlternativeColor)
        {
            //2�F�ڂ�����Ȃ�

            _shotCounter = _charactorMove.GetCurrentShotCount;

            Sprite currentShotSprite = default;

            //�������
            if(_shotCounter % 2 == 1)
            {
                //��Ȃ�

                _animator.SetBool("AltColor", false);             //�X�v���C�g�ύX
            }
            else
            {
                //�����Ȃ�

                _animator.SetBool("AltColor", true);
            }

            _spriteRenderer.sprite = currentShotSprite;
        }

        //�������Ƃɉ���/�������Ȃ��e�Ȃ�
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;     //�I��
        }

        //�ȉ��������Ƃɉ���������e�̏ꍇ

        //�v���C���[�ł͖���?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //�v���C���[�ł͂Ȃ�

            //���˂��Ƃɉ��������邩?
            if (!_isDecerationPerShot)
            {
                //���Ȃ�(�������Ƃɉ�����)
                _pelletCounter = _charactorMove.GetCurrentPelletCount;      //_charactorMove���猻�݂̐����e�����󂯎��

                _maxPelletCount = _charactorMove.GetMaxPelletCount;         //_charactorMove���瓯�������e�����󂯎��
            }
            else
            {
                //����
                _shotCounter = _charactorMove.GetCurrentShotCount;      //_charactorMove���猻�݂̐����e�����󂯎��

                _maxShotCount = _charactorMove.GetMaxShotCount;         //_charactorMove����ő唭�˒e�����󂯎��
            }
        }
        else
        {
            //�v���C���[�ł���

            //���˂��Ƃɉ��������邩?
            if (!_isDecerationPerShot)
            {
                //���Ȃ�(�������Ƃɉ�����)
                _pelletCounter = _playerMove.GetCurrentPelletCount;      //_playerMove���猻�݂̐����e�����󂯎��

                _maxPelletCount = _playerMove.GetMaxPelletCount;         //_playerMove���瓯�������e�����󂯎��
            }
            else
            {
                //����
                _shotCounter = _playerMove.GetCurrentShotCount;      //_playerMove���猻�݂̔��˒e�����󂯎��

                _maxShotCount = _playerMove.GetMaxShotCount;         //_playerMove����ő唭�˒e�����󂯎��
            }
        }
    }

    /// <summary>
    /// <para>Reset</para>
    /// <para>�ϐ��������p���\�b�h �f�o�b�O���ɂ��Ăяo����悤�Ƀ��\�b�h�����Ă܂�</para>
    /// </summary>
    private void Reset()
    {
        _shooterPosition = _shooter.transform.position;                 //�ˎ�̍��W���Đݒ�

        this.transform.position = _shooterPosition;                     //���g���ˎ�̍��W�Ɉړ�

        _time = 0;                                                      //�o�ߎ��Ԃ����Z�b�g

        //�����蔻��̑傫����_shotMoveData�̂��̂ƈقȂ邩
        if (this._colliderRadius != _shotMoveData._colliderRadius)
        {
            //�قȂ�

            this._colliderRadius = _shotMoveData._colliderRadius;       //�����蔻��̃T�C�Y���Đݒ�
        }
    }

    /// <summary>
    /// <para>OnBecameInvisible</para>
    /// <para>��ʊO�ɏo���e����������</para>
    /// </summary>
    private void OnBecameInvisible()
    {
        ObjectDisabler();
    }

    private void FixedUpdate()
    {
        _time += Time.deltaTime;                        //���Ԃ̉��Z

        Vector2 currentPos = this.transform.position;   //���݂̎��g�̍��W���擾

        //�����ω������p���[�J���ϐ�

                int shotLength = 0;         //��x�ɐ�������e�� or ���˂���e�� + 1�i�[�p

                int shotCount = 0;          //��������e�̓����݉����ڂ����i�[����

        /*�e�̏����v�Z��������
         * 
         * Nomal      : �����v�Z����(�f�t�H���g�l���̂܂�)
         * FastToSlow : ���� or �������Ƃɏ���������
         * SlowToFast : ���� or �������Ƃɏ���������
         */

        switch (_shotMoveData._shotVelocity)
        {
            case ShotMoveData.ShotVelocity.Nomal:   //�ʏ�e(�����ω��Ȃ�)

                this._initialVelocity = _shotMoveData._initialVelocity;     //�����Ƀf�t�H���g�l�̏�����ݒ�

                break;

            case ShotMoveData.ShotVelocity.FastToSlow:      //���� or �������Ƃɏ���������


                //���ˏ�ɔ�Ԓe��?
                if (!_isDecerationPerShot)
                {
                    //�Ⴄ

                    shotLength = _maxPelletCount + 1;         //��x�ɐ�������e�� + 1���i�[

                    shotCount = _pelletCounter;               //���݉����ڂ̐�����
                }
                else
                {
                    //���ˏ�ɔ�Ԓe

                    shotLength = _maxShotCount + 1;           //�A�˂���e�� + 1���i�[

                    shotCount = _shotCounter;                 //���݉����ڂ����i�[
                }

                //���� or �������Ƃ̌����l���Z�o
                float decelerationValue = _shotMoveData._initialVelocity / (shotLength * _shotMoveData._shotVelocityRate);

                //���� or �������Ƃ̌����l * ���ˁE���������v�Z(�f�t�H���g�̏�������������x)
                float subtractionValue = decelerationValue * shotCount;

                //�f�t�H���g�l������������x�����̒e�̏����Ƃ��Ċi�[
                this._initialVelocity = _shotMoveData._initialVelocity - subtractionValue;

                break;

            case ShotMoveData.ShotVelocity.SlowToFast:      //���� or �������Ƃɏ���������


                //���ˏ�ɔ�Ԓe��?
                if (!_isDecerationPerShot)
                {
                    //�Ⴄ

                    shotLength = _maxPelletCount + 1;         //��x�ɐ�������e�� + 1���i�[

                    shotCount = _pelletCounter;               //���݉����ڂ̐��������i�[
                }
                else
                {
                    //���ˏ�ɔ�Ԓe

                    shotLength = _maxShotCount + 1;           //�A�˂���e�� + 1���i�[

                    shotCount = _shotCounter;                 //���݉����ڂ����i�[
                }

                //���� or �������Ƃ̌����l���Z�o
                float accelerationValue = _shotMoveData._initialVelocity / (shotLength * _shotMoveData._shotVelocityRate);

                //���� or �������Ƃ̌����l * ���ˁE���������v�Z(�f�t�H���g�̏����ɉ��Z���鑬�x)
                float additionValue = accelerationValue * shotCount;

                //�f�t�H���g�l������Z�������x�����̒e�̏����Ƃ��Ċi�[
                this._initialVelocity = _shotMoveData._initialVelocity + additionValue;

                break;
        }

        /*����O��    ��ɑ��x�֘A
         * Nomal                     : ����O������
         * Acceleration_Deceleration : ������
         * Laser                     : ���[�U�[
         */

        switch (_shotMoveData._shotSettings)
        {
            case ShotMoveData.ShotSettings.Nomal:       //���x�ω��Ȃ�

                _speed = this._initialVelocity;       //���x�ɏ����l���i�[

                break;

            case ShotMoveData.ShotSettings.Acceleration_Deceleration:       //���ˌ�ɉ�����

                //�A�j���[�V�����J�[�u�̒l * �����l�ŎZ�o�������x���i�[
                _speed = _shotMoveData._speedCurve.Evaluate((float)_time / _shotMoveData._timeToSpeedChange) * this._initialVelocity;

                break;

            case ShotMoveData.ShotSettings.Laser:       //���[�U�[�e

                break;
        }

        /*�V���b�g�̔�ѕ�
         * Straight       : ���i
         * Curve          : �J�[�u
         */

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                float radian = _shotAngle * (Mathf.PI / 180);
                _shotVector = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
                currentPos += _shotVector * _speed * Time.deltaTime;
                break;

            case ShotMoveData.ShotType.Curve:

                if (_time < 1)
                {
                    currentPos = CalcuateBezierCurve();
                }
                else
                {
                    currentPos += _shotVector * _speed * Time.deltaTime;
                }


                break;

        }

        this.transform.position = currentPos;
    }

    private void ObjectDisabler()
    {
        if (!_shotMoveData._isDebug)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            Reset();
        }
    }

    /// <summary>
    /// <para>StraightShotCalculateVector</para>
    /// <para>�ڕW�n�_ - ���˒n�_�Ԃ̃x�N�g�������߂�</para>
    /// </summary>
    /// <param name="shotPos">���˒n�_</param>
    /// <param name="targetPos">�ڕW�n�_</param>
    /// <returns>direction = ���˒n�_�`�^�[�Q�b�g�n�_�Ԃ̃x�N�g��</returns>
    private Vector2 StraightShotCalculateVector(Vector2 shotPos, Vector2 targetPos)
    {

        Vector2 direction = (targetPos - shotPos).normalized;

        return direction;
    }

    /// <summary>
    /// <para>CalcuateBezierCurve</para>
    /// <para>�J�[�u�e�̃x�W�F�Ȑ��𐶐��E���ݒn�_���Z�o����X�N���v�g</para>
    /// </summary>
    /// <returns>currentCurvePos = �Z�o���ꂽ�x�W�F�Ȑ���̍��W</returns>
    private Vector2 CalcuateBezierCurve()
    {
        //���˒n�_ - �^�[�Q�b�g�Ԃ̃x�N�g�����Z�o
        _relayPointVector = _shooterPosition - _targetPosition;

        /*�x�N�g�����0.0�`1.0�ŃI�t�Z�b�g�������ԓ_���Z�o
         * 0.0 : ���˒n�_
         * 0.5 : ���˒n�_�ƃ^�[�Q�b�g�̒���
         * 1.0 : �^�[�Q�b�g�̍��W
         */
        _relayPointY = Vector2.Lerp(_shooterPosition, _targetPosition, _shotMoveData._curveShotVerticalOffset);

        /*�e�O���̍��E�l�ɉ����Čv�Z����x�N�g���̌�����ύX����
         * 
         * ���ɔ�΂��ꍇ��_relayPointVector�ɑ΂��č������̐����x�N�g���ɑ΂��č��E�l��������
         * �E�ɔ�΂��ꍇ��_relayPointVector�ɑ΂��ĉE�����̐����x�N�g���ɑ΂��č��E�l��������
         * 
         * _relayPointY�ŋ��߂��x�N�g����̒��Ԓn�_�����Ƃɐ����x�N�g�����o��
         */

        float horizontalAxisOffset = _shotMoveData._curveShotHorizontalOffset;      //�x�N�g���ɑ΂��鉡���I�t�Z�b�g�l��ݒ�

        //������
        if (_shotMoveData._curveShotHorizontalOffset <= 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂��鍶�����ɐ����ȃx�N�g�������߂�
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            //�x�N�g����̒��ԓ_���x�_�Ƀx�W�F�Ȑ��̒��ԓ_�̍��W�����ɂ��炷
            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        //�E����
        else if (_shotMoveData._curveShotHorizontalOffset > 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂���E�����ɐ����ȃx�N�g�������߂�
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            //�x�N�g����̒��ԓ_���x�_�Ƀx�W�F�Ȑ��̒��ԓ_�̍��W���E�ɂ��炷
            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        //���˒n�_�`���Ԓn�_�Ԃ�Lerp�ړ�
        Vector2 firstVec = Vector2.Lerp(_shooterPosition, _fixedRelayPoint, _time);

        //���Ԓn�_�ԁ`�^�[�Q�b�g���W�Ԃ�Lerp�ړ�
        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _targetPosition, _time);

        //Lerp�ړ����̍��W2�Ԃ�Lerp�ړ�
        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _time);

        /*�^�[�Q�b�g�n�_��ʂ�߂��Ă��^��������Ԃ��߂ɒ��Ԓn�_�`�^�[�Q�b�g�n�_�Ԃ̃x�N�g����ݒ�
         * 
         * �^�[�Q�b�g�n�_��ʂ�߂������_�ł����Őݒ肵���x�N�g���Ŕ��ł���
         */
        _shotVector = StraightShotCalculateVector(_fixedRelayPoint, _targetPosition);


        return currentCurvePos;     //�x�W�F�Ȑ���̌��݂̍��W��Ԃ�
    }

    private void GetShooter()
    {
        switch (_shotMoveData._shooterType)
        {
            case ShotMoveData.ShooterType.Player:

                GetSetshooter = GameObject.FindGameObjectWithTag(PLAYER_TAG);

                break;

            case ShotMoveData.ShooterType.Boss:

                GetSetshooter = GameObject.FindGameObjectWithTag(BOSS_TAG);

                break;

            case ShotMoveData.ShooterType.Common:

                break;
        }


    }

    /// <summary>
    /// <para>AnimEvent_Disable</para>
    /// <para>����������(�A�j���[�V�����C�x���g�p)</para>
    /// </summary>
    public void AnimEvent_Disable()
    {
        this.gameObject.SetActive(false);       //���g�𖳌���
    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>�x�W�F�Ȑ��̎n�_ - ���ԓ_�ԁA���ԓ_ - �I�_�Ԃ���ŕ`�悷�郁�\�b�h �f�o�b�O�p</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        Gizmos.DrawSphere(this.transform.position,_colliderRadius);

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                Gizmos.DrawSphere(_shooterPosition, 0.1f);
                Gizmos.DrawSphere(_targetPosition, 0.1f);

                Gizmos.DrawLine(_shooterPosition, _targetPosition);

                break;

            case ShotMoveData.ShotType.Curve:

                Gizmos.DrawSphere(_shooterPosition, 0.1f);
                Gizmos.DrawSphere(_targetPosition, 0.1f);

                Gizmos.DrawSphere(_fixedRelayPoint, 0.2f);

                Gizmos.DrawSphere(_relayPointY, 0.1f);

                Gizmos.DrawLine(_shooterPosition, _fixedRelayPoint);

                Gizmos.DrawLine(_fixedRelayPoint, _targetPosition);

                break;

        }

#endif

    }
}



