using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

namespace OpenCVForUnity
{
    public class watermark : MonoBehaviour
    {
        [SerializeField] private Image m_srcImage;
        Mat srcMat, logoMat;

        void Start()
        {
            srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1); //512,512
            Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
            logoMat = Imgcodecs.imread(Application.dataPath + "/Textures/head.png", 1);
            Imgproc.cvtColor(logoMat, logoMat, Imgproc.COLOR_BGR2RGB);

            Mat ROI = srcMat.submat(new Rect(20, 20, logoMat.cols(), logoMat.rows()));
            logoMat.copyTo(ROI);//logo复制到ROI上面

            Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
            Utils.matToTexture2D(srcMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            m_srcImage.sprite = sp;
            m_srcImage.preserveAspect = true;
            m_srcImage.rectTransform.offsetMin = new Vector2(0, 0);
            m_srcImage.rectTransform.offsetMax = new Vector2(t2d.width, t2d.height);
            m_srcImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}