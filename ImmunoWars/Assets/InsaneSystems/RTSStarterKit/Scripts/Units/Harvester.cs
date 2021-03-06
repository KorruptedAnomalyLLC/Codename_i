﻿using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Harvester : Module
	{
		const float randomFieldDistance = 2f;
		const float sqrRandomFieldDistance = 24f;

		public enum HarvestState
		{
			MoveToField,
			Harvest,
			MoveToRefinery,
			CarryOutResources,
			Idle
		}

		public event HarvesterResourcesChanged harvesterResourcesChangedEvent;

		public int MaxResources { get { return selfUnit.data.harvestMaxResources; } }
		public int harvestedResources { get; protected set; }

		HarvestState harvestState;

		Refinery nearestRefinery;
		ResourcesField resourcesField;
		float recheckTimer = 1f;

		float harvestTimeLeft;
		float carryOutTimeLeft;

		int addedToRefineryResources;
		/// <summary> Field for temporary colliders store for some physical radius checks of harvester. </summary>
		readonly Collider[] nearestColliders = new Collider[15];

		public delegate void HarvesterResourcesChanged(float newValue, float maxValue);

		void Start()
		{
			selfUnit.unitReceivedOrderEvent += OnUnitReceivedOrder;

			if (harvesterResourcesChangedEvent != null)
				harvesterResourcesChangedEvent.Invoke(0, MaxResources);
		}

		void Update()
		{
			if (!nearestRefinery)
			{
				SearchNearestRefinery();

				return;
			}

			if (!resourcesField)
			{
				SearchNearestResourcesField();

				return;
			}

			switch (harvestState)
			{
				case HarvestState.MoveToField:
					if ((transform.position - resourcesField.transform.position).sqrMagnitude < sqrRandomFieldDistance)
						SetHarvestState(HarvestState.Harvest);
					break;

				case HarvestState.Harvest:
					harvestTimeLeft -= Time.deltaTime;
					harvestedResources = (int)Mathf.Lerp(0, MaxResources, 1f - harvestTimeLeft / selfUnit.data.harvestTime);

					if (harvesterResourcesChangedEvent != null)
						harvesterResourcesChangedEvent.Invoke(harvestedResources, MaxResources);

					if (harvestTimeLeft <= 0)
					{
						harvestTimeLeft = 0f;
						harvestedResources = MaxResources;

						SetHarvestState(HarvestState.MoveToRefinery);
					}
					break;

				case HarvestState.MoveToRefinery:
					if ((transform.position - nearestRefinery.CarryOutResourcesPoint.position).sqrMagnitude < 8f)
						SetHarvestState(HarvestState.CarryOutResources);
					break;

				case HarvestState.CarryOutResources:
					carryOutTimeLeft -= Time.deltaTime;

					if (carryOutTimeLeft <= 0)
					{
						carryOutTimeLeft = 0f;
				
						nearestRefinery.AddResources(harvestedResources);
						harvestedResources = 0;

						if (harvesterResourcesChangedEvent != null)
							harvesterResourcesChangedEvent.Invoke(harvestedResources, MaxResources);

						SetHarvestState(HarvestState.MoveToField);
					}
					break;
			}
		}

		void SearchNearestRefinery()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
				return;
			}

			var allRefineries = Refinery.allRefineries;
			allRefineries = allRefineries.FindAll(refinery => refinery.selfUnit.OwnerPlayerId == selfUnit.OwnerPlayerId);

			float distance = float.MaxValue - 1f;

			for (int i = 0; i < allRefineries.Count; i++)
			{
				float curDistance = (transform.position - allRefineries[i].transform.position).sqrMagnitude;

				if (curDistance < distance)
				{
					nearestRefinery = allRefineries[i];
					distance = curDistance;
				}
			}

			recheckTimer = 1f;
		}

		void SearchNearestResourcesField()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
				return;
			}

			var allFields = ResourcesField.sceneResourceFields;
			float distance = float.MaxValue - 1f;

			for (int i = 0; i < allFields.Count; i++)
			{
				float curDistance = (transform.position - allFields[i].transform.position).sqrMagnitude;

				if (curDistance < distance)
				{
					resourcesField = allFields[i];
					distance = curDistance;
				}
			}

			if (resourcesField)
				SetHarvestState(HarvestState.MoveToField);

			recheckTimer = 1f;
		}

		public void SetHarvestState(HarvestState newState)
		{
			harvestState = newState;

			switch (harvestState)
			{
				case HarvestState.MoveToField:
					var order = new MovePositionOrder();
					order.executor = selfUnit;
					order.movePosition = resourcesField.transform.position + new Vector3(Random.Range(-randomFieldDistance, randomFieldDistance), 0, Random.Range(-randomFieldDistance, randomFieldDistance)); // todo change to proportion of resource field size
					selfUnit.AddOrder(order, false, isReceivedEventNeeded: false);
					break;

				case HarvestState.Harvest:
					harvestTimeLeft = selfUnit.data.harvestTime;
					break;

				case HarvestState.MoveToRefinery:
					var orderBack = new MovePositionOrder();
					orderBack.movePosition = nearestRefinery.CarryOutResourcesPoint.position;
					selfUnit.AddOrder(orderBack, false, isReceivedEventNeeded: false);
					break;

				case HarvestState.CarryOutResources:
					carryOutTimeLeft = selfUnit.data.harvestCarryOutTime;
					addedToRefineryResources = 0;
					break;
			}
		}

		public void OnUnitReceivedOrder(Unit unit, Order order)
		{
			if (order is MovePositionOrder)
			{
				var position = (order as MovePositionOrder).movePosition;

				var size = Physics.OverlapSphereNonAlloc(position, 7f, nearestColliders);

				for (int i = 0; i < size; i++)
				{
					var field = nearestColliders[i].GetComponent<ResourcesField>();

					if (field)
					{
						resourcesField = field;
						SetHarvestState(HarvestState.MoveToField);

						return;
					}
				}
			}
			else if (order is FollowOrder)
			{
				var target = (order as FollowOrder).followTarget;

				var refinery = target.GetComponent<Refinery>();

				if (refinery)
				{
					SetRefinery(refinery);

					if (harvestedResources > 0)
						SetHarvestState(HarvestState.MoveToRefinery);

					return;
				}
			}

			SetHarvestState(HarvestState.Idle);
		}

		public void SetRefinery(Refinery refinery) { nearestRefinery = refinery; }
	}
}