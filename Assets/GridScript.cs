using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridScript : MonoBehaviour, IPointerClickHandler
{
	private Collider collider;
	public delegate void onTouchTrigger ();
	public event onTouchTrigger onTouchEvents;

	void Start()
	{
		collider = this.gameObject.GetComponent<Collider>();
	}

	void Update()
	{
		
	}

	public void SetTarget()
	{
		MovementController.instance.SetTarget(this.transform);
		Debug.Log("Clicked");
	}

	public void OnPointerClick(PointerEventData pED)
	{
		MovementController.instance.SetTarget(this.transform);
		Debug.Log("Clicked");
	}
}
