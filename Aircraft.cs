using ATCGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace ATCGame.Entities;

public enum AircraftDiagnostics
{
    DISABLE,
    CLICKABLE_BOX,
    WHEEL_BASE,
    VECTORING
}

public class BlackBoxEvent
{

    public DateTime DateTime;
    public string Title;
    public string Description;
    public BlackBoxEvent(DateTime dateTime, string title, string description)
    {
        this.DateTime = dateTime;
        this.Title = title; 
        this.Description = description;

    }
}

public class Aircraft
{
    public string Callsign;
    public string AircraftName;
    public PlaneState State;

    public Texture2D _texture;
    public Rectangle Bounds {  get; set; }
    public float Speed;
    public float WheelBase;
    public float NoseAngle;

    public Vector2 Position;
    public Vector2 ScreenPosition;
    public float Heading;

    public NavigationInstruction ActiveInstruction = null;
    public Queue<NavigationInstruction> InstructionsQueue;
    public NavigationInstruction[] InstructionsHistory;

    public List<BlackBoxEvent> BlackBox;

    public Aircraft(string callsign, Texture2D texture, Vector2 position, int heading = 45)
    {
        this.Callsign = callsign;
        this.State = PlaneState.COLD_AND_DARK;
        this._texture = texture;
        this.Position = position;
        this.Reposition(position, heading);
        this.BlackBox = new List<BlackBoxEvent>();
    }

    public bool StartEngine(DateTime dateTime)
    {
        if (this.State == PlaneState.COLD_AND_DARK)
        {
            this.State = PlaneState.PARKED;
            this.BlackBox.Add(new BlackBoxEvent(dateTime, "Engine start", "Move to parked."));
            return true ;
        }
        return false;
    }

    public void Update(float dt)
    {
        float turnRate = (Speed / WheelBase) * MathF.Tan(NoseAngle);
        Heading += turnRate * dt;
        Position.X += Speed * MathF.Cos(Heading) * dt;
        Position.Y += Speed * MathF.Sin(Heading) * dt;
    }

    public void Reposition(Vector2 position)
    {
        int SIZE_OF_CLICK_TARGET = 30;
        this.ScreenPosition = position;
        this.Bounds = new Rectangle((int)position.X - (SIZE_OF_CLICK_TARGET / 2), (int)position.Y - (SIZE_OF_CLICK_TARGET / 2), SIZE_OF_CLICK_TARGET, SIZE_OF_CLICK_TARGET);
    }

    public void Reposition(Vector2 position, int heading)
    {
        int SIZE_OF_CLICK_TARGET = 30;
        this.ScreenPosition = position;
        this.Rotate(heading);
        this.Bounds = new Rectangle((int)position.X-(SIZE_OF_CLICK_TARGET/2), (int)position.Y- (SIZE_OF_CLICK_TARGET / 2), SIZE_OF_CLICK_TARGET, SIZE_OF_CLICK_TARGET);
    }

    public void Rotate(int? heading)
    {
        if (heading == null)
        {
            return;
        }
        this.Heading = (((int)heading) - 45f) % 360f;
    }

    public void Move()
    {
        if (ActiveInstruction == null)
        {
            if (this.InstructionsQueue.Count > 0) return;
            this.ActiveInstruction = this.InstructionsQueue.Dequeue();
        }

        this.ActiveInstruction.Step(this);
    }

    public bool Load(NavigationInstruction target)
    {
        this.InstructionsQueue.Enqueue(target);
        target.Readback();
        return false;
    }

    public bool Load(List<NavigationInstruction> targets)
    {
        return false;
    }

    public void Draw(SpriteBatch spriteBatch, Artist artist, int size)
    {
        AircraftDiagnostics DEBUG_FLAG = AircraftDiagnostics.CLICKABLE_BOX;

        Rectangle destinationRect = new Rectangle((int)(ScreenPosition.X), (int)(ScreenPosition.Y), size, size);
        float rotation = MathHelper.ToRadians(this.Heading);

        if (DEBUG_FLAG == AircraftDiagnostics.CLICKABLE_BOX)
        {
            var points = new List<Vector2>();
            foreach(var side in Bounds.GetCorners())
            {
                points.Add(side.ToVector2());
            }
            points.Add(Bounds.GetCorners()[0].ToVector2());
            artist.DrawLine(spriteBatch, points, Color.Red, 2);
        }
        else if (DEBUG_FLAG == AircraftDiagnostics.WHEEL_BASE)
        {
            artist.DrawCircle(spriteBatch, this.ScreenPosition, 3, Color.Red, true, 8);
        }
        else if (DEBUG_FLAG == AircraftDiagnostics.VECTORING)
        {
            float rotationAgain = MathHelper.ToRadians(this.Heading - 45f);
            Vector2 forwardDirection = new Vector2(
                (float)Math.Cos(rotationAgain),
                (float)Math.Sin(rotationAgain)
            );
            Vector2 endPoint = this.ScreenPosition + (forwardDirection * 100);
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
