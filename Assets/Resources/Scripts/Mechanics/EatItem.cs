using UnityEngine;
using Photon.Pun;

public class EatItem : MonoBehaviour, IPunObservable
{
    [Tooltip("Компонент SpriteRenderer объекта")]
    [SerializeField] protected SpriteRenderer _spriteRenderer; 

    internal float score; 
    internal bool isForced = false; 
    private PhotonView _photonView; 
    internal Vector2 forcedPos; 
    [SerializeField] private bool _inEndPosition; 
    private bool _destroyed = false;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>(); 
    }
    private void Start()
    {
        if (!isForced) 
        {
            _inEndPosition = true; 
            score = Random.Range(0.2f, 0.5f); 
        }

        _spriteRenderer.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f); 
        float size = transform.localScale.x * score; 
        transform.localScale = new Vector3(size, size, 1f);
    }

    private void Update()
    {
        if (isForced && !_inEndPosition) 
        {
            transform.position = Vector2.MoveTowards(transform.position, forcedPos, 0.1f); 
            if (transform.position == new Vector3(forcedPos.x, forcedPos.y, transform.position.z)) 
                _inEndPosition = true;
        }

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

    /// <param name="col">Коллайдер объекта, вошедшего в триггер</param>
    private void OnTriggerStay2D(Collider2D col)
    {
        if (_inEndPosition && col.CompareTag("Player")) 
        {
            if (!player.died) 
            {
                if (_photonView == null) 
                    _photonView = GetComponent<PhotonView>(); 
                player.scoreSetted = true; 
                player.Score += score; 
                _photonView.RPC("DestroyThis", RpcTarget.AllBuffered, gameObject.name); 
            }
        }
    }

    [PunRPC]
    private void DestroyThis(string nameObject)
    {
        if (gameObject.name == nameObject) 
            Destroy(gameObject); 
    }

    /// <param name="stream">Стрим</param>
    /// <param name="info">Информация</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (isForced) 
        {
            if (stream.IsWriting) 
            {
               
                stream.SendNext(transform.position.x);
                stream.SendNext(transform.position.y);

                stream.SendNext(_inEndPosition); 
            }
            else
            {
                
                float posX = (float)stream.ReceiveNext();
                float posY = (float)stream.ReceiveNext();
                transform.position = new Vector2(posX, posY); 

                _inEndPosition = (bool)stream.ReceiveNext(); 
            }
        }
    }
}