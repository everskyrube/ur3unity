using UnityEngine;

public class StickyPad : MonoBehaviour
{
    [Tooltip("The other pad — drag the other pad cube here")]
    public StickyPad otherPad;

    [Tooltip("Half-extents of the detection zone around the pad")]
    public Vector3 detectSize = new Vector3(0.015f, 0.035f, 0.005f);

    [Tooltip("Max distance between the two pads to consider the gripper closed")]
    public float closedDistance = 0.06f;

    [Tooltip("Distance between pads to release the object (should be > closedDistance)")]
    public float openDistance = 0.08f;

    [HideInInspector] public bool isGripping;

    private GameObject grippedObject;
    private Rigidbody grippedRb;
    private bool wasKinematic;
    private bool wasGravity;
    private Vector3 localOffset;
    private Quaternion localRotOffset;

    float PadDistance()
    {
        if (otherPad == null) return 0f;
        return Vector3.Distance(transform.position, otherPad.transform.position);
    }

    void FixedUpdate()
    {
        if (grippedObject == null)
        {
            if (otherPad != null && otherPad.isGripping) return;
            if (PadDistance() > closedDistance) return;

            Collider[] hits = Physics.OverlapBox(
                transform.position, detectSize, transform.rotation);

            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform)) continue;
                if (hit.GetComponentInParent<ArticulationBody>() != null) continue;

                Grip(hit.gameObject);
                break;
            }
        }
        else
        {
            grippedObject.transform.position =
                transform.TransformPoint(localOffset);
            grippedObject.transform.rotation =
                transform.rotation * localRotOffset;

            if (PadDistance() > openDistance)
                Release();
        }
    }

    void Grip(GameObject target)
    {
        grippedRb = target.GetComponentInParent<Rigidbody>();

        if (grippedRb != null)
        {
            wasKinematic = grippedRb.isKinematic;
            wasGravity = grippedRb.useGravity;
            grippedRb.isKinematic = true;
            grippedRb.useGravity = false;
            grippedObject = grippedRb.gameObject;
        }
        else
        {
            grippedObject = target;
        }

        localOffset = transform.InverseTransformPoint(grippedObject.transform.position);
        localRotOffset = Quaternion.Inverse(transform.rotation) * grippedObject.transform.rotation;
        isGripping = true;

        Debug.Log($"[StickyPad] Gripped '{grippedObject.name}'");
    }

    void Release()
    {
        if (grippedObject == null) return;

        if (grippedRb != null)
        {
            grippedRb.isKinematic = wasKinematic;
            grippedRb.useGravity = wasGravity;
            grippedRb.velocity = Vector3.zero;
            grippedRb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"[StickyPad] Released '{grippedObject.name}'");
        grippedObject = null;
        grippedRb = null;
        isGripping = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectSize * 2f);
    }
}
