using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class faceeye : MonoBehaviour
{
    [SerializeField] private Image m_srcImage;
    string haarcascade_frontalface_default_xml_filepath, haarcascade_eye_xml_filepath;
    Mat srcMat, grayMat;

    void Start()
    {
        //训练集路径
        haarcascade_frontalface_default_xml_filepath = Application.dataPath + "/Cascades/haarcascade_frontalface_default.xml";
        haarcascade_eye_xml_filepath = Application.dataPath + "/Cascades/haarcascade_eye.xml";

        //读取原图
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/face.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
        //转为灰度图像
        grayMat = new Mat();
        Imgproc.cvtColor(srcMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        //检测图像中的所有脸
        MatOfRect faces = new MatOfRect();
        CascadeClassifier cascade = new CascadeClassifier(haarcascade_frontalface_default_xml_filepath);
        cascade.detectMultiScale(grayMat, faces, 1.1d, 2, 2, new Size(20, 20), new Size());
        //Debug.Log(faces); //检测到多少个脸 [ elemSize*1*CV_32SC4, isCont=True, isSubmat=False, nativeObj=0x1128611568, dataAddr=0x0 ]
        OpenCVForUnity.Rect[] rects = faces.toArray();
        for (int i = 0; i < rects.Length; i++)
        {
            //Debug.Log("detect faces " + rects[i]);
            Imgproc.rectangle(srcMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(0, 255, 0, 255), 2); //绿

            //眼长在脸上
            Mat roi_gray_img = new Mat(grayMat, new OpenCVForUnity.Rect(0, 0, rects[i].x + rects[i].width, rects[i].y + rects[i].height));
            Mat roi_img = new Mat(srcMat, new OpenCVForUnity.Rect(0, 0, rects[i].x + rects[i].width, rects[i].y + rects[i].height));
            MatOfRect eyes = new MatOfRect();
            CascadeClassifier eyecascade = new CascadeClassifier(haarcascade_eye_xml_filepath);
            eyecascade.detectMultiScale(roi_gray_img, eyes, 1.3d, 5, 2, new Size(20, 20), new Size()); //参数还需要找资料了解下
            //Debug.Log(eyes.elemSize());
            if (eyes.elemSize() > 0)
            {
                OpenCVForUnity.Rect[] eye_rects = eyes.toArray();
                for (int t = 0; t < eye_rects.Length; t++)
                {
                    //Debug.Log("detect eyes " + rects[t]);
                    //Imgproc.rectangle(roi_img, new Point(eye_rects[t].x, eye_rects[t].y), new Point(eye_rects[t].x + eye_rects[t].width, eye_rects[t].y + eye_rects[t].height), new Scalar(255, 255, 0, 255), 2); //黄
                    Point center = new Point((eye_rects[t].x + eye_rects[t].x + eye_rects[t].width) / 2, (eye_rects[t].y + eye_rects[t].y + eye_rects[t].height) / 2);
                    Imgproc.circle(roi_img, center, eye_rects[t].width / 2, new Scalar(255, 255, 0, 255), 2);
                }
            }

            
        }

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0,0, t2d.width, t2d.height), Vector2.zero);
        m_srcImage.sprite = sp;
        m_srcImage.preserveAspect = true;
    }
}
