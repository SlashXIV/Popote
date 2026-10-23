#if ANDROID
using Android.Graphics;
using Android.Media;
#endif

namespace Popote.Services;

// Miniatures des photos pour la liste des recettes (docs/recipe-photo.md) : la liste n'a besoin
// que de 64 dp, l'original (jusqu'à 4000 × 3000) coûtait ~70 Mo de mémoire au défilement.
// « recipe_x.jpg » → « recipe_x.min.jpg » à côté ; pas de colonne en base : le fichier fait foi.
public static class PhotoThumbnails
{
    private const int ShortSide = 360; // px : ~1,6 × la vignette de 64 dp à densité 3,5
    private const int JpegQuality = 80;

    public static string PathFor(string photoPath) => System.IO.Path.ChangeExtension(photoPath, ".min.jpg");

    // La miniature si elle existe, sinon l'original (photo pas encore traitée).
    public static string? Best(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath)) return photoPath;
        var thumb = PathFor(photoPath);
        return File.Exists(thumb) ? thumb : photoPath;
    }

    // Crée les miniatures absentes. Ne lève jamais : une photo illisible garde l'original.
    public static void CreateMissing(IEnumerable<string> photoPaths)
    {
        foreach (var photo in photoPaths)
        {
            try
            {
                if (File.Exists(photo) && !File.Exists(PathFor(photo))) Create(photo);
            }
            catch (Exception) { /* fichier illisible : la liste affichera l'original */ }
        }
    }

    private static void Create(string photoPath)
    {
#if ANDROID
        var bounds = new BitmapFactory.Options { InJustDecodeBounds = true };
        BitmapFactory.DecodeFile(photoPath, bounds);
        var shortSide = Math.Min(bounds.OutWidth, bounds.OutHeight);
        if (shortSide <= 0) return; // pas une image

        // Décodage déjà réduit (puissance de 2) : on ne charge jamais l'original en entier.
        var sample = 1;
        while (shortSide / (sample * 2) >= ShortSide) sample *= 2;
        using var decoded = BitmapFactory.DecodeFile(photoPath, new BitmapFactory.Options { InSampleSize = sample });
        if (decoded is null) return;

        var scale = Math.Min(1f, (float)ShortSide / Math.Min(decoded.Width, decoded.Height));
        using var matrix = new Matrix();
        matrix.PostScale(scale, scale);
        matrix.PostRotate(ExifRotation(photoPath)); // BitmapFactory ignore l'orientation EXIF des photos d'appareil
        using var thumb = Bitmap.CreateBitmap(decoded, 0, 0, decoded.Width, decoded.Height, matrix, true);

        var target = PathFor(photoPath);
        var tmp = target + ".tmp"; // jamais de miniature à moitié écrite
        using (var f = File.Create(tmp))
            thumb.Compress(Bitmap.CompressFormat.Jpeg!, JpegQuality, f);
        File.Move(tmp, target, overwrite: true);
#endif
    }

#if ANDROID
    // shortcut: orientations miroir (2, 4, 5, 7) traitées comme normales, rares hors selfies retouchés.
    private static float ExifRotation(string path)
    {
        using var exif = new ExifInterface(path);
        return exif.GetAttributeInt(ExifInterface.TagOrientation, 1) switch
        {
            6 => 90,
            3 => 180,
            8 => 270,
            _ => 0,
        };
    }
#endif
}
