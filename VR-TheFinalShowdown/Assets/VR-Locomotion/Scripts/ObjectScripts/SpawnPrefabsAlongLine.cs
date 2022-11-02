using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnPrefabsAlongLine : MonoBehaviour
{
    [SerializeField] private Transform StartPoint;
    [SerializeField] private Transform EndPoint;
    [SerializeField] GameObject ObjectToSpawn;
    [SerializeField] float SpawnsPerMeter = 1;
    [SerializeField] Transform ObjectParent;

    private int SegmentsToCreate;
    private float Distance;
    private float LerpValue;
    private Vector3 PreviousPos;

    void Awake()
    {
        SpawnObjects(ObjectToSpawn);
    }

    void SpawnObjects(GameObject Object)
    {
        //Here we calculate how many segments will fit between the two points
        SegmentsToCreate = Mathf.RoundToInt(((Vector3.Distance(StartPoint.position, EndPoint.position)*(SpawnsPerMeter * 0.5f)) / 0.5f));

        //As we'll be using vector3.lerp we want a value between 0 and 1, and the distance value is the value we have to add
        Distance = 1.0f / SegmentsToCreate;
        for (int i = 0; i < SegmentsToCreate; i++)
        {
            //We increase our lerpValue
            LerpValue += Distance;

            GameObject Obj = Instantiate(ObjectToSpawn, ObjectParent, true);
            Obj.transform.position = Vector3.Lerp(StartPoint.position, EndPoint.position, LerpValue);

            if (Obj.transform.position == PreviousPos)
            {
                Destroy(Obj);
            }
            else
            {
                PreviousPos = Obj.transform.position;
            }
            


        }
    }
}
