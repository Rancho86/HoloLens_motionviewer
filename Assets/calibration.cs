using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
#endif

public class calibration : MonoBehaviour
{
    public TextMesh tm = null;
    static String temp = "OK";
    public String ziduan;
    public String zonghe;
    static bool bDataOK = false;
    public GameObject MainCamera;
    public GameObject prefab;
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
#if !UNITY_EDITOR
    StreamSocket socket;
    StreamSocketListener listener;
    String port;
#endif

    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR
        //实例化一个监听对象
        listener = new StreamSocketListener();
        port = "12345";
        //监听在接收到tcp连接后所触发的事件
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
            //指定监听端口
            await listener.BindServiceNameAsync(port);
        }
        catch (Exception e)
        {
            tm.text = "Error: " + e.Message;
            Debug.Log("Error: " + e.Message);
        }
        tm.text = "正在监听ing~";
        Debug.Log("******Listening at start");
    }

    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        Debug.Log("******进入到了Listener_ConnectionReceived()");
        DataReader reader = new DataReader(args.Socket.InputStream);
        DataWriter writer = new DataWriter(args.Socket.OutputStream);
        try
        {
            while(true)
            {
                //发送数据格式：先发送4个字节长度，再发送数据 例如: 0005 hello
                uint sizeFieldCount = await reader.LoadAsync(4);
                if(sizeFieldCount != 4)
                {
                    return;//socket提前close()了
                }
                //读取后续数据的长度
                uint stringLength = uint.Parse(reader.ReadString(4));

                //从输入流加载后续数据
                uint actualStringLength = await reader.LoadAsync(stringLength);
                if(actualStringLength != stringLength)
                {
                    return;//socket提前close()了
                }
                
                //ziduan中以字符串形式储存所有数据
                ziduan = reader.ReadString(actualStringLength);

                //将data原封不动发送回去，做测试
                writer.WriteString(zonghe);
                await writer.StoreAsync();
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
        if(bDataOK == true)
        {
            Destroy(prefab);
            Destroy(prefab1);
            Destroy(prefab2);
            Destroy(prefab3);
            Destroy(prefab4);
            Destroy(prefab5);
            Debug.Log("Model distroy");
            //tm.text = ziduan;
            bDataOK = false;
            Showmodel();
        }
      //  tm.text = temp;
    }
   
   public void Showmodel()

    {
        MainCamera = GameObject.FindWithTag("MainCamera");
        Vector3 maincamera_postion = MainCamera.transform.position;
        float px = maincamera_postion.x;
        float py = maincamera_postion.y;
        float pz = maincamera_postion.z;
        float rw = MainCamera.transform.rotation.w;
        float rx = MainCamera.transform.rotation.x;
        float ry = MainCamera.transform.rotation.y;
        float rz = MainCamera.transform.rotation.z;



        float aa = MainCamera.transform.localToWorldMatrix.m00;
        float ab = MainCamera.transform.localToWorldMatrix.m01;
        float ac = MainCamera.transform.localToWorldMatrix.m02;
        float ad = MainCamera.transform.localToWorldMatrix.m03;
        float ba = MainCamera.transform.localToWorldMatrix.m10;
        float bb = MainCamera.transform.localToWorldMatrix.m11;
        float bc = MainCamera.transform.localToWorldMatrix.m12;
        float bd = MainCamera.transform.localToWorldMatrix.m13;
        float ca = MainCamera.transform.localToWorldMatrix.m20;
        float cb = MainCamera.transform.localToWorldMatrix.m21;
        float cc = MainCamera.transform.localToWorldMatrix.m22;
        float cd = MainCamera.transform.localToWorldMatrix.m23;
        float da = MainCamera.transform.localToWorldMatrix.m30;
        float db = MainCamera.transform.localToWorldMatrix.m31;
        float dc = MainCamera.transform.localToWorldMatrix.m32;
        float dd = MainCamera.transform.localToWorldMatrix.m33;



        string zuobiao = "";
        string xuanzhuan = "";
        string juzhen = "";
        string douhao = "a";
        zuobiao = Convert.ToString(px) + douhao + Convert.ToString(py) + douhao + Convert.ToString(pz);
        xuanzhuan = Convert.ToString(rw) + douhao + Convert.ToString(rx) + douhao + Convert.ToString(ry) + douhao + Convert.ToString(rz);
        juzhen = Convert.ToString(aa) + douhao + Convert.ToString(ab) + douhao + Convert.ToString(ac) + douhao + Convert.ToString(ad) + douhao + Convert.ToString(ba) + douhao + Convert.ToString(bb) + douhao + Convert.ToString(bc) + douhao + Convert.ToString(bd) + douhao + Convert.ToString(ca) + douhao + Convert.ToString(cb) + douhao + Convert.ToString(cc) + douhao + Convert.ToString(cd) + douhao + Convert.ToString(da) + douhao + Convert.ToString(db) + douhao + Convert.ToString(dc) + douhao + Convert.ToString(dd);
        zonghe = juzhen;
        string[] strArray = ziduan.Split(',');
        float[] ff = strArray.Select(x => Convert.ToSingle(x)).ToArray();
        Debug.Log("ff.Length"+ ff.Length);
        prefab = Instantiate(prefab);
        prefab1 = Instantiate(prefab1);
        prefab2 = Instantiate(prefab2);
        prefab3 = Instantiate(prefab3);
        prefab4 = Instantiate(prefab4);
        prefab5 = Instantiate(prefab5);
        if (ff.Length==16)
        {
           
       Vector3 vy1 = new Vector3(ff[3], ff[4], ff[5]);
       Vector3 vz1 = new Vector3(ff[6], ff[7], ff[8]);
       Quaternion qua1 = Quaternion.LookRotation(new Vector3(vz1.x, vz1.y, vz1.z), new Vector3(vy1.x, vy1.y, vy1.z));
            prefab3.transform.position = new Vector3(ff[9], ff[10], ff[11]);
            prefab3.transform.rotation = new Quaternion(ff[12], ff[13], ff[14], ff[15]);
            prefab4.transform.position = new Vector3(ff[0], ff[1], ff[2]);
            prefab4.transform.rotation = qua1;
            prefab5.transform.position = new Vector3(ff[0], ff[1], ff[2]);
            prefab5.transform.rotation = qua1;

            //      prefab.transform.position = new Vector3(ff[0], ff[1], ff[2]);
            //     prefab1.transform.position = new Vector3(ff[3], ff[4], ff[5]);
            //      prefab2.transform.position = new Vector3(ff[6], ff[7], ff[8]);
            //    prefab3.transform.position = new Vector3(ff[9], ff[10], ff[11]);
            //  prefab3.transform.rotation = new Quaternion(ff[12], ff[13], ff[14], ff[15]);
            // prefab4.transform.position = new Vector3(ff[2], ff[3], ff[4]);
            // prefab4.transform.rotation = new Quaternion(ff[5], ff[6], ff[7], ff[8]);
            Debug.Log("Model display");
        }

    }
 
}
