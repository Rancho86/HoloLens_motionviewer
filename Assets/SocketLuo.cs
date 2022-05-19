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
    //渲染文字用的
    public TextMesh tm =null;
    //public TextMesh tm2 = null;
    //public TextMesh tm3 = null;
    public string needel_name = "Needle";

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
    public GameObject prefab4;//needle
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
       
            
        getHoloLensLocaltoWorld();
#if !UNITY_EDITOR

        //启动监听
        listener = new StreamSocketListener();
        //监听的端口
        port = "12345";
        //新连接接入时的事件
        listener.ConnectionReceived += Listener_ConnectionReceived;
        //listener.Control.KeepAlive = false;

        Listener_Start();
#endif
    }

#if !UNITY_EDITOR
    private async void Listener_Start()
    {
        try
        {
            //监听端口ing
            await listener.BindServiceNameAsync(port);
        }
        catch (Exception e)
        {
            tm.text = "Error: " + e.Message;
        }
    }

    //新连接接入时触发
    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        temp="connected!";//显示在unity中的文字

        //获取新接入的Socket的InputStream来读取远程目标发来的数据
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
                
                /**
                 * QT发过来得数据包结构
                 * Data的长度是64的整数倍 
                 *      如果是128就是发送了两个矩阵
                 * ------------------------------------
                 *|command|    Datalen   | Data   |
                 *|------------------------------------ 
                 *|        0        |         1         |  2->65|
                 * ------------------------------------
                 * */

                 //解析命令:1字节[byte].这里把收到的命令进行了类型转换标成了无符号整形
                command = uint.Parse(reader.ReadByte().ToString()); 

                //读取数据长度, 1字节
                actualLength = await reader.LoadAsync(1);

                if (actualLength != 1)
                    return;

                //解释数据长度
                uint dataLength = uint.Parse(reader.ReadByte().ToString()); ; //解析数据长度

               // Matrix4x4[] matrixGroup;

                //读取所有数据，matrixlength长度
                if (dataLength != 0)
                {
                    bDataOK = true;
                    //将matrix数据加载到了reader中但还没读
                    actualLength = await reader.LoadAsync(dataLength);
                    if (actualLength != dataLength)
                    {
                        return;//socket提前close()了
                    }

                    //矩阵的数量，一个矩阵的大小是64，数据长度/64就可以算出来有多少个矩阵
                    uint numberOfMatrix = dataLength / 64;
         

                    if (numberOfMatrix < 1)
                    {
                        return; //未收到一个矩阵
                    }

                    //新建一个矩阵数组用来存储接收到的矩阵
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
                                //将temp的byte[]转化成浮点类型 
                                column[k] = BitConverter.ToSingle(temp, 0);
                            }
                            matrixGroup[i].SetColumn(j, new Vector4(column[0], column[1], column[2], column[3]));
                        }
                    }
                }
                
                 

                //返回LocaltoWorld
                Matrix4x4 mat = new Matrix4x4(); //这里要发送的矩阵
                mat=fanhui;
           //     tm.text = fanhui + "";
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

      //  tm.text = temp;
        //这个坐标只让获取一次,在strat()中
        getHoloLensLocaltoWorld();//初始化fanhui矩阵
        if (bDataOK == true)
        {
            Debug.Log("bDataOK true");
            showModel();
            bDataOK = false;
        }
    }
    
    /**
     * 初始化fanhui矩阵,也就是将当前的相机的世界坐标赋值给fanhui
     * 其实maincamera也就是HoloLens的左边位置
     * */
    void getHoloLensLocaltoWorld()
    {
        MainCamera = GameObject.FindWithTag("MainCamera");
        fanhui = MainCamera.transform.localToWorldMatrix;

        //输出holo的世界坐标
       // tm3.text = fanhui+"";

         //Debug.Log( fanhui);
    }


    /**
     * 显示模型
     **/
    void showModel()
    {
        //打印下矩阵信息
        //tm.text = "收:"+matrixGroup[0];


        //Debug.Log("MainCamera"+ MainCamera.transform.localToWorldMatrix);
        //getHoloLensLocaltoWorld();
        //if(command==0)啥也不干
        /**
         * 显示骨头模型--
         **/
        if (command==1)//一次显示模型  ---功能正常
        {
            if (c1 == false)
            {
                //模型实例化
                prefab3=Instantiate(prefab3);//HoloLens初始坐标轴
                prefab6 =Instantiate(prefab6);//HoloLens初始坐标轴
                prefab7= Instantiate(prefab7);//bone
            }
          //  tm.text = "模型显示" + prefab7.transform.position; 
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
            // 位置 
            Vector4 pos = matrixGroup[0].GetColumn(3);

         
            //旋转
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            //设置prefab3[HoloLens的初始点坐标系]的位置
            prefab3.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab3.transform.rotation = newQ;//设置prefab3[HoloLens的初始点坐标系]的旋转角度

            prefab7.transform.position = new Vector3(pos.x, pos.y, pos.z);//设置prefab7[骨头]的位置
            prefab7.transform.rotation = newQ;//设置prefab[骨头]的旋转
            Debug.Log("Model display");
            
            //prefab6 = Instantiate(prefab6);
            prefab6.transform.position = new Vector3(0, 0, 0);//设置prefab6[Hololens的初始点坐标系]的位置为坐标原点
            c1 = true;


        }
        if (command == 2)//一次显示标定针 prefab4为模型[needle]---功能正常--不知道是否收到矩阵数据
        {
            if (c2 == false)
            {
                //之前的代码是这样的 Instantiate(prefab4);这样会造成一个问题,就是全局定义的prefab的引用是空的.无法对当前实例化的预制体进行引用.
                prefab4 = Instantiate(prefab4);
                //初始化结束之后更改他的name属性---在脚本calulateRotate脚本中采用find函数查找
                prefab4.name = needel_name;
             }

            Vector4 vy = matrixGroup[0].GetColumn(1);
            Vector4 vz = matrixGroup[0].GetColumn(2);
            Vector4 pos = matrixGroup[0].GetColumn(3);//位置(x,y,z)

         //   tm3.text = "command2" + matrixGroup[0];

            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));

            //指定needle的位置
            prefab4.transform.position = new Vector3(pos.x, pos.y, pos.z);
        
            //制定needle的旋转
            prefab4.transform.rotation = newQ;

            //测试用：打印针的位置MainCamera.transform.localToWorldMatrix;
           // tm2.text = prefab4.transform.position + "np";

            Debug.Log("Needle display");
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
            if (c4 == false)
            {
                //实例化一个预制体
               prefab4= Instantiate(prefab4);//prefab4是针
               //初始化结束之后更改他的name属性---在脚本calulateRotate脚本中采用find函数查找
                prefab4.name = needel_name;
                c4 = true;
            }
         //   tm.text = prefab4.name;
            Vector4 vy = matrixGroup[0].GetColumn(1);
            Vector4 vz = matrixGroup[0].GetColumn(2);
            Vector4 pos = matrixGroup[0].GetColumn(3);
            Quaternion newQ = Quaternion.LookRotation(new Vector3(vz.x, vz.y, vz.z), new Vector3(vy.x, vy.y, vy.z));
            prefab4.transform.position = new Vector3(pos.x, pos.y, pos.z);
            prefab4.transform.rotation = newQ;
            Debug.Log("Surgical Instruments display");
        }
    }

}
