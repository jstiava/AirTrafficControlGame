using ATCGame.Core;
using ATCGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using MonoGame.Extended;
using System.Xml.Linq;

namespace ATCGame;

public class AirTrafficControlGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Artist artist;
    public ViewportTransformer ViewportTransformer;
    public GraphReader graphReader;
    bool isDataLoaded = false;

public AirTrafficControlGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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
    }

    private void TransformNodes()
    {
        foreach (var node in graphReader.GetNodes())
        {
            node.ScreenPosition = this.ViewportTransformer.TransformPoint(node.Position);
        }
    }

    private void OnWindowResize(object sender, EventArgs e)
    {
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();
        OnWindowResize();
    }

    private void OnWindowResize()
    {
        // 1. Update the GraphicsDevice to the new window size
       

        // 2. Re-calculate the Transformer
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

    }

    protected override async void Initialize()
    {
        // TODO: Add your initialization logic here

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowResize;
        base.Initialize();

        this.artist = new Artist(GraphicsDevice);
        this.graphReader = new GraphReader();
        Viewport viewport = GraphicsDevice.Viewport;
        await graphReader.ReadAsync("graph.json");

        OnWindowResize();
        isDataLoaded = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

    }

    protected override void Update(GameTime gameTime)
    {

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!isDataLoaded) return;

        // TODO: Add your update logic here

        int nodeCount = this.graphReader.GetNodes().Count;
        Window.Title = $"Chicago ATC - Nodes Loaded: {nodeCount}";

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (!isDataLoaded) return;

        _spriteBatch.Begin();

        foreach (var edge in graphReader.GetEdges())
        {
            Console.WriteLine($"Edge ID: {edge.Id}");

            artist.DrawLine(_spriteBatch, edge.ScreenPoints, edge.Is_Runway ? Color.White : Color.SlateGray, edge.Is_Runway ? 5 : 1);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
