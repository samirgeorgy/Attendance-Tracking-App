using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using Michsky.UI.ModernUIPack;

#region JSON Classes

/// <summary>
/// Root Class containing list of classes for JSON parsing
/// </summary>
[Serializable]
public class RootClasses
{
    public AvailableClass[] classes;
}

/// <summary>
/// Defines an available class
/// </summary>
[Serializable]
public class AvailableClass
{
    public int Id;
    public string ClassTitle;
}

/// <summary>
/// Root Class containing list of participants for JSON parsing
/// </summary>
[Serializable]
public class RootParticipant
{
    public ClassParticipant[] participants;
}

/// <summary>
/// Defines a class participant
/// </summary>
[Serializable]
public class ClassParticipant
{
    public int Id;
    public string FullName;
}

/// <summary>
/// Root Class containing list of servants for JSON parsing
/// </summary>
[Serializable]
public class RootServant
{
    public Servant[] servants;
}

/// <summary>
/// Defines a servant participant
/// </summary>
[Serializable]
public class Servant
{
    public int Id;
    public string FullName;
}

#endregion

public class ApplicationManager : MonoBehaviour
{
    #region Private Variables

    static private ApplicationManager _instance;

    [SerializeField] private string _classesURL;
    [SerializeField] private string _participantsURL;
    [SerializeField] private string _servantsURL;
    [SerializeField] private CustomDropdown _dllClasses;
    [SerializeField] private CustomDropdown _dllServants;
    [SerializeField] private CustomDropdown _dllSessions;

    private AvailableClass[] _availableClasses;
    private ClassParticipant[] _classParticipants;
    private Servant[] _servants;
    private int _selectedSession = 1;
    private int _selectedClass = 0;
    private int _servantID = -1;

    private bool _dataDownloaded = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// An instance of the application manager
    /// </summary>
    public static ApplicationManager Instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Gets a list of available classes
    /// </summary>
    public AvailableClass[] AvailableClasses
    {
        get { return _availableClasses; }
    }

    /// <summary>
    /// Gets a list of class participants
    /// </summary>
    public ClassParticipant[] ClassParticipants
    {
        get { return _classParticipants; }
    }

    /// <summary>
    /// Gets a list of the servants
    /// </summary>
    public Servant[] Servants
    {
        get { return _servants; }
    }

    /// <summary>
    /// Gets the ID of the current selected session
    /// </summary>
    public int SelectedSession
    {
        get { return _selectedSession; }
    }

    /// <summary>
    /// Gets the ID of the current selected servant
    /// </summary>
    public int SelectedServant
    {
        get { return _servantID; }
    }

    /// <summary>
    /// Gets the ID of the current selected Class
    /// </summary>
    public int SelectedClass
    {
        get { return _selectedClass; }
    }

    /// <summary>
    /// Gets the status of the downloaded data
    /// </summary>
    public bool DataDownloaded
    {
        get { return _dataDownloaded; }
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
        StartCoroutine(GetClassesRoutine());
        StartCoroutine(GetServantsRoutine());

        _dllSessions.dropdownEvent.AddListener(AssignSession);
    }

    #endregion

    #region Supporting Functions

    /// <summary>
    /// Gets The participant list when selected from the drop down list.
    /// </summary>
    /// <param name="n">The index of the selected class in the list</param>
    public void GetParticipants(int n)
    {
        StartCoroutine(GetParticipantsRoutine(_selectedClass));
    }

    /// <summary>
    /// Assigns the current Class
    /// </summary>
    /// <param name="n">The index of the Class</param>
    public void AssignClass(int n)
    {
        string selectedItem = _dllClasses.dropdownItems[n].itemName;

        if (selectedItem != null)
        {
            foreach (var item in _availableClasses)
            {
                if (selectedItem.Equals(item.ClassTitle))
                {
                    _selectedClass = item.Id;

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Assigns the current Servant
    /// </summary>
    /// <param name="n">The index of the servant</param>
    public void AssignServant(int n)
    {
        string selectedItem = _dllServants.dropdownItems[n].itemName;

        if (selectedItem != null)
        {
            foreach (var item in _servants)
            {
                if (selectedItem.Equals(item.FullName))
                {
                    _servantID = item.Id;

                    PlayerPrefs.SetString("ServantName", item.FullName);

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Changes the session ID
    /// </summary>
    /// <param name="n">The index of the session</param>
    public void AssignSession(int n)
    {
        if (_dllSessions.dropdownItems[n].itemName.Equals("Session 1"))
            _selectedSession = 1;
        else if (_dllSessions.dropdownItems[n].itemName.Equals("Session 2"))
            _selectedSession = 2;

        UIManager.Instance.UpdateSessionNumberInCameraView(_selectedSession);
    }

    /// <summary>
    /// Changes the attendance session
    /// </summary>
    /// <param name="n">The session number</param>
    public void ChangeSession(int n)
    {
        _selectedSession = n;
    }

    /// <summary>
    /// Reads the data from the web service and adding them to the interface
    /// </summary>
    IEnumerator GetClassesRoutine()
    {
        //Processing the request
        using (UnityWebRequest request = UnityWebRequest.Get(_classesURL))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    _dataDownloaded = false;
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    _dataDownloaded = false;
                    break;
                case UnityWebRequest.Result.Success:
                    //Downloading the JSON data
                    var jsonData = request.downloadHandler.text;
                    var jsonClasses = JsonUtility.FromJson<RootClasses>("{\"classes\":" + jsonData + "}");

                    _availableClasses = jsonClasses.classes;

                    _dataDownloaded = true;

                    //Adding the data to the interface list
                    UIManager.Instance.UpdateClassesList(_availableClasses);
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the data from the web service and adding them to the interface
    /// </summary>
    IEnumerator GetParticipantsRoutine(int classID)
    {
        //Processing the request
        using (UnityWebRequest request = UnityWebRequest.Get(_participantsURL + classID.ToString()))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    _dataDownloaded = false;
                    UIManager.Instance.UpdateConnectionStatus(false);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    _dataDownloaded = false;
                    UIManager.Instance.UpdateConnectionStatus(false);
                    break;
                case UnityWebRequest.Result.Success:
                    //Downloading the JSON data
                    var jsonData = request.downloadHandler.text;
                    var jsonParticipants = JsonUtility.FromJson<RootParticipant>("{\"participants\":" + jsonData + "}");

                    _classParticipants = jsonParticipants.participants;

                    _dataDownloaded = true;

                    UIManager.Instance.UpdateConnectionStatus(true);

                    UIManager.Instance.UpdateUserMsg("Ready", "Ready to scan for attendance.");
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the data from the web service and adding them to the interface
    /// </summary>
    IEnumerator GetServantsRoutine()
    {
        //Processing the request
        using (UnityWebRequest request = UnityWebRequest.Get(_servantsURL))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    _dataDownloaded = false;
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                    _dataDownloaded = false;
                    break;
                case UnityWebRequest.Result.Success:
                    //Downloading the JSON data
                    var jsonData = request.downloadHandler.text;
                    var jsonClasses = JsonUtility.FromJson<RootServant>("{\"servants\":" + jsonData + "}");

                    _servants = jsonClasses.servants;

                    _dataDownloaded = true;

                    //Adding the data to the interface list
                    UIManager.Instance.UpdateServantsList(_servants);
                    break;
            }
        }
    }

    #endregion
}
