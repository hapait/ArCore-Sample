using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class SceneController : MonoBehaviour {
	public GameObject trackedPlanePrefab;
	public Camera firstPersonCamera;
	public ScoreboardController scoreboard;
	public SnakeController snakeController;

	// Use this for initialization
	void Start () {
		QuitOnConnectionErrors ();
	}
	
	// Update is called once per frame
	void Update () {

		// The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
		if (Frame.TrackingState != TrackingState.Tracking)
		{
			const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
			Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
			return;
		}
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		ProcessNewPlanes();
		ProcessTouches();

		scoreboard.SetScore(snakeController.GetLength());
	}

	private void QuitOnConnectionErrors()
	{
		// Do not update if ARCore is not tracking.
		if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
		{
			StartCoroutine(CodelabUtils.ToastAndExit(
				"Camera permission is needed to run this application.", 5));
		}
		else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
		{
			StartCoroutine(CodelabUtils.ToastAndExit(
				"ARCore encountered a problem connecting.  Please start the app again.", 5));
		}
	}

	private void ProcessNewPlanes()
	{
		List<TrackedPlane> planes = new List<TrackedPlane>();
		Frame.GetPlanes(planes, TrackableQueryFilter.New);

		for(int i=0;i<planes.Count;i++)
		{
			// Instantiate a plane visualization prefab and set it to track the new plane.
			// The transform is set to the origin with an identity rotation since the mesh
			// for our prefab is updated in Unity World coordinates.
			GameObject planeObject = Instantiate(trackedPlanePrefab, Vector3.zero,
				Quaternion.identity, transform);
			planeObject.GetComponent<TrackedPlaneController>().SetTrackedPlane(planes[i]);

		}
	}

	private void ProcessTouches ()
	{
		Touch touch;
		if (Input.touchCount != 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		} 

		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;

		if (Session.Raycast (touch.position.x, touch.position.y, raycastFilter, out hit))
		{
			SetSelectedPlane (hit.Trackable as TrackedPlane);
		}
	}

	private void SetSelectedPlane(TrackedPlane selectedPlane)
	{
		Debug.Log("Selected plane centered at " + selectedPlane.Position);
		// Add to the end of SetSelectedPlane.
		scoreboard.SetSelectedPlane(selectedPlane);
		// Add to SetSelectedPlane()
		snakeController.SetPlane(selectedPlane);

		// Add to the bottom of SetSelectedPlane() 
		GetComponent<FoodController>().SetSelectedPlane(selectedPlane);
	}

}
