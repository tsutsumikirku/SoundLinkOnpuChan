using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UndoUI
{
    static List<object> _objects = new List<object>();
    static List<GameObject> _gameObjects = new List<GameObject>();
    static List<Action<object>> _undoActions = new List<Action<object>>();
    public static void Add(object obj, Action<object> action, GameObject gameObj)
    {
        if (_undoActions.Count > 20)
        {
            _undoActions.RemoveAt(0);
            _objects.RemoveAt(0);
        }
        _undoActions.Add(action);
        _objects.Add(obj);
        _gameObjects.Add(gameObj);
        Debug.Log("Undoに追加しました");
    }
    public static void UndoAction()
    {
        if (_undoActions.Count == 0) return;
        var gameObj = _gameObjects[_gameObjects.Count - 1];
        var action = _undoActions[_undoActions.Count - 1];
        var obj = _objects[_objects.Count - 1];
        if (!gameObj)
        {
            _undoActions.RemoveAt(_undoActions.Count - 1);
            _objects.RemoveAt(_objects.Count - 1);
            _gameObjects.RemoveAt(_gameObjects.Count - 1);
            UndoAction();
            return;
        }
        _undoActions.RemoveAt(_undoActions.Count - 1);
            _objects.RemoveAt(_objects.Count - 1);
            _gameObjects.RemoveAt(_gameObjects.Count - 1);
        action(obj);
        Debug.Log("Undoを実行しました");
    }
}
