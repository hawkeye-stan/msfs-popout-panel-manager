using AForge.Imaging;
using System.Drawing;
using System.Linq;
namespace MSFSPopoutPanelManager
{
    public class ImageAnalysis
    {
        public static Point ExhaustiveTemplateMatchAnalysis(Bitmap sourceImage, Bitmap templateImage, int imageShrinkFactor, float similarityThreshHold)
        {
            // Full image pixel to pixel matching algorithm
            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(similarityThreshHold);
            TemplateMatch[] templateMatches = etm.ProcessImage(sourceImage, templateImage);

            // Highlight the matchings that were found and saved a copy of the highlighted image
            if (templateMatches != null && templateMatches.Length > 0)
            {
                var match = templateMatches.OrderByDescending(x => x.Similarity).First();  // Just look at the first match since only one operation can be accomplished at a time on MSFS side

                var x = match.Rectangle.X * imageShrinkFactor + templateImage.Width * imageShrinkFactor / 4;
                var y = match.Rectangle.Y * imageShrinkFactor + templateImage.Height * imageShrinkFactor / 4;

                return new Point(x, y);
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

            // Full image pixel to pixel matching algorithm
            //ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(similarityThreshHold);
            //TemplateMatch[] templateMatches = etm.ProcessImage(sourceImage, templateImage);

            //// Highlight the matchings that were found and saved a copy of the highlighted image
            //if (templateMatches != null && templateMatches.Length > 0)
            //{
            //    var imageMatched = templateMatches.ToList().Max(x => x.Similarity);
            //    return imageMatched;
            //}

            //return 0;
        }
    }
}
