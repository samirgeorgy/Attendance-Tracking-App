using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DecodeQRCode : MonoBehaviour
{
    #region Private Variables

    [SerializeField] private QRCodeDecodeController qrcodecontroller;
    [SerializeField] private Image _cameraBox;

    [SerializeField] private string _attendanceFormURL;

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
            string participantName = result.Replace('+', ' ');

            if (ApplicationManager.Instance.DataDownloaded)
            {
                if (IsParticipant(participantName))
                {
                    RecordAttendance(result);
                }
                else
                    UIManager.Instance.UpdateUserMsg("ATTENTION", participantName + " is not registered in the course!");
            }
            else
            {
                UIManager.Instance.UpdateUserMsg("ATTENTION", participantName + " Attendance Taken without validation!");
                RecordAttendance(result);
            }
        }
    }

    /// <summary>
    /// Records the attendance of the participant
    /// </summary>
    /// <param name="name">The name of the participant as scanned from the QR Code</param>
    private void RecordAttendance(string name)
    {
        StartCoroutine(RecordAttendanceRoutine(name));
    }

    /// <summary>
    /// Checks if the participant is registered in the course or not
    /// </summary>
    /// <param name="name">The name of the scanner participant</param>
    /// <returns>True if the participant is registered and false otherwise.</returns>
    private bool IsParticipant(string name)
    {
        for (int i = 0; i < ApplicationManager.Instance.ClassParticipants.Length; i++)
            if (ApplicationManager.Instance.ClassParticipants[i].full_name.Equals(name))
                return true;

        return false;
    }

    /// <summary>
    /// Runs the request to register the participant's attendance
    /// </summary>
    /// <param name="name">The participant name</param>
    IEnumerator RecordAttendanceRoutine(string name)
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

    #endregion
}
