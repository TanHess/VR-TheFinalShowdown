using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WaterVolume : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> RigidBodyList;
    [SerializeField] private List<float> TempDragList;
    [SerializeField] private List<float> TempAngDragList;

    [SerializeField] private List<float> VolumesList;
    [SerializeField] private List<float> DensitiesList;
    [SerializeField] private List<float> EntryPointsList;

    [SerializeField] LayerMask ObjectsToReact;
    [SerializeField] Rigidbody WaterPhysicsProfile;

    [SerializeField] float WaterDensity = 9.97f;
    [SerializeField] float WaterDrag = 0.75f;

    private void OnTriggerEnter(Collider other)
    {
        if (!RigidBodyList.Contains(other.GetComponent<Rigidbody>()))
        {
            if (((1 << other.gameObject.layer) & ObjectsToReact) != 0)
            {
                Rigidbody CurrentRB = other.GetComponent<Rigidbody>();
                TempDragList.Add(CurrentRB.drag);
                TempAngDragList.Add(CurrentRB.angularDrag);
                RigidBodyList.Add(CurrentRB);

                CurrentRB.drag = WaterDrag;
                CurrentRB.angularDrag = WaterDrag;

                //Fetch the size of the Collider volume
                Vector3 BoundsSize = other.bounds.size;
                float volume = BoundsSize.x * BoundsSize.y * BoundsSize.z;
              //  float density = CurrentRB.mass / volume;
                float EntryPointY = CurrentRB.position.y - (BoundsSize.y/2);
                VolumesList.Add(volume);
              //  DensitiesList.Add(density);
                EntryPointsList.Add(EntryPointY);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (RigidBodyList.Contains(other.GetComponent<Rigidbody>()))
        {
            Rigidbody CurrentRB = other.GetComponent<Rigidbody>();
            
            CurrentRB.drag = TempDragList[RigidBodyList.IndexOf(CurrentRB)];
            CurrentRB.angularDrag = TempAngDragList[RigidBodyList.IndexOf(CurrentRB)];

            TempDragList.RemoveAt(RigidBodyList.IndexOf(CurrentRB));
            TempAngDragList.RemoveAt(RigidBodyList.IndexOf(CurrentRB));
            VolumesList.RemoveAt(RigidBodyList.IndexOf(CurrentRB));
         //   DensitiesList.RemoveAt(RigidBodyList.IndexOf(CurrentRB));
            RigidBodyList.Remove(CurrentRB);
            
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Rigidbody Body in RigidBodyList)
        {
            float DistanceFromSurface = ((EntryPointsList[RigidBodyList.IndexOf(Body)]) - (Body.position.y));
            DistanceFromSurface = Mathf.Clamp(DistanceFromSurface, 0, 2);
            Body.AddForce(-Vector3.up * Physics.gravity.y * VolumesList[RigidBodyList.IndexOf(Body)] * DistanceFromSurface * WaterDensity, ForceMode.Force);
        }
    }
}
