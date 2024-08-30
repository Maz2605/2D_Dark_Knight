using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerStateMachine StateMachine {  get; private set; }

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
    }
    private void Start()
    {
        //TODO: Init StateMachine
    }
    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();
    }
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicUpdate();
    }
}
