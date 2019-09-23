using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class TouchControls : MonoBehaviour
{

    public float swivelSensitivity = 100f;
    public float rotateSensitivity = 0.5f;
    public float scaleSensitivity = 0.002f;
    public float pinchDamper = 500f;

    public float scaleMin = 0.01f;
    public float scaleMax = 3f;

    private bool doubleTap;
    private bool skip;

    private bool pinching;
    private float pinchBuffer;

    private bool swivelling;
    private Vector2 swivelBuffer;

    private bool touchingUIElement;

    private GraphicRaycaster uiRaycaster;
    private EventSystem eventSystem;


    private void Awake()
    {
        uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();

    }

    public Vector3 GetRotationFromScreenSpaceValue(Vector2 rotateDirection, Transform objTransform)
    {

        //This type of rotation offers a better perspective, but makes it difficult to make precise adjustments. 

        Vector3 weight_y = new Vector3(0, 0, 0);
        Vector3 weight_x = new Vector3(0, 0, 0);

        weight_y.x = Vector3.Dot(objTransform.right, Camera.main.transform.up);
        weight_y.y = Vector3.Dot(objTransform.up, Camera.main.transform.up);
        weight_y.z = Vector3.Dot(objTransform.forward, Camera.main.transform.up);

        weight_x.x = Vector3.Dot(objTransform.right, Camera.main.transform.right);
        weight_x.y = Vector3.Dot(objTransform.up, Camera.main.transform.right);
        weight_x.z = Vector3.Dot(objTransform.forward, Camera.main.transform.right);

        return (rotateDirection.y * weight_x) + (rotateDirection.x * -weight_y);
    }

    public Vector3 GetRotationFromSwivel(float angle, Transform objTransform)
    {

        //This type of rotation offers a better perspective, but makes it difficult to make precise adjustments. 
        Vector3 weight_z = new Vector3(0, 0, 0);

        weight_z.x = Vector3.Dot(objTransform.right, Camera.main.transform.forward);
        weight_z.y = Vector3.Dot(objTransform.up, Camera.main.transform.forward);
        weight_z.z = Vector3.Dot(objTransform.forward, Camera.main.transform.forward);

        return weight_z * angle;
    }

    public Vector3 TempGetRotationFromScreenSpaceValue(Vector2 rotateDirection, Transform objTransform)
    {

        return new Vector3(rotateDirection.y, -rotateDirection.x, 0);
    }

    public Vector3 TempGetRotationFromSwivel(float angle, Transform objTransform)
    {
        return new Vector3(0, 0, angle);
    }

    public bool DetectDoubleTap()
    {

        if (Input.touchCount > 0 && !IsTouchingUIElement())
        {
            Touch touch = Input.GetTouch(0);
            if (touch.deltaPosition.magnitude > 10f)
            {
                skip = true;
            }

            if (touch.tapCount > 1 && !skip)
            {
                doubleTap = true;
            }
            return false;
        }
        else if (doubleTap && !skip)
        {
            doubleTap = false;
            return true;
        }
        else
        {
            doubleTap = false;
            skip = false;
            return false;
        }
    }


    public float DetectPinch()
    {
        if (Input.touchCount >= 2)
        {
            if (pinching)
            {
                //Need to reassign pinchBuffer to be the new distance, then return the difference of distances
                float ret = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) - pinchBuffer;
                pinchBuffer = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                return ret;
            }
            else
            {
                pinching = true;
                pinchBuffer = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                return 0.0f;
            }
        }
        else
        {
            pinching = false;
            return 0.0f;
        }
    }

    public Vector2 DetectScroll()
    {
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                touchingUIElement = IsTouchingUIElement();
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchingUIElement = false;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && !touchingUIElement)
            {
                return Input.GetTouch(0).deltaPosition;
            }
        }
        return Vector2.zero;
    }

    public float DetectSwivel()
    {
        //A swivel is the gesture where two fingers are rotated. 

        if (Input.touchCount == 2 && Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) > 400)
        {
            if (swivelling)
            {
                Vector2 temp = swivelBuffer;
                swivelBuffer = (Input.GetTouch(0).position - Input.GetTouch(1).position).normalized;
                return Mathf.Atan2(swivelBuffer.y, swivelBuffer.x) - Mathf.Atan2(temp.y, temp.x);
            }
            else
            {
                swivelling = true;
                swivelBuffer = (Input.GetTouch(0).position - Input.GetTouch(1).position).normalized;
                return 0.0f;
            }
        }
        else
        {
            swivelling = false;
            return 0.0f;
        }
    }

    private bool IsTouchingUIElement()
    {
        if (Input.touchCount > 0)
        {
            PointerEventData pointer = new PointerEventData(eventSystem);
            pointer.position = Input.GetTouch(0).position;

            List<RaycastResult> hits = new List<RaycastResult>();

            uiRaycaster.Raycast(pointer, hits);

            if (hits.Count > 0)
            {
                return true;
            }
        }
        return false;
    }
}
