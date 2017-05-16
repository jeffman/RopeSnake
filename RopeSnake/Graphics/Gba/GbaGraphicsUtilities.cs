using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using RopeSnake.Core;
using System.Collections;
using ColorSet = System.Collections.Generic.HashSet<System.Drawing.Color>;

namespace RopeSnake.Graphics
{
    public static class GbaGraphicsUtilities
    {
        internal sealed class GbaColorComparer : IEqualityComparer<Color>
        {
            public bool Equals(Color x, Color y)
            {
                // Two fully transparent colors are equal
                if (x.A == 0 && y.A == 0)
                    return true;

                // Color channels will be truncated to 5 bits anyhow, so ignore the
                // lowest 3 bits for comparison
                return (x.ToArgb() & 0x1F8F8F8) == (y.ToArgb() & 0x1F8F8F8);
            }

            public int GetHashCode(Color obj)
            {
                // Two fully transparent colors are equal
                if (obj.A == 0)
                    unchecked { return (int)0xFF000000; }

                // Partially transparent colors are disallowed
                if (obj.A != 255)
                    throw new InvalidOperationException("Colors must be fully transparent or fully opaque");

                return obj.ToArgb() & 0xFFFFFF;
            }
        }

        internal static IEqualityComparer<Color> _colorComparer;
        internal static bool[] _intToBool = { false, true };

        static GbaGraphicsUtilities()
        {
            _colorComparer = new GbaColorComparer();
        }

        public static GraphicsSet ParseBitmap(Bitmap image, int numPalettes, int colorsPerPalette)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (numPalettes < 1)
                throw new ArgumentException(nameof(numPalettes));

            if (colorsPerPalette < 1)
                throw new ArgumentException(nameof(colorsPerPalette));

            if (((image.Width & 7) != 0) || ((image.Height & 7) != 0))
                throw new InvalidOperationException("Bitmap dimensions must be a multiple of 8");

            using (var canvas = Canvas.Create(image))
            {
                switch (image.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppArgb:
                        return ResolveRasterImage(canvas, numPalettes, colorsPerPalette);

                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format8bppIndexed:
                        return ResolveIndexedImage(canvas, numPalettes, colorsPerPalette);

                    default:
                        throw new InvalidOperationException($"Invalid pixel format: {image.PixelFormat}");
                }
            }
        }

        /// <summary>
        /// Resolves an image (accessed through a Canvas) into a tileset, palette, and tilemap.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="maxPalettes"></param>
        /// <param name="maxColorsPerPalette"></param>
        /// <returns></returns>
        /// <remarks>
        /// The goal here is to find a resolution with the lowest probability of failure. This is
        /// kinda related to set cover/maximum coverage problems, which are NP-hard. So this is
        /// sub-optimal algorithm
        /// 
        /// I will explain this assuming maxColorsPerPalette == 15 for simplicity, but it works
        /// for any positive number. (15 instead of 16 because color 0 is always transparent.)
        /// 
        /// Start by mapping every tile to a set of colors contained within that tile. If any set
        /// exceeds 15 colors, fail because there is no possible resolution (only one palette
        /// is allowed per tile).
        /// 
        /// Find a set that is a subset of another set. Map the corresponding tile to the superset
        /// and delete the subset. (Every set must be resolved, including all supersets; and if a
        /// superset is resolved, then necessarily all its subsets are resolved as well.) Repeat
        /// for all such subsets.
        /// 
        /// We now have only sets that are neither a subset nor a superset of any other set.
        /// 
        /// Combining phase: (greedy algorithm implemented here)
        /// 
        /// Take the union of the first and second sets. (1) If the resulting size exceeds 15, resolve
        /// the first set and remove it from consideration. Repeat with the union of the second and
        /// third set, and so on. (2) If the resulting size is less than 15, merge the two sets and repeat
        /// with the union of this union and the third set, and so on. (3) If the resulting size equals
        /// 15, resolve the union and remove both from consideration. Repeat until all sets are resolved.
        /// 
        /// Combining phase: (more optimized for palette space, slower, not implemented here)
        /// 
        /// Find all sets that contain exactly 15 colors. These sets cannot be added to since they
        /// already contain the highest number of allowed colors. Mark these sets as "resolved" and
        /// put them into a collection of allocated palettes. 
        /// 
        /// Find a set that contains 14 colors. There is room to merge this set with one other
        /// set that partially overlaps it with at most one "odd color out". Find all such overlapping
        /// sets and pick one with the largest union. (If more than one such set has the largest
        /// size, pick any one of them). Merge that set with the 14-color set and map all tiles
        /// that point to either of those two sets to point to the merged set. Mark it as "resolved"
        /// and allocate it. If no sets overlap as such, mark the current set as resolved without
        /// merging it with anything.
        /// 
        /// Find a set that contains 13 colors. Find all sets that overlap it with at most two odd
        /// colors out. Pick one with the largest union. Merge the two sets. If there is still room
        /// for one more color, repeat until there is no more room.
        /// 
        /// And so on, until either all sets are resolved or it is determined that there is no
        /// possible resolution.
        /// 
        /// Color resolving phase:
        /// 
        /// For 16-color images, assign transparent to index 0 and only worry about the remaining
        /// 15 colors. For 256-color images, there's no transparency and we can resolve colors
        /// somewhat arbitrarily. One caveat is that color 0 will not show up if the background
        /// is covered by any opaque pixels from sprites or other BG layers. So we should resolve
        /// colors starting at index 1 anyway if possible.
        /// 
        /// The assignment of opaque colors is important. There is no order to the color sets, but
        /// there is order to the pixels within each tile. Consider two tiles that have identical
        /// pixel structure but different palettes. If the color indices do not match, then those
        /// tiles will not be considered equivalent during the tile optimizing phase. To increase
        /// the probability that the tiles will be matched later on, assign colors to indices
        /// according to their first appearance within the pixels of a tile in row-major order.
        /// 
        /// This can be done lazily during the pixel resolving phase.
        /// 
        /// Pixel resolving phase:
        /// 
        /// Map each set to a lookup of color to int. Iterate over each tile. Get the set
        /// corresponding to that tile, and then get the color lookup. Iterate over the pixels of
        /// the tile. For each pixel: (1) If transparent and in 16-color mode, assign it index 0.
        /// (2) If not transparent, see if the color exists in the lookup. If so, assign that value
        /// to the pixel. If not, allocate that color in the lookup and assign that value to the
        /// pixel.
        /// 
        /// Return the tileset, palettes and tilemap.
        /// </remarks>
        internal static GraphicsSet ResolveRasterImage(Canvas canvas, int maxPalettes, int maxColorsPerPalette)
        {
            if (maxColorsPerPalette != 16 & maxColorsPerPalette != 256)
                throw new ArgumentException(nameof(maxColorsPerPalette));

            bool transparency = (maxColorsPerPalette == 16);

            if (transparency)
            {
                // Subtract 1 for transparency
                maxColorsPerPalette--;
            }

            var uniqueColors = new ColorSet(_colorComparer);
            var resolvedSets = new List<ColorSet>();

            // Map tiles to color sets
            var tileToSet = new ColorSet[canvas.Width * canvas.Height / 64];
            var setToTile = new Dictionary<ColorSet, HashSet<int>>();

            iterateTiles((tileX, tileY) =>
            {
                var set = new ColorSet(_colorComparer);
                for (int pixelY = 0; pixelY < 8; pixelY++)
                {
                    for (int pixelX = 0; pixelX < 8; pixelX++)
                    {
                        var color = canvas.GetColor(pixelX + tileX * 8, pixelY + tileY * 8);
                        bool isColorTransparent = _colorComparer.Equals(color, Color.Transparent);

                        if (!transparency && isColorTransparent)
                            throw new Exception("Transparency not supported for 256-color images");

                        if (!isColorTransparent)
                        {
                            set.Add(color);
                            uniqueColors.Add(color);

                            if (uniqueColors.Count > maxColorsPerPalette * maxPalettes)
                                throw new Exception("Maximum unique colors exceeded");
                        }
                    }
                }

                if (set.Count > maxColorsPerPalette)
                    throw new Exception($"Maximum allowed colors per palette exceeded: 8x8 tile at ({tileX * 8}, {tileY * 8})");

                int tileIndex = coordToIndex(tileX, tileY);
                tileToSet[tileIndex] = set;
                setToTile[set] = new HashSet<int>
                {
                        tileIndex
                };
            });

            if (uniqueColors.Count <= maxColorsPerPalette)
            {
                for (int i = 0; i < tileToSet.Length; i++)
                    tileToSet[i] = uniqueColors;

                resolvedSets.Add(uniqueColors);
            }
            else
            {
                // Remap subsets
                for (int currentIndex = 0; currentIndex < tileToSet.Length; currentIndex++)
                {
                    var currentSet = tileToSet[currentIndex];

                    for (int superIndex = 0; superIndex < tileToSet.Length; superIndex++)
                    {
                        var superSet = tileToSet[superIndex];
                        if (superSet == currentSet)
                            continue;

                        if (superSet.IsSupersetOf(currentSet))
                        {
                            setToTile.Remove(currentSet);
                            setToTile[superSet].Add(currentIndex);

                            tileToSet[currentIndex] = superSet;
                            currentSet = superSet;
                        }
                    }
                }

                // Merge
                var uniqueSets = new HashSet<ColorSet>(tileToSet);
                var sortedSets = new List<ColorSet>(uniqueSets.OrderByDescending(s => s.Count));

                var lastSet = sortedSets[0];
                sortedSets.RemoveAt(0);

                while (sortedSets.Count > 0)
                {
                    var set = sortedSets[0];
                    var union = new ColorSet(lastSet.Union(set), _colorComparer);

                    if (union.Count <= maxColorsPerPalette)
                    {
                        var tiles = new HashSet<int>(setToTile[set].Union(setToTile[lastSet]));
                        foreach (int tile in tiles)
                            tileToSet[tile] = union;

                        setToTile.Remove(lastSet);
                        setToTile.Remove(set);
                        setToTile[union] = tiles;

                        if (union.Count == maxColorsPerPalette && sortedSets.Count > 1)
                        {
                            lastSet = sortedSets[1];
                        }
                        else
                        {
                            lastSet = union;
                        }
                    }
                    else
                    {
                        resolvedSets.Add(lastSet);
                        lastSet = set;
                    }

                    sortedSets.Remove(set);
                }

                resolvedSets.Add(lastSet);
            }

            // Generate tiles
            var tilemap = new Tilemap(canvas.Width / 8, canvas.Height / 8, 8, 8);
            var uniqueTiles = new Dictionary<HashedTile, int>();

            var colorLookup = new Dictionary<Color, int>[resolvedSets.Count];
            var setLookup = new Dictionary<ColorSet, int>();

            for (int i = 0; i < resolvedSets.Count; i++)
            {
                colorLookup[i] = new Dictionary<Color, int>(_colorComparer);
                setLookup[resolvedSets[i]] = i;

                if (transparency)
                    colorLookup[i][Color.Transparent] = 0;
            }

            iterateTiles((tileX, tileY) =>
            {
                var tile = new Tile(8, 8);
                int tileIndex = coordToIndex(tileX, tileY);
                int paletteIndex = setLookup[tileToSet[tileIndex]];
                var lookup = colorLookup[paletteIndex];

                    // Assign colors and render pixels
                    for (int pixelY = 0; pixelY < 8; pixelY++)
                {
                    for (int pixelX = 0; pixelX < 8; pixelX++)
                    {
                        var color = canvas.GetColor(pixelX + tileX * 8, pixelY + tileY * 8);

                        if (!_colorComparer.Equals(color, Color.Transparent))
                        {
                            if (!lookup.TryGetValue(color, out int colorIndex))
                            {
                                colorIndex = lookup.Count + (transparency ? 0 : 1);
                                if (colorIndex == 256)
                                {
                                        // Use color 0 as a last resort
                                        colorIndex = 0;
                                    RLog.Warn("Using all 256 available color slots");
                                }

                                lookup.Add(color, colorIndex);
                            }

                            tile[pixelX, pixelY] = (byte)colorIndex;
                        }
                        else
                        {
                            tile[pixelX, pixelY] = 0;
                        }
                    }
                }

                    // Check for equivalence to an existing tile
                    tilemap[tileX, tileY] = AddTileAndGetInfo(tile, paletteIndex, uniqueTiles); ;
            });

            var tileset = uniqueTiles.OrderBy(kv => kv.Value).Select(kv => kv.Key.Tile).ToList();

            // Generate palettes
            if (transparency)
                maxColorsPerPalette++;

            var palette = new Palette(maxPalettes, maxColorsPerPalette);

            for (int i = 0; i < resolvedSets.Count; i++)
            {
                var lookup = colorLookup[i];

                foreach (var kv in lookup)
                    palette.SetColor(i, kv.Value, kv.Key);
            }

            return new GraphicsSet(tilemap, tileset, palette);

            int coordToIndex(int tileX, int tileY)
                => tileX + (tileY * canvas.Width / 8);

            void iterateTiles(Action<int, int> action)
            {
                for (int tileY = 0; tileY < canvas.Height / 8; tileY++)
                {
                    for (int tileX = 0; tileX < canvas.Width / 8; tileX++)
                    {
                        action(tileX, tileY);
                    }
                }
            }
        }

        internal static GraphicsSet ResolveIndexedImage(Canvas canvas, int numPalettes, int colorsPerPalette)
        {
            int colorIndexMask = colorsPerPalette - 1;
            int paletteIndexShift = (int)Math.Log(colorsPerPalette, 2);

            var palette = new Palette(numPalettes, colorsPerPalette);
            var imageColorEntries = canvas.BaseBitmap.Palette.Entries;

            for (int i = 0; i < imageColorEntries.Length; i++)
            {
                palette.SetColor(i, imageColorEntries[i]);
            }

            var tilemap = new Tilemap(canvas.Width / 8, canvas.Height / 8, 8, 8);
            var uniqueTiles = new Dictionary<HashedTile, int>();

            for (int tileY = 0; tileY < tilemap.TileHeight; tileY++)
            {
                for (int tileX = 0; tileX < tilemap.TileWidth; tileX++)
                {
                    var tile = new Tile(8, 8);
                    int paletteIndex = -1;

                    for (int pixelY = 0; pixelY < 8; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < 8; pixelX++)
                        {
                            int pixel = canvas.GetValue(pixelX + (tileX * 8), pixelY + (tileY * 8));

                            int currentPalette = pixel >> paletteIndexShift;
                            int colorIndex = pixel & colorIndexMask;

                            if (paletteIndex >= 0)
                            {
                                if (currentPalette != paletteIndex)
                                    throw new Exception($"Cannot have more than one palette per tile: ({tileX}, {tileY})");
                            }
                            else
                            {
                                paletteIndex = currentPalette;
                            }

                            tile[pixelX, pixelY] = (byte)colorIndex;
                        }
                    }

                    tilemap[tileX, tileY] = AddTileAndGetInfo(tile, paletteIndex, uniqueTiles);
                }
            }

            var tileset = uniqueTiles.OrderBy(kv => kv.Value).Select(kv => kv.Key.Tile).ToList();
            return new GraphicsSet(tilemap, tileset, palette);
        }

        internal static TileInfo AddTileAndGetInfo(Tile tile, int paletteIndex, Dictionary<HashedTile, int> uniqueTiles)
        {
            HashedTile hashedTileNoFlip = null;
            TileInfo tileInfo = default(TileInfo);
            bool found = false;
            int uniqueTileIndex = 0;

            for (int iFlipY = 0; iFlipY < 2 && !found; iFlipY++)
            {
                for (int iFlipX = 0; iFlipX < 2 && !found; iFlipX++)
                {
                    bool flipX = _intToBool[iFlipX];
                    bool flipY = _intToBool[iFlipY];

                    var hashedTile = new HashedTile(tile, flipX, flipY);

                    if (!(flipX || flipY))
                        hashedTileNoFlip = hashedTile;

                    if (uniqueTiles.TryGetValue(hashedTile, out uniqueTileIndex))
                    {
                        tileInfo = new TileInfo(uniqueTileIndex, paletteIndex, flipX, flipY);
                        found = true;
                    }
                }
            }

            if (!found)
            {
                uniqueTileIndex = uniqueTiles.Count;
                tileInfo = new TileInfo(uniqueTileIndex, paletteIndex, false, false);
                uniqueTiles.Add(hashedTileNoFlip, uniqueTileIndex);
            }

            return tileInfo;
        }
    }
}
