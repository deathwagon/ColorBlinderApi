using ColorBlinder.Api.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ColorBlinder.Api.Analysis
{
  public static class EdgeCorrelate
  {
    public static IDictionary<ColorBlindTypes, double> GetScores(string folder)
    {
      var results = new Dictionary<ColorBlindTypes, double>(Enum.GetNames(typeof(ColorBlindTypes)).Count());

      var originalFile = Path.Combine(folder, "Normal.png");
      var files = Directory.GetFiles(folder, "*.png");
      var originalEdges = DetectEdges(originalFile);
      foreach (var file in files.Where(f => f != originalFile))
      {
        try
        {
          var type = (ColorBlindTypes)Enum.Parse(typeof(ColorBlindTypes), Path.GetFileNameWithoutExtension(file), true);
          var edges = DetectEdges(file);
          var score = CompareEdgesTemplateMatch(edges, originalEdges, file);
          results.Add(type, score);
        }
        catch
        {
          // TODO: Error reporting
        }
      }

      return results;
    }

    private static double CompareEdgesTemplateMatch(Image<Bgr, byte> edges1, Image<Bgr, byte> edges2, string imagePath)
    {
      var imgMatch = edges1.MatchTemplate(edges2, TemplateMatchingType.CcorrNormed);
      float[,,] matches = imgMatch.Data;
      var highestScore = 0d;
      for (int x = 0; x < matches.GetLength(0); x++)
      {
        for (int y = 0; y < matches.GetLength(1); y++)
        {
          var matchScore = matches[x, y, 0];
          if (matchScore > highestScore)
          {
            highestScore = matchScore;
          }
        }
      }
      return highestScore;
    }

    private static Image<Bgr, byte> DetectEdges(string imagePath)
    {
      var image = new Image<Bgr, byte>(imagePath).Resize(0.5, Inter.Linear);
      var edges = new Image<Bgr, byte>(image.Width, image.Height);
      CvInvoke.Canny(image, edges, 180d, 120d);
      var saveDir = Path.Combine(Path.GetDirectoryName(imagePath), "edges");
      if (!Directory.Exists(saveDir))
      {
        Directory.CreateDirectory(saveDir);
      }
      edges.Save(Path.Combine(saveDir, Path.GetFileName(imagePath)));
      return edges;
    }
  }
}