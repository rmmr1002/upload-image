using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Reflection;

public class UploadImages : MonoBehaviour {
    public Text txt;
    public new string name;
    public float width;
    public string image;
    public string application_metadata;

    public Texture2D tex;

    private string access_key = "6498e1a027b2a6472e7162a437ee6be50f1575a8";
    private string secret_key = "1a5e990f4a5bfa0eec827fcf75253db7b85168d4";
    private string url = "https://vws.vuforia.com/";
    private string targetName = "MyTarget"; // must change when upload another Image Target, avoid same as exist Image on cloud

    private byte[] requestBytesArray;

    public void CallPostTarget()
    {
        StartCoroutine(PostNewTarget());
    }

    IEnumerator PostNewTarget()
    {
        txt.text = "In Post New target";
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        Debug.Log(date);

        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        tex = new Texture2D(4, 4);
        yield return 0;
        WWW imglink = new WWW("https://firebasestorage.googleapis.com/v0/b/unity-ec9f0.appspot.com/o/Preacher_El_Greeco.jpg?alt=media&token=45546859-3f82-4c7e-a215-700b2228f35e");
        yield return imglink;
        tex = imglink.texture;
        byte[] image = tex.EncodeToPNG();

        //string metadataStr = "Vuforia metadata";//May use for key,name...in game
        //byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        UploadImages model = new UploadImages();
        model.name = targetName;
        model.width = 64.0f; // don't need same as width of texture
        model.image = System.Convert.ToBase64String(image);
        txt.text = "Image loaded";

        //model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonConvert.SerializeObject(model);
        string requestBody = JsonUtility.ToJson(model);
        txt.text = "Stuff converted to strings";

        /*WWWForm form = new WWWForm();

        var headers = form.headers;
        byte[] rawData = form.data;
        headers["Host"] = url;
        headers["Date"] = date;
        headers["Content-Type"] = contentType;*/

        txt.text = "About to make webrequest";
        HttpWebRequest httpWReq = (HttpWebRequest)HttpWebRequest.Create(serviceURI);
        httpWReq.Method = httpAction;
        MethodInfo priMethod = httpWReq.Headers.GetType().GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
        priMethod.Invoke(httpWReq.Headers, new[] { "Date", date });
        httpWReq.ContentType = contentType;

        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }

        txt.text = "Halfway through auth";

        string contentMD5 = sb.ToString();

        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);

        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);

        httpWReq.Headers.Add("Authorization", string.Format("VWS {0}:{1}", access_key, signature));
        //headers["Authorization"] = string.Format("VWS {0}:{1}", access_key, signature); 

        txt.text = "About to start stream";
        var streamWriter = httpWReq.GetRequestStream();
        byte[] buffer = System.Text.Encoding.ASCII.GetBytes(requestBody);
       
        requestBytesArray = buffer;
        txt.text = "Not written to stream yet";
        streamWriter.Write(buffer, 0, buffer.Length);
        txt.text = "Wrote to stream";
        streamWriter.Flush();
        streamWriter.Close();

        WebResponse response = httpWReq.GetResponse();
        txt.text = "Got http resonse";
       /* Debug.Log("<color=green>Signaturere" + "</ color > ");
         txt.text = "Authorization done";

         WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);
         yield return request;

         txt.text = "Sent web request";
 ;
         if (request.error != null)
         {
             Debug.Log("requestr: " + request.error);
         }
         else
         {
             Debug.Log("requestess");
             Debug.Log("returned" + request.text);
             txt.text = "returned" + request.text;

         }*/
        txt.text = "About to open receive stream";
        Stream receiveStream = response.GetResponseStream();
        StreamReader sr = new StreamReader(receiveStream, System.Text.Encoding.UTF8);
        string responseData = sr.ReadToEnd();
        response.Close();
        sr.Close();
        UploadImages result = JsonUtility.FromJson<UploadImages>(responseData);
        
    
           

txt.text = "End of program";


    }

    // Use this for initialization
    void Start () {
        CallPostTarget();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
