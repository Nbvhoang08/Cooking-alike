using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Home : UICanvas
{
    public List<Button> btn = new List<Button>();

    public void Start()
    {
        btn[0].onClick.AddListener(() => LoadLevelScene("hard", 0.3f));
        btn[1].onClick.AddListener(() => LoadLevelScene("normal", 0.3f));
        btn[2].onClick.AddListener(() => LoadLevelScene("easy", 0.3f));
    }
    /// <summary>
    /// Load scene với tên tương ứng sau một khoảng thời gian delay.
    /// </summary>
    /// <param name="sceneName">Tên scene cần load</param>
    /// <param name="delay">Thời gian chờ trước khi load</param>
    void LoadLevelScene(string sceneName, float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName, delay));
        SoundManager.Instance.PlayClickSound();
    }

    /// <summary>
    /// Coroutine để thực hiện việc load scene sau một khoảng delay
    /// </summary>
    /// <param name="sceneName">Tên scene cần load</param>
    /// <param name="delay">Thời gian chờ</param>
    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        Debug.Log($"Starting to load scene: {sceneName} after {delay} seconds...");

        // Chờ một khoảng thời gian
        yield return new WaitForSeconds(delay);

        // Bắt đầu load scene
        SceneManager.LoadScene(sceneName);
        UIManager.Instance.CloseUI<Home>(0.3f);
        UIManager.Instance.OpenUI<InGame>();
        Debug.Log($"Scene {sceneName} loaded.");
    }
}
