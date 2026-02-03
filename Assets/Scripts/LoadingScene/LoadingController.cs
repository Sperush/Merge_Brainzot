using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro; // Tận dụng DOTween bạn vừa hỏi

public class LoadingController : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public TMP_Text version;
    public Slider loadingBar;
    public Transform characterSpawnPoint; // Vị trí nhân vật đứng
    public GameObject charObj;
    [Header("Dữ liệu")]
    public List<GameObject> characterPrefabs; // Kéo thả các nhân vật vào đây

    void Start()
    {
        // 1. Random nhân vật
        ShowRandomCharacter();
        version.SetText(Noti.Get("version", Application.version));
        // 2. Bắt đầu load scene
        StartCoroutine(LoadSceneAsync());
    }

    void ShowRandomCharacter()
    {
        if (characterPrefabs.Count > 0)
        {
            // Chọn ngẫu nhiên 1 index
            int randomIndex = Random.Range(0, characterPrefabs.Count);

            // Tạo nhân vật tại vị trí định sẵn
            charObj = Instantiate(characterPrefabs[randomIndex], characterSpawnPoint);

            // Reset vị trí về 0 so với cha để căn giữa
            charObj.transform.localPosition = Vector3.zero;

        }
    }
    IEnumerator LoadSceneAsync()
    {
        // Bắt đầu load ngầm scene gameplay
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");

        // Ngăn không cho chuyển scene ngay lập tức (để chờ loading bar chạy hết)
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (progress <= 1f)
        { 
            // Ta cộng thêm thời gian để bar chạy mượt (giả vờ load)
            progress += Time.deltaTime * 0.2f; // Tốc độ chạy thanh loading

            // Update thanh Slider
            loadingBar.value = progress;

            // Nếu thanh đã đầy (>= 1) và Scene thực tế đã load xong (0.9)
            if (progress >= 1f && operation.progress >= 0.9f)
            {
                // Cho phép chuyển cảnh
                operation.allowSceneActivation = true;
                charObj.transform.DOKill();
            }

            yield return null;
        }
    }
}