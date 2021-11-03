using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Michsky.UI.ModernUIPack;

public class UIManager : MonoBehaviour
{
    #region Private Variables

    private static UIManager _instance;

    [SerializeField] private NotificationManager _userNotification;
    [SerializeField] private CustomDropdown _dllClasses;
    [SerializeField] private CustomDropdown _dllServants;
    [SerializeField] private Sprite _notificationIcon;
    [SerializeField] private Sprite _classIcon;
    [SerializeField] private Text _dateText;
    [SerializeField] private Text _servantNameText;
    [SerializeField] private Text _connectionStatusText;
    [SerializeField] private Text _versionText;
    [SerializeField] private Text _cameraPanelDateText;
    [SerializeField] private Text _cameraPanelSessionText;
    [SerializeField] private GameObject _sandClock;

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
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-EG");

            _dateText.text = System.DateTime.Now.ToShortDateString();
            _servantNameText.text = Authenticator.Instance.UserInfo.FullName;
            _cameraPanelDateText.text = System.DateTime.Now.ToShortDateString();
            UpdateSessionNumberInCameraView(1);
            _versionText.text = "v" + Application.version;
        }
        
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
            _dllClasses.CreateNewItemFast(item.ClassTitle, _classIcon);
            _dllClasses.SetupDropdown();
        }

        _dllClasses.dropdownEvent.AddListener(ApplicationManager.Instance.GetParticipants);
        _dllClasses.dropdownEvent.AddListener(ApplicationManager.Instance.AssignClass);

        _dllClasses.ChangeDropdownInfo(0);
        ApplicationManager.Instance.AssignClass(0);
        ApplicationManager.Instance.GetParticipants(0);
    }

    /// <summary>
    /// Updates the connection status text on the UI
    /// </summary>
    /// <param name="status">The connection status bool</param>
    public void UpdateConnectionStatus (bool status)
    {
        if (status == true)
        {
            _connectionStatusText.text = "ONLINE";
            _connectionStatusText.color = Color.green;
        }
        else
        {
            _connectionStatusText.text = "OFFLINE";
            _connectionStatusText.color = Color.red;
        }
    }

    /// <summary>
    /// Updates the session number in the camera view
    /// </summary>
    /// <param name="n">The session number</param>
    public void UpdateSessionNumberInCameraView(int n)
    {
        _cameraPanelSessionText.text = n.ToString();
    }

    /// <summary>
    /// Shows or hides the sand clock icon in the camera panel
    /// </summary>
    /// <param name="status">The toggle value of the sand clock</param>
    public void ToggleSandClock(bool status)
    {
        _sandClock.SetActive(status);
    }

    #endregion
}
