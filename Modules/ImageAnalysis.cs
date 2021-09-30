using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
namespace MSFSPopoutPanelManager
{
    public class ImageAnalysis
    {
        public static Point ExhaustiveTemplateMatchAnalysis(Bitmap sourceImage, Bitmap templateImage, int imageShrinkFactor, float similarityThreshHold)
        {
            const int MICROSOFT_WINDOWS_HANDLE_FRAME_X_VALUE_ADJUSTMENT = -8;

            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(similarityThreshHold);
            TemplateMatch[] templateMatches = etm.ProcessImage(sourceImage, templateImage);

            // Highlight the matchings that were found and saved a copy of the highlighted image
            if (templateMatches != null && templateMatches.Length > 0)
            {
                var match = templateMatches.OrderByDescending(x => x.Similarity).First();  // Just look at the first match since only one operation can be accomplished at a time on MSFS side

                var x = match.Rectangle.X * imageShrinkFactor;
                var y = match.Rectangle.Y * imageShrinkFactor;
                //var width = match.Rectangle.Width * imageShrinkFactor;
                //var height = match.Rectangle.Height * imageShrinkFactor;
                //var centerX = x + width / 2 + MICROSOFT_WINDOWS_HANDLE_FRAME_X_VALUE_ADJUSTMENT;
                //var centerY = y + height / 2;

                return new Point(x, y);
            }

            return Point.Empty;
        }

        public static float ExhaustiveTemplateMatchAnalysisScore(Bitmap sourceImage, Bitmap templateImage, float similarityThreshHold)
        {
            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(similarityThreshHold);
            TemplateMatch[] templateMatches = etm.ProcessImage(sourceImage, templateImage);

            // Highlight the matchings that were found and saved a copy of the highlighted image
            if (templateMatches != null && templateMatches.Length > 0)
            {
                var imageMatched = templateMatches.ToList().Max(x => x.Similarity);
                return imageMatched;
            }

            return 0;
        }
    }
}
