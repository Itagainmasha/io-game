using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ScoreMenu : MonoBehaviour
{
    [SerializeField] private Text[] _textsScore;

    public void SetScoreTop()
    {
        for (int i = 0; i < _textsScore.Length; i++) 
            _textsScore[i].text = i + "."; 
        List<Player> players = new List<Player>(); 
        List<float> scores = new List<float>(); 
        float[] score = new float[3];
        foreach (Player player in FindObjectsOfType<Player>()) 
        {
            players.Add(player);
            scores.Add(player.Score); 
        }
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < scores.Count; i++) 
            {
                if (score[j] < scores[i]) 
                    score[j] = scores[i]; 
            }
            scores.Remove(score[j]); 
        }
        for (int i = 0; i < players.Count; i++) 
        {
            for (int j = 0; j < players.Count; j++) 
            {
                if (score[i] == players[j].Score) 
                    _textsScore[i].text += players[j].nickname + "   ->   " + ((int)(score[i]-10)).ToString(); // Вывод текста
            }
        }
    }
}