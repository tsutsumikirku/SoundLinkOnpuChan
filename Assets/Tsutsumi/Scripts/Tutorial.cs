using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject _turorial;
    [SerializeField] Sprite _addSprite;
    void Start()
    {
        var turial = JsonSave.Load<TutorialLoader>(SceneManager.GetActiveScene().name + "tutorial");
        if (turial == default)
        {
            _turorial.SetActive(true);
            var tutorial = new TutorialLoader() { IsPlayedTutorial = true };
            AddInfo().Forget();
            JsonSave.Save(SceneManager.GetActiveScene().name + "tutorial", tutorial);
        }
    }
    async UniTask AddInfo()
    {
        await UniTask.Yield();
        if(_addSprite)
        _turorial.GetComponent<InfoView>().SetWindow(_turorial.GetComponent<InfoView>().AddSprite(_addSprite) - 1);
    }
}
[System.Serializable]
public class TutorialLoader
{
    public bool IsPlayedTutorial;
}
