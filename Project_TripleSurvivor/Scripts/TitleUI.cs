using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public Button jungleButton;
    public Button fantasyButton;
    public Button sfButton;

    void Start()
    {
        jungleButton.onClick.AddListener(() => Jungle());
        fantasyButton.onClick.AddListener(() => Fantasy());
        sfButton.onClick.AddListener(() => Alien());
    }

    public void Jungle()
    {
        GameManager.instance.Story = 0;
        SceneManager.LoadScene("MainTitle");
        Debug.Log("정글");
    }

    public void Fantasy()
    {
        GameManager.instance.Story = 1;
        SceneManager.LoadScene("MainTitle");
        Debug.Log("판타지");
    }

    public void Alien()
    {
        GameManager.instance.Story = 2;
        SceneManager.LoadScene("MainTitle");
        Debug.Log("외계인");
    }
}
