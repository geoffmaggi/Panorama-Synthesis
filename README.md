# Panorama-Synthesis

<p><b>Project:</b> Panorama Synthesis</p>
<p><b>Assignment:</b> <a href="http://web.cecs.pdx.edu/~fliu/courses/cs410/prj2.htm">Link to Assignment Outline</a></p>
<p><b>Sources:</b></p>
<ul>
  <li><a href="http://www.cs.ubc.ca/~lowe/papers/07brown.pdf">Matthew Brown and David G. Lowe, "Automatic panoramic image stitching using invariant features," International Journal of Computer Vision, 74, 1 (2007)</a></li>
  <li><a href="http://www.emgu.com/wiki/index.php/Documentation">Emgu CV Documentation</a></li>
  <li><a href="http://www.aforgenet.com/framework/docs/">AForge.NET Documentation</a></li>
  <li><a href="http://accord-framework.net/docs/html/R_Project_Accord_NET.htm">Accord.NET Documentation</a></li>
</ul>

<p><b>Source Code:</b></p>
<div style="margin-left:20px;">
  <p>The project was written as an ASP.NET Web Application with a C# Backend</p>
  <p><b>A Live Demo can be viewed here:</b> <a href="https://projects.laxer.net/Panorama%20Synthesis/">Panorama Synthesis</a></p>
  
  <p><b>Note:</b> To run the source code you may need to grab the following NuGet Packages: Emgu CV, AForge, AForge Imaging, AForge Imaging, AForge Math, Accord, Accord Imaging, Accord Math, Accord Statistics</p>
</div>
<hr/>
<p><b>Task:</b></p>
<div style="margin-left:20px;">
  <p>a. The goal of this project is to develop a panoramic image synthesis system. You can either implement the basic stitching algorithm in [1] or use the algorithms/APIs implemented in OpenCV. (40 points)<br/>
  b. Experiment with at least two image blending algorithms, such as those provided by OpenCV (10 points)<br/>
  c. Implement the panorama straightening algorithm in [1] or come up with your own way to straighten a panorama  (10 points)<br/>
  d. In-class presentation (0-10 points)<br/>
  e. Project report (0-20 points)</p>
</div>

<p><b>Additions:</b></p>
<div style="margin-left:20px;">
  <p>+ More feature detection algorithms<br/>+ more feature correlation algorithms<br/>+ more blending algorithms</p>
</div>
<hr/>
<h2>About:</h2>
<p>This project started out as described in the task above, while working on setting up the project I ran into linking issues with OpenCV and decided to go with a few avalible opensource wrappers(See sources above)</p>
<p>In the libraries I was using SIFT was not avalible due to licensing issues so I started out by using <a href="https://en.wikipedia.org/wiki/Speeded_up_robust_features">SURF</a>.<p>
<p><b>Note</b>: The program reads in JPEG images in any order and always attempts to stitch them together. If the images aren to a good match you will get interesting results!</p>