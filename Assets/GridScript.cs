using UnityEngine;
using UnityEngine.EventSystems;

public class GridScript : MonoBehaviour, IPointerClickHandler
{
    public delegate void onTouchTrigger();

    private Collider collider;
    [SerializeField] private GameObject redHighlight;
    [SerializeField] private GameObject greenHighlight;
    public CharacterScript Occupant;
    private float stayTime;

    public void OnPointerClick(PointerEventData pED)
    {
        MovementController.instance.SetTarget(transform);
        Debug.Log("Clicked");
    }

    public event onTouchTrigger onTouchEvents;

    private void Start()
    {
        collider = gameObject.GetComponent<Collider>();
        redHighlight.SetActive(false);
        greenHighlight.SetActive(false);
    }

    private void Update()
    {
    }

    private void OnCollisionStay(Collision other)
    {
        if (Occupant == null)
        {
            stayTime += Time.deltaTime;
        }

        if (stayTime > 1.5f)
        {
            Occupant = other.gameObject.GetComponent<CharacterScript>();
            stayTime = 0;
            other.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, gameObject.transform.position.y + 0.5f, gameObject.transform.position.z);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.GetComponent<CharacterScript>() == Occupant)
        {
            Occupant = null;
            stayTime = 0;
        }
    }

    public void RedHighlight(bool _show)
    {
        redHighlight.SetActive(_show);
    }
    public void GreenHighlight(bool _show)
    {
        greenHighlight.SetActive(_show);
    }

    public void SetTarget()
    {
        MovementController.instance.SetTarget(transform);
        Debug.Log("Clicked");
    }
}
