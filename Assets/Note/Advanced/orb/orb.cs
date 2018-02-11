/* Can't use SURF/SIFT with this plugin.
 * Unfortunately, SURF/SIFT does not plan to be implemented.
 * "DescriptorMatcher.FLANNBASED" seems to only be used in "SIFT, SURF".
 * But, Because "SIFT,SURT" is nonfree module, it is not implemented in "OpenCV for Untiy".
 */
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class orb : MonoBehaviour
{
    [SerializeField] private Image m_dstImage;
    Mat p1Mat, p2Mat, dstMat;

    void Start()
    {
        Orb();
    }

    void Orb()
    {
        p1Mat = Imgcodecs.imread(Application.dataPath + "/Textures/1.jpg", 1);
        p2Mat = Imgcodecs.imread(Application.dataPath + "/Textures/3.jpg", 1);
        Imgproc.cvtColor(p1Mat, p1Mat, Imgproc.COLOR_BGR2RGB);
        Imgproc.cvtColor(p2Mat, p2Mat, Imgproc.COLOR_BGR2RGB);
        Imgproc.resize(p2Mat, p2Mat, new Size(p1Mat.width(), p1Mat.height()));
        Debug.Log(p2Mat);

        /*
        //仿射变换（矩阵旋转）
        float angle = UnityEngine.Random.Range(0, 360), scale = 1.0f;
        Point center = new Point(img2Mat.cols() * 0.5f, img2Mat.rows() * 0.5f);

        Mat affine_matrix = Imgproc.getRotationMatrix2D(center, angle, scale);
        Imgproc.warpAffine(img1Mat, img2Mat, affine_matrix, img2Mat.size());
        
        Texture2D texture = new Texture2D(img2Mat.cols(), img2Mat.rows());
        Utils.matToTexture2D(img2Mat, texture);
        outputRawImage.texture = texture;
        */

        ORB detector = ORB.create();
        ORB extractor = ORB.create();

        //提取图一特征点
        MatOfKeyPoint keypoints1 = new MatOfKeyPoint();
        Mat descriptors1 = new Mat();
        detector.detect(p1Mat, keypoints1);
        extractor.compute(p1Mat, keypoints1, descriptors1);

        //提取图二特征点
        MatOfKeyPoint keypoints2 = new MatOfKeyPoint();
        Mat descriptors2 = new Mat();
        detector.detect(p2Mat, keypoints2);
        extractor.compute(p2Mat, keypoints2, descriptors2);

        //第一次匹配结果（密密麻麻）
        DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
        MatOfDMatch matches = new MatOfDMatch();
        matcher.match(descriptors1, descriptors2, matches);

        //筛选（非官方）
        //计算向量距离的最大值/最小值
        double max_dist = 0;
        double min_dist = 15; //通过距离控制需要的特征。
        //（设到10，最终只有2个耳朵匹配。。。）
        //（设到15，尾巴也开始匹配。。。。。。）

        //新建两个容器存放筛选样本
        List<DMatch> matchesArray = matches.toList(); //用Unity版API多转一步
        //Debug.Log(matchesArray.Count); //500
        List<DMatch> goodmatchesArray = new List<DMatch>();

        //Debug.Log(img1Mat.rows()); //512
        for (int i = 0; i < matchesArray.Count; i++)
        {
            Debug.Log("[" + i + "]" + matchesArray[i].distance);
            if (matchesArray[i].distance > max_dist)
            {
                //max_dist = matchesArray[i].distance;
            }
            if (matchesArray[i].distance < min_dist)
            {
                min_dist = matchesArray[i].distance;
            }
        }
        //Debug.Log("The max distance is: " + max_dist);
        Debug.Log("The min distance is: " + min_dist);

        for (int i = 0; i < matchesArray.Count; i++)
        {
            if (matchesArray[i].distance < 2 * min_dist) //
            {
                goodmatchesArray.Add(matchesArray[i]);
            }
        }
        MatOfDMatch newMatches = new MatOfDMatch();
        newMatches.fromList(goodmatchesArray);
        Debug.Log(newMatches.toList().Count); //第二次筛选后符合的

        //绘制第二次筛选结果
        dstMat = new Mat();
        Features2d.drawMatches(p1Mat, keypoints1, p2Mat, keypoints2, newMatches, dstMat);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_dstImage.sprite = sp;
        m_dstImage.preserveAspect = true;
    }
}
