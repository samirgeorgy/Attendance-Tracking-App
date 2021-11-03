using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Threading;
using System.Text;

[Serializable]
public class AttendanceRecordJson
{
    public int Fk_Participant_Id;
    public int Fk_Class_Id;
    public int Fk_Session_Id;
    public int Fk_User_Id;
    public string Attendance_Date;
}

public class DecodeQRCode : MonoBehaviour
{
    #region Private Variables

    [SerializeField] private QRCodeDecodeController qrcodecontroller;
    [SerializeField] private Image _cameraBox;

    [SerializeField] private string _attendanceCheckURL;
    [SerializeField] private string _attendanceFormURL;
    [SerializeField] private string _attendanceCloudURL;

    private int _participantId = -1;
    private int attendanceSessionCount = 0;
    private DateTime attendanceDate;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {
        qrcodecontroller.onQRScanFinished += GetResult;
        _cameraBox.color = new Color(0, 255, 24);
    }

    private void OnDisable()
    {
        qrcodecontroller.onQRScanFinished -= GetResult;
    }

    #endregion

    #region Supporting Functions

    /// <summary>
    /// Gets the result from the QR Code
    /// </summary>
    /// <param name="resultStr">The scanned result</param>
    private void GetResult(string resultStr)
    {
        if (!string.IsNullOrEmpty(resultStr))
        {
            _cameraBox.color = new Color(255, 38, 0);
            ProcessPartcipant(resultStr);
        }
    }

    /// <summary>
    /// Resets the scanner
    /// </summary>
    public void ResetScanner()
    {
        qrcodecontroller.Reset();
        _cameraBox.color = new Color(0, 255, 24);
    }

    /// <summary>
    /// Submits the participant name to the google form
    /// </summary>
    /// <param name="result">The raw name of the participant as extracted from the QR Code</param>
    public void ProcessPartcipant(string result)
    {
        if (!result.Equals(""))
        {
            if (ApplicationManager.Instance.DataDownloaded)
            {
                if (IsParticipant(result))
                {
                    StartCoroutine(AttendanceRoutine(result));
                }
                else
                    UIManager.Instance.UpdateUserMsg("ATTENTION", result + " is not registered in the course!");
            }
            else
            {
                UIManager.Instance.UpdateUserMsg("ATTENTION", result + " Attendance will be taken without validation!");
                RecordOffLineAttendance(result);
            }
        }
    }

    /// <summary>
    /// Checks if the participant is registered in the course or not
    /// </summary>
    /// <param name="name">The name of the scanner participant</param>
    /// <returns>True if the participant is registered and false otherwise.</returns>
    private bool IsParticipant(string name)
    {
        for (int i = 0; i < ApplicationManager.Instance.ClassParticipants.Length; i++)
            if (ApplicationManager.Instance.ClassParticipants[i].FullName.Equals(name))
                return true;

        return false;
    }

    /// <summary>
    /// Records the attendance of the participant
    /// </summary>
    /// <param name="name">The name of the participant as scanned from the QR Code</param>
    private void RecordOffLineAttendance(string name)
    {
        StartCoroutine(RecordOfflineAttendanceRoutine(name));
    }

    /// <summary>
    /// The Attendance Recording Routine when offline
    /// </summary>
    /// <param name="name">The participant name</param>
    IEnumerator RecordOfflineAttendanceRoutine(string name)
    {
        string uri = _attendanceFormURL + name;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    UIManager.Instance.UpdateUserMsg("SUCCESS", name + " Attendance Taken!");
                    break;
            }
        }
    }

    /// <summary>
    /// The Attendance Recording Routine when online
    /// </summary>
    /// <param name="name">The participant name</param>
    IEnumerator AttendanceRoutine(string name)
    {
        UIManager.Instance.ToggleSandClock(true);

        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-EG");

        bool attendanceFound = false;
        bool attendanceRecordedInGoogle = false;
        bool attendanceRecordedInCloud = false;

        foreach (var item in ApplicationManager.Instance.ClassParticipants)
        {
            if (item.FullName.Equals(name))
            {
                _participantId = item.Id;
                break;
            }
        }

        if (_participantId != -1)
        {
            //Check whether the current participant's attendance was taken in the selected session or not
            string attendanceCheckUri = string.Format(_attendanceCheckURL, _participantId.ToString(), ApplicationManager.Instance.SelectedClass, ApplicationManager.Instance.SelectedSession, DateTime.Now.ToShortDateString());

            using (UnityWebRequest webRequest = UnityWebRequest.Get(attendanceCheckUri))
            {
                yield return webRequest.SendWebRequest();

                string[] pages = attendanceCheckUri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        //Downloading the data
                        var jsonData = webRequest.downloadHandler.text;

                        if (!string.IsNullOrEmpty(jsonData))
                            attendanceSessionCount = int.Parse(jsonData);
                        else
                            attendanceSessionCount = 0;

                        break;
                }
            }

            if (attendanceSessionCount > 0)
                attendanceFound = true;
            else
                attendanceFound = false;

            if (attendanceFound == false)
            {
                //Record attendance on the google sheet
                string attendanceGoogleSheetUri = _attendanceFormURL + name;

                using (UnityWebRequest webRequest = UnityWebRequest.Get(attendanceGoogleSheetUri))
                {
                    yield return webRequest.SendWebRequest();

                    string[] pages = attendanceGoogleSheetUri.Split('/');
                    int page = pages.Length - 1;

                    switch (webRequest.result)
                    {
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.DataProcessingError:
                            UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                            break;
                        case UnityWebRequest.Result.ProtocolError:
                            UIManager.Instance.UpdateUserMsg("ERROR", webRequest.error);
                            break;
                        case UnityWebRequest.Result.Success:
                            attendanceRecordedInGoogle = true;
                            break;
                    }
                }

                //Record attendance on the cloud
                AttendanceRecordJson record = new AttendanceRecordJson();
                record.Fk_Participant_Id = _participantId;
                record.Fk_Class_Id = ApplicationManager.Instance.SelectedClass;
                record.Fk_Session_Id = ApplicationManager.Instance.SelectedSession;
                record.Fk_User_Id = Authenticator.Instance.UserInfo.Id;
                record.Attendance_Date = DateTime.Now.ToShortDateString();

                string data = JsonUtility.ToJson(record);

                using (UnityWebRequest request = new UnityWebRequest(_attendanceCloudURL, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                    request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    yield return request.SendWebRequest();

                    switch (request.result)
                    {
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.DataProcessingError:
                            UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                            break;
                        case UnityWebRequest.Result.ProtocolError:
                            UIManager.Instance.UpdateUserMsg("ERROR", request.error);
                            break;
                        case UnityWebRequest.Result.Success:
                            attendanceRecordedInCloud = true;
                            break;
                    }
                }

                if (attendanceRecordedInCloud && attendanceRecordedInGoogle)
                    UIManager.Instance.UpdateUserMsg("SUCCESS", name + " Attendance Recorded!");
                else
                {
                    if (attendanceRecordedInGoogle == false)
                        UIManager.Instance.UpdateUserMsg("SUCCESS", name + " Attendance Recorded on cloud only.");
                    else if (attendanceRecordedInCloud == false)
                        UIManager.Instance.UpdateUserMsg("SUCCESS", name + " Attendance Recorded on Google only.");
                    else if (!attendanceRecordedInCloud && !attendanceRecordedInGoogle)
                        UIManager.Instance.UpdateUserMsg("ERROR", "Attendance was not recorded on Google or Cloud!");
                }
            }
            else
                UIManager.Instance.UpdateUserMsg("ATTENTION", name + "'s attendance has been taken for this session.");
        }

        UIManager.Instance.ToggleSandClock(false);
    }

    #endregion
}
