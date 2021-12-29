using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Класс игрока
/// </summary>
public class Player : MonoBehaviour, IPunObservable
{
    [Header("GameObjects")]
    [Tooltip("Камера игрока")]
    [SerializeField] internal GameObject _playerCamera; 
    
    [Header("Components")]
    [Tooltip("Компонент выпрыскивания очков игрока при смерти")]
    [SerializeField] internal DestroyPlayer _destroyPlayer; 
    [Tooltip("Компонент SpriteRenderer объекта")]
    [SerializeField] internal SpriteRenderer _spriteRenderer; 
    [Tooltip("Компонент синхронизации игрока с сервером")]
    [SerializeField] private PhotonView _photonView; 
    [Tooltip("Физика игрока")]
    [SerializeField] private Rigidbody2D _rig; 

    [Header("PlayerInput: Options")]
    [Tooltip("Количество очков игрока")]
    [SerializeField] private float _score = 10f; 
    [Tooltip("Скорость передвижения игрока")]
    [SerializeField] private float _speed = 1f; 

    [SerializeField] internal int nickname; 
    internal bool scoreSetted = false; 
    public bool died = false; 

    public float Score 
    {
        get { return _score; }
        set
        {
            _score = value; 
        }
    }

    private Vector2 _direction; 

  
    private void Start()
    {
        _spriteRenderer.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1); 
        if (!_photonView.IsMine) 
            _playerCamera.SetActive(false); 
        else
            Camera.SetupCurrent(_playerCamera.GetComponent<Camera>()); 
    }

    
    private void Update()
    {
        if (scoreSetted) 
        {
            ChangePlayerSize(); =
            scoreSetted = false; 
        }

        if (died) 
        {
            if (_spriteRenderer.enabled == true)
                _spriteRenderer.enabled = false; 
        }

        if (!_photonView.IsMine || died) 
            return; 

        _direction = DirectionCalculate();      

        if (!CheckVisible()) 
        {
            if (_spriteRenderer.color.a == 1f) 
                _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0f); 
        }
        else
        {
            if (_spriteRenderer.color.a == 0f) 
                _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f); 
        }
    }

    
    private bool CheckVisible()
    {
        if (Camera.main.gameObject.activeInHierarchy) 
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), GetComponent<Collider2D>().bounds);
        else
            return false;
    }


    private void FixedUpdate()
    {
        if (!_photonView.IsMine || died) 
            return; 

        MovePlayer(); 
    }

  =
    private void ChangePlayerSize()
    {
        float size = transform.localScale.x + (_score / 100f); 
        transform.localScale = new Vector3(size, size, 1f); 
        _playerCamera.GetComponent<Camera>().orthographicSize += _score / 50f; 
    }

   
    private void MovePlayer()
        => _rig.MovePosition(_rig.position + _direction * _speed * Time.fixedDeltaTime); 

   
    private Vector2 DirectionCalculate()
    {
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); 
        return direction; 
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_score); 

            
            stream.SendNext(transform.position.x);
            stream.SendNext(transform.position.y);

            
            stream.SendNext(transform.localScale.x);
            stream.SendNext(transform.localScale.y);

            

            if (_photonView.IsMine)
            {
                int nick;
                int.TryParse(string.Join("", PhotonNetwork.NickName.Where(c => char.IsDigit(c))), out nick);
                stream.SendNext(nick);
                if (nickname == 0)
                {
                    GetComponentInChildren<Text>().text = nick.ToString();
                    nickname = nick;
                }
            }
        }
        else
        {                
            _score = (float)stream.ReceiveNext(); 

           
            float posX = (float)stream.ReceiveNext();
            float posY = (float)stream.ReceiveNext();
            transform.position = new Vector2(posX, posY); 

            
            float scaleX = (float)stream.ReceiveNext();
            float scaleY = (float)stream.ReceiveNext();
            transform.localScale = new Vector2(scaleX, scaleY); 

          
            if (died && _spriteRenderer.enabled == true) 
            {
                _spriteRenderer.enabled = false; 
                enabled = false; 
            }

            if (!_photonView.IsMine && nickname == 0)
            {
                nickname = (int)stream.ReceiveNext();
                GetComponentInChildren<Text>().text = nickname.ToString();
            }
        }
    }

    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!died && col.CompareTag("Player")) 
        {
            Player otherPlayer = col.GetComponent<Player>(); 
            if (_score < otherPlayer._score) 
            {
                GetComponent<CircleCollider2D>().enabled = false; 
                _spriteRenderer.enabled = false; 
                _photonView.RPC("SetDied", RpcTarget.AllBuffered, null); 
            }
        }
    }

    
    [PunRPC]
    private void SetDied()
    {
        died = true; 
        _destroyPlayer.StartDestroy(_score, transform); 
    }
}