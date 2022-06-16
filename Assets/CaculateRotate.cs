using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Date :2022年5月11日08:42:39
 * author:bitk
 **/
public class CaculateRotate : MonoBehaviour
{
    /**
*  全局变量说明"
*    needle:手术针的引用实例
*    guider:模型上的引导的引用实例
**/
    //获取到手术针的引用实例对象
    public GameObject needle = null;
    //获取到模型引导的引用实例对象
    public GameObject guider = null;
    //相机
    public GameObject mainCamera = null;
    //角度UI远离物体系数
    private float msg_dis_factor= 0.02f;

    //显示夹角信息
    public TextMesh msg_angle_result = null;

    // Start is called before the first frame update
    void Start()
    {
        //初始化msg_angle_result显示的信息
        msg_angle_result.text = "用来显示手术针和模型引导的夹角信息的";


        //检查手术针是否初始化
        if (GameObject.Find("Needle") != null)
        {
            //在Scene中找到手术针的对象实例
            needle = GameObject.Find("Needle");
        }
        else
            Debug.Log("手术针暂未初始化");

        //检查模型引导是否初始化
        if (GameObject.Find("Guider") != null)
        {
            //在Scene中找到模型引导的对象实例
            guider = GameObject.Find("Guider");
        }else
            Debug.Log(GameObject.Find("Guider"));

    }


    // Update is called once per frame
    void Update()
    {
        //检查手术针是否初始化
        if (GameObject.Find("Needle") != null)
        {
            //在Scene中找到手术针的对象实例
            needle = GameObject.Find("Needle");
        }
        else
            msg_angle_result.text = "未找到手术针";

        //检查模型引导是否初始化
        if (GameObject.Find("Guider") != null)
        {
            //在Scene中找到模型引导的对象实例
            guider = GameObject.Find("Guider");
        }
        else
            msg_angle_result.text = "未找到模型引导";
        
        //每一帧都调用计算夹角函数-然后将计算结果显示到视野中
        msg_angle_result.text = "手术器械与规划的夹角:"+this.calulateAngle(needle, guider).ToString("f2")+ "度";
        msg_angle_result.transform.forward=msg_angle_result.transform.position-mainCamera.transform.position ;
        Vector3 msg_rotation ;
        msg_rotation = msg_angle_result.transform.position - mainCamera.transform.position;
        msg_rotation.Normalize();
        msg_angle_result.transform.position = guider.transform.position - msg_rotation * msg_dis_factor;
    }

    /**
   * 功能:实现计算两个物体之间的夹角
   * 参数说明:
   *      obj1:物体1
   *      obj2:物体2
   *      
   *      返回值:
   *          -1:代表手术针未找到
   *          -2:代表模型引导未找到
   **/
    float calulateAngle(GameObject obj1, GameObject obj2)
    {

        if (GameObject.Find("Guider") == null)
        {
           // Debug.Log("Guider模型引导暂未初始化,夹角计算失败");
            return -2f;
        }
      //  Debug.Log("Guider初始化成功");

        if (GameObject.Find("Needle") == null)
        {
         //   Debug.Log("Needle手术针暂未初始化,夹角计算失败");
            return -1f;
        }
       // Debug.Log("Needle手术针初始化成功");

        //如果手术针和模型引导有一个没有初始化成功,就不执行
        //计算的夹角结果----这里采用每个物体坐标的X计算夹角
        float angle_result = Vector3.Angle(obj1.transform.up, obj2.transform.up);
            //Debug.Log(obj1.name + "与" + obj2.name + "的夹角是:" + angle_result);
            return System.Math.Abs(angle_result-90);
    }

}
