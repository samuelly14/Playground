using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(TouchControls))]
public class ObjectAlteration : MonoBehaviour
{
    //Object alteration script is going to need...
    //Raycast for selection
    //Select mode and Edit mode
    //Select mode sends out a raycast. Double tap to select the first placeable object hit by the raycast
    //Once something is selected, the user can go into edit mode. 
    //Edit mode allows the user to resize, rotate, and translate an object. 
    //Edit mode should also allow a user to change certain properties of the object (i.e. isKinematic)
    //Need to figure out gestures for these
    //Resizing: Probs a pinch/expand touch gesture
    //Rotating: single finger scroll 
    //Translating: Just parent it to the camera transform? Need to think out the implications though
    public GameObject UI;
    public GameObject raycastIndicator;

    private int mask = 1 << 8; //Selection tool should only work on Object layer

    private GameObject selected = null;

    public GameObject target = null;

    private LineRenderer raycastLine;

    private TouchControls touchControls;
    private Save save;
    private bool kinematicBuffer;

    public void DeleteObject()
    {
        if (selected != null)
        {
            save.RemoveObject(selected);
            Destroy(selected);
            selected = null;
        }
    }

    private void Awake()
    {
        touchControls = GetComponent<TouchControls>();
        raycastLine = raycastIndicator.GetComponent<LineRenderer>();
        save = GetComponent<Save>();
    }

    private void OnEnable()
    {
        UI.SetActive(true);
    }

    private void OnDisable()
    {
        UI.SetActive(false);
        raycastIndicator.SetActive(false);

        if (selected != null)
        {
            selected.transform.SetParent(null);
            selected.GetComponent<Rigidbody>().isKinematic = kinematicBuffer;
            selected.GetComponent<Collider>().isTrigger = false;
            selected = null;
        }
        if (target != null)
        {
            SetEmissionOfMaterials(target.GetComponent<MeshRenderer>(), Color.black);
            target = null;
        }
    }

    private void Update()
    {
        if (selected == null)
        {
            DetectObject();
            if (touchControls.DetectDoubleTap() && target != null)
            {
                selected = target;
                selected.transform.SetParent(Camera.main.transform);
                kinematicBuffer = selected.GetComponent<Rigidbody>().isKinematic;
                selected.GetComponent<Rigidbody>().isKinematic = true;
                selected.GetComponent<Collider>().isTrigger = true;

            }
        }
        else if (selected != null && touchControls.DetectDoubleTap())
        {
            selected.transform.SetParent(null);

            selected.GetComponent<Rigidbody>().isKinematic = kinematicBuffer;
            selected.GetComponent<Collider>().isTrigger = false;

            save.ChangeObject(selected);

            selected = null;
        }
        else
        {
            float deltaSize = touchControls.DetectPinch() / touchControls.pinchDamper;

            if (Mathf.Abs(deltaSize) > 0.01f)
            {
                float x = Mathf.Clamp(selected.transform.lossyScale.x + deltaSize, touchControls.scaleMin, touchControls.scaleMax);
                float y = Mathf.Clamp(selected.transform.lossyScale.y + deltaSize, touchControls.scaleMin, touchControls.scaleMax);
                float z = Mathf.Clamp(selected.transform.lossyScale.z + deltaSize, touchControls.scaleMin, touchControls.scaleMax);
                selected.transform.localScale = new Vector3(x, y, z);
            }

            Vector2 deltaScroll = touchControls.DetectScroll();

            if (deltaScroll.magnitude > 0.1f)
            {
                Vector3 rot = touchControls.GetRotationFromScreenSpaceValue(deltaScroll, selected.transform);
                selected.transform.Rotate(rot * touchControls.rotateSensitivity);
            }

            float deltaSwivel = touchControls.DetectSwivel();

            if (Mathf.Abs(deltaSwivel) > 0)
            {
                Vector3 fwdRot = touchControls.GetRotationFromSwivel(deltaSwivel, selected.transform);
                selected.transform.Rotate(fwdRot * touchControls.swivelSensitivity);
            }

            RaycastHit hit;
            Ray ray = new Ray(selected.transform.position, new Vector3(0, -1, 0));
            if (Physics.Raycast(ray, out hit))
            {
                raycastIndicator.SetActive(true);

                raycastIndicator.transform.position = hit.point;
                raycastLine.SetPositions(new Vector3[] { raycastIndicator.transform.position, selected.transform.position });
            }
            else
            {
                raycastLine.SetPositions(new Vector3[] { raycastIndicator.transform.position, raycastIndicator.transform.position });
                raycastIndicator.SetActive(false);
            }
        }
    }

    private void DetectObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            if (target != null)
            {
                SetEmissionOfMaterials(target.GetComponent<MeshRenderer>(), Color.black);
            }

            target = hit.collider.gameObject;
            raycastIndicator.SetActive(true);
            raycastIndicator.transform.position = hit.point;
            raycastLine.SetPositions(new Vector3[] { raycastIndicator.transform.position, raycastIndicator.transform.position });

            SetEmissionOfMaterials(target.GetComponent<MeshRenderer>(), Color.grey);
        }
        else
        {
            if (target != null) SetEmissionOfMaterials(target.GetComponent<MeshRenderer>(), Color.black);
            target = null;
            raycastIndicator.SetActive(false);
        }
    }

    public void ResetRotation()
    {
        if (selected != null)
        {
            selected.transform.eulerAngles = Vector3.zero;
        }
    }

    private void SetEmissionOfMaterials(MeshRenderer rend, Color col)
    {
        for (int i = 0; i < rend.materials.Length; i++)
        {
            rend.materials[i].SetColor("_EmissionColor", col);
        }
    }
}
