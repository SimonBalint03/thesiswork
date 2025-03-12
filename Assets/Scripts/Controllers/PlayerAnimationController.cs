using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Hexes;
using Singletons;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int Moving = Animator.StringToHash("Moving");
    public Animator animator;

    private void OnEnable()
    {
        Player.OnSingleMoveEvent += OnSingleMoveEvent;
        MovementController.OnMovementStopped += OnMovementStopped;
        MovementController.OnMovementStarted += OnMovementStarted;
    }

    private void OnDisable()
    {
        Player.OnSingleMoveEvent -= OnSingleMoveEvent;
        MovementController.OnMovementStopped -= OnMovementStopped;
        MovementController.OnMovementStarted -= OnMovementStarted;
    }

    private void OnSingleMoveEvent(HexTile from, HexTile to)
    {
        // Get direction to target, ignoring y-axis
        Vector3 direction = to.transform.position - Player.Instance.transform.position;
        direction.y = 0; // Lock the y-axis rotation

        // Set rotation
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Player.Instance.character.transform.rotation = targetRotation;
        }
    }
    
    private void OnMovementStopped()
    {
        animator.SetTrigger("StopMoving");
    }
    
    private void OnMovementStarted()
    {
        animator.SetTrigger("StartMoving");
    }

}
