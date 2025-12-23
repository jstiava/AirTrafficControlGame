using ATCGame.Core;
using ATCGame.Utilities;
using ATCGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using MonoGame.Extended;
using System.Xml.Linq;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;

namespace ATCGame;

public class AirTrafficControlGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Artist artist;
    public ViewportTransformer ViewportTransformer;
    public GraphReader graphReader;
    bool isDataLoaded = false;
    public RenderTarget2D _baseLayer = null;
    public bool _isBaseLayerDirty = true;
    public int _baseLayerVersion = 0;
    private SpriteFont _debugFont;
    public Texture2D _planeImage;
    public List<Aircraft> _radarList;
    public int _airplaneSize;

    public AirTrafficControlGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private void RedrawBaseLayer()
    {
        var nodes = graphReader.GetNodes();
        if (nodes.Count > 0)
        {
            this.ViewportTransformer = new ViewportTransformer(
                minX: nodes.Min(n => n.X),
                maxX: nodes.Max(n => n.X),
                minY: nodes.Min(n => n.Y),
                maxY: nodes.Max(n => n.Y),
                screenWidth: Window.ClientBounds.Width,
                screenHeight: Window.ClientBounds.Height
            );
        }

        TransformEdges();
        TransformNodes();

        if (this._baseLayer != null)
        {
            _baseLayer.Dispose();
        }
        this._isBaseLayerDirty = true;
    }


    private void TransformEdges()
    {
        foreach (var edge in graphReader.GetEdges())
        {
            if (edge.Points != null)
            {
                edge.ScreenPoints.Clear();
                for (int i = 0; i < edge.Points.Count; i++)
                {
                    edge.ScreenPoints.Add(this.ViewportTransformer.TransformPoint(edge.Points[i]));
                }
            }
        }

        foreach (var obstr in graphReader.GetObstructions())
        {
            if (obstr.Points != null)
            {
                obstr.ScreenPoints.Clear();
                for (int i = 0; i < obstr.Points.Count; i++)
                {
                    obstr.ScreenPoints.Add(this.ViewportTransformer.TransformPoint(obstr.Points[i]));
                }
            }
        }
    }

    private void TransformNodes()
    {
        foreach (var node in graphReader.GetNodes())
        {
            node.ScreenPosition = this.ViewportTransformer.TransformPoint(node.Position);
        }

        foreach (var gate in graphReader.GetGates())
        {
            gate.ScreenPosition = this.ViewportTransformer.TransformPoint(gate.Position);
        }

        foreach (var plane in _radarList)
        {
            if (plane.Position != null)
            {
                plane.ScreenPosition = this.ViewportTransformer.TransformPoint(plane.Position);
            }
        }
    }

    private void OnWindowResize(object sender, EventArgs e)
    {
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();
        RedrawBaseLayer();
    }

    protected override async void Initialize()
    {
        // TODO: Add your initialization logic here

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowResize;
        base.Initialize();

        this._radarList = new List<Aircraft>();
        this.artist = new Artist(GraphicsDevice);
        this.graphReader = new GraphReader();
        Viewport viewport = GraphicsDevice.Viewport;
        await graphReader.ReadAsync("graph.json");

        var nodes = graphReader.GetNodes();
        if (nodes.Count > 0)
        {
            this.ViewportTransformer = new ViewportTransformer(
                minX: nodes.Min(n => n.X),
                maxX: nodes.Max(n => n.X),
                minY: nodes.Min(n => n.Y),
                maxY: nodes.Max(n => n.Y),
                screenWidth: Window.ClientBounds.Width,
                screenHeight: Window.ClientBounds.Height
            );
        }

        foreach (var gate in graphReader.GetGates())
        {
            var newPlane = new Aircraft(_planeImage, gate.Position);
            newPlane.Rotate((int)gate.At_Gate_Heading + 45);
            this._radarList.Add(newPlane);
        }

        TransformEdges();
        TransformNodes();


        _isBaseLayerDirty = true;
        isDataLoaded = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("ScoreFont");
        _planeImage = Content.Load<Texture2D>("airplane");

        _baseLayer = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height
        );

        

    }

    protected override void Update(GameTime gameTime)
    {

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!isDataLoaded) return;

        int nodeCount = this.graphReader.GetNodes().Count;
        Window.Title = $"Chicago ATC - Nodes Loaded: {nodeCount}";

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (!isDataLoaded) return;

        if (this._isBaseLayerDirty)
        {

            float percentOfWidth = 0.0125f;
            _airplaneSize = (int)(Window.ClientBounds.Width * percentOfWidth);

            if (_baseLayer == null || _baseLayer.IsDisposed)
            {
                _baseLayer = new RenderTarget2D(GraphicsDevice,
                    Window.ClientBounds.Width, Window.ClientBounds.Height);
            }
            GraphicsDevice.SetRenderTarget(_baseLayer);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            foreach (var edge in graphReader.GetEdges())
            {
                Console.WriteLine($"Edge ID: {edge.Id}");
                artist.DrawLine(_spriteBatch, edge.ScreenPoints, edge.Is_Runway ? Color.White : Color.SlateGray, edge.Is_Runway ? 5 : 2);
            }

            foreach (var gate in graphReader.GetGates())
            {
                artist.DrawCircle(_spriteBatch, gate.ScreenPosition, 2, Color.LightGray);
            }

            foreach (var gate in graphReader.GetObstructions())
            {
                artist.FillPolygonConcave(_spriteBatch, gate.ScreenPoints, new Color(61, 61, 61));
            }

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            ++_baseLayerVersion;
            this._isBaseLayerDirty = false;
        }

        _spriteBatch.Begin();

        if (_baseLayer != null)
        {

            _spriteBatch.Draw(_baseLayer, Vector2.Zero, Color.White);
        }

        Vector2 textSize = _debugFont.MeasureString(_baseLayerVersion.ToString());
        float x = GraphicsDevice.Viewport.Width - textSize.X - 20;
        float y = 20;
        _spriteBatch.DrawString(_debugFont, _baseLayerVersion.ToString(), new Vector2(x, y), Color.White);


        foreach (var plane in _radarList)
        {
            plane.Draw(_spriteBatch, artist, _airplaneSize);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

