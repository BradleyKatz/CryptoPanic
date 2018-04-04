using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System;

public class VisualCryptoLibrary : MonoBehaviour 
{
    public static VisualCryptoLibrary instance = null;

    public const int THUMBNAIL_WIDTH = 100, THUMBNAIL_HEIGHT = 100;

    private List<Texture2D> cipherImages = new List<Texture2D>(), dudImages = new List<Texture2D>();
    private Texture2D mergedImage = null;

	private Texture2D convertImageToMonochrome(string fileName)
    {
        System.Drawing.Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailFailCallback);
        Bitmap imageToConvert = (Bitmap) (new Bitmap(fileName).GetThumbnailImage(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, callback, IntPtr.Zero));

        for (int i = 0; i < imageToConvert.Width; i++)
        {
            for (int j = 0; j < imageToConvert.Height; j++)
            {
                var currentPixelColor = imageToConvert.GetPixel(i, j);
                int conversionFactor = (int)((currentPixelColor.R * .3f) + (currentPixelColor.G * .59f) + (currentPixelColor.B * .11f));

                if (conversionFactor > 127)
                    imageToConvert.SetPixel(i, j, System.Drawing.Color.White);
                else
                    imageToConvert.SetPixel(i, j, System.Drawing.Color.Black);
            }
        }

        return convertThumbnailToTexture(imageToConvert);
    }

    private Texture2D generateRandomImage(int imageWidth, int imageHeight)
    {
        Texture2D randomImage = new Texture2D(imageWidth, imageHeight);

        for (int i = 0; i < imageWidth; i++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                int randomNumber = Random.Range(5, 10); // 50/50 chance

                if (randomNumber <= 8)
                    randomImage.SetPixel(i, j, UnityEngine.Color.black);
                else
                    randomImage.SetPixel(i, j, UnityEngine.Color.white);
            }
        }
        
        randomImage.Apply();
        
        return randomImage;
    }
    
    public void createCipherImages(int numToProduce, string fileName)
    {
        cipherImages.Clear();

        Texture2D sourceImage = convertImageToMonochrome(fileName);
        
        Texture2D[] randomImages = new Texture2D[numToProduce - 1];

        for (int i = 0; i < randomImages.Length; i++)
        {
            randomImages[i] = generateRandomImage(sourceImage.width, sourceImage.height);
            cipherImages.Add(randomImages[i]);
        }

        Texture2D finalCipherImage = sourceImage;

        for (int i = 0; i < randomImages.Length; i++)
        {
            for (int j = 0; j < sourceImage.width; j++)
            {
                for (int k = 0; k < sourceImage.height; k++)
                {
                    var finalCipherBits = finalCipherImage.GetPixel(j, k);
                    var randPixelBits = randomImages[i].GetPixel(j, k);

                    finalCipherImage.SetPixel(j, k, new UnityEngine.Color((int)finalCipherBits.r ^ (int)randPixelBits.r, (int)finalCipherBits.g ^ (int)randPixelBits.g, (int)finalCipherBits.b ^ (int)randPixelBits.b, (int)finalCipherBits.a ^ (int)randPixelBits.a));
                }
            }
        }
        
        finalCipherImage.Apply();
        cipherImages.Add(finalCipherImage);
    }

    public void createDudImages(int imageWidth, int imageHeight, int numToProduce)
    {
        dudImages.Clear();

        for (int i = 0; i < numToProduce; i++)
        {
            Texture2D currentDud = generateRandomImage(imageWidth, imageHeight);

            if (dudImages.Count > 0)
            {
                bool isUniqueDud = false;

                while (!isUniqueDud)
                {
                    isUniqueDud = true;

                    foreach (Texture2D dud in dudImages)
                    {
                        if (dud.Equals(currentDud))
                        {
                            isUniqueDud = false;
                            currentDud = generateRandomImage(imageWidth, imageHeight);
                        }
                    }
                }
            }

            currentDud.Apply();
            dudImages.Add(currentDud);
        }
    }

    public void mergeNextCipherImage(Texture2D textureToMerge)
    {
        if (mergedImage == null)
        {
            mergedImage = textureToMerge;
        }
        else
        {
            for (int i = 0; i < textureToMerge.width; i++)
            {
                for (int j = 0; j < textureToMerge.height; j++)
                {
                    var mergedPixelBits = mergedImage.GetPixel(i, j);
                    var nextPixelBits = textureToMerge.GetPixel(i, j);

                    mergedImage.SetPixel(i, j, new UnityEngine.Color((int)mergedPixelBits.r ^ (int)nextPixelBits.r, (int)mergedPixelBits.g ^ (int)nextPixelBits.g, (int)mergedPixelBits.b ^ (int)nextPixelBits.b, (int)mergedPixelBits.a ^ (int)nextPixelBits.a));
                }
            }
        }

        mergedImage.Apply();
        HUDController.instance.updateMergedCipherPreview(Sprite.Create(mergedImage, new Rect(0, 0, mergedImage.width, -mergedImage.height), new Vector2(0.5f, 0.5f), 40));
    }

    private bool ThumbnailFailCallback()
    {
        return false;
    }

    private Texture2D convertThumbnailToTexture(Bitmap thumbnailToConvert)
    {
        Texture2D spriteTexture = new Texture2D(thumbnailToConvert.Width, thumbnailToConvert.Height);

        for (int i = 0; i < thumbnailToConvert.Width; i++)
        {
            for (int j = 0; j < thumbnailToConvert.Height; j++)
            {
                // Make Unity's color class play nicely with Microsoft's. Unity stores colors in RGBA format, whereas Microsoft stores colors in ARGB format.
                // Microsoft uses 255 color ARGB format, whereas Unity uses 0-1 color RGBA format
                System.Drawing.Color currentPixelColor = thumbnailToConvert.GetPixel(i, j);
                
                spriteTexture.SetPixel(i, j, new UnityEngine.Color(currentPixelColor.R / 255f, currentPixelColor.G / 255f, currentPixelColor.B / 255f, currentPixelColor.A / 255f));
            }
        }

        spriteTexture.Apply();
        return spriteTexture;
    }

    public Sprite getNextCipherImageThumbnail()
    {
        Sprite generatedSprite = Sprite.Create(cipherImages[0], new Rect(0, 0, cipherImages[0].width, cipherImages[0].height), new Vector2(0.5f, 0.5f), 40);
        cipherImages.RemoveAt(0);
        return generatedSprite;
    }

    public Sprite getNextDudImageThumbnail()
    {
        Sprite generatedSprite = Sprite.Create(dudImages[0], new Rect(0, 0, dudImages[0].width, dudImages[0].height), new Vector2(0.5f, 0.5f), 40);
        dudImages.RemoveAt(0);
        return generatedSprite;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(this.gameObject);
    }
}