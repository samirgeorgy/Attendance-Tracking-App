using UnityEngine;
using UnityEngine.UI;

using Michsky.UI.ModernUIPack;

public class AuthUI : MonoBehaviour
{
    #region Private Variables

    private static AuthUI _instance;

    [SerializeField] private CustomInputField _usernameInput;
    [SerializeField] private CustomInputField _passwordInput;
    [SerializeField] private Toggle _rememberMe;
    [SerializeField] private NotificationManager _userNotification;
    [SerializeField] private Sprite _notificationIcon;
    [SerializeField] private GameObject _sandClock;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets an instance of the AuthUI
    /// </summary>
    public static AuthUI Instance
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
        int remember = PlayerPrefs.GetInt("remember", 0);

        if (remember == 1)
        {
            string name = PlayerPrefs.GetString("name");
            string key = PlayerPrefs.GetString("key");

            _usernameInput.inputText.text = name;
            _passwordInput.inputText.text = key;

            Authenticator.Instance.Authenticate(name, key);
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
    /// Executes the button click
    /// </summary>
    public void AuthenticateButtonClicked()
    {
        if (!string.IsNullOrEmpty(_usernameInput.inputText.text) && !string.IsNullOrEmpty(_passwordInput.inputText.text))
        {
            _sandClock.SetActive(true);

            if (_rememberMe.isOn)
            {
                PlayerPrefs.SetString("name", _usernameInput.inputText.text);
                PlayerPrefs.SetString("key", _passwordInput.inputText.text);
                PlayerPrefs.SetInt("remember", 1);
            }
            else
            {
                PlayerPrefs.DeleteKey("name");
                PlayerPrefs.DeleteKey("key");
                PlayerPrefs.DeleteKey("remember");
            }

            Authenticator.Instance.Authenticate(_usernameInput.inputText.text, _passwordInput.inputText.text);
        }
        else
            UpdateUserMsg("Missing Fields", "Please fill in the missing fields");
    }

    #endregion
}
