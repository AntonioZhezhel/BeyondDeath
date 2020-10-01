using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    private float destroyHeight;

    public float timeSink = 5;
    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.tag == "Ragdoll")
        {
            Invoke("StartSink",timeSink);
            Debug.Log("++++++++++++");
        }
            
        }

    public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        Collider[] colList = this.transform.GetComponentsInChildren<Collider>();
        foreach (Collider c in colList)
        {
            Destroy(c);
        }

        InvokeRepeating("SinkIntoGround", timeSink, 0.1f);
    }

    void SinkIntoGround()
    {
        this.transform.Translate(0, -0.001f, 0);
        if (this.transform.position.y < destroyHeight)
        {
            Destroy(this.gameObject);
        }
    }
   
}
