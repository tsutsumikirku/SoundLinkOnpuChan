using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInterface : MonoBehaviour
{
    public interface IDestroyable
    {
        void OnEnemyDestroy();
    }
}
