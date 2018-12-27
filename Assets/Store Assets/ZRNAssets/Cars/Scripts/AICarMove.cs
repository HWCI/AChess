using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AICarMove : MonoBehaviour
{
    private const float CAR_SPEED_MAX = 1.0f;

    private NavMeshAgent navMeshAgentCompornent;

    private Vector3 startPos;
    private Vector3 startRot;

    [SerializeField] private GameObject targetAICar;

    private int targetNavMeshObjectCounts;
    private int targetNavMeshObjectNow;

    [SerializeField] private GameObject[] targetNavMeshObjects;

    // Use this for initialization
    private void Start()
    {
        navMeshAgentCompornent = GetComponent<NavMeshAgent>();
        startPos = targetNavMeshObjects[0].transform.localPosition;
        startRot = targetNavMeshObjects[0].transform.localEulerAngles;
        targetNavMeshObjectCounts = targetNavMeshObjects.Length - 1;
    }

    public void InitAICar()
    {
        navMeshAgentCompornent.speed = 0.0f;
        targetAICar.GetComponent<Animation>().Play("00_Stop");
        StartCoroutine(startCar(3.0f));
    }

    private IEnumerator startCar(float startDelayTime)
    {
        navMeshAgentCompornent.speed = 0.0f;
        targetAICar.GetComponent<Animation>().Play("00_Stop");
        yield return new WaitForSeconds(startDelayTime);

        // Set destination
        targetNavMeshObjectNow = 1;
        navMeshAgentCompornent.SetDestination(targetNavMeshObjects[targetNavMeshObjectNow].transform.localPosition);
        transform.localPosition = startPos;
        transform.localEulerAngles = startRot;

        yield return new WaitForSeconds(0.5f);
        navMeshAgentCompornent.speed = CAR_SPEED_MAX;
        targetAICar.GetComponent<Animation>().Play("01_Run");
    }


    // Update is called once per frame
    private void Update()
    {
        if (navMeshAgentCompornent.remainingDistance < 0.1f)
        {
            targetNavMeshObjectNow++;
            if (targetNavMeshObjectNow <= targetNavMeshObjectCounts)
            {
                navMeshAgentCompornent.SetDestination(targetNavMeshObjects[targetNavMeshObjectNow].transform
                    .localPosition);
            }
            else if (targetNavMeshObjectNow > targetNavMeshObjectCounts)
            {
                targetNavMeshObjectNow = 1;
                navMeshAgentCompornent.SetDestination(targetNavMeshObjects[targetNavMeshObjectNow].transform
                    .localPosition);
            }
        }
    }
}