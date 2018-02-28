using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

namespace OpenCVForUnity
{
    /// <summary>
    /// 掩膜mask
    /// </summary>
    public class CvMask : MonoBehaviour
    {
        [SerializeField] private Image m_srcImage, m_dstImage;
        Mat srcMat, dstMat, detected_edges;

        void Start()
        {
            srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1); //512,512
            Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

            Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
            Utils.matToTexture2D(srcMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            m_srcImage.sprite = sp;
            m_srcImage.preserveAspect = true;
            m_srcImage.rectTransform.offsetMin = new Vector2(0, 0);
            m_srcImage.rectTransform.offsetMax = new Vector2(t2d.width, t2d.height);
            m_srcImage.rectTransform.anchoredPosition = Vector2.zero;

            //--------------------------------------------------//

            dstMat = Imgcodecs.imread(Application.dataPath + "/Textures/0.jpg", 1); //500,500
            Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGB);
            //dstMat = new Mat();
            Mat grayMat = new Mat();
            detected_edges = new Mat();
            double threshold1 = 1;
            double threshold2 = 100;
            int kernel_size = 3;
            //使用 3x3内核降噪
            Imgproc.cvtColor(srcMat, grayMat, Imgproc.COLOR_RGB2GRAY);
            Imgproc.blur(grayMat, detected_edges, new Size(3, 3));
            Imgproc.Canny(detected_edges, detected_edges, threshold1, threshold2, kernel_size, false);

            //使用 Canny算子输出边缘作为掩码显示原图像  
            //dstMat.setTo(new Scalar(0));
            Imgproc.resize(dstMat, dstMat, srcMat.size());
            //srcMat.copyTo(dstMat, detected_edges); //保证srcMat,dstMat是一样大的
            //左.copyTo(中, 右); //左作为像素源，右作为mask，合成到中

            OpenCVForUnity.Rect rect = new OpenCVForUnity.Rect(25, 25, 125, 200);
            srcMat.submat(rect).copyTo(dstMat);

            Texture2D dst_t2d = new Texture2D(dstMat.width(), dstMat.height());
            Utils.matToTexture2D(dstMat, dst_t2d);
            Sprite dst_sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
            m_dstImage.sprite = dst_sp;
            m_dstImage.preserveAspect = true;
            m_dstImage.rectTransform.offsetMin = new Vector2(0, 0);
            m_dstImage.rectTransform.offsetMax = new Vector2(dst_t2d.width, dst_t2d.height);
            m_dstImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}