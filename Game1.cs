using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture;

    // 1 = Parede, 0 = Caminho livre
    private int[,] _map =
    {
        { 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 1, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1 },
    };

    private Vector2 _playerPos = new Vector2(1.5f, 1.5f); // Centro da célula 1,1 (caminho livre)
    private float _playerAngle = 0f;

    private float _fov = (float)Math.PI / 3.0f; // Campo de visão de 60 graus
    private float _depth = 16.0f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Resolução clássica/simples em tela
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Criando textura em tempo de execução de 1x1 branco
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
            Exit();

        KeyboardState k = Keyboard.GetState();

        // --- AJUSTE: O uso de Delta Time torna os movimentos fluídos independente dos FPS! ---
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float moveSpeed = 3.0f * dt;
        float rotSpeed = 2.0f * dt;

        float dirX = (float)Math.Cos(_playerAngle);
        float dirY = (float)Math.Sin(_playerAngle);

        // --- AJUSTE: Sliding Collision ---
        // Checando X e Y separadamente permite que o jogador "deslize" na parede se bater em diagonal
        if (k.IsKeyDown(Keys.W))
        {
            if (_map[(int)_playerPos.Y, (int)(_playerPos.X + dirX * moveSpeed)] == 0)
                _playerPos.X += dirX * moveSpeed;
            if (_map[(int)(_playerPos.Y + dirY * moveSpeed), (int)_playerPos.X] == 0)
                _playerPos.Y += dirY * moveSpeed;
        }

        if (k.IsKeyDown(Keys.S))
        {
            if (_map[(int)_playerPos.Y, (int)(_playerPos.X - dirX * moveSpeed)] == 0)
                _playerPos.X -= dirX * moveSpeed;
            if (_map[(int)(_playerPos.Y - dirY * moveSpeed), (int)_playerPos.X] == 0)
                _playerPos.Y -= dirY * moveSpeed;
        }

        if (k.IsKeyDown(Keys.A))
            _playerAngle -= rotSpeed;

        if (k.IsKeyDown(Keys.D))
            _playerAngle += rotSpeed;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // 1. TETO e CHÃO
        // Desenha metades da tela ao invés de calcular linha a linha para economizar processamento
        _spriteBatch.Draw(
            _pixelTexture,
            new Rectangle(0, 0, screenWidth, screenHeight / 2),
            new Color(40, 40, 40)
        );
        _spriteBatch.Draw(
            _pixelTexture,
            new Rectangle(0, screenHeight / 2, screenWidth, screenHeight / 2),
            new Color(90, 50, 30)
        );

        // 2. RAYCASTING
        for (int x = 0; x < screenWidth; x++)
        {
            float rayAngle = (_playerAngle - _fov / 2f) + (x / (float)screenWidth) * _fov;

            float distanceToWall = 0;
            bool hitWall = false;

            float eyeX = (float)Math.Cos(rayAngle);
            float eyeY = (float)Math.Sin(rayAngle);

            while (!hitWall && distanceToWall < _depth)
            {
                distanceToWall += 0.05f; // Decreases step size for better precision

                int testX = (int)(_playerPos.X + eyeX * distanceToWall);
                int testY = (int)(_playerPos.Y + eyeY * distanceToWall);

                // Garante que o raio não ultrapasse os limites do próprio mapa
                if (
                    testX < 0
                    || testX >= _map.GetLength(1)
                    || testY < 0
                    || testY >= _map.GetLength(0)
                )
                {
                    hitWall = true;
                    distanceToWall = _depth;
                }
                else if (_map[testY, testX] == 1)
                {
                    hitWall = true;
                }
            }

            // --- AJUSTE VITAL: Corrige o distorcimento olho-de-peixe (fisheye)! ---
            distanceToWall *= (float)Math.Cos(rayAngle - _playerAngle);

            // Prevenção de divisão por zero ou muito perto da parede
            if (distanceToWall <= 0.01f)
                distanceToWall = 0.01f;

            int ceiling = (int)((screenHeight / 2.0f) - screenHeight / distanceToWall);
            int floor = screenHeight - ceiling;
            int wallHeight = Math.Max(0, floor - ceiling);

            // --- AJUSTE ESTÉTICO: Efeito de sombreamento por distância ---
            float shade = 1.0f - Math.Min(distanceToWall / _depth, 1.0f);
            Color wallColor = new Color((int)(160 * shade), (int)(160 * shade), (int)(160 * shade));

            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, ceiling, 1, wallHeight), wallColor);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
