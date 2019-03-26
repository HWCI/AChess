using UnityEngine;

[CreateAssetMenu(fileName = "ARReferenceImagesSet", menuName = "UnityARKitPlugin/ARReferenceImagesSet", order = 3)]
public class ARReferenceImagesSet : ScriptableObject
{
    public ARReferenceImage[] referenceImages;

    public string resourceGroupName;
}