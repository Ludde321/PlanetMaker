using System.IO;
using System.Linq;
using TiffExpress;

namespace PlanetBuilder
{
    public class JoinViewFinderPanoramas
    {
        private string[] _inputFiles = {
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-A.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-B.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-C.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-D.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-E.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-F.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-G.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-H.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-I.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-J.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-K.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-L.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-M.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-N.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-O.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-P.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-Q.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-R.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-S.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-T.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-U.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-V.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-W.tif",
            @"Datasets\Planets\Earth\ViewFinderPanoramas\dem15\15-X.tif",
        };

        public void Join()
        {
            var tiffReaders = _inputFiles.Select(p => new TiffReader(File.OpenRead(p))).ToArray();
            var inputBitmaps = tiffReaders.Select(r => r.ReadImageFile<short>()).ToArray();

            var b1 = BitmapTools.Concatenate(inputBitmaps[0], inputBitmaps[1], inputBitmaps[2], inputBitmaps[3], inputBitmaps[4], inputBitmaps[5]);
            var b2 = BitmapTools.Concatenate(inputBitmaps[6], inputBitmaps[7], inputBitmaps[8], inputBitmaps[9], inputBitmaps[10], inputBitmaps[11]);
            var b3 = BitmapTools.Concatenate(inputBitmaps[12], inputBitmaps[13], inputBitmaps[14], inputBitmaps[15], inputBitmaps[16], inputBitmaps[17]);
            var b4 = BitmapTools.Concatenate(inputBitmaps[18], inputBitmaps[19], inputBitmaps[20], inputBitmaps[21], inputBitmaps[22], inputBitmaps[23]);

            var b = BitmapTools.Append(b1, b2, b3, b4);

            using (var tiffWriter = new TiffWriter(File.Create($@"Datasets\Planets\Earth\ViewFinderPanoramas\dem15.tif")))
            {
                tiffWriter.BigTiff = true;
//                var bitmap = _topElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                tiffWriter.WriteImageFile(b);
            }

            foreach(var reader in tiffReaders)
                reader.Dispose();
        }
    }
}