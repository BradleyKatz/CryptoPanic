using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using XInputDotNetPure;

public class PlayerControls : MonoBehaviour
{
    public static PlayerControls instance = null;
    public bool waitingToExit = false;

    private const int MAX_BULLETS = 5;
    private const int PLAYER_ONE = 1;
    private const int PLAYER_TWO = 2;

    private Vector3 P1_START_POS = new Vector3(-10, 0, 0);
    private Vector3 P2_START_POS = new Vector3(10, 0, 0);

    private int numP1Bullets, numP2Bullets;
    public bool p1IsShooting = false, p2IsShooting = false;

    public GameObject p1CrosshairPrefab, p2CrosshairPrefab;
    private GameObject p1Crosshair, p2Crosshair;
    public AudioSource p1Audio, p2Audio;
    public AudioClip emptyClipSound, reloadSound, shootSound;

    private bool playerIndexSet = false;
    private PlayerIndex playerControllerIndex;
    private GamePadState currentState, previousState;

    private void initializeController()
    {
        if (!playerIndexSet || !previousState.IsConnected)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);

                if (testState.IsConnected)
                {
                    playerControllerIndex = testPlayerIndex;
                    playerIndexSet = true;
                }
            }
        }
    }

    public void initializePlayerCrosshairs()
    {
        if (GameVariables.IsMultiplayerSession)
        {
            p1Crosshair = (GameObject)Instantiate(p1CrosshairPrefab, P1_START_POS, Quaternion.identity);
            p2Crosshair = (GameObject)Instantiate(p2CrosshairPrefab, P2_START_POS, Quaternion.identity);
            p1Crosshair.layer = 9;
            p2Crosshair.layer = 9;

            numP1Bullets = MAX_BULLETS;
            numP2Bullets = MAX_BULLETS;
        }
        else
        {
            p1Crosshair = (GameObject) Instantiate(p1CrosshairPrefab, P1_START_POS, Quaternion.identity);
            p1Crosshair.layer = 9;

            numP1Bullets = MAX_BULLETS;
        }
    }

    private void playP1SoundEffect(AudioClip clipToPlay)
    {
        p1Audio.PlayOneShot(clipToPlay);
    }

    private void playP2SoundEffect(AudioClip clipToPlay)
    {
        p2Audio.PlayOneShot(clipToPlay);
    }

    void reloadBullets(int playerNum)
    {
        switch(playerNum)
        {
            case PLAYER_ONE:
                if (numP1Bullets < MAX_BULLETS)
                {
                    numP1Bullets = MAX_BULLETS;
                    playP1SoundEffect(reloadSound);
                    HUDController.instance.restoreP1Bullets();
                }

                break;
            case PLAYER_TWO:
                if (numP2Bullets < MAX_BULLETS)
                {
                    numP2Bullets = MAX_BULLETS;
                    playP2SoundEffect(reloadSound);
                    HUDController.instance.restoreP2Bullets();
                }
                break;
        }
    }

    void shootBullet(int playerNum)
    {
        switch(playerNum)
        {
            case PLAYER_ONE:
                if (numP1Bullets >= 1)
                {
                    p1IsShooting = true;
                    playP1SoundEffect(shootSound);
                    HUDController.instance.decrementP1Bullets(numP1Bullets);
                    numP1Bullets--;
                }
                else
                {
                    playP1SoundEffect(emptyClipSound);
                }

                break;
            case PLAYER_TWO:
                if (numP2Bullets >= 1)
                {
                    p2IsShooting = true;
                    playP2SoundEffect(shootSound);
                    HUDController.instance.decrementP2Bullets(numP2Bullets);
                    numP2Bullets--;
                }
                else
                {
                    playP2SoundEffect(emptyClipSound);
                }

                break;
        }
    }

	// Use this for initialization
	void Awake ()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this.gameObject);
        }
        
        if (GameVariables.IsMultiplayerSession)
            HUDController.instance.setP2HUDVisible();

        initializeController();
        initializePlayerCrosshairs();
    }

    private void pause()
    {
        if (GameManager.instance.isPaused)
        {
            HUDController.instance.hideMessage();

            GameManager.instance.isPaused = false;
            Time.timeScale = 1;
        }
        else
        {
            HUDController.instance.displayMessage("PAUSED");
            GameManager.instance.isPaused = true;
            GameManager.instance.playPauseSound();
            Time.timeScale = 0;
        }
    }

	// Update is called once per frame
	void Update ()
    {
        previousState = currentState;
        currentState = GamePad.GetState(playerControllerIndex);

        if (previousState.Buttons.Start == ButtonState.Released && currentState.Buttons.Start == ButtonState.Pressed)
            pause();
        else if (previousState.Buttons.Back == ButtonState.Released && currentState.Buttons.Back == ButtonState.Pressed)
            pause();

        if (!waitingToExit && !GameManager.instance.isPaused)
        {
            if (previousState.Triggers.Left == 0f && currentState.Triggers.Left >= 0.1f)
            {
                reloadBullets(PLAYER_ONE);
            }
            else if (previousState.Buttons.LeftShoulder == ButtonState.Released && currentState.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                shootBullet(PLAYER_ONE);
            }
            else if ((previousState.Buttons.LeftShoulder == ButtonState.Pressed && currentState.Buttons.LeftShoulder == ButtonState.Released) || (previousState.Buttons.LeftShoulder == ButtonState.Pressed && currentState.Buttons.LeftShoulder == ButtonState.Pressed))
            {
                p1IsShooting = false;
            }

            var screenLeft = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
            var screenRight = Camera.main.ViewportToWorldPoint(Vector3.one).x;
            var screenTop = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
            var screenBottom = Camera.main.ViewportToWorldPoint(Vector3.one).y;

            float leftStickX = currentState.ThumbSticks.Left.X;
            float leftStickY = currentState.ThumbSticks.Left.Y;

            p1Crosshair.transform.position += new Vector3(leftStickX, leftStickY, 0) / 3;

            // Keep the crosshair within the bounds of the screen
            if (p1Crosshair.transform.position.x < screenLeft)
                p1Crosshair.transform.position = new Vector3(screenLeft, p1Crosshair.transform.position.y, p1Crosshair.transform.position.z);
            if (p1Crosshair.transform.position.x > screenRight)
                p1Crosshair.transform.position = new Vector3(screenRight, p1Crosshair.transform.position.y, p1Crosshair.transform.position.z);
            if (p1Crosshair.transform.position.y < screenTop)
                p1Crosshair.transform.position = new Vector3(p1Crosshair.transform.position.x, screenTop, p1Crosshair.transform.position.z);
            if (p1Crosshair.transform.position.y > screenBottom)
                p1Crosshair.transform.position = new Vector3(p1Crosshair.transform.position.x, screenBottom, p1Crosshair.transform.position.z);

            if (GameVariables.IsMultiplayerSession) // Only process righthand side of controller if the user chose to make the current play session multiplayer
            {
                if (previousState.Triggers.Right == 0f && currentState.Triggers.Right >= 0.1f)
                {
                    reloadBullets(PLAYER_TWO);
                }
                else if (previousState.Buttons.RightShoulder == ButtonState.Released && currentState.Buttons.RightShoulder == ButtonState.Pressed)
                {
                    shootBullet(PLAYER_TWO);
                }
                else if ((previousState.Buttons.RightShoulder == ButtonState.Pressed && currentState.Buttons.RightShoulder == ButtonState.Released) || (previousState.Buttons.RightShoulder == ButtonState.Pressed && currentState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    p2IsShooting = false;
                }

                float rightStickX = currentState.ThumbSticks.Right.X;
                float rightStickY = currentState.ThumbSticks.Right.Y;

                p2Crosshair.transform.position += new Vector3(rightStickX, rightStickY, 0) / 3;

                // Keep the crosshair within the bounds of the screen
                if (p2Crosshair.transform.position.x < screenLeft)
                    p2Crosshair.transform.position = new Vector3(screenLeft, p2Crosshair.transform.position.y, p2Crosshair.transform.position.z);
                if (p2Crosshair.transform.position.x > screenRight)
                    p2Crosshair.transform.position = new Vector3(screenRight, p2Crosshair.transform.position.y, p2Crosshair.transform.position.z);
                if (p2Crosshair.transform.position.y < screenTop)
                    p2Crosshair.transform.position = new Vector3(p2Crosshair.transform.position.x, screenTop, p2Crosshair.transform.position.z);
                if (p2Crosshair.transform.position.y > screenBottom)
                    p2Crosshair.transform.position = new Vector3(p2Crosshair.transform.position.x, screenBottom, p2Crosshair.transform.position.z);
            }
        }
        else if (waitingToExit)
        {
            if (previousState.Buttons.Start == ButtonState.Released && currentState.Buttons.Start == ButtonState.Pressed)
            {
                SceneManager.LoadScene(0);
            }
            else if (previousState.Buttons.Back == ButtonState.Released && currentState.Buttons.Back == ButtonState.Pressed)
            {
                SceneManager.LoadScene(0);
            }
        }
	}
}
