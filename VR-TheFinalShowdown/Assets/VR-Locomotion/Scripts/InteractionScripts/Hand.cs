using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
    public List<Rigidbody> contactInteractables = new List<Rigidbody>();
    public InputActionReference pickup;
    public InputActionReference activate;
    public ConfigurableJoint handJoint;
    public Hand otherHand;
    public Rigidbody handRB;
    [Range(0, 10000)] public float handSpeedMultiplier = 1;
    [Range(0, 100)] public float handAngularSpeedMultiplier = 100;
    [Range(0, 10000)] public float damper = 0;
    private Vector3 wrongVelComp;
    public bool slowMotion;
    [Range(0, 10000)] public float slowHandSpeedMultiplier = 1;
    [Range(0, 10)] public float dropAmplifier = 1;
    public Vector3[] handVelocities = new Vector3[5];
    public Vector3[] handAngularVelocities = new Vector3[5];
    public int handVelocityCounter;


    private void FixedUpdate()
    {
        if (handVelocityCounter < handVelocities.Length)
        {
            handVelocities[handVelocityCounter] = handRB.velocity;
            handAngularVelocities[handVelocityCounter] = handRB.angularVelocity;
            handVelocityCounter++;
        }
        else
        {
            handVelocityCounter = 0;
            handVelocities[handVelocityCounter] = handRB.velocity;
            handAngularVelocities[handVelocityCounter] = handRB.angularVelocity;
        }

        if (slowMotion == false)
        {
            wrongVelComp = Vector3.ProjectOnPlane(handRB.velocity, (transform.position - handRB.transform.position));
            handRB.AddForce((transform.position - handRB.position) * handSpeedMultiplier, ForceMode.VelocityChange);
            handRB.AddForce((-wrongVelComp * handRB.mass) * damper, ForceMode.VelocityChange);

            Quaternion deltaRot = transform.rotation * Quaternion.Inverse(handRB.rotation);
            deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
            axis.Normalize();
            angle = ((angle % 360) + 360) % 360;
            if (angle > 180) angle = -(360f - angle);
            Vector3 generatedTorque = angle * axis;
            generatedTorque += (-handRB.angularVelocity);
            handRB.maxAngularVelocity = 30f;
            if (angle < -0.01f || angle > 0.01f)
            {
                handRB.AddTorque(generatedTorque * 1f, ForceMode.VelocityChange);
            }
        }
        else
        {
            wrongVelComp = Vector3.ProjectOnPlane(handRB.velocity, (transform.position - handRB.transform.position));
            handRB.AddForce((transform.position - handRB.position) * slowHandSpeedMultiplier, ForceMode.VelocityChange);
            handRB.AddForce((-wrongVelComp * handRB.mass), ForceMode.VelocityChange);

            Quaternion deltaRot = transform.rotation * Quaternion.Inverse(handRB.rotation);
            deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
            axis.Normalize();
            angle = ((angle % 360) + 360) % 360;
            if (angle > 180) angle = -(360f - angle);
            Vector3 generatedTorque = angle * axis;
            handRB.maxAngularVelocity = float.MaxValue;
            if (angle < -0.01f || angle > 0.01f)
            {
                handRB.AddTorque(generatedTorque * handAngularSpeedMultiplier, ForceMode.VelocityChange);
            }
        }

        if (pickup.action.triggered == true)
        {
            doPickup();
        }

        if (pickup.action.ReadValue<float>() < 0.5f)
        {
            doDrop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Interactable") || other.tag.Contains("Interactable2"))
        {
            Rigidbody interactableRB = other.GetComponent<Rigidbody>();
            if (interactableRB != null)
            {
                if (!contactInteractables.Contains(interactableRB))
                {
                    contactInteractables.Add(interactableRB);
                }
            }
            else
            {
                Debug.LogWarning("Interactable " + other.name + " is missing a Rigidbody");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("Interactable") || other.tag.Contains("Interactable2"))
        {
            Rigidbody interactableRB = other.GetComponent<Rigidbody>();
            if (contactInteractables.Contains(interactableRB))
            {
                contactInteractables.Remove(interactableRB);
            }
        }
    }
    private void doPickup()
    {
        if (contactInteractables.Count > 0)
        {
            Rigidbody selectedInteractable = findNearest();
            if (otherHand.handJoint.connectedBody == selectedInteractable)
            {
                if (selectedInteractable.tag != "2HInteractable")
                {
                    Debug.Log("DROP");
                    otherHand.doDrop();
                }
            }
            if (selectedInteractable.tag == "Interactable2")
            {
                selectedInteractable.SendMessage("doOffset", SendMessageOptions.DontRequireReceiver);
            }
            selectedInteractable.SendMessage("doAction", activate, SendMessageOptions.DontRequireReceiver);
            lockJoint();
            handJoint.connectedBody = selectedInteractable;
            Debug.Log("GRABBED");
        }
    }

    public void doDrop()
    {
        if (handJoint.connectedBody != null)
        {
            Rigidbody droppedObject = handJoint.connectedBody;
            Vector3 combinedVelocity = Vector3.zero;
            Vector3 combinedAngularVelocity = Vector3.zero;
            handJoint.connectedBody = null;
            for (int i = 0; i < handVelocities.Length; i++)
            {
                combinedVelocity += handVelocities[i];
                combinedAngularVelocity += handAngularVelocities[i];
            }
            combinedVelocity /= handVelocities.Length;
            combinedAngularVelocity /= handAngularVelocities.Length;
            droppedObject.velocity = combinedVelocity * dropAmplifier;
            droppedObject.angularVelocity = combinedAngularVelocity;
            unlockJoint();
        }
    }

    private void lockJoint()
    {
        handJoint.xMotion = ConfigurableJointMotion.Locked;
        handJoint.yMotion = ConfigurableJointMotion.Locked;
        handJoint.zMotion = ConfigurableJointMotion.Locked;
        handJoint.angularXMotion = ConfigurableJointMotion.Locked;
        handJoint.angularYMotion = ConfigurableJointMotion.Locked;
        handJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }
    private void unlockJoint()
    {
        handJoint.xMotion = ConfigurableJointMotion.Free;
        handJoint.yMotion = ConfigurableJointMotion.Free;
        handJoint.zMotion = ConfigurableJointMotion.Free;
        handJoint.angularXMotion = ConfigurableJointMotion.Free;
        handJoint.angularYMotion = ConfigurableJointMotion.Free;
        handJoint.angularZMotion = ConfigurableJointMotion.Free;
    }

    private Rigidbody findNearest()
    {
        Rigidbody nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0f;

        foreach (Rigidbody interactable in contactInteractables)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }

        return nearest;
    }
}