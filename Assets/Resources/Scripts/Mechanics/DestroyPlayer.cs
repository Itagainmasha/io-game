using UnityEngine;
using Photon.Pun;

public class DestroyPlayer : MonoBehaviour
{
    [Tooltip("Префаб выприскиваемого объекта")]
    [SerializeField] private GameObject _eatItemPrefab; 
    private PhotonView _photonView; 
    private int _interationDeath = 0; 

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }

    
    /// <param name="score">Количество очков игрока</param>
    /// <param name="otherPlayer">Transform игрока-убийцы</param>
    internal void StartDestroy(float score, Transform otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            _photonView.RPC("Squirting", RpcTarget.AllBuffered, score, otherPlayer.position.x, otherPlayer.position.y); // Синхронизируем смерть
    }

    
    /// <param name="score">Количество очков игрока</param>
    /// <param name="posX">Позиция игрока-убийцы по оси X</param>
    /// <param name="posY">Позиция игрока-убийцы по оси Y</param>
    [PunRPC]
    private void Squirting(float score, float posX, float posY)
    {
        GameObject eatItemGameObject; 
        for (int i = 0; i < score/2; i++) 
        {
            eatItemGameObject = PhotonNetwork.Instantiate(_eatItemPrefab.name, transform.position, Quaternion.identity); 
            eatItemGameObject.name += score.ToString() + _interationDeath.ToString() + gameObject.GetComponent<PhotonView>().ViewID; 
            EatItem eatItem = eatItemGameObject.GetComponent<EatItem>(); 
            eatItem.score = 0.5f; 
            eatItem.isForced = true; 
            eatItem.forcedPos = new Vector2(posX + Random.Range(-5f, 5f), posY + Random.Range(-5f, 5f)); 
        }
        _interationDeath++; 
    }
}