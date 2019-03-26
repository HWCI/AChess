using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class CharacterScript : MonoBehaviour, IPointerClickHandler
{
    private Animator _anim;

    private Transform _transform;
    private NavMeshAgent navAgent;

    public void OnPointerClick(PointerEventData pED)
    {
        MovementController.instance.SetChara(this);
        Debug.Log("Clicked");
    }

    // Use this for initialization
    private void Start()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        _transform = gameObject.GetComponent<Transform>();
        _anim = gameObject.GetComponent<Animator>();
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

    public void Skill1()
    {
    }

    public void Skill2()
    {
    }

    public void Skill3()
    {
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