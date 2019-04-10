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
        AIManager.instance.CollectAi(this);
        //GameManager.instance.GameStateChange += CalculateActions;
    }
    

    public void CalculateActions()
    {
        if (gameObject.activeSelf)
        {
            if (GameManager.instance.gameStage == GameManager.GameState.EnemyTurn)
            {
                GameObject target = null;
                float priority = 0;
                foreach (GameObject o in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (Mathf.Max(getPriority(o), 0.1f) > priority)
                    {
                        target = o;
                        priority = getPriority(o);
                    }

                }

                StartCoroutine(approachTarget(target));
            }
        }
    }

    private IEnumerator approachTarget(GameObject target)
    {
        print(target + "is targetted");
        //if (meleeOnly)
        {
            //_chara.Move(getNearestValidGrid(target.transform));
            _chara.AIMove((target.transform.position));
        }
        yield return new WaitForSeconds(3f);
        attackTarget(target);
    }

    private Vector3 getNearestValidGrid(Transform t)
    {
        float x = (gameObject.transform.localPosition.x - t.localPosition.x);
        if (x > 0)
        {
            x = 1;
        }
        else
        {
            x = -1;
        }
        //float y = (gameObject.transform.position.y - t.position.y);
        float z = (gameObject.transform.localPosition.z - t.localPosition.z);
        if (z > 0)
        {
            z = 1;
        }
        else
        {
            z = -1;
        }

        Vector3 grid = new Vector3 (t.localPosition.x + x, t.localPosition.y, t.localPosition.z + z);
        return grid;
    }

    private void attackTarget(GameObject target)
    {
        _chara.Skill1(target.GetComponent<CharacterScript>());
        AIManager.instance.AiComplete();
    }

    private float getPriority(GameObject unit)
    {
        return threatEvaluation(unit) - distanceCoefficient(unit)*0.1f; //+ effectivenessEvaluation(unit);
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
            unit.GetComponent<CharacterScript>().GetSkill3().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill3().Range - dc)),0);
        return avgPower / 3 * threatWeight;
    }

    private float effectivenessEvaluation(GameObject unit)
    {
        float dc = distanceCoefficient(unit);
        float avgPower = this.GetComponent<CharacterScript>().GetSkill1().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill1().Range - dc)), 0) +
                         this.GetComponent<CharacterScript>().GetSkill2().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill2().Range - dc)),0) +
                         this.GetComponent<CharacterScript>().GetSkill3().Power * Mathf.Max((1 - (unit.GetComponent<CharacterScript>().GetSkill3().Range - dc)),0);
        return avgPower / 3 * threatWeight;
    }
    
}
