using UnityEngine;

public class CallAudioManager : MonoBehaviour
{
    //�A�j���[�V�����Ȃǂ�AudioManager���g�p���邽�߂�class�B
    public void PlaySE(string pass)
    {
        CriSEManager.Instance.PlaySE(pass, playOneShot: true);
    }
}
