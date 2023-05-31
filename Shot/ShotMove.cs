using UnityEngine;

public class ShotMove : MonoBehaviour
{
    [Label("�V���b�g���[�u�f�[�^")]
    public ShotMoveData _shotMoveData = default;            //�e�O���̃X�N���v�^�u���I�u�W�F�N�g

    private SpriteRenderer _spriteRenderer = default;       //���g��SpriteRenderer�i�[�p

    private Animator _animator = default;                   //���g��Animator�i�[�p

    private EnemyCharacterMove _characterMove = default;    //�G�L�����N�^�[�̋����X�N���v�g�̊i�[�p

    private PlayerMove _playerMove = default;               //�v���C���[�̋����X�N���v�g�̊i�[�p

    private GameObject _shooter = default;                  //�ˎ�i�[�p

    private float _timer = default;                         //�o�ߎ��Ԍv���p

    private float _initialVelocity = default;               //�����l

    private float _speed = default;                         //���x�i�[�p

    private float _colliderRadius = default;                //�����蔻��̑傫�����i�[

    private float _shotAngle = default;                     //���ˊp�i�[�p

    private int _shotCounter = default;                     //���̒e�𔭎˂��������i�[����(CharactorData����󂯎��)

    private int _maxShotCount = default;                    //���̒e�̍ő唭�ː����i�[����(CharactorData����󂯎��)

    private int _pelletCounter = default;                   //���ݐ������ꂽ�e�̐����i�[����(CharactorData����󂯎��)

    private int _maxPelletCount = default;                  //�����ɐ�������e�̐����i�[����(CharactorData����󂯎��)

    private bool _isVelocityChangePerShot = false;          //���˂��Ƃɏ��������������邩���i�[����

    private Vector2 _shooterPosition = default;             //�ˎ��Position�i�[�p

    private Vector2 _shotVector = default;                  //�e�̔��˃x�N�g���i�[�p

    private Vector2 _targetPosition = default;              //_target��Position�i�[�p

    private const string PLAYER_TAG = "Player";             //�v���C���[�̃^�O

    private const string BOSS_TAG = "Boss";                 //�{�X�̃^�O

    public float GetColliderRadius
    {
        //_colliderRadius��Ԃ�
        get { return _colliderRadius; }
    }

    public GameObject GetSetshooter
    {
        //_shooter��Ԃ�
        get { return _shooter; }
        //�n���ꂽ�l��_shooter�Ɋi�[
        set { _shooter = value; }
    }

    public float GetSetShotAngle
    {
        //_shotAngle��Ԃ�
        get { return _shotAngle; }
        //�n���ꂽ�l��_shotAngle�ɐݒ�
        set 
        {
            _shotAngle = value;
        }
    }

    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A���[�J���ϐ��Ŏ������̂ł������������

    private Vector2 _fixedRelayPoint = default;     //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;    //�ˎ� - _target�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;         //_relayPointVector��̏c(Y)�����W�i�[�p

    private void Awake()
    {
        //SpriteRenderer�擾
        _spriteRenderer = this.GetComponent<SpriteRenderer>();

        //�X�v���C�g�ύX
        _spriteRenderer.sprite = _shotMoveData._shotSprite;

        //���g��Animator���擾
        _animator = this.GetComponent<Animator>();

        //�����蔻��̃T�C�Y��ݒ�
        this._colliderRadius = _shotMoveData._colliderRadius;

        //�e�����L�������擾(�v���C���[�ƃ{�X�̂�)
        GetShooter();

        //����/���˂��Ƃ̉�����������e��?
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;
        }

        //���̒e�̎ˎ�͓G��?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //�G

            //�ˎ��EnemyCharactorMove���擾
            _characterMove = _shooter.GetComponent<EnemyCharacterMove>();

            //���˂��ƂɌ������邩���擾
            _isVelocityChangePerShot = _characterMove.GetIsChangeSpeedPerShot;
        }
        else
        {
            //�v���C���[

            //PlayerMove���擾
            _playerMove = _shooter.GetComponent<PlayerMove>();

            //���˂��ƂɌ������邩���擾
            _isVelocityChangePerShot = _playerMove.GetIsChengeSpeedPerShot;
        }
    }

    private void OnEnable()
    {
        //����������
        Reset();
    }

    /// <summary>
    /// <para>Reset</para>
    /// <para>�ϐ��������p���\�b�h �f�o�b�O���ɂ��Ăяo����悤�Ƀ��\�b�h�����Ă܂�</para>
    /// </summary>
    private void Reset()
    {
        //�ˎ�̍��W���Đݒ�
        _shooterPosition = _shooter.transform.position;

        //���g���ˎ�̍��W�Ɉړ�
        this.transform.position = _shooterPosition;

        //�o�ߎ��Ԃ����Z�b�g
        _timer = 0;

        //�����蔻��̑傫����_shotMoveData�̂��̂ƈقȂ邩
        if (this._colliderRadius != _shotMoveData._colliderRadius)
        {
            //�قȂ�

            //�����蔻��̃T�C�Y���Đݒ�
            this._colliderRadius = _shotMoveData._colliderRadius;
        }

        //�e�̉�]�t���O��true��
        if (_shotMoveData._isSpinning)
        {
            //true�Ȃ烉���_���ŉ�]������
            this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        //�ˎ肪�v���C���[��
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //�v���C���[�ł͂Ȃ�

            //�e�̃A�j���[�V������������
            _animator.SetTrigger("Enable");
        }

        //2�F�ڂ�����e��
        if (_shotMoveData._hasAlternativeColor)
        {
            //2�F�ڂ�����Ȃ�

            //���݉����ڂ̒e�����擾
            _shotCounter = _characterMove.GetCurrentShotCount;

            //�������
            if (_shotCounter % 2 == 1)
            {
                //��Ȃ�

                //�f�t�H���g�̃X�v���C�g��
                _animator.SetBool("AltColor", false);
            }
            else
            {
                //�����Ȃ�

                //2�F�ڂ̃X�v���C�g��
                _animator.SetBool("AltColor", true);
            }
        }

        //�������Ƃɉ���/�������Ȃ��e�Ȃ�
        if (_shotMoveData._shotVelocity == ShotMoveData.ShotVelocity.Nomal)
        {
            return;
        }

        //�ȉ��������Ƃɉ���������e�̏ꍇ

        //�v���C���[�ł͖���?
        if (_shotMoveData._shooterType != ShotMoveData.ShooterType.Player)
        {
            //�v���C���[�ł͂Ȃ�

            //���˂��ƂɌ������邩���擾
            _isVelocityChangePerShot = _characterMove.GetIsChangeSpeedPerShot;

            //���˂��Ƃɉ��������邩?
            if (!_isVelocityChangePerShot)
            {
                //���Ȃ�(�������Ƃɉ�����)

                //_characterMove���猻�݂̐����e�����󂯎��
                _pelletCounter = _characterMove.GetCurrentPelletCount;

                //_characterMove���瓯�������e�����󂯎��
                _maxPelletCount = _characterMove.GetMaxPelletCount;
            }
            else
            {
                //����

                //_characterMove���猻�݂̐����e�����󂯎��
                _shotCounter = _characterMove.GetCurrentShotCount;

                //_characterMove����ő唭�˒e�����󂯎��
                _maxShotCount = _characterMove.GetMaxShotCount;
            }
        }
        else
        {
            //�v���C���[�ł���

            //���˂��ƂɌ������邩���擾
            _isVelocityChangePerShot = _playerMove.GetIsChengeSpeedPerShot;

            //���˂��Ƃɉ��������邩?
            if (!_isVelocityChangePerShot)
            {
                //���Ȃ�(�������Ƃɉ�����)

                //_playerMove���猻�݂̐����e�����󂯎��
                _pelletCounter = _playerMove.GetCurrentPelletCount;

                //_playerMove���瓯�������e�����󂯎��
                _maxPelletCount = _playerMove.GetMaxPelletCount;
            }
            else
            {
                //����

                //_playerMove���猻�݂̔��˒e�����󂯎��
                _shotCounter = _playerMove.GetCurrentShotCount;

                //_playerMove����ő唭�˒e�����󂯎��
                _maxShotCount = _playerMove.GetMaxShotCount;
            }
        }

    }

    /// <summary>
    /// <para>OnBecameInvisible</para>
    /// <para>��ʊO�ɏo���e����������</para>
    /// </summary>
    private void OnBecameInvisible()
    {
        //�e�̖���������
        ObjectDisabler();
    }

    private void FixedUpdate()
    {
        _timer += Time.deltaTime;                        //���Ԃ̉��Z

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

        switch (_shotMoveData._shotVelocity)        //�e�̏����̐ݒ�ɉ����ď�������
        {
            //�ʏ�e(�����ω��Ȃ�)
            case ShotMoveData.ShotVelocity.Nomal:

                //�����Ƀf�t�H���g�l�̏�����ݒ�
                this._initialVelocity = _shotMoveData._initialVelocity;

                break;


            //���� or �������Ƃɏ���������
            case ShotMoveData.ShotVelocity.FastToSlow:


                //���˂��Ƃɉ���������e��?
                if (!_isVelocityChangePerShot)
                {
                    //�Ⴄ

                    //��x�ɐ�������e�� + 1���i�[
                    shotLength = _maxPelletCount + 1;

                    //���݉����ڂ̐�����
                    shotCount = _pelletCounter;
                }
                else
                {
                    //���˂��Ƃɉ���������e

                    //�A�˂���e�� + 1���i�[
                    shotLength = _maxShotCount + 1;

                    //���݉����ڂ����i�[
                    shotCount = _shotCounter;
                }

                //���� or �������Ƃ̌����l���Z�o
                float decelerationValue = _shotMoveData._initialVelocity / (shotLength * _shotMoveData._shotVelocityRate);

                //���� or �������Ƃ̌����l * ���ˁE���������v�Z(�f�t�H���g�̏�������������x)
                float subtractionValue = decelerationValue * shotCount;

                //�f�t�H���g�l������������x�����̒e�̏����Ƃ��Ċi�[
                this._initialVelocity = _shotMoveData._initialVelocity - subtractionValue;

                break;


            //���� or �������Ƃɏ���������
            case ShotMoveData.ShotVelocity.SlowToFast:


                //���˂��Ƃɉ���������e��?
                if (!_isVelocityChangePerShot)
                {
                    //�Ⴄ

                    //��x�ɐ�������e�� + 1���i�[
                    shotLength = _maxPelletCount + 1;

                    //���݉����ڂ̐��������i�[
                    shotCount = _pelletCounter;
                }
                else
                {
                    //���˂��Ƃɉ���������e

                    //�A�˂���e�� + 1���i�[
                    shotLength = _maxShotCount + 1;

                    //���݉����ڂ����i�[
                    shotCount = _shotCounter;
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
            //���x�ω��Ȃ�
            case ShotMoveData.ShotSettings.Nomal:

                //���x�ɏ����l���i�[
                _speed = this._initialVelocity;

                break;


            //���ˌ�ɉ�����
            case ShotMoveData.ShotSettings.Acceleration_Deceleration:

                //�A�j���[�V�����J�[�u�̒l * �����l�ŎZ�o�������x���i�[
                _speed = _shotMoveData._speedCurve.Evaluate((float)_timer / _shotMoveData._timeToSpeedChange) * this._initialVelocity;

                break;

            case ShotMoveData.ShotSettings.Laser:       //���[�U�[�e

                break;
        }

        /*�V���b�g�̔�ѕ�
         * Straight       : ���i
         * Curve          : �J�[�u
         */

        switch (_shotMoveData._shotType)        //�e�̋N���ݒ�ɉ����ď�������
        {
            //���i�e
            case ShotMoveData.ShotType.Straight:

                //���ˊp�������������ɂ����ۂ̊p�x�ɏC��
                float radian = _shotAngle * (Mathf.PI / 180);

                //�ϊ������p�x���x�N�g���ɕϊ�
                _shotVector = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

                //���߂��x�N�g���ɑ��x�A�o�ߎ��Ԃ��|�����l�����ݒn�ɂ���
                currentPos += _shotVector * _speed * Time.deltaTime;
                break;

                //�J�[�u�e
            case ShotMoveData.ShotType.Curve:

                //�o�ߎ��Ԃ�1�b������
                if (_timer < 1)
                {
                    //1�b����

                    //�x�W�F�Ȑ������ߌ��ʂ����݂̍��W�Ƃ���
                    currentPos = CalcuateBezierCurve();
                }
                else
                {
                    //1�b�ȏ�o��

                    //�ŏI�I�Ȍ����ɒ�����ɔ��
                    currentPos += _shotVector * _speed * Time.deltaTime;
                }


                break;

        }


        //�̎Z���ʂ̍��W�����g�̍��W�ɂ���
        this.transform.position = currentPos;
    }

    /// <summary>
    /// <para>ObjectDisabler</para>
    /// <para>setActive(false)���s���B �f�o�b�O�p�ɏ�����Ԃɖ߂����̕�����܂�</para>
    /// </summary>
    private void ObjectDisabler()
    {
        //�f�o�b�O���[�h��
        if (!_shotMoveData._isDebug)
        {
            //�ʏ���

            //�e�𖳌�������
            this.gameObject.SetActive(false);
        }
        else
        {
            //�f�o�b�O���[�h

            //���ˎ��̏�Ԃɖ߂�
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
        //���˒n�_�`�ڕW�n�_�Ԃ̃x�N�g�������߂�
        Vector2 direction = (targetPos - shotPos).normalized;

        //�Ԃ茌�Ƃ��ĕԂ�
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

        //�x�N�g���ɑ΂��鉡���I�t�Z�b�g�l��ݒ�
        float horizontalAxisOffset = _shotMoveData._curveShotHorizontalOffset;

        //������
        if (horizontalAxisOffset <= 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂��鍶�����ɐ����ȃx�N�g�������߂�
            Vector2 leftPointingVector = new Vector2(-_relayPointVector.y, _relayPointVector.x);

            //�x�N�g����̒��ԓ_���x�_�Ƀx�W�F�Ȑ��̒��ԓ_�̍��W�����ɂ��炷
            _fixedRelayPoint = _relayPointY + leftPointingVector * Mathf.Abs(horizontalAxisOffset);
        }
        //�E����
        else if (horizontalAxisOffset > 0)
        {
            //���˒n�_�`�^�[�Q�b�g�ԃx�N�g���ɑ΂���E�����ɐ����ȃx�N�g�������߂�
            Vector2 rightPointingVector = new Vector2(_relayPointVector.y, -_relayPointVector.x);

            //�x�N�g����̒��ԓ_���x�_�Ƀx�W�F�Ȑ��̒��ԓ_�̍��W���E�ɂ��炷
            _fixedRelayPoint = _relayPointY + rightPointingVector * Mathf.Abs(horizontalAxisOffset);
        }

        //���˒n�_�`���Ԓn�_�Ԃ�Lerp�ړ�
        Vector2 firstVec = Vector2.Lerp(_shooterPosition, _fixedRelayPoint, _timer);

        //���Ԓn�_�ԁ`�^�[�Q�b�g���W�Ԃ�Lerp�ړ�
        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _targetPosition, _timer);

        //Lerp�ړ����̍��W2�Ԃ�Lerp�ړ�
        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _timer);

        /*�^�[�Q�b�g�n�_��ʂ�߂��Ă��^��������Ԃ��߂ɒ��Ԓn�_�`�^�[�Q�b�g�n�_�Ԃ̃x�N�g����ݒ�
         * 
         * �^�[�Q�b�g�n�_��ʂ�߂������_�ł����Őݒ肵���x�N�g���Ŕ��ł���
         */
        _shotVector = StraightShotCalculateVector(_fixedRelayPoint, _targetPosition);

        //�x�W�F�Ȑ���̌��݂̍��W��Ԃ�
        return currentCurvePos;
    }

    /// <summary>
    /// <para>GetShooter</para>
    /// <para>�ˎ��ݒ肷��B �v���C���[�A�{�X�̓L�����𒼐ړ����B ����ȊO�̎G���G�̓L��������󂯎��</para>
    /// </summary>
    private void GetShooter()
    {
        //�ˎ�̃^�C�v�ŏ�������
        switch (_shotMoveData._shooterType)
        {
            case ShotMoveData.ShooterType.Player:       //�v���C���[

                //�v���C���[�^�O�̃I�u�W�F�N�g���擾
                GetSetshooter = GameObject.FindGameObjectWithTag(PLAYER_TAG);

                break;

            case ShotMoveData.ShooterType.Boss:         //�{�X

                //�{�X�^�O�̃I�u�W�F�N�g���擾
                GetSetshooter = GameObject.FindGameObjectWithTag(BOSS_TAG);

                break;

            case ShotMoveData.ShooterType.Common:       //���̑��G���G

                break;
        }


    }

    /// <summary>
    /// <para>AnimEvent_Disable</para>
    /// <para>����������(�A�j���[�V�����C�x���g�p)</para>
    /// </summary>
    public void AnimEvent_Disable()
    {
        //���g�𖳌���
        this.gameObject.SetActive(false);
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



