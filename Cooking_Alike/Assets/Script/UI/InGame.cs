using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class InGame : UICanvas
{
    // Start is called before the first frame update
    public Sprite OnVolume;
    public Sprite OffVolume;

    [SerializeField] private Image buttonImage;

    void Start()
    {

        UpdateButtonImage();

    }
    public void Levelbtn()
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
}
