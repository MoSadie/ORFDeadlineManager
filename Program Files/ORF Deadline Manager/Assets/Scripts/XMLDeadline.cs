using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class XMLDeadline : MonoBehaviour {

   public static XMLDeadline XML;

   public InputField Title;
   public InputField Description;
   public InputField DueDate;
   public InputField Author;
   public InputField Team;

   public Button SubmitButton;
   public Button CompletedButton;
   
   public DeadlineDB deadlineDB;

   public GameObject ErrorObject;

    // Use this for initialization
    public void Start()
    {
        XML = this;
        LoadDeadline();
    }


    public void SaveDeadline()
    {
        WWWForm form = new WWWForm();
        form.AddField("accessKey", ConfigFile.getPassword()); //FIXME
        form.AddField("fileType", "deadline");
        form.AddField("owner", Author.text);
        form.AddField("title", Title.text);
        form.AddField("date", DueDate.text);
        form.AddField("description", JsonConvert.SerializeObject(new string[1] {Description.text}));
        form.AddField("team", Team.text);

        UnityWebRequest request = UnityWebRequest.Post(ConfigFile.getBaseURL()+"/scripts/create.php", form);

        StartCoroutine(sendCreateRequest(request));

    }

    private IEnumerator sendCreateRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        Debug.Log("Result: IsError:" + request.isHttpError + " Code:" + request.responseCode + " Data: "+ request.downloadHandler.text);

        if (!request.isHttpError)
        {
            Title.text = "";
            Description.text = "";
            DueDate.text = "";
            Author.text = "";
            Team.text = "";
            DeadLineEntry entry = new DeadLineEntry(Title.text, Description.text, DueDate.text, Author.text, Team.text, request.downloadHandler.text);
            deadlineDB.DeadLineList.Add(entry);
            SceneManager.LoadScene("Deadlines");
        }
    }

    public void LoadDeadline() {
        
        //FileStream stream = new FileStream(Application.dataPath + "/StreamingAssets/XML/DeadlineDB.xml", FileMode.Open);
        UnityWebRequest uwr = UnityWebRequest.Get(ConfigFile.getBaseURL() + "/scripts/export.php");

        StartCoroutine(sendGetRequest(uwr));
    }

    private IEnumerator sendGetRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        if (!request.isHttpError)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeadlineDB));
            StringReader stream = new StringReader(request.downloadHandler.text);
            deadlineDB = serializer.Deserialize(stream) as DeadlineDB;
            stream.Close();
        }
    }

    public void DeleteDeadline(GameObject entry) 
    {
        string title = "";
        foreach (Text t in entry.GetComponentsInChildren<Text>())
            if (t.name.ToLower() == "project")
            {
                title = t.text;
                break;
            }

        foreach (DeadLineEntry ded in deadlineDB.DeadLineList) //I realize that "deadline" and "ded" aren't the right names, but what were you thinking when you put them in as filler? b/c I got no clue
        {
            
            if (title.ToLower() == ded.Title.ToLower()) //ded.DeadLineEntry.ToLower() || ded is already of the type DeadLineEntry, what we want to get is the variable Title of ded
            {
                WWWForm form = new WWWForm();
                form.AddField("accessKey", ConfigFile.getPassword());
                form.AddField("fileType", "deadline");
                form.AddField("fileName", ded.FileName);
                

                UnityWebRequest request = UnityWebRequest.Post(ConfigFile.getBaseURL() + "/scripts/delete.php", form);

                StartCoroutine(sendDeleteRequest(request));

            
                break;
            }
        }
    }

    private IEnumerator sendDeleteRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        Debug.Log("Result: IsError:" + request.isHttpError + " Code:" + request.responseCode);

        if (!request.isHttpError)
        {
            SceneManager.LoadScene("Deadlines");
        }
    }

    public void ValidateInput()
    {
        bool gotError = false;
        string errorMessage = "";
        foreach (DeadLineEntry entry in deadlineDB.DeadLineList)
        {
            if (entry.Title.ToLower() == Title.text.ToLower())
            {
                gotError = true;
                errorMessage += "A deadline with this title does already exist!\n";
            }
        }
        DateTime date;
        Debug.Log(DueDate.text.Trim());
        if (!DateTime.TryParseExact(DueDate.text.Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            gotError = true;
            errorMessage += "Due date has to be in the format of: \"MM/dd/yyyy\"!";
        }

        if (gotError)
        {
            StartCoroutine(showWarning(errorMessage));
        }
        else
        {
            SaveDeadline();
        }
    }

    IEnumerator showWarning(string errorMessage)
    {
        ErrorObject.GetComponentInChildren<Text>().text = errorMessage;
        ErrorObject.SetActive(true);
        yield return new WaitForSeconds(5);
        ErrorObject.SetActive(false);
    }


    [System.Serializable]
    public class DeadLineEntry
    {
        public DeadLineEntry()
        {
            Title = "";
            Description = "";
            DueDate = "";
            Author = "";
            Team = "";
            FileName = "";
        }

        public DeadLineEntry(string title, string desc, string due, string author, string team, string fileName)
        {
            Title = title;
            Description = desc;
            DueDate = due;
            Author = author;
            Team = team;
            FileName = fileName;
        }

        public string
        Author,
        Title,
        DueDate,
        Description,
        Team, FileName;
    }

    [System.Serializable]
    public class DeadlineDB  {
        public List<DeadLineEntry> DeadLineList = new List<DeadLineEntry>();
     
    }
}
