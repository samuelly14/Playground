using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save : MonoBehaviour
{
    public struct ObjectData
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public bool Kinematic;

        public ObjectData(Vector3 p, Vector3 r, Vector3 s, bool k)
        {
            Position = p;
            Rotation = r;
            Scale = s;
            Kinematic = k;
        }
    }

    public Dictionary<GameObject, ObjectData> locations = new Dictionary<GameObject, ObjectData>();

    public void AddObject(GameObject obj)
    {
        ObjectData data = new ObjectData(obj.transform.position, obj.transform.eulerAngles, obj.transform.lossyScale, obj.GetComponent<Rigidbody>().isKinematic);
        locations.Add(obj, data);
    }

    public void ChangeObject(GameObject obj)
    {
        ObjectData temp = new ObjectData(obj.transform.position, obj.transform.eulerAngles, obj.transform.lossyScale, locations[obj].Kinematic);

        locations[obj] = temp;
    }

    public void RemoveObject(GameObject obj)
    {
        locations.Remove(obj);
    }

    public void ForceSave()
    {
        GameObject[] temp = new GameObject[locations.Count];
        locations.Keys.CopyTo(temp, 0);
        for (int i = 0; i < temp.Length; i++)
        {
            ChangeObject(temp[i]);
        }
    }

    public void Reset()
    {
        foreach (KeyValuePair<GameObject, ObjectData> kvp in locations)
        {
            Rigidbody rb = kvp.Key.GetComponent<Rigidbody>();
            bool buf = rb.isKinematic;
            rb.isKinematic = true;
            kvp.Key.transform.position = kvp.Value.Position;
            kvp.Key.transform.eulerAngles = kvp.Value.Rotation;
            kvp.Key.transform.localScale = kvp.Value.Scale;
            rb.isKinematic = buf;
        }
    }

    public void DeleteAll()
    {
        GameObject[] temp = new GameObject[locations.Count];
        locations.Keys.CopyTo(temp, 0);
        for(int i = 0; i < temp.Length; i++)
        {
            RemoveObject(temp[i]);
            Destroy(temp[i]);
        }
    }
}

