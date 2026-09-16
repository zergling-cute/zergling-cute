using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    AsyncOperation async;

    float delayTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadingNextScene(GameManager.instance.nextSceneName));
        delayTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        delayTime += Time.deltaTime;
    }

    IEnumerator LoadingNextScene(string sceneNamne)
    {
        async = SceneManager.LoadSceneAsync(sceneNamne);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return true;
        }

        while (async.progress >= 0.9f)
        {
            yield return new WaitForSeconds(0.1f);
            if (delayTime >= 2.0f)
                break;
        }

        async.allowSceneActivation = true;
        yield return true;
    }
}
