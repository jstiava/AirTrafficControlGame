using ATCGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace ATCGame;


public class Artist
{
    private Texture2D _pixel;

    public Artist(GraphicsDevice graphics)
    {
        _pixel = new Texture2D(graphics, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawLine(SpriteBatch spriteBatch, List<Vector2> points, Color color, int thickness)
    {
        if (points == null || points.Count < 2) return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 startWorld = points[i];
            Vector2 endWorld = points[i + 1];
            this.DrawLine(spriteBatch, startWorld, endWorld, color, thickness);
        }
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.DrawLine(start, end, color, thickness);
    }

    public void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, bool filled = true, int segments = 16)
    {
        Vector2[] vertex = new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float theta = (float)(i * 2.0 * Math.PI / segments);
            vertex[i] = new Vector2(
                center.X + (float)(radius * Math.Cos(theta)),
                center.Y + (float)(radius * Math.Sin(theta))
            );

            // If filled, draw a line from the center to this new point
            if (filled)
            {
                DrawLine(spriteBatch, center, vertex[i], color, 1);
            }
        }

        // Draw the outer edge (the outline)
        for (int i = 0; i < segments; i++)
        {
            DrawLine(spriteBatch, vertex[i], vertex[i + 1], color, 1);
        }
    }
}