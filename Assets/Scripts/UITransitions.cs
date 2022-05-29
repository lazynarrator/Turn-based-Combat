using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UITransitions : MonoBehaviour
{
    public GameObject exit;
    private GameObject exitWindow;

    public void ExitWindow(string text)
    {
        exitWindow = Instantiate(exit);
        GameObject Canvas = GetComponent<Canvas>().gameObject;
        exitWindow.transform.SetParent(Canvas.transform, false);
        exitWindow.GetComponentsInChildren<Button>()[0].onClick.AddListener(Exit);
        exitWindow.GetComponentsInChildren<TextMeshProUGUI>()[0].text = text;
    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Close()
    {
        Application.Quit();
    }

}
