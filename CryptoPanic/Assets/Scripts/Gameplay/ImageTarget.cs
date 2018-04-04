using UnityEngine;
using Random = UnityEngine.Random;
using System.Drawing;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class ImageTarget : MonoBehaviour
{
    public bool startsFromTop = false, startsFromBottom = false;
    private SpriteRenderer spriteRendererComponent = null;

    // Use this for initialization
    void Awake()
    {
        spriteRendererComponent = gameObject.GetComponent<SpriteRenderer>();
        spriteRendererComponent.sprite = VisualCryptoLibrary.instance.getNextCipherImageThumbnail();

        int spawnLocationChance = Random.Range(1, 10);

        if (spawnLocationChance >= 5)
        {
            startsFromTop = true;
            startsFromBottom = false;

            this.gameObject.transform.position += new Vector3(GameManager.instance.numTargetsOnTop * 4, 0, 0);
            GameManager.instance.numTargetsOnTop++;
        }
        else
        {
            startsFromBottom = true;
            startsFromTop = false;

            this.gameObject.transform.position += new Vector3(GameManager.instance.numTargetsOnBottom * 4, -3, 0);
            GameManager.instance.numTargetsOnBottom++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected abstract void OnTargetShot();

    void OnCollisionStay2D(Collision2D collidingObject)
    {
        if (collidingObject.gameObject.tag == "Player")
            OnTargetShot();
    }
}
