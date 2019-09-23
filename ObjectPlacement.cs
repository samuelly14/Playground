using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


[RequireComponent(typeof(TouchControls))]
public class ObjectPlacement : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject indicator;
    public GameObject raycastIndicator;
    public GameObject UI;

    private LineRenderer raycastLine;

    public float heightAdjustDamper = 300;
    public float freezeTime = 0.25f;

    private ARSessionOrigin arOrigin;
    private ARRaycastManager arRaycast;
    private TouchControls touchControls;
    private Save save;

    private Vector3 placementLocation;
    private float heightOffset; //Height offset should be clamped between 0 - some max height value;

    private bool placementValid;

    private Vector2 touchAnchor;

    public void Select(GameObject obj)
    {
        objectToPlace = obj;
    }

    public void PlaceObject()
    {
        if (objectToPlace != null)
        {
            GameObject temp = Instantiate(objectToPlace, indicator.transform.position, indicator.transform.rotation);
            temp.transform.localScale = indicator.transform.lossyScale;
            save.AddObject(temp);

            //StartCoroutine("FreezeRotationOnPlacement", temp);
        }
    }
    private void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        arRaycast = FindObjectOfType<ARRaycastManager>();
        touchControls = GetComponent<TouchControls>();
        save = GetComponent<Save>();
        raycastLine = raycastIndicator.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        //Double tap to place object
        if (touchControls.DetectDoubleTap())
        {
            PlaceObject();
        }

        //Scroll with a single finger to rotate left/right, or to adjust height
        Vector2 scroll = touchControls.DetectScroll();

        if (scroll.magnitude > 0.01f)
        {
            if (Mathf.Abs(scroll.x) > Mathf.Abs(scroll.y))
            {
                indicator.transform.Rotate(new Vector3(0, -scroll.x, 0));
            }
            else
            {
                heightOffset = Mathf.Clamp(heightOffset + (scroll.y / heightAdjustDamper), 0, 3);
            }
        }

        //Pinch with two fingers to resize
        float resize = touchControls.DetectPinch() / touchControls.pinchDamper;

        if (Mathf.Abs(resize) > 0.01f)
        {
            float x = Mathf.Clamp(indicator.transform.lossyScale.x + resize, touchControls.scaleMin, touchControls.scaleMax);
            float y = Mathf.Clamp(indicator.transform.lossyScale.y + resize, touchControls.scaleMin, touchControls.scaleMax);
            float z = Mathf.Clamp(indicator.transform.lossyScale.z + resize, touchControls.scaleMin, touchControls.scaleMax);
            indicator.transform.localScale = new Vector3(x, y, z);
        }

        UpdatePlacementLocation();
        UpdateIndicator();
    }

    private void OnDisable()
    {
        indicator.SetActive(false);
        raycastLine.SetPositions(new Vector3[] { raycastIndicator.transform.position, raycastIndicator.transform.position });
        raycastIndicator.SetActive(false);
        UI.SetActive(false);
    }

    private void OnEnable()
    {
        UI.SetActive(true);
    }

    private void UpdatePlacementLocation()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            placementValid = true;
            placementLocation = hit.point;
        }
        else
        {
            placementValid = false;
        }
    }

    private void UpdateIndicator()
    {
        if (objectToPlace == null)
        {
            indicator.GetComponent<MeshFilter>().mesh = null;
        }
        else
        {

            indicator.GetComponent<MeshFilter>().mesh = objectToPlace.GetComponent<MeshFilter>().sharedMesh;

            for (int i = 0; i < indicator.GetComponent<MeshRenderer>().materials.Length; i++)
            {
                indicator.GetComponent<MeshRenderer>().materials[i] = indicator.GetComponent<MeshRenderer>().material;
            }

        }

        indicator.SetActive(placementValid);
        indicator.transform.position = new Vector3(placementLocation.x, placementLocation.y + heightOffset, placementLocation.z);

        raycastIndicator.SetActive(placementValid);
        raycastIndicator.transform.position = placementLocation;
        raycastLine.SetPositions(new Vector3[] { raycastIndicator.transform.position, indicator.transform.position });
    }

    IEnumerator FreezeRotationOnPlacement(GameObject placed)
    {
        float timeElapsed = 0f;
        Rigidbody rb = placed.GetComponent<Rigidbody>();
        RigidbodyConstraints buffer = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        while (timeElapsed < freezeTime)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        placed.GetComponent<Rigidbody>().constraints = buffer;
    }
}