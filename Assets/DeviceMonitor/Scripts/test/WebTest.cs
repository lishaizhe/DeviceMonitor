using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class WebTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField uri;
    [SerializeField] private TMP_InputField postData;
    [SerializeField] private TMP_InputField token;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void GetButtonClick()
    {
        if (uri?.text != string.Empty)
            StartCoroutine(GetDataFromWeb(uri?.text));
        else
            Debug.Log("输入为空");
    }

    public void PostButtonClick()
    {
        if (uri?.text != string.Empty)
            StartCoroutine(PostDataFromWeb(uri?.text, postData?.text.Trim()));
        else
            Debug.Log("输入为空");
    }

    IEnumerator GetDataFromWeb(string uri)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.SetRequestHeader("X-Auth-Token", token?.text.Trim());
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.isDone)
        {
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
    IEnumerator PostDataFromWeb(string uri, string json)
    {
        if (json == string.Empty)
            yield return null;

        UnityWebRequest request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        request.downloadHandler = downloadHandler;
        request.SetRequestHeader("x-auth-token", token?.text.Trim());
        request.SetRequestHeader("Content-Type", $"application/json");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.isDone)
        {
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

}

