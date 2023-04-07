using System.Linq;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [SerializeField, Label("�����蔻��T�C�Y"), Range(0.0f, 1.0f)]
    private float _colliderRadius = 0.05f;

    public float GetColliderRadius
    {
        //_colliderRadius��Ԃ�
        get { return _colliderRadius; }
    }

    [SerializeField, Label("�v���C���[")]
    private bool _isPlayer = false;             //�v���C���[���ۂ��̃t���O

    private PlayerMove _playerMove = default;   //�v���C���[��PlayerMove�̊i�[�p�ϐ�

    [SerializeField, Label("�G�e�̃^�O"), TagFieldDrawer]
    private string _enemyShotTag = default;     //���̃L�����ɂƂ��Ă̓G�̒e�̃^�O���i�[����ϐ�

    private bool _isHit = false;                //��e����t���O

    public bool GetSetHitFlag
    {
        //_isHit��Ԃ�
        get { return _isHit; }

        //_isHit�Ɏ󂯎�����l������
        set { _isHit = value; }
    }

    private void Awake()
    {
        //�v���C���[��?
        if(_isPlayer)
        {
            //�v���C���[�ł���

            //�v���C���[��PlayerMove�R���|�[�l���g���擾
            _playerMove = this.gameObject.GetComponent<PlayerMove>();
        }
    }

    private void Update()
    {
        //��ʓ��̑S�G�e���擾
        Transform[] enemyShotsInScene = GameObject.FindGameObjectsWithTag(_enemyShotTag).Select(enemyShot => enemyShot.transform).ToArray();

        //�G�e������Ȃ�
        if(enemyShotsInScene.Length == 0)
        {
            //�����������߂�
            return;
        }

        //�擾�����G�e�������̏Ə��Ƀ\�[�g
        Transform[] sortedByDistance = 
                    enemyShotsInScene.OrderBy(enemyShots => Vector3.Distance(enemyShots.transform.position, transform.position)).ToArray();

        //��ԋ߂��e�̓����蔻��̔��a + ���g�̓����蔻��̔��a�����߂�
        float SumOfColliderRadius = sortedByDistance[0].gameObject.GetComponent<ShotMove>().GetColliderRadius + this._colliderRadius;

        //���g���v���C���[���A���G��Ԃ̏ꍇ
        if(_isPlayer && _playerMove.GetIsInvincible)
        {
            //���������Ȃ�
            return;
        }

        //�e�Ƃ̋����������蔻��̔��a�̍��v�����������Ȃ�����
        if (Vector3.Distance(sortedByDistance[0].position, transform.position) <= SumOfColliderRadius && !_isHit)
        {
            //��e�t���O��true��
            _isHit = true;

            //��ʓ��̓G�e�̔z���������
            enemyShotsInScene = new Transform[0];

            //�������̔z���������
            sortedByDistance = new Transform[0];
        }

    }

    /// <summary>
    /// <para>OnDrawGizmos</para>
    /// <para>�����蔻��̃T�C�Y��_�ŕ`�悷�郁�\�b�h �f�o�b�O�p</para>
    /// </summary>
    private void OnDrawGizmos()
    {

#if UNITY_EDITOR

        Gizmos.DrawSphere(this.gameObject.transform.position, _colliderRadius);
#endif

    }
}
