using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace ATCGame.Entities;

public class Aircraft
{
    public Texture2D _texture;
    public Rectangle _bounds;
    public float _speed;

    public Vector2 Position;
    public Vector2 ScreenPosition;
    public float Heading;

    public Aircraft(Texture2D texture, Vector2 position)
    {
        this._texture = texture;
        this.Position = position;
        this.ScreenPosition = position;
        this.Heading = 45;
    }

    public void Reposition(Vector2 position, int heading = 45)
    {
        this.ScreenPosition = position;
        this.Rotate(heading);
    }

    public void Rotate(int heading = 45)
    {
        this.Heading = (heading - 45f) % 360f;
    }

    public void Draw(SpriteBatch spriteBatch, Artist artist, int size)
    {
        bool DEBUG_FLAG = false;

        Rectangle destinationRect = new Rectangle((int)(ScreenPosition.X), (int)(ScreenPosition.Y), size, size);
        float rotation = MathHelper.ToRadians(this.Heading);

        if (DEBUG_FLAG)
        {

            float rotationAgain = MathHelper.ToRadians(this.Heading - 45f);

            // 2. Calculate the tip of the line using Trigonometry
            Vector2 forwardDirection = new Vector2(
                (float)Math.Cos(rotationAgain),
                (float)Math.Sin(rotationAgain)
            );

            Vector2 endPoint = this.ScreenPosition + (forwardDirection * 100);

            // 3. Draw the line using your artist helper
            // If you have a DrawLine method:
            artist.DrawLine(spriteBatch, this.ScreenPosition, endPoint, Color.White, 1);

            artist.DrawCircle(spriteBatch, this.ScreenPosition, size * 0.75f, Color.Red, false);
        }

        spriteBatch.Draw(
            texture: _texture,
            destinationRectangle: destinationRect,
            sourceRectangle: null,
            color: Color.White,
            rotation: rotation,
            origin: new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f),
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}
