using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class CharacterScript : MonoBehaviour, IPointerClickHandler
{

	private Transform _transform;
	private Animator _anim;
	private NavMeshAgent navAgent;
	// Use this for initialization
	void Start ()
	{
		this.navAgent = this.gameObject.GetComponent<NavMeshAgent>();
		_transform = this.gameObject.GetComponent<Transform>();
		_anim = this.gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		_anim.SetFloat("Speed", navAgent.velocity.magnitude);
		if (_anim.GetFloat("Speed") <= 0.1)
		{
			_anim.SetBool("Rest", true);
		}
		else
		{
			_anim.SetBool("Rest", false);
		}
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
	public void OnPointerClick(PointerEventData pED)
	{
		MovementController.instance.SetChara(this);
		Debug.Log("Clicked");
	}

	public void Move(Vector3 location)
	{
		navAgent.destination = location;
	}
}
