using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

    public List<AIEnemy> AllAi;

    private int TotalAI = 0;

    private int curAI = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;

        if (instance != this) DestroyImmediate(gameObject);
        GameManager.instance.GameStateChange += StartAiTurn;
    }
    
    

    // Update is called once per frame
    public void CollectAi(AIEnemy ai)
    {
        AllAi.Add(ai);
        TotalAI++;
    }

    public void StartAiTurn()
    {
        if (GameManager.instance.gameStage == GameManager.GameState.EnemyTurn)
        {
            foreach (AIEnemy ai in AllAi)
            {
                ai.CalculateActions();
            }

        }
    }

    public void AiComplete()
    {
        curAI++;
        if (curAI >= TotalAI)
        {
            curAI = 0;
            GameManager.instance.gameStage = GameManager.GameState.PlayerTurn;
        }
    }
}
