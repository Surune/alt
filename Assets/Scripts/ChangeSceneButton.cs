using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string sceneName;

    private void Awake()
    {
        button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
    }
}
