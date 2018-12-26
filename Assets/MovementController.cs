using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{

	public static MovementController instance;

	public CharacterScript _chara;

	public Transform _target;
	// Use this for initialization
	void Start ()
	{
		if (instance == null)
		{
			instance = this;
		}
		if (instance != this)
		{
			DestroyImmediate(this.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
                
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200)) {
				Debug.Log(hit.collider.gameObject.name);
				if (hit.collider.gameObject.CompareTag("Grid"))
				{
					if (_chara != null)
					{
						_chara.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
					}
				}
				if (hit.collider.gameObject.CompareTag("Player"))
				{
					_chara = hit.transform.GetComponent<CharacterScript>();
				}
			}
		}
	}

	public void SetChara(CharacterScript chara)
	{
		_chara = chara;
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		Move();
	}

	public void Move()
	{
		if (_chara != null && _target != null)
		{
			_chara.Move(_target);
		}
	}
}
