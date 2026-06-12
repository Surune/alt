using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private SceneAsset scene;

    private void Awake()
    {
        button.onClick.AddListener(() => SceneManager.LoadScene(scene.name));
    }
}
