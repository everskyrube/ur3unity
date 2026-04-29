using UnityEngine;

public class PadFriction : MonoBehaviour
{
    [Header("Friction Settings")]
    [Range(0f, 2f)]
    public float staticFriction = 1.5f;
    [Range(0f, 2f)]
    public float dynamicFriction = 1.2f;
    public PhysicMaterialCombine frictionCombine = PhysicMaterialCombine.Maximum;
    [Range(0f, 1f)]
    public float bounciness = 0f;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"[PadFriction] No Collider found on '{gameObject.name}'.");
            return;
        }

        col.material = new PhysicMaterial("GripperRubber")
        {
            staticFriction = staticFriction,
            dynamicFriction = dynamicFriction,
            frictionCombine = frictionCombine,
            bounciness = bounciness,
            bounceCombine = PhysicMaterialCombine.Minimum
        };

        Debug.Log($"[PadFriction] Applied rubber material to '{gameObject.name}' (static={staticFriction}, dynamic={dynamicFriction}).");
    }
}
