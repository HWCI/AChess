using UnityEngine;

[CreateAssetMenu(fileName = "ARReferenceObjectAsset", menuName = "UnityARKitPlugin/ARReferenceObjectAsset", order = 4)]
public class ARReferenceObjectAsset : ScriptableObject
{
    public string objectName;
    public Object referenceObject;
}