using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private List<UICanvas> uiCanvases; // Danh sách các UI canvas có sẵn

    /*      [SerializeField] private Transform parent; // Vị trí cha để tạo UI mới*/
    private bool isPaused = false; // Trạng thái tạm dừng của gameA

    protected override void Awake()
    {
        base.Awake();
        InitializeUICanvases();
    }

    // Khởi tạo tất cả UI Canvas, đặt chúng ở trạng thái không hoạt động
    private void InitializeUICanvases()
    {
        foreach (var canvas in uiCanvases)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    // Mở một UI cụ thể
    public T OpenUI<T>() where T : UICanvas
    {
        T canvas = GetUI<T>();
        if (canvas != null)
        {
            canvas.Setup();
            canvas.Open();
        }

        return canvas;
    }

    // Mở một UI với vị trí cha tùy chỉnh
    public T OpenUI<T>(Transform customParent) where T : UICanvas
    {
        T canvas = GetUI<T>();
        if (canvas != null)
        {
            /* canvas.transform.SetParent(customParent, false);*/
            canvas.Setup();
            canvas.Open();
        }

        return canvas;
    }

    // Đóng UI sau một khoảng thời gian
    public void CloseUI<T>(float time) where T : UICanvas
    {
        T canvas = GetUI<T>();
        if (canvas != null)
        {
            canvas.Close(time);
        }
    }

    // Đóng UI ngay lập tức
    public void CloseUIDirectly<T>() where T : UICanvas
    {
        T canvas = GetUI<T>();
        if (canvas != null)
        {
            canvas.CloseDirectly();
        }
    }

    // Kiểm tra xem một UI có đang mở không
    public bool IsUIOpened<T>() where T : UICanvas
    {
        T canvas = GetUI<T>();
        return canvas != null && canvas.gameObject.activeSelf;
    }

    // Lấy một UI cụ thể từ danh sách
    public T GetUI<T>() where T : UICanvas
    {
        return uiCanvases.Find(c => c is T) as T;
    }

    // Kích hoạt một UI cụ thể
    public void ActiveUI<T>() where T : UICanvas
    {
        T canvas = GetUI<T>();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
    }

    // Tạo một UI mới từ mẫu có sẵn trong danh sách
    /*  public T CreateNewUI<T>(Transform customParent = null) where T : UICanvas
      {
          T prefab = uiCanvases.Find(c => c is T) as T;
          if (prefab != null)
          {
             */ /* T newCanvas = Instantiate(prefab, customParent ?? parent);*/ /*
              uiCanvases.Add(newCanvas);
              return newCanvas;
          }
          return null;
      }*/

    // Đóng tất cả các UI đang mở
    public void CloseAll()
    {
        foreach (var canvas in uiCanvases)
        {
            if (canvas.gameObject.activeSelf)
            {
                canvas.Close(0);
            }
        }
    }

    // Tạm dừng hoặc tiếp tục game
    public void PauseGame()
    {
        isPaused = !isPaused;
        Debug.Log(isPaused);
        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            ResumeGame();
        }
    }

    // Tiếp tục game sau khi tạm dừng
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
    }

}
