﻿using UnityEngine;
using System.Collections;

public interface InteractiveObject {
	void Interact(Transform interacter);
	bool CanInteract(Transform interacter);
}
