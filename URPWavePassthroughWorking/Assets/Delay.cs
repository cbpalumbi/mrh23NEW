using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delay : MonoBehaviour
{

    public Transform posToFollow;
    public Transform toLookAt;
    public float delayAmount;
    private Vector3 positionVelocity;
    public float maxSpeed = 10f;

    private Coroutine followCoroutine;
    

    private IEnumerator FollowPosition() {
        while(transform.position != posToFollow.position) {
            transform.position = Vector3.SmoothDamp(transform.position, posToFollow.position, ref positionVelocity, delayAmount);
            yield return null;
        }

        followCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(toLookAt);

        if(followCoroutine == null) {
            followCoroutine = StartCoroutine(FollowPosition());
        }

        //transform.position = cameraTransform.position;
    }
}
