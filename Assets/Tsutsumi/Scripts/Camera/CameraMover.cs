using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Transform _target;
    [SerializeField] CinemachineVirtualCamera _cinemachineVirtual;
    [SerializeField] CinemachineConfiner2D _cinemachineConfiner; 
    [SerializeField] private float _dragSpeed = 0.5f;
    [SerializeField] private float _maxCameraHeight = 10f;
    [SerializeField] private float _minCameraHeight = 0.1f;
    [SerializeField] private float _leftCameraPlus = 0f;
    [SerializeField] private float _rightCameraPlus = 0f;
    [SerializeField] private float _minFOV = 10f;   // 最小ズーム
    [SerializeField] private float _maxFOV = 60f;   // 最大ズーム
    [SerializeField] private GameObject _cameraButton;

    private Vector3 _playerPos;
    private Vector3 _goalPos;
    private Transform _default;
    private float _beforeTouchDistance;
    private float _defaultFOV;
    
    void Awake()
    {
        if (_cinemachineVirtual == null)
        {
            var mainCameraObj = GameObject.FindWithTag("MainCamera");
            if (mainCameraObj != null)
                _cinemachineVirtual = mainCameraObj.GetComponent<CinemachineVirtualCamera>();
        }

        if (_cinemachineVirtual == null)
        {
            Debug.LogError("CameraMover: CinemachineVirtualCamera が見つかりません。Inspector で設定するか MainCamera タグのオブジェクトを確認してください。");
            enabled = false;
            return;
        }

        if (_cinemachineConfiner == null)
            _cinemachineConfiner = _cinemachineVirtual.GetComponent<CinemachineConfiner2D>();

        _defaultFOV = _cinemachineVirtual.m_Lens.FieldOfView;
        _default = _cinemachineVirtual.Follow;
        if (_default == null)
        {
            Debug.LogError("CameraMover: VirtualCamera の Follow が未設定です。");
            enabled = false;
            return;
        }

        var cameraChanger = FindAnyObjectByType<CameraChanger>();
        if (cameraChanger == null)
        {
            Debug.LogError("CameraMover: CameraChanger がシーンに見つかりません。");
            enabled = false;
            return;
        }

        if (_target == null)
        {
            Debug.LogError("CameraMover: _target が Inspector で未設定です。");
            enabled = false;
            return;
        }

        _target = Instantiate(_target, cameraChanger.transform);
        _playerPos = _default.position;
        _playerPos = new Vector3(_playerPos.x + _leftCameraPlus, _playerPos.y, _playerPos.z);
        _target.position = _playerPos;

        var goalBehaviour = FindAnyObjectByType<GoalBehaviour>();
        if (goalBehaviour == null)
        {
            Debug.LogError("CameraMover: GoalBehaviour がシーンに見つかりません。");
            enabled = false;
            return;
        }

        _goalPos = goalBehaviour.transform.position;
        _goalPos = new Vector3(_goalPos.x + _rightCameraPlus, _goalPos.y, _goalPos.z);
    }

    public void CameraReset()
    {
        _cameraButton.SetActive(false);
        if (_cinemachineVirtual.Follow == _default) return;
        _cinemachineVirtual.Follow = _default;
        _target.position = _playerPos;
        _cinemachineVirtual.m_Lens.FieldOfView = _defaultFOV;
        CameraUpdate().Forget();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _cinemachineVirtual.Follow = _target;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _cameraButton.SetActive(true);
        if (Input.touchCount == 2)
        {
            var currentTouchDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position) * 7;
            if (_beforeTouchDistance > 0)
            {
                float delta = currentTouchDistance - _beforeTouchDistance;
                float newFOV = _cinemachineVirtual.m_Lens.FieldOfView - delta * _dragSpeed * 0.05f;
                newFOV = Mathf.Clamp(newFOV, _minFOV, _maxFOV);

                _cinemachineVirtual.m_Lens.FieldOfView = newFOV;
                CameraUpdate().Forget();

            }

            _beforeTouchDistance = currentTouchDistance;
            return;
        }
        else
        {
            _beforeTouchDistance = -1;
        }

        // 📌 ドラッグでカメラ移動
        _target.position += new Vector3(eventData.delta.x, eventData.delta.y, 0) * _dragSpeed * -1;

        // X方向の制限
        if (_target.position.x < _playerPos.x)
            _target.position = _playerPos;
        else if (_target.position.x > _goalPos.x)
            _target.position = new Vector3(_goalPos.x, _target.position.y, _target.position.z);

        // Y方向の制限
        if (_target.position.y < _minCameraHeight)
            _target.position = new Vector3(_target.position.x, _minCameraHeight, _target.position.z);
        else if (_target.position.y > _maxCameraHeight)
            _target.position = new Vector3(_target.position.x, _maxCameraHeight, _target.position.z);
        
    }

    async UniTask CameraUpdate()
    {
        await UniTask.Yield(); 
        _cinemachineConfiner.InvalidateCache();
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        _cinemachineVirtual.Priority = 10;
        _beforeTouchDistance = -1;
    }
}

