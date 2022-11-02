using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class KeyItem_Object : MonoBehaviour
{
    [SerializeField] float RespawnDistance = 25.0f;
    [SerializeField] XRGrabInteractable ThisGrabbable;
    [Header("Destroys duplicates upon revisiting scene.")]
    public bool DestroyDuplicates = true;

    public bool Found;
    private bool Lost;
    private LostAndFound LostAndFound;
    private XROrigin PlayerRig;
    private KeyItem_Manager ItemManager;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        ThisGrabbable.selectEntered.AddListener(RegisterAsFound);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        ThisGrabbable.selectEntered.RemoveListener(RegisterAsFound);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LostAndFound = FindObjectOfType<LostAndFound>(); //Find the Lost and Found so it can be reset
        PlayerRig = FindObjectOfType<XROrigin>(); //Find the player to detect when it's too far from them
        ItemManager = FindObjectOfType<KeyItem_Manager>(); //Find the Key Item Manager to parent to, to make it part of the Persistent scene
    }
    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log(scene.name + " was unloaded. Reset " + this.name);
        ResetObject();
    }

    public void ResetObject()
    {
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        if (!ThisGrabbable.isSelected)
        {
            LostAndFound.RetrieveItem(ThisGrabbable);
            Lost = false;
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        //Will only respawn if the player has found it first
        if (Vector3.Distance(PlayerRig.transform.position, transform.position) > RespawnDistance && !Lost && Found && !ThisGrabbable.isSelected)
        {
            Lost = true;
            ResetObject();
        }
    }

    private void RegisterAsFound(SelectEnterEventArgs args)
    {
        Lost = false;
        //We have grabbed the item at least once
        Found = true;
        
        ThisGrabbable.retainTransformParent = false;
        transform.parent = ItemManager.transform; // If we've found this object, then it'll be safe inside the Key Item Manager
    }
}
