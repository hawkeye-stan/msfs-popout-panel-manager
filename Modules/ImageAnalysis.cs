using AForge.Imaging;
using System;
using System.Drawing;
using System.Linq;

namespace MSFSPopoutPanelManager
{
    public class ImageAnalysis
    {
        public static Point ExhaustiveTemplateMatchAnalysisAsync(Bitmap sourceImage, Bitmap templateImage, float similarityThreshHold, int panelStartingTop, int panelStartingLeft)
        {
            var x = panelStartingLeft - Convert.ToInt32(templateImage.Width * 1.5);
            var y = 0;
            var width = Convert.ToInt32(templateImage.Width * 1.5);
            var height = sourceImage.Height;

            var searchZone = new Rectangle(x, y, width, height);

            var point = AnalyzeExpandImageBitmap(sourceImage, templateImage, similarityThreshHold, searchZone);

            if (point != Point.Empty)
                point.Y += panelStartingTop;

            return point;
        }

        public static Point AnalyzeExpandImageBitmap(Bitmap sourceImage, Bitmap templateImage, float similarityThreshHold, Rectangle searchZone)
        {
            // Full image pixel to pixel matching algorithm
            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(similarityThreshHold);
            TemplateMatch[] templateMatches = etm.ProcessImage(sourceImage, templateImage, searchZone);

            if (templateMatches != null && templateMatches.Length > 0)
            {
                var match = templateMatches.OrderByDescending(x => x.Similarity).First();  // Just look at the first match

                var xCoor = match.Rectangle.X + templateImage.Width / 12;
                var yCoor = match.Rectangle.Y + templateImage.Height / 4;

                return new Point(Convert.ToInt32(xCoor), Convert.ToInt32(yCoor));
            }

            return Point.Empty;
        }

        public static float ExhaustiveTemplateMatchAnalysisScore(Bitmap sourceImage, Bitmap templateImage, float similarityThreshHold)
        {
            // SUSAN corner block matching algorithm
            SusanCornersDetector scd = new SusanCornersDetector(50, 8);
            var points = scd.ProcessImage(sourceImage);

            // process images searching for block matchings
            ExhaustiveBlockMatching bm = new ExhaustiveBlockMatching(4, 8);
            sourceImage = ImageOperation.ResizeImage(sourceImage, 800, 600);
            templateImage = ImageOperation.ResizeImage(templateImage, 800, 600);
            var templateMatches = bm.ProcessImage(sourceImage, points, templateImage);

            return templateMatches.Count;
        }
    }
}
