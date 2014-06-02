using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Scott.Matrix
{
    class MatrixScreen
    {
        enum GlyphType { Header, Trailer };

        // Falling Glyph Object Class
        private class Glyph
        {
            public Vector2 pos;
            public GlyphType type;
            public int lifespan;
            public int trailerLife;

            public Color color = new Color(183, 255, 235);
            public int colorTimer = 0;
            public int colorDelay = 225;

            public string symbol;
            public int symbolTimer = 0;
            public int symbolDelay = 850;

            private Random rand = new Random();


            public Glyph(int x, int y)
            {
                pos = new Vector2(x, y);
                type = GlyphType.Header;
                lifespan = rand.Next(1500, 15000);
                trailerLife = rand.Next((int)(lifespan * .5), (int)(lifespan * .75));
                symbol = newSymbol();
            }
            public Glyph(int x, int y, int life)
            {
                pos = new Vector2(x, y);
                type = GlyphType.Trailer;
                lifespan = life;
                symbol = newSymbol();
            }

            public string newSymbol()
            {
                return ("" + (char)rand.Next(68, 127));
                //2E80 - 31BF
            }
        }


        // Matrix screen data members
        private List<Glyph> GlyphList = new List<Glyph>();
        public int dropTimer = 0;
        
        private Rectangle mScreen;      //screen size
        private Random rand = new Random();

        private const int dropDelay = 120;
        private const int gw = 20;    //Glyph width
        private const int gh = 20;    //Glyph height

        private Texture2D blackout;
        private SpriteFont fontTemp;


        // Matrix screen Constructor
        public MatrixScreen(int width, int height, GraphicsDevice gDev, SpriteFont font)
        {
            mScreen =  new Rectangle(0, 0, width, height);

            blackout = new Texture2D(gDev, gw, gh, true, SurfaceFormat.Color);
            UInt32[] toSet = new UInt32[gw * gh];
            for (int i = 0; i < (gw * gh); i++)
                toSet[i] = 0xFF000000;
            blackout.SetData<UInt32>(toSet);

            fontTemp = font;
        }


        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="sprBatch"></param>
        public void Draw(SpriteBatch sprBatch)
        {
            foreach (Glyph s in GlyphList)
            {
                sprBatch.Draw(blackout, new Rectangle((int)s.pos.X - gw, (int)s.pos.Y - gh, gw, gh), Color.White);
                sprBatch.DrawString(fontTemp, s.symbol, s.pos, s.color, (float)Math.PI, new Vector2(0,0), 1.0f, SpriteEffects.FlipHorizontally, 0);
            }
        }

        /// <summary>
        /// Adjust symbol, color, position
        /// </summary>
        /// <param name="time"></param>
        public void Update(GameTime time)
        {
            List<Glyph> trailers = new List<Glyph>();
            List<Glyph> garbage = new List<Glyph>();
            
            int elapsedMS = time.ElapsedGameTime.Milliseconds;
            dropTimer += elapsedMS;

            foreach (Glyph s in GlyphList)
            {
                s.lifespan -= elapsedMS;                                //decrease life timer
                s.colorTimer += elapsedMS;                              //increment color-change timer
                s.symbolTimer += elapsedMS;                             //increment symbol-change timer

                if (s.pos.Y > mScreen.Height || s.lifespan <= 0)        //destroy glyph?
                    garbage.Add(s);
                else
                {
                    if (s.type == GlyphType.Header)
                    {
                        if (dropTimer >= dropDelay)
                        {
                            s.symbol = s.newSymbol();                                           //change symbol
                            trailers.Add(new Glyph((int)s.pos.X, (int)s.pos.Y, s.trailerLife));     //add trailer
                            s.pos.Y += gh;                                                          //drop down
                        }
                    }
                    else if (s.type == GlyphType.Trailer)
                    {
                        if (rand.Next(s.colorTimer) > s.colorDelay)
                        {
                            s.colorTimer = s.colorTimer % s.colorDelay;
                            s.color = new Color(0, rand.Next(128, 224), rand.Next(16, 48));     //change trailer color
                        }

                        if (rand.Next(s.symbolTimer) > s.symbolDelay)
                        {
                            s.symbolTimer = s.symbolTimer % s.symbolDelay;
                            s.symbol = s.newSymbol();                                       //change trailer symbol
                        }
                    }
                }
            }
            dropTimer = dropTimer % dropDelay;

            //Clean up garbage
            foreach (Glyph del in garbage)
                GlyphList.Remove(del);
            garbage.Clear();

            //Insert new glyphs into master list
            foreach (Glyph add in trailers)
            {
                add.color = new Color(0, rand.Next(128, 224), rand.Next(16, 48));    //perform first color-change
                add.symbolDelay = rand.Next(300, 550);                           //randomize symbol-change delay
                if (rand.Next(100) <= 80)               //chance of being blank
                    GlyphList.Add(add);
            }
            trailers.Clear();

            //Add new Glyph?
            if (rand.Next(time.TotalGameTime.Milliseconds % 1000) <= 18)
            {
                int intervals = mScreen.Width / gw;
                int column = rand.Next(intervals + 1) * gw;
                GlyphList.Add(new Glyph(column - 5, -gh));
            }
        }
    }
}
