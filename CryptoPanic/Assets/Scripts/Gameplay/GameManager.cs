using UnityEngine;
using System.IO;
using System.Collections;

public class GameManager : MonoBehaviour
{
    const int NUM_CIPHERS_TO_CREATE = 10;
    const int NUM_DUDS_TO_CREATE = 90;

    [HideInInspector]
    public static GameManager instance = null;

    [Tooltip("Prefab of the GameObject to be spawned in the game as a CipherTarget")]
    public GameObject cipherTargetPrefab;

    [Tooltip("Prefab of the GameObject to be spawned in the game as a DudTarget")]
    public GameObject dudTargetPrefab;

    private bool isPaused = false;
    private int numCiphersShot, numDudsShot;

    private void distributeTargets()
    {
        //TODO
    }

    protected void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(this.gameObject);

        numCiphersShot = 0;
        numDudsShot = 0;
    }

    // Update is called once per frame
    protected void Update()
    {
    }

    public bool getGamePauseState() { return isPaused; }
    public void toggleGamePauseState() { isPaused = !isPaused; }
}
