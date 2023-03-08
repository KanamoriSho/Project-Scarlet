using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotsList
{
    [Label("�V���b�g")]
    public GameObject[] _shots;
}

[System.Serializable]
public class ShotCountList
{
    [Label("�V���b�g��")]
    public int[] _shotCounts;
}

[CreateAssetMenu(menuName = "ScriptableObject/Create CharactorMoveData")]
public class CharactorMoveData : ScriptableObject
{
    [Label("�L�����N�^�[��")]
    public string _charactorName;

    [Label("HP")]
    public int _maxHp;

    [Label("�V���b�g����SE")]
    public AudioClip _shotSoundEffect = default;

    [Label("�ړ��X�s�[�h")]
    public int _speed;

    [Label("�E�F�[�u��")]
    public int _waveCount;

    [Label("�E�F�[�u")]
    public ShotsList[] _waves = default;

    [Label("�E�F�[�u")]
    public ShotCountList[] _shotCounts = default;

    public enum MovementPatern
    {
        [EnumLabel("�ړ��̎d��", "�ړ��̂�")]
        OnlyMove,
        [EnumLabel("�ړ��̎d��", "�����Ȃ���")]
        ShootingWhileMoving,
    }

    [Label("�ړ��̎d��"), EnumElements(typeof(MovementPatern))]
    public List<MovementPatern> _movementPaterns = new List<MovementPatern>();

    public bool _isCurve = false;

    [Label("�ړ��p�����x�J�[�u")]
    public AnimationCurve _speedCurve = default;

    [Label("���̈ړ��͈�")]
    public float _xLimitOfMoving = 5.0f;

    [Label("�c�̈ړ��͈�")]
    public float _yLimitOfMoving = 10.0f;

    [Label("�e")]
    public Sprite _sprite;

    [Label("�b�Ԕ��ː�"), Range(1, 100)]
    public int[] _shotPerSeconds = default;

    [Label("�v�[���p���������e��")]
    public int _initiallyGeneratedShots = 50;

    [Label("�J�[�u�p�c���I�t�Z�b�g"), Range(0.0f, 1.0f)]
    public float _verticalOffset = default;

    [Label("�J�[�u�p�����I�t�Z�b�g"), Range(-1.0f, 1.0f)]
    public float _horizontalOffset = default;

    public enum ShotPatern
    {
        [EnumLabel("�e�̌�����", "�P��")]
        OneShot,
        [EnumLabel("�e�̌�����", "������")]
        MultipleShots,
        [EnumLabel("�e�̌�����", "���ˏ�")]
        RadialShots,
    }

    [Label("�e�̌�����"), EnumElements(typeof(ShotPatern))]
    public List<ShotPatern> _shotPaterns = new List<ShotPatern>();

    public enum ShotVelocity
    {
        [EnumLabel("�e�̑��x", "����")]
        Nomal,
        [EnumLabel("�e�̑��x", "���X�Ɍ���")]
        FastToSlow,
        [EnumLabel("�e�̑��x", "���X�ɉ���")]
        SlowToFast,

    }

    [Label("�e�̌�����"), EnumElements(typeof(ShotVelocity))]
    public List<ShotVelocity> _shotVelocity = new List<ShotVelocity>();

    [HideInInspector]
    public int _pelletCountInShot = 2;

    [HideInInspector]
    public int _formedAngle = 30;
}
