using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour{
    #region configurable parameters
    public float speed = 4.5f;
    public float waitTime = .2f;
    public float turnSpeed = 75f;
    public float timeToSpotPlayer = .25f;
    #endregion

    #region guard attributes
    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    float viewAngle;
    float playerFoundTimer;
    #endregion

    #region events
    public static event System.Action OnPlayerSpotted;
    #endregion

    public Transform pathSegments; // holds entire path of guard
    Transform player; // need info on player position to spot
    Color originalColor; // in order to revert after spotting player
    
    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        viewAngle = spotlight.spotAngle;
        originalColor = spotlight.color;

        Vector3[] waypoints = new Vector3[pathSegments.childCount];
        for (int i = 0; i < waypoints.Length; i++){
            waypoints[i] = pathSegments.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        if (waypoints.Length > 1){
            StartCoroutine(FollowPath(waypoints));
        }
    }

    void Update(){
        if (PlayerIsVisible()){
            // spotlight.color = Color.red; // debug
            playerFoundTimer += Time.deltaTime;
        } else {
            // spotlight.color = originalColor; // debug
            playerFoundTimer -= Time.deltaTime;
        }
        playerFoundTimer = Mathf.Clamp(playerFoundTimer, 0, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalColor, Color.red, playerFoundTimer/timeToSpotPlayer); // linear interpolation
        
        if (playerFoundTimer >= timeToSpotPlayer){
            OnPlayerSpotted?.Invoke();
        }
    }

    bool PlayerIsVisible(){
        if (Vector3.Distance(transform.position, player.position) < viewDistance){
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle/2f){
                if (!Physics.Linecast(transform.position, player.position, viewMask)){
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints){
        transform.position = waypoints[0];

        int curIndex = 1;
        Vector3 nextWaypoint = waypoints[curIndex];
        transform.LookAt(nextWaypoint);

        while (true){
            transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, speed * Time.deltaTime);
            if (transform.position == nextWaypoint){
                curIndex = (curIndex + 1) % waypoints.Length;
                nextWaypoint = waypoints[curIndex];

                if (waitTime != 0.0f) yield return new WaitForSeconds(waitTime);

                yield return StartCoroutine(TurnTowards(nextWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnTowards(Vector3 target){
        Vector3 adjustment = (target - transform.position).normalized;

        // unity angle is from +ve y-axis clockwise
        float targetAngle = 90 - Mathf.Atan2(adjustment.z, adjustment.x) * Mathf.Rad2Deg;

        // use small delta because of float imprecisions
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > .1f) {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;

            yield return null;
        }
    }

    void OnDrawGizmos(){
        // visualize guard's path for debug
        Vector3 start = pathSegments.GetChild(0).position;
        Vector3 previous = start;

        foreach (Transform waypoint in pathSegments){
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previous, waypoint.position);
            previous = waypoint.position;
        }
        Gizmos.DrawLine(previous, start);

        // visualize guard's current direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
