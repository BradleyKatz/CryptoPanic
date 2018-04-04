using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public static TitleScreen instance = null;

	// Use this for initialization
	void Awake ()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
	
	// Update is called once per frame
	void Update ()
    {
	}
}
