using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public class HighScoreManager : MonoBehaviour
{
    public IEnumerator GetHTTP()
    {
        var request = UnityWebRequest.Get("url");
        yield return request.SendWebRequest();

        if (request.isDone && !request.isHttpError)
            Debug.Log(request.downloadHandler.text);
    }

    public async void GetHttpAsync()
    {
        using(var client = new HttpClient())
        {
            var result = await client.GetAsync("url");
            if(result.IsSuccessStatusCode)
                Debug.Log(await result.Content.ReadAsStringAsync());
        }
    }
}

