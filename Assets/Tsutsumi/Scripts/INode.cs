using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    float Second { get; }
    float Power{ get;}
    PlayerState PlayerAction{ get;}
    void StartNode();
    void EndNode();
}
