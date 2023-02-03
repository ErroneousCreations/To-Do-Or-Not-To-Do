using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;

[Serializable]
public class SavedBackgrounds
{
    public List<string> BackgroundPaths;
}
public enum PriorityEnum { Low, Normal, High }
public enum TaskListEnum { Todo, Doing, Done }
public enum SortType { Alphabetically, Priority, Dueby, Created }
[Serializable]
public class SavedTasks
{
    [Serializable]
    public struct TaskStruct
    {
        public string myTitle, myDesc;
        public TaskListEnum myList;
        public SerializableDateTime mysetdate, mydeadline;
        public PriorityEnum myPriority;

        public TaskStruct(string title, string desc, SerializableDateTime createddate, SerializableDateTime deadline, PriorityEnum priority, TaskListEnum list)
        {
            myTitle = title;
            myDesc = desc;
            mysetdate = createddate;
            mydeadline = deadline;
            myPriority = priority;
            myList = list;
        }
    }
    public List<TaskStruct> Tasks;
}


public class UI_Manager : MonoBehaviour
{
    public static readonly int[] MonthDayAmounts = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    public TMP_Dropdown sortingdropdown;
    [Header("Task Description Menu")]
    public GameObject TaskMenu;
    public TMP_Text taskTitle, taskText, createdtimeText, deadlinetimeText, remainingdaysText;
    int currentopentask;
    [Header("Menus")]
    public GameObject looksMenu;
    public GameObject tasksMenu, upcomingMenu;
    [Header("Background")]
    public Image backgroundImage;
    public GameObject baseBackgroundSelectorOb;
    public TMP_InputField backgroundpathfield;
    string currentBackgroundpath;
    [Header("Tasks List")]
    public GameObject baseTaskButton;
    [Header("Add Tasks Menu")]
    public GameObject AddTaskMenu;
    public DateTimePicker picker;
    public TMP_InputField nameinput, descinput;
    string currentnameselected, currentdescselected;
    int currentpriorityselected;
    public Slider priorSlider;
    [Header("Edit Tasks Menu")]
    public GameObject EditTaskMenu;
    public DateTimePicker edit_picker;
    public TMP_InputField editnameinput, editdescinput;
    string newnameselected, newdescselected;
    public Slider EditSlider;
    int newpriorityselected;
    [Header("Drag and Drop")]
    public RectTransform todo;
    public RectTransform doing, done;
    public Transform todoContents, doingContents, doneContents;
    [Header("UpcomingMenu")]
    public GameObject BaseUpcomingMenuObject;
    public GameObject nothingUpcomingOb;
    static string backgroundpath;
    static string saveddatapath;
    SortType currentSortingMethod = SortType.Alphabetically;

    //sorting functions
    static int SortTasksByAlphabet(SavedTasks.TaskStruct a, SavedTasks.TaskStruct b)
    {
        return a.myTitle.CompareTo(b.myTitle);
    }
    static int SortTasksByDue(SavedTasks.TaskStruct a, SavedTasks.TaskStruct b)
    {
        return a.mydeadline.DateTime.CompareTo(b.mydeadline.DateTime);
    }
    static int SortTasksByCreated(SavedTasks.TaskStruct a, SavedTasks.TaskStruct b)
    {
        return a.mysetdate.DateTime.CompareTo(b.mysetdate.DateTime);
    }
    static int SortTasksByPriority(SavedTasks.TaskStruct a, SavedTasks.TaskStruct b)
    {
        int aprior = (int)a.myPriority;
        int bprior = (int)b.myPriority;
        return bprior.CompareTo(aprior);
    }
    //sorting functions end

    public static UI_Manager instance;

    public void Quit()
    {
        Application.Quit();
    }

    public void SetDarkMode(bool value)
    {
        PlayerPrefs.SetInt("DarkMode", value ? 1 : 0);
    }

    private void Awake()
    {
        backgroundpath = Application.dataPath + "/backgrounds";
        saveddatapath = Application.dataPath + "/saveddata";
        instance = this;
        if (!Directory.Exists(backgroundpath))
        {
            Directory.CreateDirectory(backgroundpath);
        }

        if (!Directory.Exists(saveddatapath))
        {
            Directory.CreateDirectory(saveddatapath);
        }

        if(!File.Exists(saveddatapath + "/backgroundslist.pp"))
        {
            File.WriteAllText(saveddatapath + "/backgroundslist.pp", JsonUtility.ToJson(new SavedBackgrounds()));
        }
        if (!File.Exists(saveddatapath + "/taskslist.pp"))
        {
            File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(new SavedTasks()));
        }
        currentSortingMethod = (SortType)PlayerPrefs.GetInt("SortingMethod", 0); //get the saved sort type
        sortingdropdown.value = PlayerPrefs.GetInt("SortingMethod", 0);
        UpdateTasksList();
        UpdateBackgroundsList();
        SetExistingBackgroundImage(PlayerPrefs.GetString("MyImagePath", ""));
    }

    public static Texture2D LoadImage(string path)
    {
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }
        else
        {
            return null;
        }
    }
    public static Sprite LoadImageAsSprite(string path)
    {
        Texture2D tex = LoadImage(path);
        if (!tex) { return null; }
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, LoadImage(path).width,
        LoadImage(path).height), new Vector2(0.5f, 0.5f), 100.0f);
        return sprite;
    }
    public void UpdateBackgroundsList()
    {
        foreach (Transform item in baseBackgroundSelectorOb.transform.parent)
        {
            if(item.gameObject != baseBackgroundSelectorOb) { Destroy(item.gameObject); }
        }
        List<string> paths = JsonUtility.FromJson<SavedBackgrounds>(File.ReadAllText(saveddatapath + "/backgroundslist.pp")).BackgroundPaths;

        //make the clear background button
        GameObject deletob = Instantiate(baseBackgroundSelectorOb, baseBackgroundSelectorOb.transform.parent);
        deletob.SetActive(true);
        deletob.transform.GetChild(1).gameObject.SetActive(false);
        deletob.GetComponent<Button>().onClick.AddListener(delegate { ClearBackgroundImage(); }); //make the button work (probably)
        deletob.transform.GetChild(2).gameObject.SetActive(false); //remove the delete button button
        //

        for (int i = 0; i < paths.Count; i++)
        {
            int x = i;
            GameObject ob = Instantiate(baseBackgroundSelectorOb, baseBackgroundSelectorOb.transform.parent);
            ob.SetActive(true);
            ob.transform.GetChild(1).GetComponent<Image>().sprite = LoadImageAsSprite(paths[x]);
            ob.GetComponent<Button>().onClick.AddListener(delegate { SetExistingBackgroundImage(paths[x]); }); //make the button work (probably)
            ob.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { DeleteBackgroundImage(x); }); //delete button
        }
    }
    private void Update()
    {
        backgroundImage.gameObject.SetActive(backgroundImage.sprite);
    }
    public void UpdateTasksList()
    {
        foreach (Transform item in todoContents.transform)
        {
            if (item.gameObject != baseTaskButton) { Destroy(item.gameObject); }
        }
        foreach (Transform item in doingContents.transform)
        {
            if (item.gameObject != baseTaskButton) { Destroy(item.gameObject); }
        }
        foreach (Transform item in doneContents.transform)
        {
            if (item.gameObject != baseTaskButton) { Destroy(item.gameObject); }
        }
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        
        for (int i = 0; i < currenttasks.Tasks.Count; i++)
        {
            int x = i;
            GameObject ob = Instantiate(baseTaskButton, currenttasks.Tasks[i].myList == TaskListEnum.Todo ? todoContents : currenttasks.Tasks[i].myList == TaskListEnum.Doing ? doingContents : doneContents);
            ob.SetActive(true);
            TaskButton task = ob.GetComponent<TaskButton>();
            task.myTaskIndex = x;
            task.myCreatedDate = currenttasks.Tasks[i].mysetdate.DateTime;
            task.myDeadlineDate = currenttasks.Tasks[i].mydeadline.DateTime;
            task.myDesc = currenttasks.Tasks[i].myDesc;
            task.myTitle = currenttasks.Tasks[i].myTitle;
            task.myPriority = currenttasks.Tasks[i].myPriority;
            //ob.GetComponent<Button>().onClick.AddListener(delegate { SetExistingBackgroundImage(paths[x]); }); //make the button work (probably)
        }
    }
    public void UpdateUpcomingTasksList() //literally a copy of the tasks list but slightly different
    {
        foreach (Transform item in BaseUpcomingMenuObject.transform.parent)
        {
            if (item.gameObject != BaseUpcomingMenuObject) { Destroy(item.gameObject); }
        }
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        currenttasks.Tasks.Sort(SortTasksByDue);
        bool noupcoming = true;
        for (int i = 0; i < currenttasks.Tasks.Count; i++)
        {
            if (currenttasks.Tasks[i].mydeadline.DateTime.Subtract(DateTime.Now).TotalDays > 7 || currenttasks.Tasks[i].myList == TaskListEnum.Done) { continue; } //only include stuff under 7 weeks due and not already done
            noupcoming = false; 
            int x = i;
            GameObject ob = Instantiate(BaseUpcomingMenuObject, BaseUpcomingMenuObject.transform.parent);
            ob.SetActive(true);
            TaskButton task = ob.GetComponent<TaskButton>();
            task.myTaskIndex = x;
            task.myCreatedDate = currenttasks.Tasks[i].mysetdate.DateTime;
            task.myDeadlineDate = currenttasks.Tasks[i].mydeadline.DateTime;
            task.myDesc = currenttasks.Tasks[i].myDesc;
            task.myTitle = currenttasks.Tasks[i].myTitle;
            task.myPriority = currenttasks.Tasks[i].myPriority;
        }
        nothingUpcomingOb.SetActive(noupcoming);
    }
    public void SetTaskSortingMethod(int sortingmethod)
    {
        currentSortingMethod = (SortType)sortingmethod;
        PlayerPrefs.SetInt("SortingMethod", sortingmethod);
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        if (currentSortingMethod == SortType.Alphabetically)
        {
            currenttasks.Tasks.Sort(SortTasksByAlphabet);
        }
        else if (currentSortingMethod == SortType.Priority)
        {
            currenttasks.Tasks.Sort(SortTasksByPriority);
        }
        else if (currentSortingMethod == SortType.Dueby)
        {
            currenttasks.Tasks.Sort(SortTasksByDue);
        }
        else if (currentSortingMethod == SortType.Created)
        {
            currenttasks.Tasks.Sort(SortTasksByCreated);
        }
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        UpdateTasksList(); //apply the sorting
    }
    public Transform GetListToPlaceIn()
    {
        if(RectTransformUtility.RectangleContainsScreenPoint(todo, Input.mousePosition))
        {
            return todoContents;
        }
        else if(RectTransformUtility.RectangleContainsScreenPoint(doing, Input.mousePosition))
        {
            return doingContents;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(done, Input.mousePosition))
        {
            return doneContents;
        }
        else
        {
            return null;
        }
    }
    public void CreateNewTask()
    {
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        currenttasks.Tasks.Add(new SavedTasks.TaskStruct(currentnameselected, currentdescselected, new SerializableDateTime(DateTime.Now), new SerializableDateTime(ConstructDateTime()), (PriorityEnum)currentpriorityselected, TaskListEnum.Todo));
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        UpdateTasksList();
        CloseTaskCreateMenu();
    }
    public void SetTasksList(int index, Transform newcontents)
    {
        TaskListEnum newlist = newcontents == todoContents ? TaskListEnum.Todo : newcontents == doingContents ? TaskListEnum.Doing : TaskListEnum.Done;
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        currenttasks.Tasks[index] = new SavedTasks.TaskStruct(currenttasks.Tasks[index].myTitle, currenttasks.Tasks[index].myDesc, currenttasks.Tasks[index].mysetdate, currenttasks.Tasks[index].mydeadline, currenttasks.Tasks[index].myPriority, newlist);
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        UpdateTasksList();
    }

    public void DeleteTask(int index)
    {
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        currenttasks.Tasks.RemoveAt(index);
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        UpdateTasksList();
    }

    public void SetLooksMenuOpen()
    {
        tasksMenu.SetActive(false);
        upcomingMenu.SetActive(false);
        looksMenu.SetActive(true);
        UpdateBackgroundsList();
    }

    public void SetTasksMenuOpen()
    {
        tasksMenu.SetActive(true);
        upcomingMenu.SetActive(false);
        looksMenu.SetActive(false);
        UpdateTasksList();
    }

    public void SetUpcomingMenuOpen()
    {
        tasksMenu.SetActive(false);
        upcomingMenu.SetActive(true);
        looksMenu.SetActive(false);
        UpdateUpcomingTasksList();
    }

    public void OpenTaskCreateMenu()
    {
        nameinput.text = "";
        descinput.text = "";
        picker.SetSelectedDateTime(DateTime.Now);
        AddTaskMenu.SetActive(true);
        priorSlider.value = 0;
    }
    public void CloseTaskCreateMenu()
    {
        AddTaskMenu.SetActive(false);
    }

    public void OpenTaskEditMenu()
    {
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));

        editnameinput.text = currenttasks.Tasks[currentopentask].myTitle;
        editdescinput.text = currenttasks.Tasks[currentopentask].myDesc;
        edit_picker.SetSelectedDateTime(currenttasks.Tasks[currentopentask].mydeadline.DateTime);
        EditSlider.value = (int)currenttasks.Tasks[currentopentask].myPriority;
        EditTaskMenu.SetActive(true);
        CloseTaskDescMenu();

    }
    public void ConfirmEditTask()
    {
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        currenttasks.Tasks[currentopentask] = new SavedTasks.TaskStruct(newnameselected, newdescselected, currenttasks.Tasks[currentopentask].mysetdate, new SerializableDateTime((DateTime)edit_picker.SelectedDateTime()), (PriorityEnum)newpriorityselected, currenttasks.Tasks[currentopentask].myList);
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        CloseTaskEditMenu();
        UpdateTasksList();
    }
    public void CloseTaskEditMenu()
    {
        EditTaskMenu.SetActive(false);
    }

    public void OpenTaskDesciptionMenu(string title, string desc, DateTime created, DateTime deadline, int index)
    {
        TaskMenu.SetActive(true);
        taskTitle.text = title + (DateTime.Now.CompareTo(deadline) > 0 ? " (OVERDUE)" : "");
        taskText.text = desc;
        createdtimeText.text = created.ToString("dd/MM/yyyy HH:mm");
        deadlinetimeText.text = deadline.ToString("dd/MM/yyyy HH:mm");
        remainingdaysText.text = Mathf.Round((float)deadline.Subtract(DateTime.Now).TotalDays) + " Days Ahead";
        currentopentask = index;
    }
    public void CloseTaskDescMenu()
    {
        TaskMenu.SetActive(false);
    }
    public void DuplicateTask()
    {
        SavedTasks currenttasks = JsonUtility.FromJson<SavedTasks>(File.ReadAllText(saveddatapath + "/taskslist.pp"));
        SavedTasks.TaskStruct task = currenttasks.Tasks[currentopentask];
        currenttasks.Tasks.Add(task); //add the duplicate task
        File.WriteAllText(saveddatapath + "/taskslist.pp", JsonUtility.ToJson(currenttasks));
        CloseTaskDescMenu();
        UpdateTasksList();
    }
    public void SetCurrentPath(string path)
    {
        currentBackgroundpath = path;
    }

    //add task
    public void SetCurrentName(string path)
    {
        currentnameselected = path;
    }
    public void SetCurrentDesc(string path)
    {
        currentdescselected = path;
    }
    public void SetCurrentPriority(float prior)
    {
        currentpriorityselected = (int)prior;
    }

    //edit task
    public void SetNewName(string path)
    {
        newnameselected = path;
    }
    public void SetNewtDesc(string path)
    {
        newdescselected = path;
    }
    public void SetNewPriority(float prior)
    {
        newpriorityselected = (int)prior;
    }

    public DateTime ConstructDateTime()
    {
        return (DateTime)picker.SelectedDateTime();
    }
    public void ClearBackgroundImage()
    {
        PlayerPrefs.SetString("MyImagePath", "");
        backgroundImage.sprite = null;
    }
    public void SetExistingBackgroundImage(string path)
    {
        Sprite sprite = LoadImageAsSprite(path);
        if (!sprite) { return; }
        backgroundImage.sprite = sprite;
        PlayerPrefs.SetString("MyImagePath", path);
    }
    public void DeleteBackgroundImage(int index)
    {
        List<string> paths = JsonUtility.FromJson<SavedBackgrounds>(File.ReadAllText(saveddatapath + "/backgroundslist.pp")).BackgroundPaths;
        File.Delete(paths[index]); //delete the file
        paths.RemoveAt(index);
        SavedBackgrounds backgrounds = new SavedBackgrounds();
        backgrounds.BackgroundPaths = paths;
        File.WriteAllText(saveddatapath + "/backgroundslist.pp", JsonUtility.ToJson(backgrounds));
        UpdateBackgroundsList();
        PlayerPrefs.SetString("MyImagePath", "");
    }
    public void SetBackgroundImage() //sets a new background image and adds it to the list of existing ones
    {
        string path = currentBackgroundpath.Replace("\\", "/").Replace("C:", "");
        Sprite sprite = LoadImageAsSprite(path);
        if (!sprite) { return; }
        List<string> paths = JsonUtility.FromJson<SavedBackgrounds>(File.ReadAllText(saveddatapath + "/backgroundslist.pp")).BackgroundPaths;
        string newpath = backgroundpath + "/" + PlayerPrefs.GetInt("ImageId", 0) +".png";
        PlayerPrefs.SetInt("ImageId", PlayerPrefs.GetInt("ImageId", 0) + 1);
        File.WriteAllBytes(newpath, File.ReadAllBytes(path)); //save a copy to the backgrounds folder
        paths.Add(newpath);
        SavedBackgrounds backgrounds = new SavedBackgrounds();
        backgrounds.BackgroundPaths = paths;
        File.WriteAllText(saveddatapath + "/backgroundslist.pp", JsonUtility.ToJson(backgrounds));
        backgroundImage.sprite = sprite;
        UpdateBackgroundsList();
        PlayerPrefs.SetString("MyImagePath", newpath);
        backgroundpathfield.text = "";
    }
}
