using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITable
{
    public void DeskUpdate();
    public void AddNode(NodeMove node);
    public void DeskSave()
    {
        Debug.Log("Hand");
    }
}
