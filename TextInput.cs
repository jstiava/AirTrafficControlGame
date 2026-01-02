using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace ATCGame;

public class TextBox
{
    public Rectangle Bounds;
    public string Text = "";
    public bool HasFocus;

    private double _cursorTimer;
    private bool _showCursor = true;

    private KeyboardState _previousKeyboardState;
    private double _backspaceTimer = 0;
    private const double BackspaceInitialDelay = 0.5; // seconds before repeat starts
    private const double BackspaceRepeatRate = 0.2;   // seconds per repeated delete
    private bool _backspaceHeld = false;


    public TextBox(Rectangle bounds)
    {
        Bounds = bounds;
    }


    public void Draw(SpriteBatch sb, Artist artist, SpriteFont font, Texture2D pixel)
    {

        sb.Draw(pixel, Bounds, new Color(50, 50, 50));

        var displayText = Text + (HasFocus && _showCursor ? "|" : "");
        sb.DrawString(font, displayText, new Vector2(Bounds.X + 5, Bounds.Y + 5), Color.White);
    }

    public void Update(KeyboardState state, GameTime gameTime)
    {
        double elapsed = gameTime.ElapsedGameTime.TotalSeconds;

        if (state.IsKeyDown(Keys.Back))
        {
            if (!_backspaceHeld)
            {
                // First press
                if (Text.Length > 0) Text = Text[..^1];
                _backspaceHeld = true;
                _backspaceTimer = 0;
            }
            else
            {

                _backspaceTimer += elapsed;

                if (_backspaceTimer >= BackspaceInitialDelay)
                {
                    // Delete character every repeat interval
                    while (_backspaceTimer >= BackspaceInitialDelay + BackspaceRepeatRate)
                        _backspaceTimer -= BackspaceRepeatRate;

                    if (Text.Length > 0)
                        Text = Text[..^1];
                }
            }
        }
        else
        {
            // Backspace released
            _backspaceHeld = false;
            _backspaceTimer = 0;
        }


        foreach (var key in state.GetPressedKeys())
        {
            if (this._previousKeyboardState.IsKeyUp(key))
            {
                char c = this.KeyToChar(key, state);
                if (c != '\0')
                {
                    this.Text += c;
                }
            }
        }

        this._previousKeyboardState = state;
    }

    private char KeyToChar(Keys key, KeyboardState state)
    {
        bool shift = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);

        // Letters
        if (key >= Keys.A && key <= Keys.Z)
            return (char)((shift ? 'A' : 'a') + (key - Keys.A));

        // Numbers
        if (key >= Keys.D0 && key <= Keys.D9)
            return shift
                ? ")!@#$%^&*("[key - Keys.D0]
                : (char)('0' + (key - Keys.D0));

        // Numpad
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            return (char)('0' + (key - Keys.NumPad0));

        // Special keys
        return key switch
        {
            Keys.Space => ' ',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            _ => '\0'
        };
    }

}
