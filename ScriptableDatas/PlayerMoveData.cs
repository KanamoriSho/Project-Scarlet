using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Create PlayerMoveData")]
public class PlayerMoveData : ScriptableObject
{
    [Label("�L�����N�^�[��")]
    public string _playerName;

    [Label("HP")]
    public int _maxHp;

    [Label("�V���b�g����SE")]
    public AudioClip _shotSoundEffect = default;

    [Label("�ړ��X�s�[�h")]
    public int _speed;

    [Label("���̈ړ��͈�")]
    public float _xLimitOfMoving = 5.0f;

    [Label("�c�̈ړ��͈�")]
    public float _yLimitOfMoving = 10.0f;

    [Label("�e")]
    public Sprite _sprite;

    [Label("�b�Ԕ��ː�"), Range(1, 100)]
    public int _shotPerSecond = default;

    [Label("�v�[���p���������e��")]
    public int _initiallyGeneratedShots = 50;
}
