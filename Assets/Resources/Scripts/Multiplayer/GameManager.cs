using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Prefabs")]
    [Tooltip("Префаб игрока")]
    [SerializeField] private GameObject _playerPrefab; 
    [Tooltip("Префаб объекта для съедения")]
    [SerializeField] private GameObject _eatItemPrefab; 
    [Header("Generation")]
    [Tooltip("Список игроков")]
    [SerializeField] private List<GameObject> _players = new List<GameObject>(); 
    [Tooltip("Количество генерируемых объектов для съедения")]
    [SerializeField] private int _instantiatesCount = 50; 
    [Tooltip("Лист занятых позиций для генерации")]
    [SerializeField] private List<Vector3> _takenPos = new List<Vector3>(); 

    [Header("End Round")]
    [Tooltip("Меню результатов очков")]
    [SerializeField] private GameObject _scoreMenu; 
    [SerializeField] private Text _timeToEndRoundText; 
    [Tooltip("Время до конца раунда нынешнее")]
    [SerializeField] private int _timeLost = 60; 
    [Tooltip("Время до конца раунда")]
    [SerializeField] private int _maxTimeLost = 60; 

    [Header("Synchronization")]
    [Tooltip("Синхронизация менеджера")]
    [SerializeField] private PhotonView _photonView; 
    private ScoreMenu _scoreMenuComponent; 
    private Coroutine _coroutine; 

    private void Awake()
    {
        Vector3 pos = new Vector3(Random.Range(-70f, 47f), Random.Range(-57f, 57f), -1f); 
        _players.Add(PhotonNetwork.Instantiate(_playerPrefab.name, pos, Quaternion.identity)); 
    }

  
    private void Start()
    {
        _scoreMenuComponent = FindObjectOfType<ScoreMenu>(); 
        _maxTimeLost += 10; 
        _timeLost = _maxTimeLost; 
        if (PhotonNetwork.IsMasterClient) 
        {
            GenerateItems(); 
            if (_coroutine == null) 
                _coroutine = StartCoroutine(TimerToEndRound()); 
        }
    }


    private void GenerateItem()
    {
        Vector3 pos = new Vector3(Random.Range(-73f, 50f), Random.Range(-60f, 60f), -1f); 
        for (int j = 0; j < _takenPos.Count; j++) 
        {
            while (pos == _takenPos[j]) 
            {
                pos = new Vector3(Random.Range(-73f, 50f), Random.Range(-60f, 60f), -1f); 
                j = 0; 
            }
        }
        PhotonNetwork.Instantiate(_eatItemPrefab.name, pos, Quaternion.identity); 
    }

    private void GenerateItems()
    {
        for (int i = 0; i < _instantiatesCount; i++) 
            GenerateItem();
    }


    private IEnumerator TimerToEndRound()
    {
        yield return new WaitForSeconds(1f); 
        GenerateItem();
        _photonView.RPC("CheckEndRound", RpcTarget.All, null); 
    }


    [PunRPC]
    private void CheckEndRound()
    {
        StopAllCoroutines(); 
        if (_timeLost > 0) 
            _timeLost--; 
        if (_timeLost > 10) 
            _timeToEndRoundText.text = (_timeLost-10).ToString(); 
        else
            _timeToEndRoundText.text = "0"; 

        if (_timeLost <= 0 && PhotonNetwork.IsMasterClient) 
            _photonView.RPC("EndRound", RpcTarget.All, null); 
        else
        {
            if (_timeLost <= 10) 
            {
                if (!_scoreMenu.activeInHierarchy) 
                {
                    _scoreMenu.SetActive(true); 
                    _scoreMenuComponent.SetScoreTop(); 
                }
            }
                
            _coroutine = null; 
            if (PhotonNetwork.IsMasterClient && _coroutine == null) 
                _coroutine = StartCoroutine(TimerToEndRound()); 
            
        } 
    }


    [PunRPC]
    private void EndRound()
    {
        foreach (Player _player in FindObjectsOfType<Player>())
        {
            if (!_player._spriteRenderer.enabled) 
                _player._spriteRenderer.enabled = true; 
            _player.scoreSetted = true; 
            _player.Score = 10f; 
            _player.died = false;
            _player.transform.localScale = new Vector3(3f, 3f, 1f);
            _player.GetComponent<CircleCollider2D>().enabled = true;
            _player._playerCamera.GetComponent<Camera>().orthographicSize = 15f;
            CircleCollider2D[] CircleColliders2D = _player.GetComponents<CircleCollider2D>();
            foreach (CircleCollider2D CircleCollider2D in CircleColliders2D) 
                CircleCollider2D.enabled = true; 
        }
        foreach (GameObject player in _players) 
        {
            if (player != null) 
            {
                if (player.GetComponent<PhotonView>().IsMine)
                {
                    Vector3 pos = new Vector3(Random.Range(-33f, 22f), Random.Range(-27f, 27f), -1f); 
                    player.transform.position = pos; 
                }
            }
        }

        _scoreMenu.SetActive(false); 
        _coroutine = null; 
        _timeLost = _maxTimeLost; 
        if (PhotonNetwork.IsMasterClient && _coroutine == null) 
            _coroutine = StartCoroutine(TimerToEndRound()); 
    }
    public void LeaveRoom()
    {
        PhotonNetwork.Destroy(_players[0]);
        PhotonNetwork.LeaveRoom(); 
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu"); 
    }

 
    private void OnApplicationQuit()
    {
        LeaveRoom(); 
    }

    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) 
        {
            stream.SendNext(_timeLost); 
        }
        else
        {
            _timeLost = (int)stream.ReceiveNext(); 
        }
    }
}