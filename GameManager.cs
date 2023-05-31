using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;

    private GameObject _player = default;

    private GameObject _boss = default;

    private PlayerMove _playerMove = default;

    private EnemyCharacterMove _bossMove = default;

    [SerializeField, Label("ゲームオーバーUI")]
    private SpriteRenderer _gameOverUI = default;

    [SerializeField, Label("ステージクリアUI")]
    private GameObject _stageClearUI = default;

    [SerializeField, Label("ボスHPバー")]
    private Slider _bossHPBar = default;

    [SerializeField]
    private TalkManager _talkManager = default;

    private bool _isTalking = false;

    private bool _isTalkOnce = false;

    private bool _isBossFirstAppear = false;

    public GameManager GetInstance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        _player = GameObject.FindGameObjectWithTag("Player");

        _boss = GameObject.FindGameObjectWithTag("Boss");

        _playerMove = _player.GetComponent<PlayerMove>();

        _bossMove = _boss.GetComponent<EnemyCharacterMove>();

        _boss.SetActive(false);

        ChangeTimeScale(false);

        _gameOverUI.enabled = false;

        _stageClearUI.SetActive(false);

        _isTalking = true;
    }

    private void Update()
    {

        if(_talkManager.GetIsBossAppear && !_isBossFirstAppear)
        {
            _boss.SetActive(true);

            _isBossFirstAppear = true;
        }

        if(_talkManager.GetIsTalkEnd)
        {
            _isTalking = false;
        }
        else
        {
            _isTalking = true;
        }

        _bossMove.SetIsTalking = _isTalking;

        _playerMove.SetIsTalking = _isTalking;


        _bossHPBar.value = _bossMove.GetCurrentHP;

        bool isPlayerDead = _playerMove.GetIsPlayerDead;

        bool isBossDead = _bossMove.GetIsDead;

        if(isPlayerDead)
        {
            ChangeTimeScale(true);

            _gameOverUI.enabled = true;
        }

        if(isBossDead)
        {
            _isTalking = true;

            if (!_isTalkOnce)
            {
                _isTalkOnce = true;

                _talkManager.SetIsTalkEnd = false;
            }

            _bossHPBar.enabled = false;

            if (_talkManager.GetIsTalkEnd)
            {
                _stageClearUI.SetActive(true);

                //ChangeTimeScale(true);
            }

        }


    }

    private void ChangeTimeScale(bool togle)
    {
        if(togle)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
