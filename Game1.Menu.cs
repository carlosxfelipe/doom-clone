using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public partial class Game1
{
    private void UpdateMenu(KeyboardState k)
    {
        if (k.IsKeyDown(Keys.Up) && !_lastK.IsKeyDown(Keys.Up))
            _selectedMenuOption = (_selectedMenuOption - 1 + 3) % 3;
        if (k.IsKeyDown(Keys.Down) && !_lastK.IsKeyDown(Keys.Down))
            _selectedMenuOption = (_selectedMenuOption + 1) % 3;

        if (k.IsKeyDown(Keys.Enter) && !_lastK.IsKeyDown(Keys.Enter))
        {
            if (_selectedMenuOption == 0)
                _currentState = GameState.Playing;
            else if (_selectedMenuOption == 1)
                _currentState = GameState.Credits;
            else if (_selectedMenuOption == 2)
                Exit();
        }
    }

    private void UpdateCredits(KeyboardState k)
    {
        if (k.IsKeyDown(Keys.Enter) && !_lastK.IsKeyDown(Keys.Enter))
            _currentState = GameState.Menu;
    }

    private void DrawMenu(int screenWidth, int screenHeight)
    {
        // SamplerState.PointClamp para texto nítido
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            null,
            SamplerState.PointClamp,
            null,
            null,
            null,
            Matrix.CreateScale(
                (float)GraphicsDevice.Viewport.Width / screenWidth,
                (float)GraphicsDevice.Viewport.Height / screenHeight,
                1.0f
            )
        );

        // Fundo (Skybox escurecido)
        _spriteBatch.Draw(
            _skyTexture,
            new Rectangle(0, 0, screenWidth, screenHeight),
            new Color(40, 40, 40)
        );

        // Título
        DrawPixelText("DOOM CLONE", screenWidth / 2 - 200, 150, 10, Color.Red);

        // Options
        Color cStartColor = _selectedMenuOption == 0 ? Color.Yellow : Color.White;
        DrawPixelText("START", screenWidth / 2 - 50, 450, 5, cStartColor);

        Color cCreditsColor = _selectedMenuOption == 1 ? Color.Yellow : Color.White;
        DrawPixelText("CREDITS", screenWidth / 2 - 70, 520, 5, cCreditsColor);

        Color cExitColor = _selectedMenuOption == 2 ? Color.Yellow : Color.White;
        DrawPixelText("EXIT", screenWidth / 2 - 40, 590, 5, cExitColor);

        // Dica
        DrawPixelText("USE ARROWS AND ENTER", screenWidth / 2 - 120, 850, 3, Color.Gray);

        _spriteBatch.End();
    }

    private void DrawCredits(int screenWidth, int screenHeight)
    {
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            null,
            SamplerState.PointClamp,
            null,
            null,
            null,
            Matrix.CreateScale(
                (float)GraphicsDevice.Viewport.Width / screenWidth,
                (float)GraphicsDevice.Viewport.Height / screenHeight,
                1.0f
            )
        );

        _spriteBatch.Draw(_skyTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

        DrawPixelText("CREDITS", screenWidth / 2 - 112, 100, 8, Color.Yellow);

        DrawPixelText("DEVELOPED BY", screenWidth / 2 - 96, 300, 4, Color.White);
        DrawPixelText("CARLOS FELIPE", screenWidth / 2 - 130, 360, 5, Color.Red);

        DrawPixelText("INSPIRED BY DOOM", screenWidth / 2 - 96, 550, 3, Color.Gray);
        DrawPixelText("1993 ID SOFTWARE", screenWidth / 2 - 100, 590, 3, Color.Gray);

        DrawPixelText("PRESS ENTER TO RETURN", screenWidth / 2 - 168, 850, 4, Color.White);

        _spriteBatch.End();
    }
}
