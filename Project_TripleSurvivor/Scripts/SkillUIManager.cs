using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SkillUIManager : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image[] skillIcons;
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private TextMeshProUGUI skillDescText;
    [SerializeField] private Image selectedSkillIconDisplay;

    [Header("Runtime")]
    public PlayerSkills playerSkills;

    // 슬롯 인덱스 -> 스킬ID
    private readonly Dictionary<int, int> skillIDBySlot = new Dictionary<int, int>();

    // 아이콘 캐시
    private readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    private static readonly string[] StoryRoots = { "Animal", "Fantasy", "Alien" };

    // ===== [추가] 스토리별 이름 처리를 위한 인덱스 맵 =====
    // GM.Story: 0=Animal, 1=Fantasy, 2=Alien
    // StoryNameKR 배열: 0=Jungle, 1=Alien, 2=Fantasy
    // (SkillSelectionUI와 동일한 로직)
    private static readonly int[] StoryToNameIndex = { 0, 2, 1 };

    private void Awake()
    {
        if (skillButtons != null)
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                skillButtons[i]?.onClick.AddListener(() => OnSkillSlotClicked(index));
            }
        }
    }

    private void OnEnable()
    {
        if (playerSkills == null)
            playerSkills = Object.FindFirstObjectByType<PlayerSkills>();

        RefreshSkillUI();
    }

    public void RefreshSkillUI()
    {
        if (playerSkills == null || skillIcons == null || skillDescText == null)
        {
            Debug.LogError("[SkillUIManager] 필수 참조 누락");
            return;
        }

        skillDescText.text = "";
        if (selectedSkillIconDisplay != null)
        {
            selectedSkillIconDisplay.sprite = null;
            selectedSkillIconDisplay.enabled = false;
            selectedSkillIconDisplay.gameObject.SetActive(false);
        }
        skillIDBySlot.Clear();

        var acquired = playerSkills.GetAcquiredSkills();

        for (int i = 0; i < skillIcons.Length; i++)
        {
            var icon = skillIcons[i];

            if (i < acquired.Count)
            {
                var kv = acquired.ElementAt(i);
                int skillID = kv.Key;
                int level = kv.Value;

                skillIDBySlot[i] = skillID;

                var so = SkillDatabase.Instance.GetSOByID(skillID.ToString());
                var pso = (so == null) ? SkillDatabase.Instance.GetPassiveSOByID(skillID.ToString()) : null;

                string iconName = so != null ? so.iconName : pso?.iconName;
                bool isPassive = (so == null && pso != null) || IsPassiveID(skillID);

                if (icon != null)
                {
                    var sprite = LoadIconByStory(iconName, isPassive);
                    icon.sprite = sprite;
                    icon.enabled = sprite != null;
                    if (sprite == null)
                        Debug.LogWarning($"[SkillUIManager] 아이콘 로드 실패: id={skillID}, icon={iconName}");
                }
            }
            else
            {
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.enabled = false;
                }
            }
        }

        if (acquired.Count > 0)
        {
            OnSkillSlotClicked(0);
        }
    }

    private void OnSkillSlotClicked(int slotIndex)
    {
        // 디버그 1: 클릭된 슬롯 인덱스와 딕셔너리 정보 확인
        // Debug.Log($"[OnSkillSlotClicked] 슬롯 {slotIndex} 클릭됨.");
        // Debug.Log($"[OnSkillSlotClicked] skillIDBySlot에 {slotIndex}번 키가 존재? {skillIDBySlot.ContainsKey(slotIndex)}");

        if (!skillIDBySlot.TryGetValue(slotIndex, out var skillID))
        {
            Debug.LogWarning($"[OnSkillSlotClicked] 슬롯 {slotIndex}에 해당하는 스킬 ID를 찾을 수 없습니다. UI를 업데이트하지 않습니다.");
            return;
        }

        // 디버그 2: 찾아낸 스킬 ID 확인
        // Debug.Log($"[OnSkillSlotClicked] 찾은 스킬 ID: {skillID}");

        int level = playerSkills != null ? playerSkills.GetCurrentLevel(skillID) : 1;

        // 디버그 3: 스킬 데이터베이스에서 SO 조회 확인
        var so = SkillDatabase.Instance.GetSOByID(skillID.ToString());
        var pso = (so == null) ? SkillDatabase.Instance.GetPassiveSOByID(skillID.ToString()) : null;

        // Debug.Log($"[OnSkillSlotClicked] SO 조회 결과: so={so}, pso={pso}");

        string desc = "";
        string iconName = null;

        // ===== [수정됨] 스토리별 이름 해석 로직 호출 =====
        string skillName = ResolveSkillName(so, pso, skillID);
        // ============================================

        if (so != null)
        {
            // [수정됨] so.krName 대신 해석된 skillName 사용
            desc = $"{skillName} Lv.{level}\n{so.skillDescription}";
            iconName = so.iconName;
        }
        else if (pso != null)
        {
            int shownLevel = level > 0 ? level : pso.level;
            // [수정됨] pso.krName 대신 해석된 skillName 사용
            desc = $"패시브 {skillName} Lv.{shownLevel}\n{pso.skillDescription}";
            iconName = pso.iconName;
        }
        else
        {
            desc = "알 수 없는 스킬";
            Debug.LogError($"[OnSkillSlotClicked] 스킬 ID {skillID}에 대한 데이터를 데이터베이스에서 찾을 수 없습니다. 설명: {desc}");
        }

        skillDescText.text = desc;

        if (selectedSkillIconDisplay != null)
        {
            bool isPassive = (so == null && pso != null) || IsPassiveID(skillID);
            var sprite = LoadIconByStory(iconName, isPassive);
            selectedSkillIconDisplay.sprite = sprite;
            selectedSkillIconDisplay.enabled = sprite != null;
            selectedSkillIconDisplay.gameObject.SetActive(sprite != null);
        }
    }

    // ===== [추가] SkillSelectionUI의 이름 해석 로직 =====
    private string ResolveSkillName(SkillData activeSo, PassiveSkillData passiveSo, int skillID)
    {
        // ===== 액티브 스킬 (스토리별 이름 적용) =====
        if (activeSo != null)
        {
            int gmStory = Mathf.Clamp(GameManager.instance?.Story ?? 0, 0, 2);
            int nameIdx = StoryToNameIndex[gmStory]; // GM → 이름배열 보정
            string storyKR = GetStoryKR_Active(activeSo, nameIdx);

            if (!string.IsNullOrWhiteSpace(storyKR)) return storyKR;

            // 폴백
            if (!string.IsNullOrEmpty(activeSo.krName)) return activeSo.krName;
            if (!string.IsNullOrEmpty(activeSo.skillName)) return activeSo.skillName;
            return skillID.ToString();
        }

        // ===== 패시브 스킬 (공통 이름) =====
        if (passiveSo != null)
        {
            if (!string.IsNullOrEmpty(passiveSo.krName)) return passiveSo.krName;
            if (!string.IsNullOrEmpty(passiveSo.skillName)) return passiveSo.skillName;
            return skillID.ToString();
        }

        // SO가 없을 때 폴백
        return skillID.ToString();
    }

    // ===== [추가] SkillSelectionUI의 헬퍼 메서드 =====
    /// <summary>
    /// 액티브 SO의 스토리별 한글명 (SkillData에 StoryNameKR[0..2] 배열이 있다고 가정)
    /// </summary>
    private static string GetStoryKR_Active(SkillData so, int idx)
    {
        var arr = so.StoryNameKR;
        if (arr != null && idx >= 0 && idx < arr.Length)
            return string.IsNullOrWhiteSpace(arr[idx]) ? null : arr[idx];
        return null;
    }

    // ---------- (이하 코드는 기존과 동일) ----------

    private Sprite LoadIconByStory(string iconKey, bool isPassive)
    {
        if (string.IsNullOrEmpty(iconKey))
            return null;

        int story = Mathf.Clamp(GameManager.instance?.Story ?? 0, 0, StoryRoots.Length - 1);
        string root = StoryRoots[story];
        string sub = isPassive ? "passive" : "Skill";
        string basePath = $"{root}/UI/{sub}";

        string cacheKey1 = $"{basePath}/{iconKey}";
        if (iconCache.TryGetValue(cacheKey1, out var cached1))
            return cached1;

        var sp = Resources.Load<Sprite>($"{basePath}/{iconKey}");
        if (sp == null)
        {
            var all = Resources.LoadAll<Sprite>(basePath);
            if (all != null && all.Length > 0)
                sp = System.Array.Find(all, s => s != null && s.name == iconKey);
        }
        if (sp != null)
        {
            iconCache[cacheKey1] = sp;
            return sp;
        }

        string cacheKey2 = $"SkillIcon/{iconKey}";
        if (iconCache.TryGetValue(cacheKey2, out var cached2))
            return cached2;

        sp = Resources.Load<Sprite>($"SkillIcon/{iconKey}");
        if (sp == null)
        {
            var all2 = Resources.LoadAll<Sprite>("SkillIcon");
            if (all2 != null && all2.Length > 0)
                sp = System.Array.Find(all2, s => s != null && s.name == iconKey);
        }
        if (sp != null) iconCache[cacheKey2] = sp;

        return sp;
    }

    private bool IsPassiveID(int id)
    {
        return id.ToString().StartsWith("2");
    }
}
