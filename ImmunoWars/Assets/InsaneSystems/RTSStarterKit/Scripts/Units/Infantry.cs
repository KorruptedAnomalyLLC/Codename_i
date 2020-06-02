using InsaneSystems.RTSStarterKit.Misc;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows to implement infantry features like animations for characters etc. </summary>
	public class Infantry : Module
	{
		[SerializeField] Animator animator;
		
		static readonly int attackId = Animator.StringToHash("Attack");
		static readonly int moveId = Animator.StringToHash("Move");
		static readonly int dieId = Animator.StringToHash("Die");

		void Start()
		{
			if (!animator)
				animator = GetComponent<Animator>();

			if (!animator)
			{
				Debug.LogWarning("Infantry soldier " + name + " does not have Animator component! It will have NO animations, if you're not add it.");
				return;
			}

			if (!animator.runtimeAnimatorController && selfUnit.data.animatorController)
				animator.runtimeAnimatorController = selfUnit.data.animatorController;

			selfUnit.GetModule<Attackable>().startAttackEvent += OnStartAttack;
			selfUnit.GetModule<Attackable>().stopAttackEvent += OnStopAttack;
			selfUnit.GetModule<Movable>().startMoveEvent += OnStartMove;
			selfUnit.GetModule<Movable>().stopMoveEvent += OnStopMove;
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
		}

		void OnStartAttack()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(attackId, true);
		}

		void OnStartMove()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(moveId, true);
		}

		void OnStopMove()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(moveId, false);
		}

		void OnStopAttack()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(attackId, false);
		}

		void OnDie(Unit unit)
		{
			animator.transform.SetParent(null);
			var timedRemover = animator.transform.gameObject.AddComponent<TimedObjectDestructor>();
			timedRemover.SetCustomTime(5f); // will remove corpses after 5 seconds.
			
			if (animator.isActiveAndEnabled)
				animator.SetBool(dieId, true);
		}
	}
}