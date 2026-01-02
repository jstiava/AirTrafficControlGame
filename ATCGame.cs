using CsvHelper.Configuration.Attributes;
using ATCGame.Core;
using ATCGame.Entities;
using ATCGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;
using CsvHelper;
using System.Globalization;

namespace ATCGame;

public class FlightPlanCsvRow
{
    public string Type { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";

    public string Callsign { get; set; } = "";

    public string Number { get; set; } = "";
    public string Aircraft { get; set; } = "";
    public string Company { get; set; } = "";

    public DateTime Scheduled { get; set; }

}

public class AirTrafficControlGame : Game
{
    private DateTime TimeStamp;
    private double _elapsedSeconds = 0;

    private GraphicsDeviceManager _graphics;
    private TextBox _commandBox;
    private SpriteBatch _spriteBatch;
    public Airport Airport;
    private Artist artist;
    public ViewportTransformer ViewportTransformer;
    bool isDataLoaded = false;
    public RenderTarget2D _baseLayer = null;
    public bool _isBaseLayerDirty = true;
    public int _baseLayerVersion = 0;
    private SpriteFont _debugFont;
    
    public Aircraft? SelectedAircraft;
    public Texture2D _arrivalImage;
    public Texture2D _departureImage;
    public int _airplaneSize;
    public Texture2D _pixel;


    public AirTrafficControlGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private void RedrawBaseLayer()
    {
        var nodes = this.Airport.Airfield.Junctions;
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
        foreach (var edge in this.Airport.Airfield.Roads)
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

        foreach (var obstr in this.Airport.Airfield.Obstructions)
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
        foreach (var node in this.Airport.Airfield.Junctions)
        {
            node.ScreenPosition = this.ViewportTransformer.TransformPoint(node.Position);
        }

        foreach (var gate in this.Airport.Airfield.Gates)
        {
            gate.Value.ScreenPosition = this.ViewportTransformer.TransformPoint(gate.Value.Position);
        }

        foreach (var plane in Airport.Radar.Objects)
        {
            if (plane.Value.Position != null)
            {
                plane.Value.Reposition(this.ViewportTransformer.TransformPoint(plane.Value.Position));
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

    public async Task<List<FlightPlanCsvRow>> ReadInAirportScheduleAsync(string filepath)
    {
        string text = await File.ReadAllTextAsync("../../../" + filepath);
        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<FlightPlanCsvRow>().ToList();
        return records;
    }

    public async Task<Airport> ReadAirportsAsync(string filepath, GraphReader graphReader)
    {
        string jsonContent = await File.ReadAllTextAsync("../../../" + filepath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var root = JsonSerializer.Deserialize<JsonElement>(jsonContent, options);

        if (root.TryGetProperty("airports", out JsonElement airports))
        {
            if (airports.TryGetProperty("MDW", out JsonElement mdw))
            {
                Airport theAirport;
                HashSet<string> airlinesWhoOwnGates = new HashSet<string>();

                if (mdw.TryGetProperty("radio_frequencies", out JsonElement radio_freqs))
                {
                    Dictionary<string, double> freqMap = new Dictionary<string, double>();

                    foreach (JsonProperty prop in radio_freqs.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Number)
                        {
                            freqMap[prop.Name] = prop.Value.GetDouble();
                        }
                    }

                    Console.WriteLine($"Found {freqMap.Count} frequencies for MDW.");
                    theAirport = new Airport(
                        mdw.GetProperty("nickname").ToString(), 
                        freqMap, 
                        graphReader,
                        _arrivalImage,
                        _departureImage
                    );
                }
                else
                {
                    throw new KeyNotFoundException("Could not find 'radio_frequencies' inside the MDW object.");
                }

                if (mdw.TryGetProperty("gates", out JsonElement gates))
                {
                    if (gates.TryGetProperty("data", out JsonElement gatesData))
                    {
                        foreach (Gate gate in graphReader._gates)
                        {
                            if (gatesData.TryGetProperty(gate.Id, out JsonElement theGate))
                            {
                                string? taxiway = null;
                                string? owner = null;
                                int? order = null;

                                if (theGate.TryGetProperty("pushbackToTaxiway", out JsonElement taxiwayElement))
                                {
                                    taxiway = taxiwayElement.GetString();
                                }

                                if (theGate.TryGetProperty("owner", out JsonElement ownerElement))
                                {
                                    owner = ownerElement.GetString();
                                    airlinesWhoOwnGates.Add(owner);
                                }

                                if (theGate.TryGetProperty("order_sw-flow", out JsonElement orderElement))
                                {
                                    order = orderElement.GetInt32();
                                }

                                theAirport.Airfield.RegisterGate(gate.Id, gate, taxiway, owner, order);
                            }
                            else
                            {
                                throw new KeyNotFoundException("Could not find the gate: " + gate.Id);
                            }
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException("Could not find gates data.");
                    }
                }
                else
                {
                    throw new KeyNotFoundException("Could not find gates.");
                }

                if (root.TryGetProperty("airlines", out JsonElement airlines))
                {

                    foreach (var airline in airlinesWhoOwnGates)
                    {
                        if (airline != null)
                        {
                            if (airlines.TryGetProperty(airline, out JsonElement theAirline))
                            {
                                string name = airline;
                                string? fullname = null;
                                string? ICAO = null;
                                string? callsign = null;

                                if (theAirline.TryGetProperty("fullname", out JsonElement fullnameElement))
                                {
                                    fullname = fullnameElement.GetString();
                                }

                                if (theAirline.TryGetProperty("ICAO", out JsonElement ICAOElement))
                                {
                                    ICAO = ICAOElement.GetString();
                                }

                                if (theAirline.TryGetProperty("callsign", out JsonElement callsignElement))
                                {
                                    callsign = callsignElement.GetString();
                                }

                                if (fullname == null | ICAO == null | callsign == null)
                                {
                                    continue;
                                }

                                theAirport.RegisterAirline(new Airline(name, fullname, ICAO, callsign));
                            }
                        }
                    }
                }

                    return theAirport;
            }
        }


        return null;

    }

    protected override async void Initialize()
    {
        TimeStamp = new DateTime(2025, 12, 31, 7, 0, 0);

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowResize;
        base.Initialize();

        this._commandBox = new TextBox(
            new Rectangle(20, 20, 400, 40)
        );
        this.artist = new Artist(GraphicsDevice);
        GraphReader graphReader = new GraphReader();
        Viewport viewport = GraphicsDevice.Viewport;
        await graphReader.ReadAsync("graph.json");
        this.Airport = await ReadAirportsAsync("Data/config.json", graphReader);
        List<FlightPlanCsvRow> schedule = await ReadInAirportScheduleAsync("Data/MDW_20251230.csv");
        this.Airport.AddSchedule(schedule);

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

        _isBaseLayerDirty = true;
        isDataLoaded = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("ScoreFont");
        _arrivalImage = Content.Load<Texture2D>("arrival");
        _departureImage = Content.Load<Texture2D>("departure");
        _baseLayer = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height
        );
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {

        _elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        if (_elapsedSeconds >= 1)
        {
            TimeStamp = TimeStamp.AddSeconds(1);
            _elapsedSeconds -= 1;
        }

        MouseState mouse = Mouse.GetState();
        KeyboardState currentKey = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!isDataLoaded) return;

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Point mousePos = mouse.Position;
            _commandBox.HasFocus = _commandBox.Bounds.Contains(mousePos);

            if (this.SelectedAircraft != null)
            {
                if (!this.SelectedAircraft.Bounds.Contains(mousePos))
                {
                    this.SelectedAircraft = null;
                }
            }
            foreach (var plane in Airport.Radar.Objects)
            {
                if (plane.Value.Bounds.Contains(mousePos))
                {
                    this.SelectedAircraft = plane.Value;
                    break;
                }
            }
        }

        Window.Title = $"Chicago ATC";
        _commandBox.Update(currentKey, gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (!isDataLoaded) return;

        
        if (this._isBaseLayerDirty)
        {

            float percentOfWidth = 0.01f;
            _airplaneSize = (int)(Window.ClientBounds.Width * percentOfWidth);

            if (_baseLayer == null || _baseLayer.IsDisposed)
            {
                _baseLayer = new RenderTarget2D(GraphicsDevice,
                    Window.ClientBounds.Width, Window.ClientBounds.Height);
            }
            GraphicsDevice.SetRenderTarget(_baseLayer);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            foreach (var edge in this.Airport.Airfield.Roads)
            {
                Console.WriteLine($"Edge ID: {edge.Id}");
                artist.DrawLine(_spriteBatch, edge.ScreenPoints, edge.Is_Runway ? Color.White : Color.SlateGray, edge.Is_Runway ? 5 : 2);
            }

            foreach (var gate in this.Airport.Airfield.Gates)
            {
                artist.DrawCircle(_spriteBatch, gate.Value.ScreenPosition, 3, gate.Value.Owner == "general" ? Color.White : Color.LightGray, gate.Value.Owner != "general", 64);
            }

            foreach (var gate in this.Airport.Airfield.Obstructions)
            {
                artist.FillPolygonConcave(_spriteBatch, gate.ScreenPoints, new Color(61, 61, 61));
            }

            foreach (var node in this.Airport.Airfield.Junctions)
            {
                artist.DrawCircle(_spriteBatch, node.ScreenPosition, 5, Color.Red, false);
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

        _commandBox.Draw(_spriteBatch, artist, _debugFont, _pixel);

        Vector2 textSize = _debugFont.MeasureString(_baseLayerVersion.ToString());
        float x = GraphicsDevice.Viewport.Width - textSize.X - 20;
        float y = 20;
        _spriteBatch.DrawString(_debugFont, _baseLayerVersion.ToString(), new Vector2(x, y), Color.White);

        if (this.SelectedAircraft != null)
        {
            _spriteBatch.DrawString(
                _debugFont, 
                this.SelectedAircraft.Callsign, 
                new Vector2(20, 90), 
                Color.White
            );
        }
        else
        {
            _spriteBatch.DrawString(
                _debugFont,
                "No object selected.",
                new Vector2(20, 90),
                Color.White
            );
        }

        string timeText = TimeStamp.ToString("hh:mm:ss tt");
        _spriteBatch.DrawString(
            _debugFont,
            timeText,
            new Vector2(20, 65),
            Color.White
        );

        foreach (var plane in Airport.Radar.Objects)
        {
            plane.Value.Draw(_spriteBatch, artist, _airplaneSize);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

