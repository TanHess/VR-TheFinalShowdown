using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BounceSpring : MonoBehaviour
{
    [SerializeField] LayerMask DesiredMask;
    [SerializeField] Transform AimDirection;
    [SerializeField] int SpringForce = 10;
    [Range(0, 1)] [SerializeField] float CooldownSeconds = 0.5f;
    public UnityEvent BounceTriggered;
    [SerializeField] Collider TriggerCollider;
    [Header("Allows the player's horizontal momentum to me maintained in the bounce")]
    [SerializeField] bool MaintainMomentum;

    private bool Active = true;
    private Rigidbody PlayerBody;

    [SerializeField] Color GizmoColor = Color.red;

    private void OnTriggerEnter(Collider other)
    {
        if ((DesiredMask.value & (1 << other.transform.gameObject.layer)) > 0 && Active) //Look for desired layer
        {
            if (PlayerBody == null)
            {
                PlayerBody = FindObjectOfType<XR_Movement_Controller>().GetComponent<Rigidbody>();
            }

            //If the player is falling onto the spring, cancel it.
           // Vector3 VelTemp = PlayerBody.velocity;
           // VelTemp.y = 0;

            //PlayerBody.velocity = VelTemp;

            if (!MaintainMomentum)
            {
                PlayerBody.velocity = Vector3.zero;
            }
            
            Vector3 StartPos = AimDirection.transform.position;
            //Get the difference between the center of the Play Area
            Vector3 BetweenHeadAndPoint = (other.transform.position - PlayerBody.position);
            //Negate the Y for simplicity
            BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

            //Get the position of the point and the subtract the difference, sending us right on top of the point!
            PlayerBody.position = StartPos - BetweenHeadAndPoint;

            PlayerBody.AddForce(AimDirection.forward * SpringForce, ForceMode.VelocityChange);

            BounceTriggered.Invoke();

            Active = false;
            StartCoroutine(BounceRefresh());
        }
    }

    private IEnumerator BounceRefresh()
    {

        yield return new WaitForSeconds(CooldownSeconds);
        Active = true;
        yield return null;

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;

        if (TriggerCollider is MeshCollider)
        {
            Gizmos.DrawWireMesh((TriggerCollider as MeshCollider).sharedMesh, -1, TriggerCollider.transform.position, TriggerCollider.transform.rotation, TriggerCollider.transform.localScale);
        }

        if (TriggerCollider is BoxCollider)
        {
            Gizmos.matrix = TriggerCollider.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        if (TriggerCollider is SphereCollider)
        {
            Gizmos.matrix = TriggerCollider.transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, (TriggerCollider as SphereCollider).radius);
        }
    }
#endif
}

