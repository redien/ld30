﻿using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour {
	
	public Transform debugPoint;
	
	Interactive FindInteractiveIfExists(Transform child) {
		var interactive = child.GetComponent<Interactive>();
		if (interactive != null) {
			return interactive;
		} else {
			if (child.parent != null) {
				return FindInteractiveIfExists(child.parent);
			} else {
				return null;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
		RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo)) {
			var hitGameObject = hitInfo.collider.gameObject;
			
			debugPoint.position = hitInfo.point;
			
			var interactive = FindInteractiveIfExists(hitGameObject.transform);
			if (interactive) {
				if (interactive.CanInteract(transform)) {
					debugPoint.gameObject.SetActive(true);
				} else {
					debugPoint.gameObject.SetActive(false);
				}
				
				if (Input.GetMouseButtonDown(0)) {
					interactive.Interact(transform);
				}
			} else {
				debugPoint.gameObject.SetActive(false);
			}
		}
	}
}
