using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct AdjustmentStructs
{
    [Label("背景")] public Transform[] Backgrounds;
    [Label("調整値")] public Vector2 AdjustAmount;
    [HideInInspector] public Vector3[] DefaultBackgroundPos;
    [Label("速度")] public float XVelocity;
    [Label("タイル幅")] public float TileWidth;
    [HideInInspector] public float MoveAmount;
}

public class AdjustBackground : MonoBehaviour
{
    [SerializeField, Label("カメラの座標")] private Transform camera;
    [SerializeField, Label("プレイヤーの座標")] private Transform target;
    [SerializeField, Label("カメラの基準の座標")] private Vector2 baseCameraPos;

    [SerializeField, Label("座標を調整するもの")] private AdjustmentStructs[] adjustmentStructs;

    void Start()
    {
        if (target != null)
        {
            baseCameraPos = target.position;
        }

        for (var index = 0; index < adjustmentStructs.Length; index++)
        {
            var ad = adjustmentStructs[index];
            ad.DefaultBackgroundPos = ad.Backgrounds.Select(x => x.position).ToArray();
            adjustmentStructs[index] = ad;
        }
    }

    void Update()
    {
        //プレイヤー(カメラ)の座標に応じて背景を動かす
        for (int index = 0; index < adjustmentStructs.Length; index++)
        {
            var ad = adjustmentStructs[index];
            for (int i = 0; i < ad.Backgrounds.Length; i++)
            {
                //主に雲を動かす。
                //違和感なくループさせるために工夫がある。
                //x座標がOOに来たら座標をここまで戻すだと、下の処理と競合するため
                //移動量のあまりを使って、本来の座標からどれくらい移動しているかを計算する
                if (ad.XVelocity != 0 && ad.TileWidth != 0)
                {
                    ad.MoveAmount = (ad.MoveAmount + ad.XVelocity * Time.deltaTime) % ad.TileWidth;
                }
                //プレイヤー(カメラ)の座標に応じて背景を動かす。
                //背景の元の座標をカメラの基準となる座標からカメラが動いた分だけ上下左右にずらす。
                //調整倍率でプレイヤーが移動した時どれくらいずらすかを設定
                //雲などには移動量を足す
                ad.Backgrounds[i].position = new Vector2(
                    ad.DefaultBackgroundPos[i].x - (baseCameraPos.x - camera.position.x) * ad.AdjustAmount.x +
                    ad.MoveAmount,
                    ad.DefaultBackgroundPos[i].y - (baseCameraPos.y - camera.position.y) * ad.AdjustAmount.y);
            }

            adjustmentStructs[index] = ad;
        }
    }
}