using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class haar : MonoBehaviour
{
    [SerializeField] private Image m_srcImage;
    Mat srcMat, grayMat;
    CascadeClassifier cascade;
    string haarcascade_frontalface_alt_xml_filepath;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/team.jpg", 1); //512
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
        grayMat = new Mat();
        Imgproc.cvtColor(srcMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        MatOfRect faces = new MatOfRect();
        haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath("haarcascade_frontalface_alt.xml");
        cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);
        if (cascade != null)
        {
            //cascade.detectMultiScale(grayMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size(grayMat.cols() * 0.15, grayMat.cols() * 0.15), new Size());
            cascade.detectMultiScale(grayMat, faces, 1.1, 2, 2, new Size(20, 20), new Size());
        }

        OpenCVForUnity.Rect[] rects = faces.toArray();
        for (int i = 0; i < rects.Length; i++)
        {
            Debug.Log("detect faces " + rects[i]);
            Imgproc.rectangle(srcMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(255, 0, 0, 255), 2);
        }

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0,0, t2d.width, t2d.height), Vector2.zero);
        m_srcImage.sprite = sp;
        m_srcImage.preserveAspect = true;
    }
}
