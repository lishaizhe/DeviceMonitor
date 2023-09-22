using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 实时事件和报警
/// </summary>
public class WindowAlarm : WindowBase
{

    [Header("Settings")]
    [SerializeField] private GameObject m_AlarmContent;
    [SerializeField] private AlarmItem m_AlarmItem;

    private float _eventUpdateTime = 2;

    [Header("Animation")]
    [SerializeField] protected GameObject m_AnimationWidonw;   //动画

    public override UITpye WindowType => UITpye.Sub;

    //UI界面
    private AlarmItem[] _uiAlarmItems;
    private bool _isAllSelect = false;

    protected override void Init()
    {
        _uiAlarmItems = m_AlarmContent.GetComponentsInChildren<AlarmItem>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateUI();
    }

    public override void OpenWindow()
    {
        m_AnimationWidonw.SetActive(false);
        gameObject.SetActive(true);
        WindowManager.Instance.StartWindowAnimation(
            m_AnimationWidonw.transform as RectTransform,
            true,
            WindowAnimationType.Right2Left,
            0.3f
            );
    }

    public override void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 选择所有报警item
    /// </summary>
    public void SelectAllItem()
    {
        var items = m_AlarmContent.GetComponentsInChildren<AlarmItem>();
        if (items == null)
        {
            return;
        }
        _isAllSelect = !_isAllSelect;
        foreach (var item in _uiAlarmItems)
        {
            item.IsSelect = _isAllSelect;
        }
    }

    private float _sumTime = 0.0f;
    private void Update()
    {
        _sumTime += Time.deltaTime;
        if (_sumTime > _eventUpdateTime)
        {
            UpdateUI();
            _sumTime = 0;
        }
    }


    private void UpdateUI()
    {
        Init();
        var eventData = DataManager.Instance.DeviceEventDict;
        if (eventData == null)
        {
            return;
        }
        var addAlarmIdList = new List<string>();
        addAlarmIdList.AddRange(eventData.Keys.ToArray());
        if (_uiAlarmItems != null)
        {
            //删除没有的item
            for (int i = 0; i < _uiAlarmItems.Length; i++)
            {
                var id = _uiAlarmItems[i].AlarmID;
                if (id == null || !eventData.ContainsKey(id))
                {
                    _uiAlarmItems[i].DestroyAlarm();
                }
                else 
                {
                    addAlarmIdList.Remove(id);
                }
            }
        }

        // 更新界面事件
        for (int i = 0; i < addAlarmIdList.Count; i++)
        {
                var item = Instantiate(m_AlarmItem, m_AlarmContent.transform,false);
                var info = eventData[addAlarmIdList[i]];
                item.SetAlarmInfo(info.id, info.starttime, info.endtime, info.name, info.msg, info.fieldrealname,info.allremarks);
        }
    }

    public void SubmitData()
    {
        Init();
        var item = new SetTemplateEvent();
        item.pagename = "实时事件";
        //
        foreach (var alarm in _uiAlarmItems)
        {      
            if (alarm.IsSelect)
            {
                item.set.Add(alarm.AlarmID);
            }
        }
        if (item.set.Count < 1)
        {
            return;
        }
        // 确认事件
        var result = DataManager.Instance.SetTemplateEvent(item);
        if (result != null)
        {
            if (result == "false")
            {
                Debug.Log("提交数据失败");
            }
            CloseWindow();
        }
    }
}
