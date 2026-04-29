using UnityEngine;

public class GripperPadFriction : MonoBehaviour
{
    [Header("Pad GameObjects (auto-found if left empty)")]
    public Collider leftPad;
    public Collider rightPad;

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
        if (leftPad == null)
            leftPad = FindPadCollider("left_inner_finger_pad");
        if (rightPad == null)
            rightPad = FindPadCollider("right_inner_finger_pad");

        PhysicMaterial rubber = new PhysicMaterial("GripperRubber")
        {
            staticFriction = staticFriction,
            dynamicFriction = dynamicFriction,
            frictionCombine = frictionCombine,
            bounciness = bounciness,
            bounceCombine = PhysicMaterialCombine.Minimum
        };

        ApplyMaterial(leftPad, rubber, "left");
        ApplyMaterial(rightPad, rubber, "right");
    }

    Collider FindPadCollider(string name)
    {
        Transform t = transform.Find(name);
        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    t = child;
                    break;
                }
            }
        }

        if (t == null)
        {
            Debug.LogWarning($"[GripperPadFriction] Could not find '{name}' under {gameObject.name}.");
            return null;
        }

        Collider col = t.GetComponent<Collider>();
        if (col == null)
            Debug.LogWarning($"[GripperPadFriction] '{name}' found but has no Collider.");
        return col;
    }

    void ApplyMaterial(Collider col, PhysicMaterial mat, string label)
    {
        if (col == null) return;
        col.material = mat;
        Debug.Log($"[GripperPadFriction] Applied rubber material to {label} pad (static={staticFriction}, dynamic={dynamicFriction}).");
    }
}
