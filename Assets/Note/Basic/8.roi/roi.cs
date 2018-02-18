﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class roi : MonoBehaviour
{
    [SerializeField] private RawImage m_srcImage; //注意不能用Image，会破坏Rect
    [SerializeField] private RawImage m_roiImage;
    Mat srcMat;
    OpenCVForUnity.Point cv_point;
    OpenCVForUnity.Size cv_size;
    OpenCVForUnity.Rect cv_rect; //感兴趣区域ROI

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1); //512
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        m_srcImage.texture = t2d;
        m_srcImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_srcImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        m_srcImage.rectTransform.anchoredPosition = Vector2.zero;

        //切下一块roi，贴到rawImage
        cv_point = new Point(50, 50); //左上角为坐标原点
        cv_size = new Size(200, 200); //(长,宽)
        cv_rect = new OpenCVForUnity.Rect(cv_point, cv_size);
        Mat roiMat = new Mat(srcMat, cv_rect); //300,300
        Debug.Log(roiMat.width() + "," + roiMat.height());
        Imgproc.cvtColor(roiMat, roiMat, Imgproc.COLOR_RGB2GRAY); //roi区域变灰

        Texture2D dst_t2d = new Texture2D(roiMat.width(), roiMat.height());
        Utils.matToTexture2D(roiMat, dst_t2d);
        m_roiImage.texture = dst_t2d;
        m_roiImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_roiImage.rectTransform.offsetMax = new Vector2(roiMat.width(), roiMat.height());
        m_roiImage.rectTransform.anchoredPosition = new Vector2((float)cv_point.x, -(float)cv_point.y); //rawImage锚点设到左上角
    }
}