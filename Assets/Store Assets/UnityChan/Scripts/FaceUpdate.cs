using UnityEngine;

public class FaceUpdate : MonoBehaviour
{
    private Animator anim;
    public AnimationClip[] animations;

    private float current;

    public float delayWeight;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnGUI()
    {
        foreach (var animation in animations)
            if (GUILayout.Button(animation.name))
                anim.CrossFade(animation.name, 0);
    }


    private void Update()
    {
        if (Input.GetMouseButton(0))
            current = 1;
        else
            current = Mathf.Lerp(current, 0, delayWeight);
        anim.SetLayerWeight(1, current);
    }
}