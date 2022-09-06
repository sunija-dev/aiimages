using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ContextMenuWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static ContextMenuWindow s_contextMenuWindowCurrent;

    public GameObject goOptionPrefab;

    private List<GameObject> liOptionGOs = new List<GameObject>();

    private float fCloseAfterXSecondsOutside = 1f;
    private Coroutine coDestroyCountdown;

    private void Start()
    {
        if (s_contextMenuWindowCurrent != null)
            Destroy(s_contextMenuWindowCurrent.gameObject);
        s_contextMenuWindowCurrent = this;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
            StartCoroutine(coDestroyDelayed()); // delay destroy, so option still receives click
    }

    private IEnumerator coDestroyDelayed()
    {
        yield return null;
        Destroy(this.gameObject);
    }

    public void Init(List<ContextMenu.Option> _liOptions)
    {
        foreach (GameObject goOption in liOptionGOs)
            Destroy(goOption);

        Transform transOptionParent = GetComponentInChildren<VerticalLayoutGroup>().transform;

        foreach (ContextMenu.Option option in _liOptions)
        {
            GameObject goOption = Instantiate(goOptionPrefab, transOptionParent);
            liOptionGOs.Add(goOption);

            // text
            goOption.GetComponentInChildren<TMP_Text>().text = option.strNameKey;

            // action
            EventTrigger eventTrigger = goOption.GetComponentInChildren<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) => { option.action.Invoke(); });
            eventTrigger.triggers.Add(entry);
        }

        PositionWindow();
    }

    private void PositionWindow()
    {
        // default position: bottom right
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Vector3 v3NewPos = gameObject.transform.position;
        v3NewPos.x += rect.sizeDelta.x / 2f;
        v3NewPos.y -= rect.sizeDelta.y / 2f;
        if (v3NewPos.x + rect.sizeDelta.x > Screen.width) v3NewPos.x -= rect.sizeDelta.x;
        if (v3NewPos.y - rect.sizeDelta.y < 0) v3NewPos.y += rect.sizeDelta.y;
        gameObject.transform.position = v3NewPos;
    }

    public void ClearOptions()
    {
        foreach (GameObject goOption in liOptionGOs)
            Destroy(goOption);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (coDestroyCountdown != null)
            StopCoroutine(coDestroyCountdown);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (coDestroyCountdown != null)
            StopCoroutine(coDestroyCountdown);

        coDestroyCountdown = StartCoroutine(coDestroyDelayed(fCloseAfterXSecondsOutside));
    }

    private IEnumerator coDestroyDelayed(float _fDestroyAfter)
    {
        yield return new WaitForSeconds(_fDestroyAfter);
        Destroy(this.gameObject);
    }


}
