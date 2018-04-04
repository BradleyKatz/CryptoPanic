using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
{
    public static HUDController instance = null;

    private GameObject[] bulletSprites = new GameObject[5];
    private bool firstImageMerge = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this.gameObject);
        }

        // There is unfortunately no better way to have the Unity Engine generate an array of objects (in proper order) that fall under a certain tag.
        bulletSprites[0] = GameObject.Find("P1Bullet1");
        bulletSprites[1] = GameObject.Find("P1Bullet2");
        bulletSprites[2] = GameObject.Find("P1Bullet3");
        bulletSprites[3] = GameObject.Find("P1Bullet4");
        bulletSprites[4] = GameObject.Find("P1Bullet5");
    }

    public void hideBulletSprites(int bulletToRemove)
    {
        Color spriteColor = bulletSprites[bulletToRemove - 1].GetComponent<Image>().color;
        bulletSprites[bulletToRemove - 1].GetComponent<Image>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, 0);
    }

    public void restoreBulletSprites()
    {
        foreach (GameObject bullet in bulletSprites)
        {
            Color spriteColor = bullet.GetComponent<Image>().color;

            if (spriteColor.a == 0)
                bullet.GetComponent<Image>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, 1);
        }
    }

    public void updateMergedCipherPreview(Sprite mergedImage)
    {
        GameObject.Find("ImagePreview").GetComponent<Image>().sprite = mergedImage;

        if (firstImageMerge)
        {
            GameObject.Find("ImagePreview").GetComponent<CanvasGroup>().alpha = 1;
            firstImageMerge = false;
        }
    }
}
