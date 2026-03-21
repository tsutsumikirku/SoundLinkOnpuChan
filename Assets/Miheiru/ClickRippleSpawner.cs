using UnityEngine;

public class ClickRippleSpawner : MonoBehaviour
{
    public ParticleSystem rippleEffect;  // 波紋エフェクトのプレハブ

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 左クリック
        {
            Vector3 clickPos = Input.mousePosition;
            clickPos.z = 10f;  // カメラからの距離（2Dなら5〜10でOK）

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(clickPos);

            // 波紋を出す
            ParticleSystem ripple = Instantiate(rippleEffect, worldPos, Quaternion.identity);
            ripple.Play();

            // 一定時間後に自動で消す（1秒後）
            Destroy(ripple.gameObject, 1f);
        }
    }
}