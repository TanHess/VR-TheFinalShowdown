using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class Player_Spawn_Manager : MonoBehaviour
{
    [SerializeField] Rigidbody PlayerBody;
    [Header("We use this to perfectly spawn the player, regardless of playspace size!")]
    [SerializeField] Transform HeadTransform;
    [Header("Use these to stop climbing upon respawn")]
    [SerializeField] XR_Climb ClimbComponent;
    [SerializeField] XRDirectInteractor LeftInteractor;
    [SerializeField] XRDirectInteractor RightInteractor;
    [SerializeField] private Player_Spawn_Location[] SpawnLocations;

    [SerializeField] Color SpawnFadeColor = Color.black;
    [SerializeField] float SpawnFadeTime = 0.75f;
    
    private static Player_Spawn_Manager instance;
    private bool Spawning;
    [SerializeField] private bool SceneLoading;
    private void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " was loaded. Spawn player");
        SceneLoading = true;

        SpawnPlayer(0);

        //If we're using single scenes, don't sweat the loading lock
        if (SceneManager.sceneCount == 1)
        {
            SceneLoading = false;
        }

    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log(scene.name + " was unloaded. Reset player");
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        PlayerBody.isKinematic = true;
        
        PlayerBody.position = Vector3.zero;

        PlayerBody.isKinematic = false;

    }

    public static void SpawnPlayer(float DelayTime)
    {
        if (!instance.Spawning)
        {
            instance.Spawning = true;
            //Instancing allows calling a coroutine inside a static function
            instance.StartCoroutine(instance.SpawnPlayerRoutine(DelayTime));
        }

    }

    private IEnumerator SpawnPlayerRoutine(float DelayTime)
    {
        PlayerBody.isKinematic = true;
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(DelayTime);
        
        //Fade before spawning
        if (!SceneLoading) // We don't do it if the Scene is loading because that has its own fading routine.
        {
            ColorFilter_Controller.FadePanelChange(SpawnFadeColor, SpawnFadeTime, true);
            // Spawning = true;
            yield return new WaitForSeconds(SpawnFadeTime);
        }
        //Stop Climbing if applicable
        ClimbComponent.EndClimbing(RightInteractor);
        ClimbComponent.EndClimbing(LeftInteractor);

        SpawnLocations = FindObjectsOfType<Player_Spawn_Location>();

        if (SpawnLocations.Length < 1)
        {
            Debug.LogWarning("We don't have any Player spawn points. We'll just put them at (0,0,0)");
            transform.position = Vector3.zero;
            yield return null;
        }

        foreach (Player_Spawn_Location SpawnPoint in SpawnLocations)
        {
            if (SpawnPoint.CurrentSpawnPoint) //You've gotta have the spawn point be marked as the current one.
            {
                //Get the difference between the center of the Play Area
                Vector3 BetweenHeadAndPoint = (HeadTransform.position - PlayerBody.position);
                //Negate the Y for simplicity
                BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

                //Get the position of our player point and the subtract the difference, sending us right on top of the point!
                PlayerBody.position = SpawnPoint.transform.position - BetweenHeadAndPoint;

                Debug.Log("Spawned player at " + SpawnPoint.transform.name);
            }
        }

        if (!SceneLoading)// We don't do it if the Scene is loading because that has its own fading routine.
        {
            //Fade back to normal
            ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, SpawnFadeTime, false);
            yield return new WaitForSeconds(SpawnFadeTime);
            
        }
        Spawning = false;

        PlayerBody.isKinematic = false;
        if (PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded)
        {
            PlayerBody.useGravity = true;
        }
        
        SceneLoading = false;

        yield return null;

    }
}
