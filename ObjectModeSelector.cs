using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.iOS;
public class ObjectModeSelector : MonoBehaviour
{
    private ObjectAlteration alter;
    private ObjectPlacement placement;
    private ARPlaneManager planeManager;
    private Play play;

    public GameObject scanUI;

    private void Awake()
    {

        alter = FindObjectOfType<ObjectAlteration>();
        placement = FindObjectOfType<ObjectPlacement>();
        planeManager = FindObjectOfType<ARPlaneManager>();
        play = FindObjectOfType<Play>();

        placement.enabled = false;
        alter.enabled = false;
        play.enabled = false;

        Application.RequestUserAuthorization(UserAuthorization.WebCam);
    }

    public void ChangeMode(int i)
    {
        switch (i)
        {
            case 0:
                placement.enabled = false;
                alter.enabled = false;
                play.enabled = false;

                planeManager.enabled = true;
                scanUI.SetActive(true);

                Application.RequestUserAuthorization(UserAuthorization.WebCam);


                break;
            case 1:
                alter.enabled = false;
                planeManager.enabled = false;
                play.enabled = false;
                scanUI.SetActive(false);

                placement.enabled = true;

                break;

            case 2:
                placement.enabled = false;
                planeManager.enabled = false;
                play.enabled = false;
                scanUI.SetActive(false);

                alter.enabled = true;

                break;
            case 3:
                placement.enabled = false;
                alter.enabled = false;
                planeManager.enabled = false;
                scanUI.SetActive(false);

                play.enabled = true;

                break;
        }
    }
}
