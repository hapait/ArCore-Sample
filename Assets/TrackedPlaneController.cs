using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class TrackedPlaneController : MonoBehaviour {
	private TrackedPlane trackedPlane;
	private PlaneRenderer planeRenderer;
	private List<Vector3> polygonVertices = new List<Vector3> ();

	void Awake()
	{
		planeRenderer = GetComponent<PlaneRenderer>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// If no plane yet, disable the renderer and return.
		if (trackedPlane == null)
		{
			planeRenderer.EnablePlane (false);
			return;
		}

		// If this plane was subsumed by another plane, destroy this object, the other
		// plane's display will render it.
		if (trackedPlane.SubsumedBy != null)
		{
			Destroy (gameObject);
			return;
		}

		// If this plane is not valid or ARCore is not tracking, disable rendering.
		if (trackedPlane.TrackingState != TrackingState.Tracking || Frame.TrackingState != TrackingState.Tracking)
		{
			planeRenderer.EnablePlane (false);
			return;
		}

		// OK! Valid plane, so enable rendering and update the polygon data if needed.
		planeRenderer.EnablePlane (true);

		List<Vector3> newPolygonVertices = new List<Vector3> ();
		trackedPlane.GetBoundaryPolygon (newPolygonVertices);
		if (!AreVerticesListsEqual (polygonVertices, newPolygonVertices)) {
			polygonVertices.Clear ();
			polygonVertices.AddRange (newPolygonVertices);

			planeRenderer.UpdateMeshWithCurrentTrackedPlane (trackedPlane.Position,
				polygonVertices);
		}
	}

	public void SetTrackedPlane (TrackedPlane plane)
	{
		trackedPlane = plane;
		trackedPlane.GetBoundaryPolygon (polygonVertices);
		planeRenderer.Initialize ();
		planeRenderer.UpdateMeshWithCurrentTrackedPlane(trackedPlane.Position,
			polygonVertices);
	}

	private bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
	{
		if (firstList.Count != secondList.Count)
		{
			return false;
		}

		for (int i = 0; i < firstList.Count; i++)
		{
			if (firstList[i] != secondList[i])
			{
				return false;
			}
		}

		return true;
	}
}
