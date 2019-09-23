using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;

public class Play : MonoBehaviour
{

    public GameObject UI;

    public GameObject recordingIndicator;

    public GameObject previewUI;

    private Save save;

    private bool physicsOn = true;

    private bool recording = false;

    string lastError = "";


    private void Awake()
    {
        save = GetComponent<Save>();
    }

    private void OnEnable()
    {
        UI.SetActive(true);
    }

    private void OnDisable()
    {
        UI.SetActive(false);
    }

    private void Update()
    {
        if (ReplayKit.recordingAvailable)
        {
            previewUI.SetActive(true);
        }
        else
        {
            previewUI.SetActive(false);
        }
    }

    public void TogglePhysics()
    {
        if (physicsOn)
        {
            foreach (GameObject obj in save.locations.Keys)
            {
                obj.GetComponent<Rigidbody>().isKinematic = true;
            }
            physicsOn = false;
        }
        else
        {
            GameObject[] objects = new GameObject[save.locations.Count];
            foreach (GameObject obj in save.locations.Keys)
            {
                obj.GetComponent<Rigidbody>().isKinematic = save.locations[obj].Kinematic;

            }
            physicsOn = true;
        }
    }

    public void Record()
    {
        if (!ReplayKit.APIAvailable)
        {
            return;
        }

        recording = ReplayKit.isRecording;

        try
        {
            recording = !recording;
            if (recording)
            {
                ReplayKit.StartRecording();
            }
            else
            {
                ReplayKit.StopRecording();
            }
            recordingIndicator.SetActive(recording);
        }
        catch (Exception e)
        {
            lastError = e.ToString();
        }
    }

    public void Preview()
    {
        ReplayKit.Preview();
    }

    public void Discard()
    {
        ReplayKit.Discard();
    }

}
