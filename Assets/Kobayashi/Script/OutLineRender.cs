#region

using System;
using UnityEngine;

#endregion

/*OutLineRender
 * 1:映したいobjectの子オブジェクトにこのコンポーネントを配置する(objectの動きにRender範囲を合わせるため)
 * 2:アウトラインをつけたいSpriteのLayerとOutLineRenderのLayerを合わせる(OutLineRenderはLayerにあわないようにする)
 * 2.5:映したい物体の大きさに合わせてviewSizeとTextureSizeを変更する
 * 3:ShaderをUnlit/OutLineShaderに変更する
 *
 * アウトラインの太さなどはShaderから変更してください
 */

[RequireComponent(typeof(SpriteRenderer))]
public class OutLineRender : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = -1;
    [SerializeField] private Vector2Int _textureSize = new(512, 512);
    [SerializeField] private float _viewSize = 5;
    [SerializeField] private int _cameraDepth = 0;
    private Camera _outLineCamera;
    private RenderTexture _rt;
    private SpriteRenderer _sr;
    private float _pixelParUnit;

    //描画終了後にメモリ解放が必要なため保持
    private Texture2D _tex;
    private Sprite _sprite;

    private void Start()
    {
        CreateOutLineCamera();

        _sr = GetComponent<SpriteRenderer>();
        _rt = new RenderTexture(_textureSize.x, _textureSize.y, 0, RenderTextureFormat.ARGB32);

        _outLineCamera.targetTexture = _rt;
        _pixelParUnit = PixelParUnit();
        CreateOutLineSprite();
    }

    private void UpdateOutLine()
    {
        //メモリ解放
        if (_tex) Destroy(_tex);
        if (_sprite) Destroy(_sprite);

        SetWorldScale(transform, Vector3.one);
        CreateOutLineSprite();
    }

    private void CreateOutLineCamera()
    {
        //平行投影カメラの作成
        var obj = new GameObject();
        obj.transform.parent = this.transform;
        _outLineCamera = obj.AddComponent<Camera>();
        _outLineCamera.orthographic = true;
        //alphaが0のtextureをSpriteにしたときに、画像そのままの枠でSpriteが生成されるためalphaを少しだけ追加しておく
        //画像そのままの枠でSpriteが生成されるとshaderで書き込めないため
        _outLineCamera.backgroundColor = new Color(0, 0, 0, 0.01f);
        _outLineCamera.cullingMask = _layerMask;
        //カメラのz軸の描画範囲の設定
        obj.transform.localPosition = new Vector3(0, 0, -10);
        _outLineCamera.nearClipPlane = -2f;
        _outLineCamera.orthographicSize = _viewSize;
        _outLineCamera.depth = _cameraDepth;
        
        //カメラにあるイベント関数を使用するためのコンポーネント配置
        var render = obj.AddComponent<RendererTextureUpdate>();
        render.OnRender += UpdateOutLine;
    }

    private Texture2D CreateTexture()
    {
        RenderTexture.active = _rt;
        var texture = new Texture2D(_rt.width, _rt.height);
        texture.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        texture.Apply(false);

        RenderTexture.active = null;

        return texture;
    }

    private void CreateOutLineSprite()
    {
        _tex = CreateTexture();
        _sprite = Sprite.Create(
            texture: _tex,
            rect: new Rect(0, 0, _tex.width, _tex.height),
            pivot: Vector2.one * 0.5f, //ピポットをオブジェクトの中心に設定する
            pixelsPerUnit:　_pixelParUnit);
        _sr.sprite = _sprite;
    }

    private void SetWorldScale(Transform target, Vector3 desiredWorldScale)
    {
        var parent = target.parent;

        if (parent != null)
        {
            Vector3 parentScale = parent.lossyScale;
            target.localScale = new Vector3(
                parentScale.x == 0f ? 0f : desiredWorldScale.x / parentScale.x,
                parentScale.y == 0f ? 0f : desiredWorldScale.y / parentScale.y,
                parentScale.z == 0f ? 0f : desiredWorldScale.z / parentScale.z
            );
        }
        else
        {
            target.localScale = desiredWorldScale;
        }
    }

    /// <summary>
    /// カメラのサイズと描画サイズを合わせる
    /// </summary>
    private float PixelParUnit()
    {
        return _textureSize.y / (_viewSize * 2);
    }
}

public class RendererTextureUpdate : MonoBehaviour
{
    /// <summary>
    /// OnRenderImageで呼ばれるAction
    /// </summary>
    public Action OnRender;
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest);
        OnRender?.Invoke();
    }
}