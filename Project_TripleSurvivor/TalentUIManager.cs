
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

public class TalentData
{
    public int AttackPower;
    public int MaxHP;
    public int Projectile;
    public float MoveSpeed;
    public float AttackSpeed;
    public int GoldGain;
    public int GoldRange;
}

public static class TalentTransfer
{
    public static TalentData data = new TalentData();
    public static int[] TalentLevels = new int[6];
}

public class TalentUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Image selectedTalentImage;
    public TextMeshProUGUI descriptionText;
    public Button upgradeButton;

    [Header("Talent Buttons")]
    public Button[] talentButtons;

    [Header("Talent selected")]
    public GameObject[] talentBorders;

    [Header("Talent Texts")]
    public TextMeshProUGUI[] talentLevelTexts;


    [Header("Image Options")]
    public float imageScaleMultiplier = 2.0f;

    private int[] talentLevels;

    private int selectedTalentIndex = -1;

    private TalentTableAccessor talentTableAccessor;

    public Button talentCloseButton;

    public class TalentRow
    {
        public string File;
        public int Level;
        public float Value;
        public int Cost;
        public string Desc;
    }

    public class TalentTableAccessor
    {
        private List<TalentRow> talentRows;

        public TalentTableAccessor()
        {
            var rawRows = CSVReader.Read("TalentTable").Skip(0);

            talentRows = new List<TalentRow>();

            foreach (var row in rawRows)
            {
                if (!row.ContainsKey("file") || !row.ContainsKey("level"))
                    continue;

                string file = row["file"]?.ToString();
                if (string.IsNullOrEmpty(file))
                    continue;

                if (!int.TryParse(row["level"]?.ToString(), out int level))
                    continue;

                float value = 0;
                int cost = 0;
                float.TryParse(row["value"]?.ToString(), out value);
                int.TryParse(row["cost"]?.ToString(), out cost);

                string desc = row.ContainsKey("desc") ? row["desc"]?.ToString() ?? "" : "";

                talentRows.Add(new TalentRow()
                {
                    File = file,
                    Level = level,
                    Value = value,
                    Cost = cost,
                    Desc = desc
                });
            }
        }

        public float GetValue(string file, int level)
        {
            var row = talentRows.FirstOrDefault(r => r.File == file && r.Level == level);
            return row != null ? row.Value : 0;
        }

        public int GetCost(string file, int level)
        {
            var row = talentRows.FirstOrDefault(r => r.File == file && r.Level == level);
            return row != null ? row.Cost : 0;
        }

        public string GetDescription(string file, int level)
        {
            var row = talentRows.FirstOrDefault(r => r.File == file && r.Level == level);
            return row != null ? row.Desc : "설명이 없습니다.";
        }
    }

    void Awake()
    {
        talentTableAccessor = new TalentTableAccessor();

        talentLevels = TalentTransfer.TalentLevels;

        for (int i = 0; i < talentButtons.Length; i++)
        {
            int index = i;
            talentButtons[index].onClick.AddListener(() => OnTalentSelected(index));
        }

        upgradeButton.onClick.AddListener(OnUpgradeTalent);
    }

    private void Start()
    {
        talentCloseButton.onClick.AddListener(() => gameObject.SetActive(false));
    }


    void OnTalentSelected(int i)
    {
        if (i < 0 || i >= talentButtons.Length) return;

        // 모든 테두리를 끄고, 현재 선택된(i) 테두리만 켭니다.
        for (int j = 0; j < talentBorders.Length; j++)
        {
            if (talentBorders[j] != null)
            {
                talentBorders[j].SetActive(j == i);
            }
        }

        selectedTalentIndex = i;
        Image btnImage = talentButtons[i].GetComponent<Image>();
        if (btnImage == null) return;

        selectedTalentImage.sprite = btnImage.sprite;
        selectedTalentImage.color = btnImage.color;


        if (selectedTalentImage.sprite != null)
        {
            RectTransform rt = selectedTalentImage.GetComponent<RectTransform>();
            Vector2 newSize = selectedTalentImage.sprite.rect.size * imageScaleMultiplier;
            rt.sizeDelta = newSize;
        }


        string talentBaseId = GetTalentBaseId(i);
        int currentLevel = talentLevels[i];

        string description = talentTableAccessor.GetDescription(talentBaseId, currentLevel);
        float value = talentTableAccessor.GetValue(talentBaseId, currentLevel);
        int cost = talentTableAccessor.GetCost(talentBaseId, currentLevel);

        //descriptionText.text = $"{description}\n\n누적 증가치: +{value}\n필요 골드:<color=#000000> {cost}G</color>";
        descriptionText.text = $"{description}\n\n누적 증가치 : +{value}\n필요 골드 : {cost}G";
        UpdateTalentLevelText(i);
    }

    void OnUpgradeTalent()
    {
        if (selectedTalentIndex == -1) return;

        int currentLevel = talentLevels[selectedTalentIndex];

        if (currentLevel >= 10)
        {
            Debug.LogWarning("이미 최대 레벨입니다.");
            //  최대 레벨 도달 시 실패 사운드
            EffectsSoundManager.Instance.PlaySound("Talent_uprade_fail", Vector3.zero);
            return;
        }

        string talentBaseId = GetTalentBaseId(selectedTalentIndex);

        int cost = talentTableAccessor.GetCost(talentBaseId, currentLevel);
        int playerGold = GameManager.instance.Gold;

        if (playerGold >= cost)
        {
            GameManager.instance.Minus_Gold(cost);

            int nextLevel = currentLevel + 1;
            talentLevels[selectedTalentIndex] = nextLevel;

            float value = talentTableAccessor.GetValue(talentBaseId, nextLevel);
            ApplyTalentStatToData(talentBaseId, value);

            Debug.Log($"[UPGRADE] {talentBaseId} 레벨업! 남은 골드: {GameManager.instance.Gold}");

            string description = talentTableAccessor.GetDescription(talentBaseId, nextLevel);
            int newCost = talentTableAccessor.GetCost(talentBaseId, nextLevel);

            //descriptionText.text = $"{description}\n\n누적 증가치: +{value}\n필요 골드:<color=#000000> {newCost}G</color>";
            descriptionText.text = $"{description}\n\n누적 증가치 : +{value}\n필요 골드 : {newCost}G";

            UpdateTalentLevelText(selectedTalentIndex);
            OnTalentSelected(selectedTalentIndex);

            //  업그레이드 성공 사운드
            EffectsSoundManager.Instance.PlaySound("Talent_uprade_success", Vector3.zero);
        }
        else
        {
            Debug.LogWarning($"골드 부족! 필요 골드: {cost}, 현재 골드: {playerGold}");

            //  골드 부족 시 실패 사운드
            EffectsSoundManager.Instance.PlaySound("Talent_uprade_fail", Vector3.zero);
        }
    }

    void UpdateTalentLevelText(int index)
    {
        if (index < 0 || index >= talentLevelTexts.Length) return;
        int level = talentLevels[index];
        talentLevelTexts[index].text = $"{level}/10";
    }

    string GetTalentBaseId(int index)
    {
        string[] talentBaseIds = {
      "talent_attack_power", "talent_max_hp",
      "talent_projectile_speed", "talent_move_speed",
      "talent_attack_speed",
      "talent_gold_range"
    };

        return index < talentBaseIds.Length ? talentBaseIds[index] : "talent_unknown";
    }

    void ApplyTalentStatToData(string talentId, float value)
    {
        if (talentId.Contains("attack_power"))
            TalentTransfer.data.AttackPower = Mathf.RoundToInt(value);
        else if (talentId.Contains("max_hp"))
            TalentTransfer.data.MaxHP = Mathf.RoundToInt(value);
        else if (talentId.Contains("projectile_speed"))
            TalentTransfer.data.Projectile = Mathf.RoundToInt(value);
        else if (talentId.Contains("move_speed"))
            TalentTransfer.data.MoveSpeed = value;
        else if (talentId.Contains("attack_speed"))
            TalentTransfer.data.AttackSpeed = value;
        else if (talentId.Contains("gold_gain"))
            TalentTransfer.data.GoldGain = Mathf.RoundToInt(value);
        else if (talentId.Contains("gold_range"))
            TalentTransfer.data.GoldRange = Mathf.RoundToInt(value);

        Debug.Log($"[TalentData 적용] {talentId} → {value} 저장됨");
    }
    private void OnEnable()
    {
        for (int i = 0; i < talentLevelTexts.Length; i++)
        {
            UpdateTalentLevelText(i);
        }

        if (selectedTalentIndex == -1 && talentButtons.Length > 0)
        {
            OnTalentSelected(0);
        }
    }
}



