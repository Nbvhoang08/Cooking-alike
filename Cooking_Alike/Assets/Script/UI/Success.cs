using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Success : UICanvas
{
    public Sprite OnVolume;
    public Sprite OffVolume;

    [SerializeField] private Image buttonImage;
    void Start()
    {

        UpdateButtonImage();

    }
    public void Next()
    {
        Time.timeScale = 1;
        UIManager.Instance.CloseUI<Success>(0.2f);
        LoadNextScene();

        SoundManager.Instance.PlayClickSound();

    }

    public void HomeBtn()
    {
        UIManager.Instance.CloseAll();
        Time.timeScale = 1;
        SceneManager.LoadScene("Home");
        SoundManager.Instance.PlayClickSound();
        UIManager.Instance.OpenUI<Home>();

    }
    public void SoundBtn()
    {
        SoundManager.Instance.TurnOn = !SoundManager.Instance.TurnOn;
        UpdateButtonImage();
        SoundManager.Instance.PlayClickSound();
    }

    private void UpdateButtonImage()
    {
        if (SoundManager.Instance.TurnOn)
        {
            buttonImage.sprite = OnVolume;
        }
        else
        {
            buttonImage.sprite = OffVolume;
        }
    }
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        // Kiểm tra xem scene tiếp theo có tồn tại không
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene("Home");

        }
    }

    IEnumerator NextSence()
    {
        yield return new WaitForSeconds(0.3f);
        LoadNextScene();
        Debug.Log("next");
    }
}
