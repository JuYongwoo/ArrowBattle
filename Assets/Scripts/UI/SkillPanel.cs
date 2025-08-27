using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillPanel : MonoBehaviour
{
    private enum SkillPanelEnum
    {
        Skill1CoolFadeImg, Skill2CoolFadeImg, Skill3CoolFadeImg, Skill4CoolFadeImg, Skill5CoolFadeImg,
        Skill1CoolTxt, Skill2CoolTxt, Skill3CoolTxt, Skill4CoolTxt, Skill5CoolTxt,
        Skill1IconImg, Skill2IconImg, Skill3IconImg, Skill4IconImg, Skill5IconImg
    }

    private Dictionary<SkillPanelEnum, GameObject> map;

    // 스킬별 상태(1~5 사용)
    private readonly float[] remain = new float[6];
    private readonly Dictionary<int, Coroutine> imgCo = new();
    private readonly Dictionary<int, Coroutine> txtCo = new();
    private readonly Dictionary<int, float> baseHeight = new();   // FadeImg 기준 높이

    private WaitForSeconds wait1s;

    private void Awake()
    {
        wait1s = new WaitForSeconds(1f);
        map = Util.mapDictionaryInChildren<SkillPanelEnum, GameObject>(gameObject);
        for (int i = 1; i <= 5; i++) //SO 토대로 스킬 아이콘 설정
        {

            // SkillPanelEnum 이름 만들기
            var enumName = $"Skill{i}IconImg";
            var panelEnum = (SkillPanelEnum)System.Enum.Parse(typeof(SkillPanelEnum), enumName);

            // Skills enum도 i에 맞게 선택
            var skillEnum = (Skills)System.Enum.Parse(typeof(Skills), $"Skill{i}");

            // 아이콘 적용
            map[panelEnum].GetComponent<Image>().sprite = ManagerObject.skillInfoM.attackSkillData[skillEnum].skillIcon;
        }
        ManagerObject.skillInfoM.cooldownUI = StartCooldown;
    }

    private void Start()
    {

        for (int i = 1; i <= 5; i++)
        {
            StartCooldown(i, 0);
        }
    }

    public void StartCooldown(int skillNumber, float durationSec)
    {
        if (skillNumber < 1 || skillNumber > 5) return;

        // 기존 코루틴 종료
        if (imgCo.TryGetValue(skillNumber, out var ic) && ic != null) StopCoroutine(ic);
        if (txtCo.TryGetValue(skillNumber, out var tc) && tc != null) StopCoroutine(tc);

        var rt = GetFadeRect(skillNumber);
        if (rt == null) return;

        EnsureBottomAnchored(rt);

        // 기준 높이 캐싱(없으면 현재 높이 사용)
        if (!baseHeight.ContainsKey(skillNumber))
            baseHeight[skillNumber] = rt.rect.height > 0 ? rt.rect.height : rt.sizeDelta.y;

        // 초기 상태: 100%
        SetHeight(rt, baseHeight[skillNumber]);

        // 텍스트 초기화
        SetText(skillNumber, Mathf.CeilToInt(durationSec).ToString());

        remain[skillNumber] = durationSec;

        // 코루틴 시작
        imgCo[skillNumber] = StartCoroutine(CoShrinkFade(skillNumber, durationSec));
        txtCo[skillNumber] = StartCoroutine(CoTextTick(skillNumber, Mathf.CeilToInt(durationSec)));
    }

    public void CancelCooldown(int skillNumber)
    {
        if (skillNumber < 1 || skillNumber > 5) return;

        if (imgCo.TryGetValue(skillNumber, out var ic) && ic != null) StopCoroutine(ic);
        if (txtCo.TryGetValue(skillNumber, out var tc) && tc != null) StopCoroutine(tc);
        imgCo[skillNumber] = txtCo[skillNumber] = null;

        var rt = GetFadeRect(skillNumber);
        if (rt != null) SetHeight(rt, 0f);

        SetText(skillNumber, "");
        remain[skillNumber] = 0f;
    }

    // ======================= Coroutines =======================

    // 크기 줄이기: 위에서 아래로(위치 고정, 높이만 감소)
    private IEnumerator CoShrinkFade(int idx, float duration)
    {
        var rt = GetFadeRect(idx);
        if (rt == null) yield break;

        float H = baseHeight[idx];
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(1f - t / duration); // 1→0
            SetHeight(rt, H * ratio);
            remain[idx] = Mathf.Max(0f, duration - t);
            yield return null;
        }

        SetHeight(rt, 0f);
        remain[idx] = 0f;
        imgCo[idx] = null;
    }

    private IEnumerator CoTextTick(int idx, int secs)
    {
        int t = secs;
        while (t > 0)
        {
            SetText(idx, t.ToString());
            yield return wait1s;
            t--;
        }
        SetText(idx, "");
        txtCo[idx] = null;
    }

    // ======================= UI helpers =======================

    private RectTransform GetFadeRect(int idx)
    {
        if (!map.TryGetValue(ToEnum($"Skill{idx}CoolFadeImg"), out var go) || go == null) return null;
        return go.transform as RectTransform;
    }

    private void SetHeight(RectTransform rt, float h)
    {
        var sd = rt.sizeDelta;
        sd.y = h;
        rt.sizeDelta = sd;
        // anchoredPosition.y는 유지되며, 위 고정(pivot=1, anchor=1) 상태에서 아래로만 줄어듭니다.
    }

    private void EnsureBottomAnchored(RectTransform rt)
    {
        // 아래(Bottom)에 고정 → 높이를 줄이면 위쪽이 내려오며 ‘아래로 깎이는’ 느낌이 난다
        rt.pivot = new Vector2(rt.pivot.x, 0f);
        rt.anchorMin = new Vector2(rt.anchorMin.x, 0f);
        rt.anchorMax = new Vector2(rt.anchorMax.x, 0f);
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0f);
    }
    private void SetText(int idx, string s)
    {
        if (!map.TryGetValue(ToEnum($"Skill{idx}CoolTxt"), out var go) || go == null) return;

        if (go.TryGetComponent<TextMeshProUGUI>(out var tmp)) tmp.text = s;
        else if (go.TryGetComponent<Text>(out var uiText)) uiText.text = s;
    }

    private static SkillPanelEnum ToEnum(string name)
        => (SkillPanelEnum)System.Enum.Parse(typeof(SkillPanelEnum), name);
}
