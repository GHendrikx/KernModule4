using UnityEngine;
using System.IO;
using System;

public class GenerateMessages : MonoBehaviour
{
    public string[] typeOfMessage;
    public string path;

    public void Generate()
    {
        string directoryPath = Environment.CurrentDirectory + "/Assets" + path;
        for (int i = 0; i < typeOfMessage.Length; i++)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(directoryPath, typeOfMessage[i].ToString() + "Message.cs")))
            {
                sw.WriteLine("using Unity.Networking.Transport;\n");
                sw.WriteLine($"public class {typeOfMessage[i].ToString()}Message : MessageHeader");
                sw.WriteLine("{");
                sw.WriteLine("\tpublic override MessageType Type => MessageType.None;");

                sw.WriteLine("\tpublic override void SerializeObject(ref DataStreamWriter writer)");
                sw.WriteLine("\t{");
                sw.WriteLine("\t\tbase.SerializeObject(ref writer);");
                sw.WriteLine("\t}");


                sw.WriteLine("\tpublic override void DeserializeObject(ref DataStreamReader reader)");
                sw.WriteLine("\t{");
                sw.WriteLine("\t\tbase.DeserializeObject(ref reader);");
                sw.WriteLine("\t}");

                sw.WriteLine("}");
                sw.Flush();
            }

        }
    }

}

