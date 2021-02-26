﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;



public class PlaneSelectionManager : Singleton<PlaneSelectionManager>
{
    [SerializeField]
    private Camera arCamera;
    private Vector2 touchPosition = default;
    private Transform selection;
    private ISelectionResponse selectionResponse;
    private ARAnchorManager anchorManager;
    private List<ARAnchor> aRAnchors = new List<ARAnchor>();
    private ARPlane selectedPlane;

    public UnityEvent OnSelection;
    public UnityEvent OnDeselection;

    [SerializeField]
    private Button lockPlaneButton;

    [SerializeField]
    private ARPlaneManager aRPlaneManager;

    private bool CanSelect
    {
        get;
        set;
    }

    void Awake()
    {
        selectionResponse = GetComponent<SelectionResponse>();

        aRPlaneManager = GetComponent<ARPlaneManager>();
    }
    public void Update()
    {
        if (CanSelect) { return; }

        PlaneSelectionSequence();

    }

    public void AllowSelect()
    {
        CanSelect = !CanSelect;
    }

    public void PlaneSelectionSequence()
    {
        // Deselection
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                if (selection != null)
                {
                    var _selection = selection;
                    selectionResponse.OnDeselect(_selection);
                    //LockPlaneButton(disable);
                    //HidePlanes(false);
                    OnDeselection?.Invoke();
                    if (aRAnchors.Any<ARAnchor>())
                    {
                        aRAnchors.RemoveAt(aRAnchors.Count - 1);
                    }
                }
            }
        }
        #region raycast
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out RaycastHit hitObject))
                {
                    var _selection = hitObject.transform;
                    var selectedTransform = hitObject.transform.GetComponent<PlaneSelector>();
                    selectedPlane = hitObject.transform.GetComponent<ARPlane>();
                    if (selectedPlane && selectedTransform)
                    {
                        selection = _selection;
                        DrawManager.Instance.selectedPlane = selection;

                    }
                }
            }
        }
        #endregion
        // Selection
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                if (selection != null)
                {
                    var _selection = selection;
                    selectionResponse.OnSelect(_selection);
                    //lockPlaneButton
                    //HidePlanes(true);
                    OnSelection?.Invoke();
                    anchorManager.AttachAnchor(selectedPlane, new Pose(_selection.position, _selection.rotation));
                    aRAnchors.Add(selectedPlane.GetComponent<ARAnchor>());

                }
            }
        }
    }

    public void LockPlaneButton()
    {
        //keep selected plane fixed and visible
        //hide plane prefab after 2 seconds
        //display text prompt to begin drawing
        DebugManager.Instance.LogInfo($"Lock Planes.");
    }

    public void HidePlanes()
    {
        //hide all deselected planes
        DebugManager.Instance.LogInfo($"Hide Planes.{CanSelect}");
        CanSelect = !CanSelect;
        aRPlaneManager.SetTrackablesActive(CanSelect);
        
    }

    public void SetTrackablesActive(bool active)
    { }
}