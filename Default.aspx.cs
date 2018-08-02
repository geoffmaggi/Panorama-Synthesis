/* Author: Geoff Maggi
 * Class: CS 410: Introduction to Computer Vision
 * Instructor: Feng Liu
 * 
 * About: Panorama Synthesis originally modeled after Matthew Brown and David G. Lowe, "Automatic panoramic image stitching using invariant features," International Journal of Computer Vision, 74, 1 (2007)
 *        Uses aForge and Accord as OpenCV wrappers and expands some on the above paper by also looking at other feature detection, filters and correlations
 *        
 *  Documentation:
 *    - http://www.cs.ubc.ca/~lowe/papers/07brown.pdf
 *    - http://www.aforgenet.com/framework/docs/
 *    - http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using Accord;
using Accord.Math;
using Accord.Imaging;
using Accord.Imaging.Filters;

public partial class _Default : System.Web.UI.Page {

  protected void Page_Load(object sender, EventArgs e) {

  }

  protected void run(object sender, EventArgs e) {
    if (sourcesFU.HasFile) {
      List<Bitmap> imgs = new List<Bitmap>();
      
      //Read in all the files to imgs
      HttpFileCollection files = Request.Files;
      for (int i = 0; i < files.Count; i++) {
        imgs.Add(new Bitmap(files[i].InputStream));
      }

      switch (taskDDL.SelectedValue) {
        case "fastHarrisRansacBlend":
          fastHarrisRansacBlend(imgs);
          break;
        case "harrisRansacBlend":
          harrisRansacBlend(imgs);
          break;
        case "surfRansacBlend":
          surfRansacBlend(imgs);
          break;
        case "freakRansacBlend":
          freakRansacBlend(imgs);
          break;
        case "fastHarrisRansacBlendStraight":
          fastHarrisRansacBlendStraight(imgs);
          break;
        case "harrisRansacBlendStraight":
          harrisRansacBlendStraight(imgs);
          break;
        case "surfRansacBlendStraight":
          surfRansacBlendStraight(imgs);
          break;
        case "fastHarrisFeaturesCorrelation":
          drawFastHarrisFeaturesCorrelations(imgs);
          break;
        case "harrisFeaturesCorrelation":
          drawHarrisFeaturesCorrelations(imgs);
          break;
        case "surfFeaturesCorrelation":
          drawSurfFeaturesCorrelations(imgs);
          break;
        case "freakFeaturesCorrelation":
          drawFreakFeaturesCorrelations(imgs);
          break;
        case "harrisFeatures":
          drawHarrisFeatures(imgs);
          break;
        case "surfFeatures":
          drawSurfFeatures(imgs);
          break;
        case "freakFeatures":
          drawFreakFeatures(imgs);
          break;
      }      

    }
    else {
      debug.Text = "You forgot to select images :)";
    }
  }

  protected void harrisRansacBlendStraight(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    MatrixH homography;

    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    for (int i = 0; i < imgs.Count; i++) {
      harrisPoints.Add(harris.ProcessImage(imgs[i]).ToArray());
    }

    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      //Convert my frames to grayscale so I can find and adjust the normal vectors
      AForge.Imaging.Filters.GrayscaleBT709 grayscale = new AForge.Imaging.Filters.GrayscaleBT709();
      AForge.Imaging.DocumentSkewChecker skew = new AForge.Imaging.DocumentSkewChecker();

      double finalAngle = skew.GetSkewAngle(grayscale.Apply(final));
      double imgAngle = skew.GetSkewAngle(grayscale.Apply(imgs[i]));

      //Less than 5% to account for human error with rotations and wobbles
      if (Math.Abs(finalAngle - imgAngle) < 5) {
        AForge.Imaging.Filters.RotateBilinear rotate = new AForge.Imaging.Filters.RotateBilinear(finalAngle - imgAngle);
        rotate.FillColor = Color.FromArgb(0, 255, 255, 255);
        imgs[i] = rotate.Apply(imgs[i]);

        //Update harris
        harrisPoints[i] = harris.ProcessImage(imgs[i]).ToArray();
      }

      IntPoint[] harrisFinal = harris.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      CorrelationMatching matcher = new CorrelationMatching(49, final, imgs[i]);
      IntPoint[][] matches = matcher.Match(harrisFinal, harrisPoints[i]);

      //Create the homography matrix using a RANSAC
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(1, 0.99);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    showImage(final);
  }

  protected void surfRansacBlendStraight(List<Bitmap> imgs) {
    MatrixH homography;

    List<SpeededUpRobustFeaturePoint[]> surfPoints = new List<SpeededUpRobustFeaturePoint[]>();
    //Calculate all the Surf Points
    SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector();
    double lastAngle = 0;
    for (int i = 0; i < imgs.Count; i++) {
      //Grayscale to find the edges and adjust the normal to point up
      AForge.Imaging.Filters.GrayscaleBT709 grayscale = new AForge.Imaging.Filters.GrayscaleBT709();
      AForge.Imaging.DocumentSkewChecker skew = new AForge.Imaging.DocumentSkewChecker();

      double angle = skew.GetSkewAngle(grayscale.Apply(imgs[i]));

      //Less than 5 deg change in angle to account for wobble, ignore big shifts
      if (Math.Abs(angle - lastAngle) < 5) {
        AForge.Imaging.Filters.RotateBilinear rotate = new AForge.Imaging.Filters.RotateBilinear(angle);
        rotate.FillColor = Color.FromArgb(0, 255, 255, 255);
        imgs[i] = rotate.Apply(imgs[i]);
        lastAngle = angle;
      }
      showImage(imgs[i]);
      surfPoints.Add(surf.ProcessImage(imgs[i]).ToArray());
    }


    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      SpeededUpRobustFeaturePoint[] surfFinal = surf.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      KNearestNeighborMatching matcher = new KNearestNeighborMatching(5);
      matcher.Threshold = 0.05;

      IntPoint[][] matches = matcher.Match(surfFinal, surfPoints[i]);

      //Create the homography matrix using RANSAC
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.015, 1);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    //Smooth/Sharpen if I wanted to
    AForge.Imaging.Filters.Sharpen filter = new AForge.Imaging.Filters.Sharpen();
    //AForge.Imaging.Filters.Gaussian filter = new AForge.Imaging.Filters.Guassian(5);
    //filter.ApplyInPlace(final);

    showImage(final);
  }

  protected void harrisRansacBlend(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    MatrixH homography;

    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    for (int i = 0; i < imgs.Count; i++) {
      harrisPoints.Add(harris.ProcessImage(imgs[i]).ToArray());
    }

    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      IntPoint[] harrisFinal = harris.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      CorrelationMatching matcher = new CorrelationMatching(99, final, imgs[i]);
      IntPoint[][] matches = matcher.Match(harrisFinal, harrisPoints[i]);

      //Create the homography matrix using ransac
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(1, 0.99);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    showImage(final);
  }

  //Relies less on neighbor pixels and more on RANSAC
  protected void fastHarrisRansacBlend(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    MatrixH homography;

    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    for (int i = 0; i < imgs.Count; i++) {
      harrisPoints.Add(harris.ProcessImage(imgs[i]).ToArray());
    }

    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      IntPoint[] harrisFinal = harris.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      CorrelationMatching matcher = new CorrelationMatching(5, final, imgs[i]);
      IntPoint[][] matches = matcher.Match(harrisFinal, harrisPoints[i]);

      //Create the homography matrix using ransac
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.025, 0.99);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    showImage(final);
  }

  protected void fastHarrisRansacBlendStraight(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    MatrixH homography;

    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    for (int i = 0; i < imgs.Count; i++) {
      harrisPoints.Add(harris.ProcessImage(imgs[i]).ToArray());
    }

    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      //Convert my frames to grayscale so I can find and adjust the normal vectors
      AForge.Imaging.Filters.GrayscaleBT709 grayscale = new AForge.Imaging.Filters.GrayscaleBT709();
      AForge.Imaging.DocumentSkewChecker skew = new AForge.Imaging.DocumentSkewChecker();

      double finalAngle = skew.GetSkewAngle(grayscale.Apply(final));
      double imgAngle = skew.GetSkewAngle(grayscale.Apply(imgs[i]));

      //Less than 5% to account for human error with rotations and wobbles
      if (Math.Abs(finalAngle - imgAngle) < 5) {
        AForge.Imaging.Filters.RotateBilinear rotate = new AForge.Imaging.Filters.RotateBilinear(finalAngle - imgAngle);
        rotate.FillColor = Color.FromArgb(0, 255, 255, 255);
        imgs[i] = rotate.Apply(imgs[i]);

        //Update harris
        harrisPoints[i] = harris.ProcessImage(imgs[i]).ToArray();
      }

      IntPoint[] harrisFinal = harris.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      CorrelationMatching matcher = new CorrelationMatching(5, final, imgs[i]);
      IntPoint[][] matches = matcher.Match(harrisFinal, harrisPoints[i]);

      //Create the homography matrix using ransac
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.025, 0.99);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    showImage(final);
  }

  protected void surfRansacBlend(List<Bitmap> imgs) {
    MatrixH homography;

    List<SpeededUpRobustFeaturePoint[]> surfPoints = new List<SpeededUpRobustFeaturePoint[]>();
    //Calculate all the Surf Points
    SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector();
    for (int i = 0; i < imgs.Count; i++) {
      surfPoints.Add(surf.ProcessImage(imgs[i]).ToArray());
    }


    Bitmap final = imgs[0];

    for (int i = 1; i < imgs.Count; i++) {
      SpeededUpRobustFeaturePoint[] surfFinal = surf.ProcessImage(final).ToArray();

      //Correlate the Harris pts between imgs
      KNearestNeighborMatching matcher = new KNearestNeighborMatching(5);
      matcher.Threshold = 0.05;

      IntPoint[][] matches = matcher.Match(surfFinal, surfPoints[i]);

      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.015, 1);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    //Smooth/Sharpen if I wanted to
    AForge.Imaging.Filters.Sharpen filter = new AForge.Imaging.Filters.Sharpen();
    //AForge.Imaging.Filters.Gaussian filter = new AForge.Imaging.Filters.Guassian(5);
    //filter.ApplyInPlace(final);

    showImage(final);
  }

  protected void freakRansacBlend(List<Bitmap> imgs) {
    MatrixH homography;

    List<FastRetinaKeypoint[]> freakPoints = new List<FastRetinaKeypoint[]>();
    //Calculate all the FREAK Points
    FastRetinaKeypointDetector freak = new FastRetinaKeypointDetector();
    foreach (Bitmap img in imgs) {
      freakPoints.Add(freak.ProcessImage(img).ToArray());
    }

    //Map them and draw them!
    Bitmap final = imgs[0];
    for (int i = 1; i < imgs.Count; i++) {
      FastRetinaKeypoint[] freakFinal = freak.ProcessImage(final).ToArray();
      
      KNearestNeighborMatching matcher = new KNearestNeighborMatching(500);
      matcher.Threshold = 0.005;
      IntPoint[][] matches = matcher.Match(freakFinal, freakPoints[i]);

      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.015, 1);
      homography = ransac.Estimate(matches[0], matches[1]);

      Blend blend = new Blend(homography, final);
      blend.Gradient = true;
      final = blend.Apply(imgs[i]);
    }

    //Smooth/Sharpen if I wanted to
    AForge.Imaging.Filters.Sharpen filter = new AForge.Imaging.Filters.Sharpen();
    //AForge.Imaging.Filters.Gaussian filter = new AForge.Imaging.Filters.Guassian(5);
    //filter.ApplyInPlace(final);

    showImage(final);
  }

  protected void drawFastHarrisFeaturesCorrelations(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    MatrixH homography;
    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    foreach (Bitmap img in imgs) {
      harrisPoints.Add(harris.ProcessImage(img).ToArray());
    }

    //Map them and draw them!
    Bitmap harrisImg = imgs[0];
    for (int i = 0; i < imgs.Count - 1; i++) {
      //Correlate the Harris pts between imgs
      CorrelationMatching matcher = new CorrelationMatching(5, imgs[i], imgs[i + 1]);
      IntPoint[][] matches = matcher.Match(harrisPoints[i], harrisPoints[i + 1]);

      //Create the homography matrix using ransac
      RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.025, 0.99);
      homography = ransac.Estimate(matches[0], matches[1]);

      Concatenate concat = new Concatenate(harrisImg);
      Bitmap img = concat.Apply(imgs[i + 1]);

      Color color = Color.White;
      if (i % 3 == 1) color = Color.OrangeRed;
      if (i % 3 == 2) color = Color.Blue;
      PairsMarker pairs = new PairsMarker(matches[0].Apply(p => new IntPoint(p.X + harrisImg.Width - imgs[0].Width, p.Y)), matches[1].Apply(p => new IntPoint(p.X + harrisImg.Width, p.Y)), color);
      harrisImg = pairs.Apply(img);
    }

    showImage(harrisImg);
  }

  protected void drawHarrisFeaturesCorrelations(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    foreach (Bitmap img in imgs) {
      harrisPoints.Add(harris.ProcessImage(img).ToArray());
    }

    //Map them and draw them!
    Bitmap harrisImg = imgs[0];
    for (int i = 0; i < imgs.Count - 1; i++) {
      CorrelationMatching matcher = new CorrelationMatching(99, imgs[i], imgs[i + 1]);
      IntPoint[][] matches = matcher.Match(harrisPoints[i], harrisPoints[i + 1]);

      Concatenate concat = new Concatenate(harrisImg);
      Bitmap img3 = concat.Apply(imgs[i + 1]);

      Color color = Color.White;
      if (i % 3 == 1) color = Color.OrangeRed;
      if (i % 3 == 2) color = Color.Blue;
      PairsMarker pairs = new PairsMarker(matches[0].Apply(p => new IntPoint(p.X + harrisImg.Width - imgs[0].Width, p.Y)), matches[1].Apply(p => new IntPoint(p.X + harrisImg.Width, p.Y)), color);
      harrisImg = pairs.Apply(img3);
    }

    showImage(harrisImg);
  }

  protected void drawHarrisFeatures(List<Bitmap> imgs) {
    List<IntPoint[]> harrisPoints = new List<IntPoint[]>();
    //Calculate all the Harris Points
    HarrisCornersDetector harris = new HarrisCornersDetector(0.03f, 10000f);
    foreach (Bitmap img in imgs) {
      harrisPoints.Add(harris.ProcessImage(img).ToArray());
    }

    //Draw & Show all the harris points
    for (int i = 0; i < imgs.Count; i++) {
      showImage(new PointsMarker(harrisPoints[i]).Apply(imgs[i]));
    }
  }

  protected void drawSurfFeaturesCorrelations(List<Bitmap> imgs) {
    List<SpeededUpRobustFeaturePoint[]> surfPoints = new List<SpeededUpRobustFeaturePoint[]>();
    //Calculate all the Surf Points
    SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector();
    foreach (Bitmap img in imgs) {
      surfPoints.Add(surf.ProcessImage(img).ToArray());
    }

    //Map them and draw them!
    Bitmap surfImg = imgs[0];
    for (int i = 0; i < imgs.Count - 1; i++) {
      KNearestNeighborMatching matcher = new KNearestNeighborMatching(5);
      matcher.Threshold = 0.005;
      IntPoint[][] matches = matcher.Match(surfPoints[i], surfPoints[i + 1]);

      Concatenate concat = new Concatenate(surfImg);
      Bitmap img = concat.Apply(imgs[i + 1]);

      Color color = Color.White;
      if (i % 3 == 1) color = Color.OrangeRed;
      if (i % 3 == 2) color = Color.Blue;
      PairsMarker pairs = new PairsMarker(matches[0].Apply(p => new IntPoint(p.X + surfImg.Width - imgs[0].Width, p.Y)), matches[1].Apply(p => new IntPoint(p.X + surfImg.Width, p.Y)), color);
      surfImg = pairs.Apply(img);
    }

    showImage(surfImg);
  }

  protected void drawSurfFeatures(List<Bitmap> imgs) {
    List<SpeededUpRobustFeaturePoint[]> surfPoints = new List<SpeededUpRobustFeaturePoint[]>();
    //Calculate all the Surf Points
    SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector();
    foreach (Bitmap img in imgs) {
      surfPoints.Add(surf.ProcessImage(img).ToArray());
    }

    //Draw & Show all the harris points
    for (int i = 0; i < imgs.Count; i++) {
      showImage(new PointsMarker(surfPoints[i]).Apply(imgs[i]));
    }
  }

  protected void drawFreakFeaturesCorrelations(List<Bitmap> imgs) {
    List<FastRetinaKeypoint[]> freakPoints = new List<FastRetinaKeypoint[]>();
    //Calculate all the FREAK Points
    FastRetinaKeypointDetector freak = new FastRetinaKeypointDetector();
    foreach (Bitmap img in imgs) {
      freakPoints.Add(freak.ProcessImage(img).ToArray());
    }

    //Map them and draw them!
    Bitmap img2 = imgs[0];
    for (int i = 0; i < imgs.Count - 1; i++) {
      KNearestNeighborMatching matcher = new KNearestNeighborMatching(200);
      matcher.Threshold = 0.015;
      IntPoint[][] matches = matcher.Match(freakPoints[i], freakPoints[i+1]);

      Concatenate concat = new Concatenate(img2);
      Bitmap img3 = concat.Apply(imgs[i + 1]);

      Color color = Color.White;
      if (i % 3 == 1) color = Color.OrangeRed;
      if (i % 3 == 2) color = Color.Blue;
      PairsMarker pairs = new PairsMarker(matches[0].Apply(p => new IntPoint(p.X + img2.Width - imgs[0].Width, p.Y)), matches[1].Apply(p => new IntPoint(p.X + img2.Width, p.Y)), color);
      img2 = pairs.Apply(img3);
    }

    showImage(img2);
  }

  protected void drawFreakFeatures(List<Bitmap> imgs) {
    List<FastRetinaKeypoint[]> freakPoints = new List<FastRetinaKeypoint[]>();
    //Calculate all the FREAK Points
    FastRetinaKeypointDetector freak = new FastRetinaKeypointDetector();
    foreach (Bitmap img in imgs) {
      freakPoints.Add(freak.ProcessImage(img).ToArray());
    }

    //Draw & Show all the harris points
    for (int i = 0; i < imgs.Count; i++) {
      showImage(new PointsMarker(freakPoints[i]).Apply(imgs[i]));
    }
  }

  //Addes the image to ImageUP
  protected void showImage(Bitmap img) {
    MemoryStream ms = new MemoryStream();
    img.Save(ms, ImageFormat.Jpeg);
    var base64Data = Convert.ToBase64String(ms.ToArray());

    System.Web.UI.WebControls.Image newImg = new System.Web.UI.WebControls.Image();
    newImg.ImageUrl = "data:image/jpg;base64," + base64Data;
    newImg.Visible = true;
    imageUP.ContentTemplateContainer.Controls.Add(newImg);
  }

  //Clears the form and the Image Panel
  protected void clear(object sender, EventArgs e) {
    sourcesFU.Attributes.Clear();
    sourcesFU.Attributes.Add("Multiple", "Multiple");
    
    imageUP.ContentTemplateContainer.Controls.Clear();
  }
}