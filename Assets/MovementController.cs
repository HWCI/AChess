using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class MovementController : MonoBehaviour
{
    public enum Selection
    {
        Character,
        Grid,
        Target
    }

    public static MovementController instance;
    private enum buttonType
    {
        Skill1,
        Skill2,
        Skill3,
        Scene
    } 

    public CharacterScript _chara;

    public Transform _target;
    public GameObject Trail;
    private GameObject _trail;
    private buttonType prev;

    // Use this for initialization
    private void Start()
    {
        if (instance == null) instance = this;

        if (instance != this) DestroyImmediate(gameObject);
        
    }

    // Update is called once per frame
    private void Update()
    {
#if UNITY_EDITOR //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
        
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Vector3 inputpos;
                inputpos = Input.mousePosition;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(inputpos), out hit, 100))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    Interact(hit);
                }
            }
        
    }


#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                /*if (touch.phase == TouchPhase.Began)
                {
                    var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                    ARPoint point = new ARPoint {
                        x = screenPosition.x,
                        y = screenPosition.y
                    };
                            
                    List<ARHitTestResult> hitResults =
     UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, 
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
                    if (hitResults.Count > 0) {
                        foreach (var hitResult in hitResults) {
                            Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.gameObject.CompareTag("Grid"))
                        if (_chara != null)
                        {
                            if (_target == hit.transform)
                            {
                                _chara.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                                Destroy(_trail);
                            }
                            else
                            {
                                _target = hit.transform;
                                if (_trail != null)
                                {
                                    Destroy(_trail);
                                }
                                _trail = Instantiate(Trail, _chara.transform.position, Quaternion.identity);
                                _trail.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                            }
                        }
    
                    if (hit.collider.gameObject.CompareTag("Player"))
                        _chara = hit.transform.GetComponent<CharacterScript>();
                }
                        }
                    }*/
                RaycastHit hit;
                Vector3 inputpos;
                inputpos = touch.position;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(inputpos), out hit, 100))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    Interact(hit);

                }
            }
		}
#endif 
    private void Interact(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("Grid"))
            if (_chara != null)
            {
                if (_target == hit.transform)
                {
                    _target.GetComponent<GridScript>().GreenHighlight(false);
                    _chara.PlayerMove(hit);
                    Destroy(_trail);
                    _target = null;
                }
                else
                {
                    _target.GetComponent<GridScript>().GreenHighlight(false);
                    _target = hit.transform;
                    _target.GetComponent<GridScript>().GreenHighlight(true);
                    if (_trail != null)
                    {
                        Destroy(_trail);
                    }

                    _trail = Instantiate(Trail, _chara.transform.position, Quaternion.identity);
                    _trail.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                }
            }

        if (hit.collider.gameObject.CompareTag("Player"))
        {
            _chara = hit.transform.GetComponent<CharacterScript>();
            GameManager.instance.gameStage = GameManager.GameState.PlayerEmpty;
            GameManager.instance.gameStage = GameManager.GameState.PlayerSelect;
        }
    }

    public void ReleaseTarget()
    {
        _chara = null;
        GameManager.instance.gameStage = GameManager.GameState.PlayerEmpty;
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

    public void CastSkill1()
    {
        if (prev != buttonType.Skill1)
        {
            switch (_chara.GetSkill1().Type)
            {
                case SkillType.Target:
                    _target.GetComponent<GridScript>().GreenHighlight(false);
                    _target.GetComponent<GridScript>().RedHighlight(true);
                    prev = buttonType.Skill1;
                    break;
                    //case SkillType.Directed:
                    _target.GetComponent<GridScript>().GreenHighlight(false);
                    _target.GetComponent<GridScript>().RedHighlight(true);
                    break;
                case SkillType.AOE:
                    _target.GetComponent<GridScript>().GreenHighlight(false);
                    _target.GetComponent<GridScript>().RedHighlight(true);
                    prev = buttonType.Skill1;
                    break;
            }
        }
        else
        {
            if(_target.GetComponent<GridScript>().Occupant != null)
            _chara.Skill1(_target.GetComponent<GridScript>().Occupant);
        }
    }
    public void CastSkill2()
    {
    }
    public void CastSkill3()
    {
    }

    public void Move()
    {
        if (_chara != null && _target != null) _chara.Move(_target);
    }
}