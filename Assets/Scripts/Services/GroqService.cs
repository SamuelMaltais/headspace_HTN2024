using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GroqService
{
    private MonoBehaviour obj;
    private string clientID;
    private string clientSecret;
    private string system_prompt;
    private string user_prompt;

    private const string apiRoute = "https://api.groq.com/openai/v1/chat/completions";

    public GroqService(MonoBehaviour obj)
    {
        this.obj = obj;
        clientSecret = Environment.GetEnvironmentVariable("GROQ_CLIENT_SECRET");
    }

    public void GetResponse(string user_prompt, System.Action<List<GroqObject>> callback)
    {
        system_prompt = "Your job is design a space which will reflect, support and complement the mood, content, emotional state and vibe of a given journal entry. The prompt you receive will be this journal entry. You will do this by generating a list of objects that might contribute to this environment. Each object should be cohesive, and make sense together with all the other objects. You will also need to determine the position of each object in 3D space using XYZ coordinates, where X is horizontal, Y is vertical, and Z is depth, and a unit of 1 corresponds to 1 meter. You will also return the orientation of the object using euler angles, with each value being between 0 and 360 degrees. You will return your answer in the JSON format as a list, with each list element having the following attributes: name:string, description:string, position:[float,float,float], rotation [float,float,float]. You will return only the json information, you message should contain nothing else."; 
        GroqMessage[] messages = {new GroqMessage("system", system_prompt), new GroqMessage("user", user_prompt),};

        Debug.Log("Groq Request JSON = " + messages);
        GroqRequest req = new GroqRequest(messages, "llama3-8b-8192");
        obj.StartCoroutine(Service.PostRequest(apiRoute, clientSecret, req, o => {ParseResponse((GroqResponse)o, callback); }, typeof(GroqResponse)));

    }

    private void ParseResponse(GroqResponse response, System.Action<List<GroqObject>> callback)
    {
        if (response != null && response.choices != null && response.choices.Length > 0)
        {
            Debug.Log("Groq Response: " + response.choices[0].message.content);

            // Parse JSON response
            try
            {
                string jsonContent = response.choices[0].message.content;
                
                List<GroqObject> objects = JsonUtility.FromJson<GroqObjectList>("{\"objects\":" + jsonContent + "}").objects;

                callback(objects);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON: {e.Message}");
                callback(null);
            }
        }
        else
        {
            Debug.LogError("Failed to get a valid response from Groq.");
            Debug.Log($"Groq Response = {response.choices}");
            callback(null);
        }
    }

}

[System.Serializable]
public class GroqMessage
{
    public string role;
    public string content;

    public GroqMessage(string role, string content){
        this.role = role;
        this.content = content;
    }
}

[System.Serializable]
public class GroqRequest
{
    public GroqMessage[] messages;
    public string model;

    public GroqRequest(GroqMessage[] messages, string model)
    {
        this.messages = messages;
        this.model = model;
    }
}



[System.Serializable]
public class GroqResponse
{
    public GroqChoice[] choices;
}

public class GroqChoice
{
    public GroqMessage message;
}


[System.Serializable]
public class GroqObject
{
    public string name;
    public string description;
    public float[] position;
    public float[] rotation;
}

[System.Serializable]
public class GroqObjectList
{
    public List<GroqObject> objects;
}
