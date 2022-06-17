using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Spine.Unity;
using System.IO;
using UnityEngine.UI;

[Serializable]
public class SpineConfig
{
    public string name;
    public int id;
}
[Serializable]
public class Info
{
    public string atlas;
    public string json;
    public string texture;
    public string image;
}
[Serializable]
public class Result
{
    public Info info;
    public string status;
}

public class UnityDemo : MonoBehaviour
{
    SpineConfig config;
    Result result;
    SkeletonGraphic sg;
    SpineAtlasAsset saa;
    SkeletonDataAsset sda;

    public SkeletonGraphic bayc;


    void Start()
    {

        config = new SpineConfig();
        config.name = "PhantaBear";
        config.id = 10;
        StartCoroutine(PostRequest("https://heroapi.juhuixinkj.com/api/Metaverse/getSpineConfig", JsonUtility.ToJson(config)));
    }

    void AddSpineSkeleton(Info info)
    {
        string atlas = info.atlas;
        string json = info.json;
        string texture = info.texture;

        saa = ScriptableObject.CreateInstance<SpineAtlasAsset>();
        sda = ScriptableObject.CreateInstance<SkeletonDataAsset>();
        sda.fromAnimation = new string[0];

        StartCoroutine(GetAtlas(atlas));
        StartCoroutine(GetJson(json));
        StartCoroutine(GetTexture(texture));

        sg = this.gameObject.AddComponent<SkeletonGraphic>();
        sg.skeletonDataAsset = sda;

        sg.material = new Material(Shader.Find("Spine/SkeletonGraphic"));
        sg.startingAnimation = "idle";
        sg.startingLoop = true;
    }

    public int trackIndex = 0;
    public float playbackSpeed = 2.0f;
    public string _animation = "idle";
    public void PlayAnimationLooping(Dropdown change)
    {
        //var entry = bayc.AnimationState.SetAnimation(trackIndex, change.options[change.value].text, true);
        //entry.TimeScale = playbackSpeed;

        //bayc.AnimationState.SetAnimation(0, "run", true);
        //bayc.AnimationState.SetAnimation(0, "attack2", true);
    } 

    public void PlayAnimationOnce()
    {
        var entry = sg.AnimationState.SetAnimation(trackIndex, _animation, false);
        entry.TimeScale = playbackSpeed;
    }

    public void ClearTrack()
    {
        sg.AnimationState.ClearTrack(trackIndex);
    }

    IEnumerator GetAtlas(string resUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(resUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("传递不成功" + www.error);
        }
        else
        {
            var response = www.downloadHandler.text;
            TextAsset ta = new TextAsset(response);
            saa.atlasFile = ta;
        }
    }

    IEnumerator GetJson(string resUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(resUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("传递不成功" + www.error);
        }
        else
        {
            var response = www.downloadHandler.text;
            TextAsset ta = new TextAsset(response);
            sda.skeletonJSON = ta;
        }
    }

    IEnumerator GetTexture(string resUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(resUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("传递不成功" + www.error);
        }
        else
        {
            var response = www.downloadHandler.data;
            Debug.Log(response.Length);
            //TextAsset ta = new TextAsset(response);
            saa.materials = new Material[1];
            for (int i = 0; i < saa.materials.Length; i++)
            {
                Texture2D t2d = new Texture2D(512, 1024);
                t2d.name = config.name;
                t2d.LoadImage(response);

                Material mat = new Material(Shader.Find("Spine/Skeleton"));
                mat.mainTexture = t2d;
                Debug.Log(mat.mainTexture);
                saa.materials[i] = mat;


            }
            sda.atlasAssets = new SpineAtlasAsset[1];
            sda.atlasAssets[0] = saa;
        }
    }

    IEnumerator PostRequest(string url, string json)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log(uwr.downloadHandler.text);
            var length = uwr.downloadHandler.text.Length - 1;
            string subbedStr = uwr.downloadHandler.text.Substring(1, length);
            result = JsonUtility.FromJson<Result>(subbedStr);

            AddSpineSkeleton(result.info);
        }
    }
}
