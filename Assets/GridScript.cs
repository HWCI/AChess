using UnityEngine;
using UnityEngine.EventSystems;

public class GridScript : MonoBehaviour, IPointerClickHandler
{
    public delegate void onTouchTrigger();

    private Collider collider;

    public void OnPointerClick(PointerEventData pED)
    {
        MovementController.instance.SetTarget(transform);
        Debug.Log("Clicked");
    }

    public event onTouchTrigger onTouchEvents;

    private void Start()
    {
        collider = gameObject.GetComponent<Collider>();
    }

    private void Update()
    {
    }

    public void SetTarget()
    {
        MovementController.instance.SetTarget(transform);
        Debug.Log("Clicked");
    }
}
