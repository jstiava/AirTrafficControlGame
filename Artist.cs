using ATCGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATCGame;


public class Artist
{
    private Texture2D _pixel;

    public Artist(GraphicsDevice graphics)
    {
        _pixel = new Texture2D(graphics, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    void DrawTriangle(
    SpriteBatch spriteBatch,
    Vector2 p1,
    Vector2 p2,
    Vector2 p3,
    Color color)
    {
        Vector2 edge1 = p2 - p1;
        Vector2 edge2 = p3 - p1;

        float width = edge1.Length();
        float height = edge2.Length();
        float rotation = (float)Math.Atan2(edge1.Y, edge1.X);

        spriteBatch.Draw(
            _pixel,
            p1,
            null,
            color,
            rotation,
            Vector2.Zero,
            new Vector2(width, height),
            SpriteEffects.None,
            0f
        );
    }

    public void FillPolygonConcave(
    SpriteBatch spriteBatch,
    List<Vector2> points,
    Color color)
    {
        if (points.Count < 3)
            return;

        int minY = (int)Math.Floor(points.Min(p => p.Y));
        int maxY = (int)Math.Ceiling(points.Max(p => p.Y));

        for (int y = minY; y <= maxY; y++)
        {
            List<float> intersections = new();

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % points.Count];

                // Skip horizontal edges
                if (p1.Y == p2.Y)
                    continue;

                bool intersects =
                    (y >= p1.Y && y < p2.Y) ||
                    (y >= p2.Y && y < p1.Y);

                if (!intersects)
                    continue;

                float x = p1.X + (y - p1.Y) *
                          (p2.X - p1.X) /
                          (p2.Y - p1.Y);

                intersections.Add(x);
            }

            intersections.Sort();

            for (int i = 0; i < intersections.Count - 1; i += 2)
            {
                int startX = (int)Math.Ceiling(intersections[i]);
                int endX = (int)Math.Floor(intersections[i + 1]);

                if (endX > startX)
                {
                    spriteBatch.Draw(
                        _pixel,
                        new Rectangle(startX, y, endX - startX, 1),
                        color);
                }
            }
        }
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