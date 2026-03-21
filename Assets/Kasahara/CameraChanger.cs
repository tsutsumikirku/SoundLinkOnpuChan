using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraChanger : MonoBehaviour
{
    [SerializeField] List<CameraTimeLine> _cameraTimeLine;
    [SerializeField] GameObject _cameraParent;
    [SerializeField] int _currentCameraIndex = 0;
    int _beforeIndex = 0;
    [Serializable]
    public struct CameraTimeLine
    {
        public CinemachineVirtualCamera Camera;
        public float WaitTime;
    }
    void Start()
    {
        CancellationTokenSource source = new CancellationTokenSource();
        CamChange(source).Forget();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            
        }
    }
    async UniTask CamChange(CancellationTokenSource source)
    {
        for (int i = 0; i < _cameraTimeLine.Count; i++)
        {
            // Cameraがnullでないことを確認
            if (_cameraTimeLine[i].Camera != null)
            {
                _cameraTimeLine[i].Camera.Priority = 1000;
            }
            
            try
            {
                await UniTask.Delay((int)(_cameraTimeLine[i].WaitTime * 1000), cancellationToken: source.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            
            // Cameraがnullでないことを確認してからSetActiveを呼び出す
            if (_cameraTimeLine[i].Camera != null && _cameraTimeLine[i].Camera.gameObject != null)
            {
                _cameraTimeLine[i].Camera.gameObject.SetActive(false);
            }
        }
    }
}
