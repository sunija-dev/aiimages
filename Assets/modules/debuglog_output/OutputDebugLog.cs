using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class OutputDebugLog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textmDebugLog;
    private LogMessage[] arLogMessages = new LogMessage[12];
    private int iLogCounter = 0;

    void Start()
    {
        Application.logMessageReceived += OutputDebugLogFunction;
    }

    private void OnApplicationQuit()
    {
        Application.logMessageReceived -= OutputDebugLogFunction;
    }

    public void OutputDebugLogFunction(string _strLogString, string _strStackTrace, LogType type)
    {
        List<LogMessage> liLogMessages = new List<LogMessage>(arLogMessages);

        if (liLogMessages.Exists(x => x.strMessage == _strLogString))
        {
            // same message? Change time.
            LogMessage logMessageSame = liLogMessages.Find(x => x.strMessage == _strLogString);
            logMessageSame.dateTime = System.DateTime.Now;
        }
        else
        {
            // add message
            arLogMessages[iLogCounter] = new LogMessage(System.DateTime.Now, _strLogString);
        }

        // display
        string strOutput = "";
        for (int i = 0; i < arLogMessages.Length; i++)
        {
            strOutput += "-" + arLogMessages[i].dateTime.ToString("HH:mm:ss") + "- " + arLogMessages[i].strMessage + "\n";
        }
        textmDebugLog.text = strOutput;

        iLogCounter = (iLogCounter + 1) % arLogMessages.Length;
    }

    private struct LogMessage
    {
        public System.DateTime dateTime;
        public string strMessage;

        public LogMessage(System.DateTime _dateTime, string _strMessage)
        {
            dateTime = _dateTime;
            strMessage = _strMessage;
        }
    }
}
