using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml;
using System.Globalization;
using UnityEngine.Networking;

public class XMLDeadlineDisplay : MonoBehaviour
{

    public Text Display;

    public XMLDeadline xmlDeadLineScript;

    public GameObject deadLinePrefab, entryList, teamPrefab;
    public float cellMargin = 10; //Deprecated, clean up coming soon

    Transform listEnd; //Deprecated, clean up coming soon
    List<Image> pastDeadlines = new List<Image>();

    float timer = 0;
    Color original;
    // Use this for initialization
    void Start()
    {
        UnityWebRequest uwr = UnityWebRequest.Get(ConfigFile.getBaseURL() + "/scripts/export.php");
        StartCoroutine(sendGetRequest(uwr));
        //listEnd = GameObject.FindGameObjectWithTag("listEnd").transform;
        original = deadLinePrefab.GetComponent<Image>().color;
    }

    private IEnumerator sendGetRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        if (!request.isHttpError)
        {
            parseXmlFile(request.downloadHandler.text);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;      

        if (timer >= 2f)
        {
            timer = 0;
            foreach (Image img in pastDeadlines)
            {
                if (img.color == original)
                    img.color = Color.red;
                else
                    img.color = original;
            }
        }

    }

    void parseXmlFile(string xmlData)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(new StringReader(xmlData));

        string xmlPathPattern = "//DeadLineList/DeadLineEntry";
        XmlNodeList myNodeList = xmlDoc.SelectNodes(xmlPathPattern);
        List<string[]> deadlines = new List<string[]>();


        foreach (XmlNode node in myNodeList)
        {
            XmlNode author = node["Author"];
            XmlNode title = node["Title"];
            XmlNode duedate = node["DueDate"];
            XmlNode description = node["Description"];
            XmlNode team = node["Team"];


            //deadlines.Add("Title : " + title.InnerXml + "\nAuthor : " + author.InnerXml + "\nDue Date : " + duedate.InnerXml + "\nDescription : " + description.InnerXml + "\n\n");
            string[] dedl = { title.InnerText, author.InnerXml, description.InnerXml, duedate.InnerXml, team.InnerXml };
            deadlines.Add(dedl);
        }

        deadlines = Quicksort(deadlines, 0, deadlines.Count - 1);
        if (PersistentObject.persitentObject.sortByTeam)
            StartCoroutine(PlaceEntryByTeam(deadlines));
        else
            StartCoroutine(PlaceEntry(deadlines));


    }

    public static List<string[]> Quicksort(List<string[]> elements, int left, int right)
    {
        int i = left, j = right;
        //Debug.Log(elements[(left + right) / 2].Split('\n')[2].Substring(10).Trim());

        DateTime pivot = DateTime.ParseExact(elements[(left + right) / 2][3], "MM/dd/yyyy", CultureInfo.InvariantCulture);

        while (i <= j)
        {
            while (DateTime.ParseExact(elements[i][3], "MM/dd/yyyy", CultureInfo.InvariantCulture).CompareTo(pivot) < 0)
            {
                i++;
            }

            while (DateTime.ParseExact(elements[j][3], "MM/dd/yyyy", CultureInfo.InvariantCulture).CompareTo(pivot) > 0)
            {
                j--;
            }

            if (i <= j)
            {
                // Swap
                string[] tmp = elements[i];
                elements[i] = elements[j];
                elements[j] = tmp;

                i++;
                j--;
            }
        }

        // Recursive calls
        if (left < j)
        {
            Quicksort(elements, left, j);
        }

        if (i < right)
        {
            Quicksort(elements, i, right);
        }

        return elements;
    }

    void FitList(int lenght) //Deprecated, clean up coming soon
    {
       // GameObject[] entries = GameObject.FindGameObjectsWithTag("entry");
        RectTransform list = entryList.GetComponent<RectTransform>();
        Debug.Log(lenght + " || " + deadLinePrefab.GetComponent<RectTransform>().rect.height);
        /* foreach (GameObject entry in entries)
         {
             while (listEnd.position.y > entry.transform.position.y)
             {
                 entryList.GetComponent<RectTransform>().sizeDelta += new Vector2(0, Mathf.Abs(entry.transform.position.y - listEnd.position.y) + cellMargin);
                 Debug.Log(entry.GetComponentInChildren<Text>().text + " || " + listEnd.position.y + " : " + entry.transform.position.y);
             }
         }*/
        Debug.Log(list.sizeDelta);
        float size = 0;
        for (int i = 0; i < lenght; i++)
            size += deadLinePrefab.GetComponent<RectTransform>().rect.height;//entries[i].GetComponent<RectTransform>().rect.height;

        Debug.Log(size);
        list.sizeDelta = new Vector2(list.sizeDelta.x, size);// - deadLinePrefab.GetComponent<RectTransform>().rect.height);
        Debug.Log(list.sizeDelta);

       /* while (list.sizeDelta.y < size)
            list.sizeDelta += new Vector2(0, cellMargin);*/
            
    }


    IEnumerator PlaceEntryByTeam(List<string[]> deadlines)
    {
        List<string[]>[] sorted = new List<string[]>[7];
        for (int i = 0; i <sorted.Length; i++)
            sorted[i] = new List<string[]>();

        /*
         * 
         * [0] = Build Team
         * [1] = Programming Team
         * [2] = Photo/Video Team    
         * [3] = Business Team
         * [4] = Spirit & Outreach Team
         * [5] = Chairman's Team
         * [6] = Other
         */
        foreach (string[] d in deadlines)
        {
            switch (d[4].ToLower())
            {
                case "build":
                    sorted[0].Add(d);
                    break;

                case "programming":
                    sorted[1].Add(d);
                    break;

                case "media":
                    sorted[2].Add(d);
                    break;

                case "business":
                    sorted[3].Add(d);
                    break;

                case "spirit":
                    sorted[4].Add(d);
                    break;

                case "chairman":
                    sorted[5].Add(d);
                    break;

                default:
                    sorted[6].Add(d);
                    break;
            }
        }
        Vector3 relativePos = new Vector3(0, deadLinePrefab.GetComponent<RectTransform>().rect.height / 2, 0);

        for (int i = 0; i < sorted.Length; i++)
        {
            for(int y = 0; y < sorted[i].Count; y++)
            {
                if (y == 0)
                {
                    GameObject title = Instantiate(teamPrefab, entryList.transform) as GameObject;
                    switch (i)
                    {
                        case 0:
                            title.GetComponentInChildren<Text>().text = "Build Team";
                            break;
                        case 1:
                            title.GetComponentInChildren<Text>().text = "Programming Team";
                            break;
                        case 2:
                            title.GetComponentInChildren<Text>().text = "Media Team";
                            break;
                        case 3:
                            title.GetComponentInChildren<Text>().text = "Business Team";
                            break;
                        case 4:
                            title.GetComponentInChildren<Text>().text = "Spirit & Outreach Team";
                            break;
                        case 5:
                            title.GetComponentInChildren<Text>().text = "Chairman's Team";
                            break;
                        case 6:
                            title.GetComponentInChildren<Text>().text = "Other";
                            break;
                    }
                }
                GameObject entry = Instantiate(deadLinePrefab, entryList.transform) as GameObject;

                foreach (Text t in entry.GetComponentsInChildren<Text>())
                {
                    switch (t.name)
                    {
                        case "Project":
                            t.text = sorted[i][y][0];
                            break;

                        case "Lead":
                            t.text = sorted[i][y][1];
                            break;

                        case "Desc":
                            t.text = sorted[i][y][2];
                            break;

                        case "Date":
                            t.text = sorted[i][y][3];
                            break;
                    }
            }

            if (DateTime.ParseExact(sorted[i][y][3], "MM/dd/yyyy", CultureInfo.InvariantCulture).CompareTo(DateTime.Today) < 0)
            {
                foreach (Image img in entry.GetComponentsInChildren<Image>())
                    if (img.name == "Flash")
                    {
                        pastDeadlines.Add(img);
                        break;
                    }
            }
            if (entry.GetComponentInChildren<Button>() != null)
                entry.GetComponentInChildren<Button>().onClick.AddListener(delegate { xmlDeadLineScript.DeleteDeadline(entry); });


            //entry.GetComponentInChildren<ElementResizeManager>().Resize();
                entry.GetComponent<RectTransform>().localPosition = relativePos;
                yield return null;
                // relativePos = entry.GetComponent<RectTransform>().localPosition - (new Vector3(0, cellMargin + deadLinePrefab.GetComponent<RectTransform>().rect.height, 0));
            }

        }

    }

    IEnumerator PlaceEntry(List<string[]> deadlines)
    {
       // FitList(deadlines.Count);
        Vector3 relativePos = new Vector3(0, deadLinePrefab.GetComponent<RectTransform>().rect.height/2, 0); //-deadLinePrefab.GetComponent<RectTransform>().rect.height/2
        foreach (string[] dl in deadlines)
        {
            GameObject entry = Instantiate(deadLinePrefab, entryList.transform) as GameObject;

            foreach (Text t in entry.GetComponentsInChildren<Text>())
            {
                switch (t.name)
                {
                    case "Project":
                        t.text = dl[0];
                        break;

                    case "Lead":
                        t.text = dl[1];
                        break;

                    case "Desc":
                        t.text = dl[2];
                        break;

                    case "Date":
                        t.text = dl[3];
                        break;
                }
            }

            if (DateTime.ParseExact(dl[3], "MM/dd/yyyy", CultureInfo.InvariantCulture).CompareTo(DateTime.Today) < 0)
            {
                foreach (Image img in entry.GetComponentsInChildren<Image>())
                    if (img.name == "Flash")
                    {
                        pastDeadlines.Add(img);
                        break;
                    }
            }
            if (entry.GetComponentInChildren<Button>() != null)
                entry.GetComponentInChildren<Button>().onClick.AddListener(delegate { xmlDeadLineScript.DeleteDeadline(entry); });


            //entry.GetComponentInChildren<ElementResizeManager>().Resize();
            entry.GetComponent<RectTransform>().localPosition = relativePos;
            yield return null;
           // relativePos = entry.GetComponent<RectTransform>().localPosition - (new Vector3(0, cellMargin + deadLinePrefab.GetComponent<RectTransform>().rect.height, 0));
            

        }

    }

}
 