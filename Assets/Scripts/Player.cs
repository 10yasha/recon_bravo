using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{
    #region configurable parameters
    public float moveSpeed = 7.5f;
    public float smoothedMoveTime = .11f;
    public float turnSpeed = 10;
    #endregion

    #region player attributes
    float smoothedMoveVelocity;
    float smoothedMag;
    Vector3 curVelocity;
    float curAngle;
    new Rigidbody rigidbody;
    #endregion

    #region Events
    public event System.Action OnLevelClear;
    #endregion

    // signal game over
    bool hasBeenCaught;

    void Start(){
        rigidbody = GetComponent<Rigidbody>();
        Guard.OnPlayerSpotted += Disable;
    }

    void Update(){
        // update motion here but do not move player, move performed in FixedUpdate()
        Vector3 inputDir = Vector3.zero;
        if (!hasBeenCaught){
            inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
        // smoothing player movement improves player experience
        smoothedMag = Mathf.SmoothDamp(smoothedMag, inputDir.magnitude, ref smoothedMoveVelocity, smoothedMoveTime);

        // unity angle is from +ve y-axis clockwise
        float targetAngle = 90 - Mathf.Atan2(inputDir.z, inputDir.x) * Mathf.Rad2Deg;
        
        curAngle = Mathf.LerpAngle(curAngle, targetAngle, Time.deltaTime * turnSpeed * inputDir.magnitude);
        curVelocity = transform.forward * moveSpeed * smoothedMag;
    }

    void Disable(){
        hasBeenCaught = true;
    }

    void FixedUpdate(){
        // update movement in fixedupdate because variable framerate
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * curAngle));
        rigidbody.MovePosition(rigidbody.position + curVelocity * Time.deltaTime);
    }

    void OnDestroy(){
        Guard.OnPlayerSpotted -= Disable;
    }

    void OnTriggerEnter(Collider hitCollider){
        if (hitCollider.tag == "Finish"){
            Disable();
            OnLevelClear?.Invoke();
        }
    }
}