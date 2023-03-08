using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMove : MonoBehaviour
{
    [Label("�V���b�g���[�u�f�[�^")]
    public ShotMoveData _shotMoveData = default;             //�e�O���̃X�N���v�^�u���I�u�W�F�N�g

    private SpriteRenderer _spriteRenderer = default;       //���g��SpriteRenderer�i�[�p

    [SerializeField, Label("�e�̃^�[�Q�b�g")]
    private GameObject _target = default;

    private Vector2 _targetPosition = default;              //_target��Position�i�[�p

    public Vector2 GetSetTargetPosition          //_target��Get�ASet�p�v���p�e�B
    {
        get { return _targetPosition; }
        set { _targetPosition = value; }
    }

    private float _speed = default;                 //���x�i�[�p

    public float GetSetSpeed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    private float _time = default;                  //���Ԋi�[�p

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

    //�ȉ��̕ϐ���OnDrowGizmos�Ɏ󂯓n�����߂Ƀt�B�[���h�ϐ��ɂ��Ă܂����A���[�J���ϐ��Ŏ������̂ł������������

    private Vector2 _fixedRelayPoint = default;     //�x�W�F�Ȑ��̒��ԓ_�i�[�p

    private Vector2 _relayPointVector = default;    //�ˎ� - _target�Ԃ̃x�N�g���i�[�p

    private Vector2 _relayPointY = default;         //_relayPointVector��̏c(Y)�����W�i�[�p

    private Vector2 _relayPointX = default;         //_relayPointVector��̉�(X)�����W�i�[�p

    private void Awake()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>();          //SpriteRenderer�擾

        _spriteRenderer.sprite = _shotMoveData._shotSprite;              //�X�v���C�g�ύX

        GetShooter();           //�e�����L�������擾(�v���C���[�ƃ{�X�̂�)
    }

    private void OnEnable()
    {
        Reset();        //����������

        if (_shotMoveData._isSpinning)        //�e�̉�]�t���O��true��
        {
            this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));     //true�Ȃ烉���_���ŉ�]������
        }


    }

    /// <summary>
    /// <para>Reset</para>
    /// <para>�ϐ��������p���\�b�h �f�o�b�O���ɂ��Ăяo����悤�Ƀ��\�b�h�����Ă܂�</para>
    /// </summary>
    private void Reset()
    {
        _shooterPosition = _shooter.transform.position;

        this.transform.position = _shooterPosition;

        GetTarget();
        _targetPosition = _target.transform.position;

        _time = 0;
    }

    private void GetTarget()
    {

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:

                GetSetTargetPosition = GetSetshooter.transform.GetChild(0).transform.GetChild(0).gameObject.transform.position;

                break;

            case ShotMoveData.ShotType.TargetToPlayer:

                _target = GameObject.FindGameObjectWithTag(PLAYER_TAG);

                break;

        }
    }

    private void OnBecameInvisible()
    {
        ObjectDisabler();
    }

    private void FixedUpdate()
    {
        _time += Time.deltaTime;

        Vector2 currentPos = this.transform.position;

        /*����O��    ��ɑ��x�֘A
         * Nomal                     : ����O������
         * Acceleration_Deceleration : ������
         * Laser                     : ���[�U�[
         */

        switch (_shotMoveData._shotSettings)
        {
            case ShotMoveData.ShotSettings.Nomal:

                _speed = _shotMoveData._speed;       //���x��shotMoveData������

                break;

            case ShotMoveData.ShotSettings.Acceleration_Deceleration:

                //�A�j���[�V�����J�[�u�̒l * shotMoveData��_speed�ŎZ�o�������x����
                _speed = _shotMoveData._speedCurve.Evaluate((float)_time / _shotMoveData._timeToSprrdChange) * _shotMoveData._speed;

                break;

            case ShotMoveData.ShotSettings.Laser:

                break;
        }

        /*�V���b�g�̎��
         * Straight       : ���i
         * Curve          : �J�[�u
         * TargetToPlayer : ���@�_��
         */

        switch (_shotMoveData._shotType)
        {
            case ShotMoveData.ShotType.Straight:
                _shotVector = StraightShotCalculateVector(_shooterPosition, _targetPosition);
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

            case ShotMoveData.ShotType.TargetToPlayer:

                currentPos += StraightShotCalculateVector(_shooterPosition, _targetPosition) * _speed * Time.deltaTime;


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
    /// <returns></returns>
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

        //�x�N�g����̃I�t�Z�b�g�������ԓ_���Z�o
        _relayPointY = Vector2.Lerp(_shooterPosition, _targetPosition, _shotMoveData._verticalOffset);

        /*�e�O���̍��E�l�ɉ����Čv�Z����x�N�g���̌�����ύX����
         * 
         * ���ɔ�΂��ꍇ��_relayPointVector�ɑ΂��č������̐����x�N�g���ɑ΂��č��E�l��������
         * �E�ɔ�΂��ꍇ��_relayPointVector�ɑ΂��ĉE�����̐����x�N�g���ɑ΂��č��E�l��������
         * 
         * _relayPointY�ŋ��߂��x�N�g����̒��Ԓn�_�����Ƃɐ����x�N�g�����o��
         */

        if (_shotMoveData._horizontalOffset <= 0)        //������
        {
            _fixedRelayPoint = new Vector2(-_relayPointVector.y, _relayPointVector.x) * Mathf.Abs(_shotMoveData._horizontalOffset) + _relayPointY;
        }
        else if (_shotMoveData._horizontalOffset > 0)    //�E����
        {
            _fixedRelayPoint = new Vector2(_relayPointVector.y, -_relayPointVector.x) * Mathf.Abs(_shotMoveData._horizontalOffset) + _relayPointY;
        }

        Vector2 firstVec = Vector2.Lerp(_shooterPosition, _fixedRelayPoint, _time);

        Vector2 secondtVec = Vector2.Lerp(_fixedRelayPoint, _targetPosition, _time);

        Vector2 currentCurvePos = Vector2.Lerp(firstVec, secondtVec, _time);

        _shotVector = StraightShotCalculateVector(_fixedRelayPoint, _targetPosition);


        return currentCurvePos;
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
    /// <para>OnDrawGizmos</para>
    /// <para>�x�W�F�Ȑ��̎n�_ - ���ԓ_�ԁA���ԓ_ - �I�_�Ԃ���ŕ`�悷�郁�\�b�h �f�o�b�O�p</para>
    /// </summary>
    void OnDrawGizmos()
    {

#if UNITY_EDITOR


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

            case ShotMoveData.ShotType.TargetToPlayer:



                break;

        }

#endif

    }
}



