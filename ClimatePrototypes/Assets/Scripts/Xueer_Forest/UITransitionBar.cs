﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITransitionBar : MonoBehaviour
{
    private Transform transitionBarTransform;
    //public Transform cameraTargetTransform;
    public GameObject transitionFrom;
    public GameObject transitionTo;
    public Transform UITransform;
    public GameObject volunteerUI;

    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        transitionBarTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray,Mathf.Infinity);
        
        if(hit.collider != null && hit.collider.transform == transitionBarTransform)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
        }

        if (timer >= 0.5f)
        {
            Debug.Log("load left scene");
            //Camera.main.transform.position = cameraTargetTransform.position;
            transitionFrom.SetActive(false);
            transitionTo.SetActive(true);
            volunteerUI.transform.position = UITransform.position;
        }
    }
}