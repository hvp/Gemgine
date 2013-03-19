using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui.SpriteBatchExtensions
{
    public static class DrawBorderExtension
    {
        public static void TileBlit(this SpriteBatch Batch,
            Texture2D Texture, int X, int Y, int W, int H, int SX, int SY, int SW, int SH)
        {
            int PX = X, PY = Y, EX = X + W, EY = Y + H;

            while (PX < EX)
            {
                if (PX + SW > EX)
                    while (PY < EY)
                    {
                        if (PY + SH > EY)
                            Batch.Draw(Texture, new Rectangle(PX, PY, EX - PX, EY - PY),
                                new Rectangle(SX, SY, EX - PX, EY - PY), Color.White);
                        else
                            Batch.Draw(Texture, new Rectangle(PX, PY, EX - PX, SH),
                                new Rectangle(SX, SY, EX - PX, SH), Color.White);
                        PY += SH;
                    }
                else
                    while (PY < EY)
                    {
                        if (PY + SH > EY)
                            Batch.Draw(Texture, new Rectangle(PX, PY, SW, EY - PY),
                                new Rectangle(SX, SY, SW, EY - PY), Color.White);
                        else
                            Batch.Draw(Texture, new Rectangle(PX, PY, SW, SH),
                                new Rectangle(SX, SY, SW, SH), Color.White);
                        PY += SH;
                    }
                PX += SW;
                PY = Y;
            }
        }

        public static void BorderExplodeBlit(this SpriteBatch Batch,
            Texture2D Texture, int X, int Y, int W, int H, int CornerW, int CornerH, bool DrawCenter)
        {
            int WMC = W - CornerW;
            int HMC = H - CornerH;

            int TWMC = Texture.Width - CornerW;
            int THMC = Texture.Height - CornerH;

            //Draw corners
            Batch.Draw(Texture, new Rectangle(X, Y, CornerW, CornerH), new Rectangle(0, 0, CornerW, CornerH), Color.White);
            Batch.Draw(Texture, new Rectangle(X + WMC, Y, CornerW, CornerH), new Rectangle(TWMC, 0, CornerW, CornerH), Color.White);
            Batch.Draw(Texture, new Rectangle(X, Y + HMC, CornerW, CornerH), new Rectangle(0, THMC, CornerW, CornerH), Color.White);
            Batch.Draw(Texture, new Rectangle(X + WMC, Y + HMC, CornerW, CornerH), new Rectangle(TWMC, THMC, CornerW, CornerH), Color.White);

            //draw sides
            Batch.TileBlit(Texture, X + CornerW, Y, W - CornerW - CornerW, CornerH, CornerW, 0, Texture.Width - CornerW - CornerW, CornerH);
            Batch.TileBlit(Texture, X, Y + CornerH, CornerW, H - CornerH - CornerH, 0, CornerH, CornerW, Texture.Height - CornerH - CornerH);
            Batch.TileBlit(Texture, X + W - CornerW, Y + CornerH, CornerW, H - CornerH - CornerH, Texture.Width - CornerW, CornerH, CornerW, Texture.Height - CornerH - CornerH);
            Batch.TileBlit(Texture, X + CornerW, Y + H - CornerH, W - CornerW - CornerW, CornerH, CornerW, Texture.Height - CornerH, Texture.Width - CornerW - CornerW, CornerH);

            //draw center
            if (DrawCenter)
            Batch.TileBlit(Texture, X + CornerW, Y + CornerH, W - CornerW - CornerW, H - CornerH - CornerH, CornerW, CornerH, Texture.Width - CornerW - CornerW, Texture.Height - CornerH - CornerH);

        }

    }
}
