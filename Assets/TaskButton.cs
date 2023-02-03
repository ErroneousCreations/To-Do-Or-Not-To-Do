using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskButton : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int myTaskIndex;
    public PriorityEnum myPriority;
    public DateTime myCreatedDate, myDeadlineDate;
    public TMPro.TMP_Text myTitleText, myDateTimeText;
    public UnityEngine.UI.Image myPriorityIndicator, myImage;
    public string myTitle, myDesc;
    public Button mybutton;
    Transform originalparent;

    void Start()
    {
        originalparent = transform.parent;
    }

    public void DeleteThisTask()
    {
        UI_Manager.instance.DeleteTask(myTaskIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        transform.eulerAngles = new Vector3(0, 0, -2f);
        mybutton.enabled = false;
        transform.parent = UI_Manager.instance.transform; //thats the canvas
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(UI_Manager.instance.GetListToPlaceIn() != null) { transform.parent = UI_Manager.instance.GetListToPlaceIn(); UI_Manager.instance.SetTasksList(myTaskIndex, transform.parent); return; }
        transform.parent = originalparent;
        transform.localPosition = Vector3.zero;
        transform.eulerAngles = new Vector3(0, 0, 0);
        mybutton.enabled = true;
    }

    public void OpenMyDescription()
    {
        UI_Manager.instance.OpenTaskDesciptionMenu(myTitle, myDesc, myCreatedDate, myDeadlineDate, myTaskIndex);
    }

    private void Update()
    {
        myTitleText.text = myTitle;
        myDateTimeText.text = myDeadlineDate.ToString("dd/MM/yyyy HH:mm");
        myPriorityIndicator.color = myPriority == PriorityEnum.Low ? Color.blue : myPriority == PriorityEnum.Normal ? Color.green : Color.red;
        myImage.color = DateTime.Now.CompareTo(myDeadlineDate) < 0 ? Color.white : DateTime.Now.CompareTo(myDeadlineDate) == 0 ? Color.yellow : Color.red;
    }
}
