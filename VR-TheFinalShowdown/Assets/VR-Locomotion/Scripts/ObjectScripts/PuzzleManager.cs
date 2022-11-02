using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private List<XR_Interactable_Lock> Locks;
    private bool PuzzleComplete;
    public UnityEvent PuzzleCompleted;
    public UnityEvent PuzzleUncompleted;

    // Update is called once per frame
    void LateUpdate()
    {
        if (!PuzzleComplete && LockChecker())
        {
            PuzzleComplete = true;
            PuzzleCompleted.Invoke();
        }

        if (PuzzleComplete && !LockChecker())
        {
            PuzzleComplete = false;
            PuzzleUncompleted.Invoke();
        }

    }

    private bool LockChecker()
    {
        bool Correct = true;

        //Locked until proven otherwise
        for (int i = 0; i < Locks.Count; i++)
        {
            if (Locks[i].Locked)
            {
                Correct = false;
            }
        }
        return Correct;
    }
    
}
