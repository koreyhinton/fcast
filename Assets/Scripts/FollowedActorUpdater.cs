﻿using Cinemachine;
using Osnowa.Osnowa.Unity;
using UnityEngine;
using Fcast;

public class FollowedActorUpdater : MonoBehaviour
{
	private CinemachineVirtualCamera _followPlayerCamera;
		
	void Start()
	{
		_followPlayerCamera = GetComponent<CinemachineVirtualCamera>();
	}

	public void UpdateControlledActor(GameObject playerGameObject, GameEntity playerEntity)
	{
		_followPlayerCamera.Follow = playerGameObject.transform;
		
		// todo: will the UI adjust for the new controlled entity? 
                EntityViewBehaviour playerEntityViewBehaviour = playerGameObject.GetComponent<EntityViewBehaviour>();

	}
}
