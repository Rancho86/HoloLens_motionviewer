// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// DirectionIndicator creates an indicator around the SurgicalTool showing
    /// what direction to turn to find this GameObject.
    /// </summary>
    public class DirectionIndicator : MonoBehaviour
    {
        [Tooltip("The SurgicalTool object the direction indicator will be positioned around.")]
        public GameObject SurgicalTool;

        [Tooltip("Model to display the direction to the object this script is attached to.")]
        public GameObject DirectionIndicatorObject;

        //needle的延长线
        [Tooltip("needle的延长线")]
        public GameObject NeedleEnlong;

        [Tooltip("Model to display the direction to the object this script is attached to.")]
        public GameObject SelfObject;

        [Tooltip("Allowable percentage inside the holographic frame to continue to show a directional indicator.")]
        [Range(0.0f, 0.3f)]
        public float VisibilitySafeFactor = 0.05f;

        [Tooltip("Multiplier to decrease the distance from the SurgicalTool center an object is rendered to keep it in view.")]
        [Range(0.0f, 1.0f)]
        public float MetersFromSurgicalTool = 0.02f;
        // The default rotation of the SurgicalTool direction indicator.
        private Quaternion directionIndicatorDefaultRotation = Quaternion.identity;
        //*
        private Quaternion enlongDefaultRotation = Quaternion.identity;


        // Cache the MeshRenderer for the on-SurgicalTool indicator since it will be enabled and disabled frequently.
        private Renderer directionIndicatorRenderer;
        private Renderer directionIndicatorChildRenderer;
        //*
        private Renderer enlongRenderer;
        //private Renderer enlongChildRenderer;

        // Cache the Material to prevent material leak.
        private Material indicatorMaterial;
        private Material indicatorChildMaterial;
        //*
        private Material enlongMaterial;
       // private Material enlongChildMaterial;

        // Check if the SurgicalTool direction indicator is visible.
        private bool isDirectionIndicatorVisible;
        private bool isEnlongVisible;

        public void Awake()
        {
            if (SurgicalTool == null)
            {
                Debug.Log("Please include a GameObject for the SurgicalTool.");
            }

            if (DirectionIndicatorObject == null)
            {
                Debug.Log("Please include a GameObject for the Direction Indicator.");
            }

            // Instantiate the direction indicator.
            DirectionIndicatorObject = InstantiateDirectionIndicator(DirectionIndicatorObject);

            if (DirectionIndicatorObject == null)
            {
                Debug.Log("Direction Indicator failed to instantiate.");
            }

            // Instantiate the needle的延长线
            NeedleEnlong = InstantiateNeedleEnlong(NeedleEnlong);

           // needle的延长线
            if (NeedleEnlong == null)
            {
              Debug.Log("NeedleEnlong failed to instantiate.");
            }
            Debug.Log("Awake-end`                                                                                                                                                                                                                             调用完毕~");
        }

        public void OnDestroy()
        {
            DestroyImmediate(indicatorMaterial);
            DestroyImmediate(indicatorChildMaterial);
            Destroy(DirectionIndicatorObject);
        }

        //初始化引导arrow
        private GameObject InstantiateDirectionIndicator(GameObject directionIndicator)
        {
            if (directionIndicator == null)
            {
                return null;
            }

            GameObject indicator = Instantiate(directionIndicator);

            // Set local variables for the indicator.
            directionIndicatorDefaultRotation = indicator.transform.rotation;
            directionIndicatorRenderer = indicator.GetComponent<Renderer>();
            directionIndicatorChildRenderer = indicator.transform.GetChild(0).GetComponent<Renderer>();

            // Start with the indicator disabled.
            directionIndicatorRenderer.enabled = false;
            directionIndicatorChildRenderer.enabled = false;
            // Remove any colliders and rigidbodies so the indicators do not interfere with Unity's physics system.
            foreach (Collider indicatorCollider in indicator.GetComponents<Collider>())
            {
                Destroy(indicatorCollider);
            }

            foreach (Rigidbody rigidBody in indicator.GetComponents<Rigidbody>())
            {
                Destroy(rigidBody);
            }

            indicatorMaterial = directionIndicatorRenderer.material;
            indicatorChildMaterial = directionIndicatorChildRenderer.material;

            return indicator;
        }


        //初始化needle的延长线
        private GameObject InstantiateNeedleEnlong(GameObject needleEnlong)
        {

            //这里采用find替换一下试试----注意:前提条件是在socketluo将needle已经初始化了,这里才能找到enlong
            if (GameObject.Find("enlong")!=null)
            {
                GameObject enlongobj = GameObject.Find("enlong");
                Debug.Log("needdle的延长线已经找到");
                enlongDefaultRotation = enlongobj.transform.rotation;
                enlongRenderer = enlongobj.GetComponent<Renderer>();
                //enlongChildRenderer = enlongobj.transform.GetChild(0).GetComponent<Renderer>();

                // Start with the indicator disabled.
                enlongRenderer.enabled = false;
                //enlongChildRenderer.enabled = false;
                // Remove any colliders and rigidbodies so the indicators do not interfere with Unity's physics system.
                foreach (Collider indicatorCollider in enlongobj.GetComponents<Collider>())
                {
                    Destroy(indicatorCollider);
                }

                foreach (Rigidbody rigidBody in enlongobj.GetComponents<Rigidbody>())
                {
                    Destroy(rigidBody);
                }

                enlongMaterial = enlongRenderer.material;
                //enlongChildMaterial = enlongChildRenderer.material;


                return enlongobj;
            }
            else
                return null;
            // Set local variables for the indicator.
        }

        //update()
        public void Update()
        {
            if (DirectionIndicatorObject == null)
            {
                Debug.Log("DirectionIndicator.CS Line 176 return");
                return;
            }

            //if (GameObject.Find("Needle") == null)
           //     return;
          
            //获取场景中的needle
            if (SurgicalTool==null)
            {
                if ((SurgicalTool = GameObject.Find("MD-8700345")) == null)
                {
                    Debug.Log("DirectionIndicator.CS Line 188 return");
                    return;
                }
                //获取enlong
                Debug.Log("已找到MD-8700345");
            }

            Debug.Log("调用Update() at line 195");
            //获取场景中的needle的延长线
            if (NeedleEnlong == null)
           {
                if ((NeedleEnlong = GameObject.Find("enlong")) ==null)
                {
                   Debug.Log("DirectionIndicator.CS Line 199 return");
                    return;
               }
                Debug.Log("已找到enlong");
           }

            // guider与needle的距离
            // Direction from the SurgicalTool to this script's parent gameObject.
            Vector3 SelfToSurgicalToolDirection = SelfObject.transform.position - SurgicalTool.transform.position;
            SelfToSurgicalToolDirection.Normalize();
            
            //在这里控制arrow是否显示
            // The SurgicalTool indicator should only be visible if the target is not visible.
            isDirectionIndicatorVisible = !IsTargetVisible();
            directionIndicatorRenderer.enabled = isDirectionIndicatorVisible;
            directionIndicatorChildRenderer.enabled = isDirectionIndicatorVisible;

            if (isDirectionIndicatorVisible)
            {
                Vector3 position;
                Quaternion rotation;
                GetDirectionIndicatorPositionAndRotation(
                    SelfToSurgicalToolDirection,
                    out position);

                DirectionIndicatorObject.transform.position = position;
                DirectionIndicatorObject.transform.up = SelfObject.transform.position - DirectionIndicatorObject.transform.position;
            }

            //*
            //在这里控制needle的延长线是否显示
            // The SurgicalTool indicator should only be visible if the target is not visible.
             isEnlongVisible = IsTargetVisible();
             enlongRenderer.enabled = isEnlongVisible;
             enlongChildRenderer.enabled = isEnlongVisible;
        }//update-end


        /**
         * guider到needle的距离
         * return ture:
         * return false:
         **/
        private bool IsTargetVisible()
        {
            // This will return true if the target's mesh is within the Main Camera's view frustums.
            Vector3 targetDistance = SelfObject.transform.position - SurgicalTool.transform.position;
            return (targetDistance.x < VisibilitySafeFactor && targetDistance.x > 0 - VisibilitySafeFactor &&
                    targetDistance.y < VisibilitySafeFactor && targetDistance.y > 0 - VisibilitySafeFactor &&
                    targetDistance.z < VisibilitySafeFactor && targetDistance.z > 0 - VisibilitySafeFactor);
        }

        //获得arrow的position和rotation
        private void GetDirectionIndicatorPositionAndRotation(Vector3 SelfToSurgicalToolDirection, out Vector3 position)
        {
            // Find position: 
            // Save the SurgicalTool transform position in a variable.
            Vector3 origin = SurgicalTool.transform.position;

            // Project the camera to target direction onto the screen plane.
            //Vector3 SurgicalToolIndicatorDirection = Vector3.ProjectOnPlane(SelfToSurgicalToolDirection, 1* SelfObject.transform.forward);
            Vector3 SurgicalToolIndicatorDirection = SelfToSurgicalToolDirection;
            SurgicalToolIndicatorDirection.Normalize();

            // If the direction is 0, set the direction to the right.
            // This will only happen if the camera is facing directly away from the target.
            if (SurgicalToolIndicatorDirection == Vector3.zero)
            {
                SurgicalToolIndicatorDirection = SelfObject.transform.forward;
            }

            // The final position is translated from the center of the screen along this direction vector.
            position = origin + SurgicalToolIndicatorDirection * MetersFromSurgicalTool;
        }
    }
}