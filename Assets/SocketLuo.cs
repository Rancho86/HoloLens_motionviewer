using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
#endif

public class SocketLuo : MonoBehaviour
{
    public TextMesh tm = null;
    private Matrix4x4 fanhui;
    static String temp="waiting for connect";
    static bool bDataOK = false;
    static bool c1 = false;
    static bool c2 = false;
    static bool c3 = false;
    static bool c4 = false;
    public GameObject MainCamera;
    private uint command;
    public GameObject prefab;
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
    public GameObject prefab6;
    public GameObject prefab7;
    private Matrix4x4[] matrixGroup;
#if !UNITY_EDITOR
    StreamSocket socket;
    StreamSocketListener listener;
    String port;

#endif

    // Use this for initialization
    void Start()
    {


#if !UNITY_EDITOR
        listener = new StreamSocketListener();
        port = "12345";
        listener.ConnectionReceived += Listener_ConnectionReceived;
        //listener.Control.KeepAlive = false;

        Listener_Start();
#endif
    }

#if !UNITY_EDITOR
    private async void Listener_Start()
    {
        tm.text = "Started";
        Debug.Log("Listener started");
        try
        {
            await listener.BindServiceNameAsync(port);
        }
        catch (Exception e)
        {
            tm.text = "Error: " + e.Message;
            Debug.Log("Error: " + e.Message);
        }
        //tm.text = "Listening~";
    }

    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        temp="connected!";
        DataReader reader = new DataReader(args.Socket.InputStream);
        DataWriter writer = new DataWriter(args.Socket.OutputStream);
        try
        {
            while (true)
            {

                //发送数据格式： 命令(1) 长度(1) 数据(长度)
                byte[] ba = { 0 };
                //读取命令，1字节
                uint actualLength = await reader.LoadAsync(1);
                if (actualLength != 1)
                    return;//socket提前close()了                               
                command = uint.Parse(reader.ReadByte().ToString()); //解析命令

                //读取数据长度, 1字节
                actualLength = await reader.LoadAsync(1);
                if (actualLength != 1)
                    return;
                uint dataLength = uint.Parse(reader.ReadByte().ToString()); ; //解析数据长度

               // Matrix4x4[] matrixGroup;

                //读取所有数据，matrixlength长度
                if (dataLength != 0)
                {
                    bDataOK = true;
                    actualLength = await reader.LoadAsync(dataLength);
                    if (actualLength != dataLength)
                    {
                        return;//socket提前close()了
                    }

                    uint numberOfMatrix = dataLength / 64;

                    if (numberOfMatrix < 1)
                    {
                        return; //未收到一个矩阵
                    }

                    matrixGroup = new Matrix4x4[numberOfMatrix];

                    for (int i = 0; i < numberOfMatrix; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            float[] column = new float[4];
                            for (int k = 0; k < 4; k++)
                            {
                                byte[] temp = new byte[4];
                                reader.ReadBytes(temp);
                                column[k] = BitConverter.ToSingle(temp, 0);
                            }
                            matrixGroup[i].SetColumn(j, new Vector4(column[0], column[1], column[2], column[3]));
                        }
                    }
                }
                
                 

                //返回一个初始化的矩阵做测试
                Matrix4x4 mat = new Matrix4x4(); //这里为要发送的矩阵
                mat=fanhui;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        byte[] b = BitConverter.GetBytes(mat[j, i]);
                        writer.WriteBytes(b);
                    }
                }
                await writer.StoreAsync();
                Debug.Log("Return Matrixs");
                bDataOK = true;
            }
        }
        catch (Exception exception)
        {
            // If this is an unknown status it means that the error if fatal and retry will likely fail.
            if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
            {
                throw;
            }
        }
    }
#endif
    // Update is called once per frame
    void Update()
    {

        tm.text = temp;
        getHoloLensLocaltoWorld();
        if (bDataOK == true)
        {
            Debug.Log("bDataOK true");
            bDataOK = false;
            showModel();
        }

        //  tm.text = temp;
    }
    void getHoloLensLocaltoWorld()
    {
        MainCamera = GameObject.FindWithTag("MainCamera");
        fanhui = MainCamera.transform.localToWorldMatrix;
      // Debug.Log("fanhui matrix:" + fanhui);
    }
    void showModel()
    {

        Debug.Log("matrix:" + matrixGroup);
        Debug.Log("matrix0:" + matrixGroup[0]);
        
        //Debug.Log("MainCamera"+ MainCamera.transform.localToWorldMatrix);
        //getHoloLensLocaltoWorld();
        //if(command==0)啥也不干
        if (command==1)//一次显示模型  prefab3为模型
        {
          
            /*
            if (GameObject.FindWithTag("prefab3"))
            {
                Debug.Log("FindWithTag prefab3");
                Destroy(prefab3);
            }
            if (GameObject.Find("prefab3"))
            {
                Debug.Log("Find prefab3");
                Destroy(prefab3);
            }
            */
            //prefab3 = Instantiate(prefab3);
            Vector4 vy = matrixGroup[0].GetColumn(1);
            Vector4 vz = matrixGroup[0].GetColumn(2);
            Vector4 pos = matrixGroup[0].GetColumn(3);
            Debug.Log("pos"+ pos);
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            prefab3.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab3.transform.rotation = newQ;
            prefab7.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab7.transform.rotation = newQ;
            Debug.Log("Model display");
            
            //prefab6 = Instantiate(prefab6);
            prefab6.transform.position = new Vector3(0, 0, 0);

            if (c1 == false)
            {
                Instantiate(prefab3);
                Instantiate(prefab6);
                Instantiate(prefab7);
            }
            c1 = true;


        }
        if (command == 2)//一次显示标定针 prefab4为模型
        {
            
            Vector4 vy = matrixGroup[0].GetColumn(1);
            Vector4 vz = matrixGroup[0].GetColumn(2);
            Vector4 pos = matrixGroup[0].GetColumn(3);
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            prefab4.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab4.transform.rotation = newQ;
            Debug.Log("Needle display");
            if (c2 == false)
            {
                Instantiate(prefab4);
            }
            c2 = true;
        }
        if (command == 3)//一次刷新模型和标定针
        {
            Vector4 uy = matrixGroup[0].GetColumn(1);
            Vector4 uz = matrixGroup[0].GetColumn(2);
            Vector4 upos = matrixGroup[0].GetColumn(3);
            Quaternion unewQ = Quaternion.LookRotation(new Vector3(uz.x, uz.y, uz.z), new Vector3(uy.x, uy.y, uy.z));
            prefab3.transform.position = new Vector3(upos.x, upos.y, upos.z);
            prefab3.transform.rotation = unewQ;
            Debug.Log("Model display");
            Vector4 vy = matrixGroup[1].GetColumn(1);
            Vector4 vz = matrixGroup[1].GetColumn(2);
            Vector4 pos = matrixGroup[1].GetColumn(3);
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            prefab4.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab4.transform.rotation = newQ;
            Debug.Log("Needle display");
        }
        if (command == 4)//实时刷新手术器械
        {
            Vector4 vy = matrixGroup[0].GetColumn(1);
            Vector4 vz = matrixGroup[0].GetColumn(2);
            Vector4 pos = matrixGroup[0].GetColumn(3);
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            prefab5.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab5.transform.rotation = newQ;
            Debug.Log("Surgical Instruments display");
        }
        bDataOK = false;
        if (c4 == false)
        {
            Instantiate(prefab5);
        }
        c4 = true;
    }
}
