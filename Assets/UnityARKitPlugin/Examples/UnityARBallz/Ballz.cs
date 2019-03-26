using UnityEngine;

public class Ballz : MonoBehaviour
{
    private float startingY;

    public float yDistanceThreshold;

    // Use this for initialization
    private void Start()
    {
        startingY = transform.position.y;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Mathf.Abs(startingY - transform.position.y) > yDistanceThreshold) Destroy(gameObject);
    }
}