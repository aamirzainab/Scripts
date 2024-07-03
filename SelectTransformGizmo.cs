using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RuntimeHandle;

public class SelectTransformGizmo : MonoBehaviour
{
    public RuntimeTransformHandle transformHandle; 
    public Camera arCamera;  

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = arCamera.ScreenPointToRay(touch.position);

            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.CompareTag("Selectable"))
                    {
                        transformHandle.target = hit.transform;
                        transformHandle.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
