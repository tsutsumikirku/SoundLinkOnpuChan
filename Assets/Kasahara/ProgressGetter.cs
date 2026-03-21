using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGetter : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform goal;
    float defaltPosX;

    private void Start()
    {
        defaltPosX = player.position.x;
    }

    public float GetProgress()
    {
        return Mathf.Clamp01((player.position.x - defaltPosX)/ (goal.position.x - defaltPosX));
    }
}
