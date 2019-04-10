using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneHider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.ARStateChange += HidePlanes;
    }

    public void HidePlanes()
    {
        if (GameManager.instance.state == GameManager.ARState.ScanPlane)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
