using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class orb : MonoBehaviour
{
    [SerializeField] private Texture2D imgTexture1, imgTexture2;
    [SerializeField] private RawImage outputRawImage;
    Mat img1Mat, img2Mat;

    void Start()
    {
        Run();
    }

    void Run()
    {
        Mat img1Mat = new Mat(imgTexture1.height, imgTexture1.width, CvType.CV_8UC3); //RGB3通道
        Utils.texture2DToMat(imgTexture1, img1Mat);
        Debug.Log("img1Mat.ToString() " + img1Mat.ToString());

        Mat img2Mat = new Mat(imgTexture2.height, imgTexture2.width, CvType.CV_8UC3);
        Utils.texture2DToMat(imgTexture2, img2Mat);
        Imgproc.resize(img2Mat, img2Mat, new Size(img1Mat.width(), img1Mat.height()));
        Debug.Log("img2Mat.ToString() " + img2Mat.ToString());

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
        detector.detect(img1Mat, keypoints1);
        extractor.compute(img1Mat, keypoints1, descriptors1);

        //提取图二特征点
        MatOfKeyPoint keypoints2 = new MatOfKeyPoint();
        Mat descriptors2 = new Mat();
        detector.detect(img2Mat, keypoints2);
        extractor.compute(img2Mat, keypoints2, descriptors2);

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
            ///*
            if (matchesArray[i].distance > max_dist)
            {
                //max_dist = matchesArray[i].distance;
            }
            if (matchesArray[i].distance < min_dist)
            {
                min_dist = matchesArray[i].distance;
            }
            //*/
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
        Mat resultImg = new Mat();
        Features2d.drawMatches(img1Mat, keypoints1, img2Mat, keypoints2, newMatches, resultImg);

        Texture2D t2d = new Texture2D(resultImg.cols(), resultImg.rows());
        Utils.matToTexture2D(resultImg, t2d);
        
        outputRawImage.texture = t2d;
        outputRawImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)t2d.width / (float)t2d.height;
    }
}
