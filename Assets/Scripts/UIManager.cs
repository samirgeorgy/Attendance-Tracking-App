using UnityEngine;

using Michsky.UI.ModernUIPack;

public class UIManager : MonoBehaviour
{
    #region Private Variables

    private static UIManager _instance;

    [SerializeField] private NotificationManager _userNotification;
    [SerializeField] private CustomDropdown _dllClasses;
    [SerializeField] private Sprite _notificationIcon;
    [SerializeField] private Sprite _classIcon;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets an instance of the UIManager
    /// </summary>
    public static UIManager Instance
    {
        get { return _instance; }
    }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region Supporting Functions

    /// <summary>
    /// Updates the user notification message
    /// </summary>
    /// <param name="msg">The message to be displayed</param>
    /// <param name="title">The title of the notification</param>
    public void UpdateUserMsg(string title, string msg)
    {
        _userNotification.icon = _notificationIcon;
        _userNotification.title = title;
        _userNotification.description = msg;
        _userNotification.UpdateUI();
        _userNotification.OpenNotification();
    }

    /// <summary>
    /// Updates the list of classes with the date extracted from the Database
    /// </summary>
    /// <param name="classes">The list of classes to be displayed</param>
    public void UpdateClassesList(AvailableClass[] classes)
    {
        foreach (var item in classes)
        {
            _dllClasses.CreateNewItemFast(item.class_title, _classIcon);
            _dllClasses.SetupDropdown();
        }

        _dllClasses.dropdownEvent.AddListener(ApplicationManager.Instance.GetParticipants);

        _dllClasses.ChangeDropdownInfo(0);
        ApplicationManager.Instance.GetParticipants(0);
    }

    #endregion
}
