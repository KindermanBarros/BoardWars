using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : Singleton<MouseController>, IInputHandler
{
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;
    public Action<RaycastHit> OnMouseDrag;
    public Action<RaycastHit> OnMouseUp;

    private bool isDragging = false;
    private RaycastHit currentHit;

    private void Start()
    {
        InputManager.Instance.RegisterHandler(this);
    }

    private void OnDestroy()
    {
        InputManager.Instance.UnregisterHandler(this);
    }

    public void HandleInput()
    {
        // Check if mouse is over UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return; // Don't process clicks if over UI
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick(0);
            isDragging = true;
        }
        if (Input.GetMouseButtonDown(1)) CheckMouseClick(1);
        if (Input.GetMouseButtonDown(2)) CheckMouseClick(2);

        if (isDragging && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out currentHit))
            {
                OnMouseDrag?.Invoke(currentHit);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            OnMouseUp?.Invoke(currentHit);
        }
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    void CheckMouseClick(int mouseButton)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (mouseButton == 0)
                OnLeftMouseClick?.Invoke(hit);
            else if (mouseButton == 1)
                OnRightMouseClick?.Invoke(hit);
            else if (mouseButton == 2)
                OnMiddleMouseClick?.Invoke(hit);
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        if (IsPointerOverUI())
            return Vector3.zero;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    public Vector3 GetMousePosition()
    {
        return Camera.main.WorldToScreenPoint(Input.mousePosition);
    }
}