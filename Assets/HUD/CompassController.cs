using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassController : MonoBehaviour
{

    public GameObject player;
    public GameObject target;
    public GameObject pointerArrow;
    public RectTransform compassLine;
    private RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = pointerArrow.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
		if(player == null) return;
        Vector3[] corners = new Vector3[4];
        compassLine.GetLocalCorners(corners);
        float pointerScale = Vector3.Distance(corners[1], corners[2]);
        Vector3 direction = target.transform.position - player.transform.position;
        float angleToTarget = Vector3.SignedAngle(player.transform.forward, direction, player.transform.up);
        angleToTarget = Mathf.Clamp(angleToTarget, -90, 90) / 180.0f * pointerScale;
        rect.localPosition = new Vector3(angleToTarget, rect.localPosition.y, rect.localPosition.z);
    }
}
