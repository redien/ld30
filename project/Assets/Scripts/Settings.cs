﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Settings : MonoBehaviour {
	
	public GameObject player;
	public Vector3 initialPlayerPosition;
	public Fader fader;

	public Portal portal;
	
	
	StateCollection stateCollectionPrivate;
	public StateCollection stateCollection {
		get {
			return stateCollectionPrivate;
		}
		
		set {
			value.stateChanged += HandleStateCollectionstateChanged;
			stateCollectionPrivate = value;
		}
	}
	
	PhotonView photonView;
	
	// Use this for initialization
	void Awake () {
		Application.runInBackground = true;
		photonView = GetComponent<PhotonView>();
		
		initialPlayerPosition = player.transform.position;
	}

	// Update is called once per frame
	void Update () {
		if (stateCollection != null) {
			var shouldEnablePortal = true;
			for (var i = 0; i < stateCollection.Count; ++i) {
				if (!stateCollection.GetState(i)) {
					shouldEnablePortal = false;
					break;
				}
			}
			portal.SetState(shouldEnablePortal);
		}
	}
	
	public void GenerateState() {
		var oldStateCollection = stateCollection;
		stateCollection = new StateCollection();
		
		if (oldStateCollection != null) {
			stateCollection.level = oldStateCollection.level + 1;
		}

		stateCollection.Generate();
	}
	
	public void LoadNextLevel() {
		GenerateState();

		StartCoroutine(NewLevelFade());
		
		/* Serialize states to send over RPC */
		string serializedWorldState = StateCollection.Serialize(stateCollection);
		
		photonView.RPC("SomeoneCompletedLevel", PhotonTargets.Others, serializedWorldState);
	}
	
	[RPC]
	void SomeoneCompletedLevel(string serializedWorldState) {
		/* Deserialize states we got over RPC */
		stateCollection = StateCollection.Deserialize(serializedWorldState);
		StartCoroutine(NewLevelFade());
	}

	void HandleStateCollectionstateChanged (int stateId, bool state)
	{
		photonView.RPC("SetRemoteState", PhotonTargets.Others, stateId, state);
	}
	
	[RPC]
	void SetRemoteState(int stateId, bool state) {
		stateCollection.SetState(stateId, state, false);
	}

	IEnumerator NewLevelFade() {
		Pause();
		yield return new WaitForSeconds(2.0f);
		
		GenerateLevel();
		
		player.transform.position = initialPlayerPosition;
		player.transform.localRotation = Quaternion.identity;
		
		yield return new WaitForSeconds(2.0f);
		Resume();
	}

	public void Pause() {
		Screen.lockCursor = false;
		fader.fadeIn = false;
	}

	public void Resume() {
		Screen.lockCursor = true;
		fader.fadeIn = true;
	}
	
	public GameObject[] stateSectionPrefabs;
	public GameObject[] staticSectionPrefabs;
	List<GameObject> sections = new List<GameObject>();

	public void GenerateLevel() {
		foreach (var section in sections) {
			DestroyImmediate(section);
		}
		sections.Clear();
		
		var statesSpawned = 0;
		
		var firstOffset = new Vector3(-50, 0, -50);
		for (var y = 0; y < 5; ++y) {
			for (var x = 0; x < 5; ++x) {
				// Skip the section where the portal is and where we spawn
				if (!(x == 2 && y == 4) && !(x == 2 && y == 2)) {
					GameObject sectionPrefab;
					bool spawnedState = false;
					Vector3 position = firstOffset + new Vector3(x * 25, 0, y * 25);
					Quaternion rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
					
					if (statesSpawned < stateCollection.Count) {
						sectionPrefab = stateSectionPrefabs[stateCollection.GetPrefabOf(statesSpawned)];
						spawnedState = true;
					} else {
						sectionPrefab = staticSectionPrefabs[Random.Range(0, staticSectionPrefabs.Length)];
					}

					var section = (GameObject)Instantiate(sectionPrefab, position, rotation);
					
					// Initialize states
					if (spawnedState) {
						var totem = section.GetComponentInChildren<Totem>();
						if (totem) {
							totem.stateCollection = stateCollection;
							totem.stateId = statesSpawned;
							// Update state
							totem.UpdateState();
						}
						statesSpawned += 1;
					}
					
					sections.Add(section);
				}
			}
		}
	}
}
