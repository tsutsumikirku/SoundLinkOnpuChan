using UnityEngine;

public class PlayerSavePos : CancellableComponentBase
{
    [SerializeField,Label("メモメモマークプレハブ")] private GameObject _mark;
    [SerializeField,Label("マークの位置調整")] private Vector2 _offset;
    private Vector2 defaultSavePos;
    private Vector2 savePos;
    
    private Rigidbody2D _rigidbody;
    
    public static PlayerSavePos Instance; 

    private void OnEnable()
    {
        Instance = this;
        savePos = transform.position;
        defaultSavePos = savePos;//初期のセーブ座標として代入しておく。つまりスタート地点
        
        if (_mark == null)
        {
            Debug.LogError("おんぷちゃんにメモメモ時のマークをセットしてください");
        }
        else
        {
            _mark = Instantiate(_mark);
            _mark.SetActive(false);
        }
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Save(Vector2 pos)
    {
        savePos = pos;
        _mark.SetActive(true);
        _mark.transform.position = pos + new Vector2(_offset.x , _offset.y * Mathf.Sign(_rigidbody.gravityScale));
    }

    public Vector3 GetSavePos()
    {
        return savePos;
    }
    protected override void OnExecuteCancelled()
    {
        savePos = defaultSavePos;//クリップの実行停止時にセーブ地点をスタート地点に
        _mark.SetActive(false);
    }
}
