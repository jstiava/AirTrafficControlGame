using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ATCGame.Utilities;

public class ViewportTransformer
{
    private float _minX, _minY, _scale;
    private int _padding = 50;
    private int _screenWidth, _screenHeight;

    public ViewportTransformer(float minX, float maxX, float minY, float maxY, int screenWidth, int screenHeight)
    {
        _minX = minX;
        _minY = minY;

        // 1. Calculate the size of your "World" (Figma space)
        float worldWidth = maxX - minX;
        float worldHeight = maxY - minY;

        // 2. Calculate scale to fit the screen
        // Subtract padding from both sides (left/right and top/bottom)
        float scaleX = (screenWidth - (_padding * 2)) / worldWidth;
        float scaleY = (screenHeight - (_padding * 2)) / worldHeight;

        // Use the smaller scale so the map isn't stretched or cut off
        _scale = Math.Min(scaleX, scaleY);
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public Vector2 TransformPoint(Vector2 coords)
    {
        float screenX = (coords.X - _minX) * _scale + _padding + (_screenWidth / 4);
        float screenY = (coords.Y - _minY) * _scale + _padding;

        return new Vector2(screenX, screenY);
    }

}
