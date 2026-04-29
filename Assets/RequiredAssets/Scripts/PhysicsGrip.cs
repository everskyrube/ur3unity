using UnityEngine;

public class PhysicsGrip : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The other pad — used to compute the inward squeeze direction")]
    public PhysicsGrip otherPad;

    [Header("Friction")]
    [Range(0f, 2f)] public float staticFriction = 2f;
    [Range(0f, 2f)] public float dynamicFriction = 1.8f;
    [Tooltip("Also apply the high-friction material to the gripped object's collider")]
    public bool boostObjectFriction = true;

    [Header("Detection Zone")]
    [Tooltip("Half-extents of the box where we look for objects between the pads")]
    public Vector3 detectSize = new Vector3(0.015f, 0.035f, 0.01f);

    [Header("Grip Force")]
    [Tooltip("Squeeze force applied on the cube toward this pad (Newtons). Only applied when BOTH pads are in contact with the same object.")]
    public float gripForce = 5f;

    [Tooltip("Only apply force when pads are closer than this (meters)")]
    public float maxPadDistance = 0.10f;

    [HideInInspector] public Rigidbody contactedBody;

    private PhysicMaterial rubber;
    private Collider lastBoosted;

    void Start()
    {
        rubber = new PhysicMaterial("GripperRubber")
        {
            staticFriction = staticFriction,
            dynamicFriction = dynamicFriction,
            frictionCombine = PhysicMaterialCombine.Maximum,
            bounciness = 0f,
            bounceCombine = PhysicMaterialCombine.Minimum
        };

        Collider col = GetComponent<Collider>();
        if (col != null) col.material = rubber;
    }

    void FixedUpdate()
    {
        contactedBody = null;
        if (otherPad == null) return;

        float padDist = Vector3.Distance(transform.position, otherPad.transform.position);
        if (padDist > maxPadDistance) return;

        Collider[] hits = Physics.OverlapBox(
            transform.position, detectSize, transform.rotation);

        Collider targetCol = null;
        Rigidbody targetRb = null;
        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;
            if (hit.GetComponentInParent<ArticulationBody>() != null) continue;

            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            targetCol = hit;
            targetRb = rb;
            break;
        }

        contactedBody = targetRb;

        if (targetRb == null) return;

        if (otherPad.contactedBody != targetRb) return;

        Vector3 toPad = transform.position - targetRb.worldCenterOfMass;
        if (toPad.sqrMagnitude < 1e-10f) return;

        targetRb.AddForce(toPad.normalized * gripForce, ForceMode.Force);

        if (boostObjectFriction && targetCol != lastBoosted)
        {
            targetCol.material = rubber;
            lastBoosted = targetCol;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectSize * 2f);
    }
}
