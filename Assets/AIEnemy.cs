using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{

    private CharacterScript _chara;

    [SerializeField] private float distanceWeight;
    [SerializeField] private float threatWeight;
    [SerializeField] private float effectivenessWeight;
    [SerializeField] private bool meleeOnly;
    // Start is called before the first frame update
    void Start()
    {
        _chara = GetComponent<CharacterScript>();
        GameManager.instance.GameStateChange += CalculateActions;
    }
    

    public void CalculateActions()
    {
        if (GameManager.instance.gameStage == GameManager.GameState.EnemyTurn)
        {
            GameObject target;
            float priority = 0;
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (getPriority(o) > priority)
                {
                    target = o;
                    priority = getPriority(o);
                }
            }
            
        }
    }

    private void approachTarget(GameObject target)
    {
        if (meleeOnly)
        {
            Physics.Raycast()
            target.GetComponent<CharacterScript>().PlayerMove();
        }
    }

    private void attackTarget(GameObject target)
    {
        
    }

    private float getPriority(GameObject unit)
    {
        return threatEvaluation(unit) + effectivenessEvaluation(unit);
    }

    private float distanceCoefficient(GameObject unit)
    {
        return Vector3.Distance(this.transform.localPosition, unit.transform.localPosition)*distanceWeight;
    }

    private float threatEvaluation(GameObject unit)
    {
        float dc = distanceCoefficient(unit);
        float avgPower = unit.GetComponent<CharacterScript>().GetSkill1().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill1().Range - dc)), 0) +
            unit.GetComponent<CharacterScript>().GetSkill2().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill2().Range - dc)),0) +
            unit.GetComponent<CharacterScript>().GetSkill3().Power * Mathf.Max((1 - unit.GetComponent<CharacterScript>().GetSkill3().Range - dc),0);
        return avgPower / 3 * threatWeight;
    }

    private float effectivenessEvaluation(GameObject unit)
    {
        float dc = distanceCoefficient(unit);
        float avgPower = this.GetComponent<CharacterScript>().GetSkill1().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill1().Range - dc)), 0) +
                         this.GetComponent<CharacterScript>().GetSkill2().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill2().Range - dc)),0) +
                         this.GetComponent<CharacterScript>().GetSkill3().Power * Mathf.Max((1 - unit.GetComponent<CharacterScript>().GetSkill3().Range - dc),0);
        return avgPower / 3 * threatWeight;
    }
    
}
