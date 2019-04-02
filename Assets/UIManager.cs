using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static string readyText = "Tap on screen to place scene";
    private static string enemyText = "Enemy Turn";
    private static string playerText = "Your Turn";
    private static string endText = "Game Complete";
    [Header("Text")]
    public Text phasetxt;
    public Text skill1txt;
    public Text skill2txt;
    public Text skill3txt;

    [Header("Button")] 
    public Button Skill1;
    public Button Skill2;
    public Button Skill3;
    public Button EndTurn;
    public Button Reset;

    public static UIManager instance;
    
    
    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (instance != this) DestroyImmediate(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Onclick(Button btn)
    {
        if (btn == Skill1)
        {
            
        }if (btn == Skill2)
        {
            
        }if (btn == Skill3)
        {
            
        }if (btn == EndTurn)
        {
            
        }if (btn == Reset)
        {
            
        }
    }
    public void SetSkill1Txt(string txt)
    {
        skill1txt.text = txt;
    }
    public void SetSkill2Txt(string txt)
    {
        skill2txt.text = txt;
    }
    public void SetSkill3Txt(string txt)
    {
        skill3txt.text = txt;
    }
    public void SetPhaseTxt(string txt)
    {
        skill3txt.text = txt;
    }

    public void ShowButtons(bool _show)
    {
        ShowSkill1Button(_show);
        ShowSkill2Button(_show);
        ShowSkill3Button(_show);
        ShowTurnEndButton(_show);
    }

    public void ShowSkill1Button(bool _show)
    {
        Skill1.gameObject.SetActive(_show);
    }
    public void ShowSkill2Button(bool _show)
    {
        Skill2.gameObject.SetActive(_show);
    }
    public void ShowSkill3Button(bool _show)
    {
        Skill3.gameObject.SetActive(_show);
    }
    public void ShowTurnEndButton(bool _show)
    {
        EndTurn.gameObject.SetActive(_show);
    }

    public void StateGUIChange()
    {
        if (GameManager.instance.gameStage == GameManager.GameState.Readying)
        {
            ShowButtons(false);
            SetPhaseTxt(readyText);
        }

        if (GameManager.instance.gameStage == GameManager.GameState.EnemyTurn)
        {
            ShowButtons(false);
            SetPhaseTxt(enemyText);
        }
        
        if (GameManager.instance.gameStage == GameManager.GameState.Resolution)
        {
            ShowButtons(false);
            SetPhaseTxt(endText);
        }

        if (GameManager.instance.gameStage == GameManager.GameState.PlayerTurn)
        {
            ShowButtons(true);
            SetPhaseTxt(playerText);
        }
    }
    
}
