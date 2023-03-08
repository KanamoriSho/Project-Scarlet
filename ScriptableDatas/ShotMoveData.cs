using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/Create ShotMoveData")]
public class ShotMoveData : ScriptableObject
{

    [Label("�e")]
    public Sprite _shotSprite = default;

    [Label("�U���e")]
    public bool _isLockOnShot = false;

    [Label("��]")]
    public bool _isSpinning = false;

    public enum ShooterType
    {
        Player,             //�v���C���[
        Boss,               //�{�X
        Common,             //�G���G
    }

    [Label("�e�����L����")]
    public ShooterType _shooterType;

    [Label("�v���C���[�ŗL�̒e��"), HideInInspector]
    public bool _isPlayersShot = false;

    [Label("�{�X�ŗL�̒e��"), HideInInspector]
    public bool _isBossShot = false;

    [Label("�e�̑��x")]
    public float _speed = 3;

    public enum ShotType
    {
        [EnumLabel("�e�̎��", "���i")]
        Straight,           //���i
        [EnumLabel("�e�̎��", "�J�[�u")]
        Curve,              //�J�[�u
        [EnumLabel("�e�̎��", "���@�_��")]
        TargetToPlayer,     //���@�_��
        [EnumLabel("�e�̎��", "�ǔ�")]
        Homing,             //�ǔ�
    }

    [Label("�e�̎��"), EnumElements(typeof(ShotType))]
    public ShotType _shotType;

    public enum ShotSettings
    {
        [EnumLabel("����O��", "����")]
        Nomal,                           //����O������
        [EnumLabel("����O��", "������")]
        Acceleration_Deceleration,       //������
        [EnumLabel("����O��", "���[�U�[")]
        Laser,                           //���[�U�[
    }

    [Label("����O��"), EnumElements(typeof(ShotSettings))]
    public ShotSettings _shotSettings;

    [Label("�������̎���")]
    public int _timeToSprrdChange = default;

    [Label("�������J�[�u"), HideInInspector]
    public AnimationCurve _speedCurve;          //�������J�[�u

    [Label("�J�[�u�p�c���I�t�Z�b�g"), Range(0.0f, 1.0f), HideInInspector]
    public float _verticalOffset = default;

    [Label("�J�[�u�p�����I�t�Z�b�g"), Range(-1.0f, 1.0f), HideInInspector]
    public float _horizontalOffset = default;

    [Label("�f�o�b�O���[�h")]
    public bool _isDebug = false;

}
