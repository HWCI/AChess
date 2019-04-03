using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[System.Serializable]
public struct CharacterConfig
{
    public GameObject CharacterModel;
    public Skill Skill1;
    public Skill Skill2;
    public Skill Skill3;
    public int Health;
    public int Actions;
}
public class CharacterScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CharacterConfig character;
    private Animator _anim;
    private Transform _transform;
    private NavMeshAgent navAgent;
    private int health;
    private int action;

    public void OnPointerClick(PointerEventData pED)
    {
        SetChara();
        Debug.Log("Clicked");
    }

    // Use this for initialization
    private void Start()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        _transform = gameObject.GetComponent<Transform>();
        _anim = gameObject.GetComponent<Animator>();
        ReInit();
    }

    private void ReInit()
    {
        health = character.Health;
        action = character.Actions;
    }

    // Update is called once per frame
    private void Update()
    {
        _anim.SetFloat("Speed", navAgent.velocity.magnitude);
        if (navAgent.velocity.y > 0.11)
        {
            _anim.SetBool("Jump", true);
        }
        else
        {
            _anim.SetBool("Jump", false);
        }
        if (_anim.GetFloat("Speed") <= 0.1)
            _anim.SetBool("Rest", true);
        else
            _anim.SetBool("Rest", false);
    }

    public void Move(Transform target)
    {
        navAgent.Move(target.position);
    }

    public void Skill1(CharacterScript target)
    {
        if (action > 0)
        {
            character.Skill1.Cast(target);
            action--;
        }
        else
        {
            
        }
    }

    public void Skill2(CharacterScript target)
    {
        if (action > 0)
        {
            character.Skill2.Cast(target);
            action--;
        }
    }

    public void Skill3(CharacterScript target)
    {
        if (action > 0)
        {
            character.Skill3.Cast(target);
            action--;
        }
    }

    public string GetSkill1Name()
    {
        return character.Skill1.name;
    }
    public string GetSkill2Name()
    {
        return character.Skill2.name;
    }
    public string GetSkill3Name()
    {
        return character.Skill3.name;
    }

    public void Heal(int hp)
    {
        health += hp;
    }
    public void Damage(int hp)
    {
        health -= hp;
    }

    public void SetChara()
    {
        MovementController.instance.SetChara(this);
        Debug.Log("Clicked");
    }

    public void Move(Vector3 location)
    {
        navAgent.destination = location;
    }
}