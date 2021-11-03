using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines a servant
/// </summary>
public class User
{
    public int Id;
    public string FullName;
}

public class Authenticator : MonoBehaviour
{
    #region Private Variables

    private static Authenticator _instance;

    [SerializeField] private string _authenticationURL;

    private User _userInfo;
    private bool _authenticated;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets an instance of the Authenticator
    /// </summary>
    public static Authenticator Instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Gets the user info
    /// </summary>
    public User UserInfo
    {
        get { return _userInfo; }
    }

    /// <summary>
    /// Gets the authentication status of the user
    /// </summary>
    public bool Authenticated
    {
        get { return _authenticated; }
    }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    #endregion

    #region Supporting Functions

    /// <summary>
    /// Authenticates the user
    /// </summary>
    public void Authenticate(string username, string password)
    {
        StartCoroutine(AuthenticationRoutine(username, password));
    }

    /// <summary>
    /// Logs the user out
    /// </summary>
    public void Logout()
    {
        PlayerPrefs.DeleteKey("name");
        PlayerPrefs.DeleteKey("key");
        PlayerPrefs.DeleteKey("remember");
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// The login routine
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    IEnumerator AuthenticationRoutine(string username, string password)
    {
        string encryptedUsername = Utilities.EncryptString(username);
        string encryptedPassword = Utilities.EncryptString(password);

        var uri = string.Format(_authenticationURL, encryptedUsername, encryptedPassword);

        //Processing the request
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    AuthUI.Instance.UpdateUserMsg("ERROR", request.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    AuthUI.Instance.UpdateUserMsg("ERROR", request.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Downloading the JSON data
                    var data = request.downloadHandler.text;

                    if (!string.IsNullOrEmpty(data))
                    {
                        string[] arr = data.Split(',');

                        _userInfo = new User();

                        _userInfo.Id = int.Parse(arr[0]);
                        _userInfo.FullName = arr[1];

                        _authenticated = true;

                        SceneManager.LoadScene(1);
                    }
                    else
                    {
                        AuthUI.Instance.UpdateUserMsg("ERROR", "Invalid Username or Password");
                        _authenticated = false;
                    }

                    break;
            }
        }
    }

    #endregion
}
