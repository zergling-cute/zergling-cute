using UnityEngine;
using UnityEngine.UI;

public class StoryChoice : MonoBehaviour
{

    public Image StartBtn;
    public Image TalentBtn;
    public Image TitleBtn;
    public Image OptionBtn;
    public Image Title;
    public GameObject BackgroundObj;

    private Sprite StartSprite;
    private Sprite TalentSprite;
    private Sprite TitleSprite;
    private Sprite OptionSprite;
    private Sprite TitleNameSprite;
    private Sprite background;

    public GameObject[] TalentPopup;
    public GameObject[] StartPopup;
    public GameObject[] OptionPopup;

    private string jungle = "Animal";
    private string fantasy = "Fantasy";
    private string ailen = "Alien";
    

    void Start()
    {
        switch (GameManager.instance.Story)
        {
            case 0:
                StorySprite(jungle);
                ImageChange();
                PopupSpawn();
                break;
            case 1:
                StorySprite(fantasy);
                ImageChange();
                PopupSpawn();
                break;
            case 2:
                StorySprite(ailen);
                ImageChange();
                PopupSpawn();
                break;
            

        }
    }

    void StorySprite(string Story)
    {
        StartSprite = Resources.Load<Sprite>(Story + "/UI/Main/Button_B");
        TalentSprite = Resources.Load<Sprite>(Story + "/UI/Main/Button_C");
        TitleSprite = Resources.Load<Sprite>(Story + "/UI/Main/Button_D");
        OptionSprite = Resources.Load<Sprite>(Story + "/UI/Main/Button_Setting");
        background = Resources.Load<Sprite>(Story + "/UI/Main/Main_Background_1");
        TitleNameSprite = Resources.Load<Sprite>(Story + "/UI/Main/Concept_Title");
    }

    void ImageChange()
    {
        StartBtn.sprite = StartSprite;
        TalentBtn.sprite = TalentSprite;
        TitleBtn.sprite = TitleSprite;
        OptionBtn.sprite = OptionSprite;
        Title.sprite = TitleNameSprite;
        BackgroundObj.GetComponent<SpriteRenderer>().sprite = background;
    }

    void PopupSpawn()
    {
        Transform tPopup = Instantiate(TalentPopup[GameManager.instance.Story]).transform;
        tPopup.SetParent(transform, false);
        tPopup.localPosition = Vector3.zero;
        tPopup.name = "TalentPopup";
        Transform stPopup = Instantiate(StartPopup[GameManager.instance.Story]).transform;
        stPopup.SetParent(transform, false);
        stPopup.localPosition = Vector3.zero;
        stPopup.name = "StartPopup";
        Transform opPopup = Instantiate(OptionPopup[GameManager.instance.Story]).transform;
        opPopup.SetParent(transform, false);
        opPopup.localPosition = Vector3.zero;
        opPopup.name = "OptionPopup";
    }
}
