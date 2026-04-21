using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture;
    private Texture2D _wallTexture;
    private Texture2D _floorTexture;
    private Texture2D _weaponTexture;
    private Texture2D _flashTexture;
    private Texture2D _skyTexture;
    private Color[] _floorTextureData;
    private Texture2D _screenTexture;
    private Color[] _screenBuffer;
    private Random _rng = new Random();
    private int _playerHealth = 100;
    private int _maxHealth = 100;
    private float _damageTimer = 0f;

    private float _recoilTimer = 0f;
    private float _bobTimer = 0f;
    private float _lastBob = 0f;

    private SoundEffect _sfxShotgun;
    private SoundEffect _sfxFootstep;
    private SoundEffect _sfxDeath;

    private Texture2D _demonTexture;

    private class Monster
    {
        public Vector2 Position;
        public bool Alive = true;
        public Texture2D Sprite;
    }

    private List<Monster> _monsters = new List<Monster>();
    private float[] _depthBuffer;

    // 1 = Parede, 0 = Caminho livre
    private int[,] _map =
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };

    private Vector2 _playerPos = new Vector2(2.5f, 2.5f); // Posição inicial no mapa
    private float _playerAngle = 0f;

    private float _fov = (float)Math.PI / 3.0f; // FOV (60 graus)
    private float _depth = 16.0f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        IsMouseVisible = false;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 960;
        _graphics.IsFullScreen = false;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Textura básica 1x1 branca
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Carrega texturas do diretório Content
        _wallTexture = Texture2D.FromFile(GraphicsDevice, "Content/wall_texture.png");
        _floorTexture = Texture2D.FromFile(GraphicsDevice, "Content/floor_texture.png");
        _floorTextureData = new Color[_floorTexture.Width * _floorTexture.Height];
        _floorTexture.GetData(_floorTextureData);
        _skyTexture = Texture2D.FromFile(GraphicsDevice, "Content/sky_texture.png");

        _weaponTexture = Texture2D.FromFile(GraphicsDevice, "Content/weapon.png");
        Color[] wpnData = new Color[_weaponTexture.Width * _weaponTexture.Height];
        _weaponTexture.GetData(wpnData);
        for (int i = 0; i < wpnData.Length; i++)
        {
            int r = wpnData[i].R;
            int g = wpnData[i].G;
            int b = wpnData[i].B;

            // Chroma Key: Remove fundo magenta/roxo
            if (r > g + 30 && b > g + 30 && r > 40 && b > 40)
            {
                wpnData[i] = Color.Transparent;
            }
        }
        _weaponTexture.SetData(wpnData);

        _flashTexture = Texture2D.FromFile(GraphicsDevice, "Content/flash.png");
        Color[] fData = new Color[_flashTexture.Width * _flashTexture.Height];
        _flashTexture.GetData(fData);
        for (int i = 0; i < fData.Length; i++)
        {
            int r = fData[i].R;
            int g = fData[i].G;
            int b = fData[i].B;
            if (r > g + 30 && b > g + 30 && r > 40 && b > 40)
            {
                fData[i] = Color.Transparent;
            }
        }
        _flashTexture.SetData(fData);

        _demonTexture = Texture2D.FromFile(GraphicsDevice, "Content/demon.png");
        Color[] demonData = new Color[_demonTexture.Width * _demonTexture.Height];
        _demonTexture.GetData(demonData);
        for (int i = 0; i < demonData.Length; i++)
        {
            int r = demonData[i].R;
            int g = demonData[i].G;
            int b = demonData[i].B;
            if (r > g + 40 && b > g + 40) // Chroma Key para transparência
                demonData[i] = Color.Transparent;
        }
        _demonTexture.SetData(demonData);

        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;
        _depthBuffer = new float[screenWidth];

        // Buffer de tela para desenhar o chão pixel-a-pixel bem rápido
        _screenTexture = new Texture2D(
            GraphicsDevice,
            screenWidth,
            _graphics.PreferredBackBufferHeight
        );
        _screenBuffer = new Color[
            _graphics.PreferredBackBufferWidth * _graphics.PreferredBackBufferHeight
        ];

        // Carregando os sons gerados pelo script Python
        _sfxShotgun = SoundEffect.FromFile("Content/shotgun.wav");
        _sfxFootstep = SoundEffect.FromFile("Content/footstep.wav");
        _sfxDeath = SoundEffect.FromFile("Content/death.wav");
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
            Exit();

        KeyboardState k = Keyboard.GetState();

        // Delta Time para movimentos independentes de framerate
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float moveSpeed = 3.0f * dt;
        float rotSpeed = 2.0f * dt;

        float dirX = (float)Math.Cos(_playerAngle);
        float dirY = (float)Math.Sin(_playerAngle);

        // Colisão deslizante (Sliding Collision)
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

        if (k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S))
        {
            _bobTimer += dt * 12f;
            // Toca som de passo a cada ciclo da animação
            if (Math.Sin(_bobTimer) < 0 && Math.Sin(_lastBob) >= 0)
                _sfxFootstep.Play(0.2f, 0f, 0f);
        }
        else
            _bobTimer = 0f;

        _lastBob = _bobTimer;

        // Sistema de Disparo e Recuo
        if (_recoilTimer > 0)
            _recoilTimer -= dt;

        MouseState m = Mouse.GetState();
        bool isShooting = k.IsKeyDown(Keys.Space) || m.LeftButton == ButtonState.Pressed;

        if (isShooting && _recoilTimer <= 0)
        {
            _recoilTimer = 0.5f;
            _sfxShotgun.Play(); // Som do tiro!

            foreach (var monster in _monsters)
            {
                if (!monster.Alive)
                    continue;

                Vector2 toEnemy = monster.Position - _playerPos;
                float angleToEnemy = (float)Math.Atan2(toEnemy.Y, toEnemy.X);
                float angleDiff = angleToEnemy - _playerAngle;

                while (angleDiff < -Math.PI)
                    angleDiff += (float)Math.PI * 2;
                while (angleDiff > Math.PI)
                    angleDiff -= (float)Math.PI * 2;

                if (Math.Abs(angleDiff) < 0.2f && toEnemy.Length() < 10f)
                {
                    monster.Alive = false;
                    _sfxDeath.Play(); // Som de morte!
                    break; // Dano instantâneo
                }
            }
        }

        // Sistema de Dano ao Jogador
        if (_damageTimer > 0)
            _damageTimer -= dt;

        foreach (var monster in _monsters)
        {
            if (!monster.Alive)
                continue;
            float dist = Vector2.Distance(_playerPos, monster.Position);
            if (dist < 0.8f && _damageTimer <= 0)
            {
                _playerHealth -= 10; // Perde 10 de vida por golpe
                _damageTimer = 0.5f; // Meio segundo de intervalo entre danos
                if (_playerHealth < 0)
                    _playerHealth = 0;
            }
        }

        // Sistema de Respawn Dinâmico
        _monsters.RemoveAll(mons => !mons.Alive);
        if (_monsters.Count < 3) // Limite de 3 inimigos simultâneos
        {
            int rx = _rng.Next(1, _map.GetLength(1) - 1);
            int ry = _rng.Next(1, _map.GetLength(0) - 1);
            if (_map[ry, rx] == 0) // Spawn apenas em células vazias
            {
                _monsters.Add(
                    new Monster
                    {
                        Position = new Vector2(rx + 0.5f, ry + 0.5f),
                        Sprite = _demonTexture, // Atribui sprite do Demônio
                    }
                );
            }
        }

        if (_playerHealth <= 0)
        {
            _playerHealth = 100;
            _playerPos = new Vector2(2.5f, 2.5f);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        GraphicsDevice.Clear(Color.Black);

        // Renderização de Chão (Floorcasting)
        float[] cosAngles = new float[screenWidth];
        float[] sinAngles = new float[screenWidth];
        float[] cosFix = new float[screenWidth];

        for (int x = 0; x < screenWidth; x++)
        {
            float rAngle = (_playerAngle - _fov / 2f) + (x / (float)screenWidth) * _fov;
            cosAngles[x] = (float)Math.Cos(rAngle);
            sinAngles[x] = (float)Math.Sin(rAngle);
            cosFix[x] = 1.0f / (float)Math.Cos(rAngle - _playerAngle);
        }

        for (int y = screenHeight / 2; y < screenHeight; y++)
        {
            // Cálculo de distância por linha
            float rowDistance = screenHeight / (y - screenHeight / 2.0f + 0.0001f);

            float shade = 1.0f - Math.Min(rowDistance / _depth, 1.0f);
            byte cMult = (byte)(255 * shade);

            for (int x = 0; x < screenWidth; x++)
            {
                float straightDist = rowDistance * cosFix[x];
                float floorX = _playerPos.X + cosAngles[x] * straightDist;
                float floorY = _playerPos.Y + sinAngles[x] * straightDist;

                int texX = (int)(floorX * _floorTexture.Width) % _floorTexture.Width;
                int texY = (int)(floorY * _floorTexture.Height) % _floorTexture.Height;
                if (texX < 0)
                    texX += _floorTexture.Width;
                if (texY < 0)
                    texY += _floorTexture.Height;

                Color pColor = _floorTextureData[texY * _floorTexture.Width + texX];

                // Sombreamento (Fog)
                pColor.R = (byte)(pColor.R * cMult / 255);
                pColor.G = (byte)(pColor.G * cMult / 255);
                pColor.B = (byte)(pColor.B * cMult / 255);

                _screenBuffer[y * screenWidth + x] = pColor;
            }
        }

        // Desenha pixels renderizados na CPU
        _screenTexture.SetData(_screenBuffer);

        // Escala para tela cheia/redimensionamento
        Matrix scaleMatrix = Matrix.CreateScale(
            (float)GraphicsDevice.Viewport.Width / screenWidth,
            (float)GraphicsDevice.Viewport.Height / screenHeight,
            1.0f
        );

        // Sampler State: Pixel Art + Wrap infinito
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            null,
            SamplerState.PointWrap,
            null,
            null,
            null,
            scaleMatrix
        );

        // Skybox (Céu Dinâmico)
        int skyOffset = (int)(_playerAngle * 250); // Deslocamento por ângulo

        _spriteBatch.Draw(
            _skyTexture,
            new Rectangle(0, 0, screenWidth, screenHeight / 2), // Metade superior
            new Rectangle(skyOffset, 0, screenWidth, _skyTexture.Height / 2), // Repetição infinita
            Color.White
        );

        _spriteBatch.Draw(_screenTexture, Vector2.Zero, Color.White);

        // Renderização de Paredes (Raycasting)
        for (int x = 0; x < screenWidth; x++)
        {
            float rayAngle = (_playerAngle - _fov / 2f) + (x / (float)screenWidth) * _fov;

            float rayDirX = (float)Math.Cos(rayAngle);
            float rayDirY = (float)Math.Sin(rayAngle);

            int mapX = (int)_playerPos.X;
            int mapY = (int)_playerPos.Y;

            float deltaDistX = (rayDirX == 0) ? 1e30f : Math.Abs(1.0f / rayDirX);
            float deltaDistY = (rayDirY == 0) ? 1e30f : Math.Abs(1.0f / rayDirY);

            float sideDistX;
            float sideDistY;
            int stepX;
            int stepY;

            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (_playerPos.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - _playerPos.X) * deltaDistX;
            }

            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (_playerPos.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0f - _playerPos.Y) * deltaDistY;
            }

            bool hitWall = false;
            int side = 0; // 0 para vertical, 1 para horizontal

            // Algoritmo DDA
            while (!hitWall)
            {
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;
                }

                // Limites do mapa
                if (mapX < 0 || mapX >= _map.GetLength(1) || mapY < 0 || mapY >= _map.GetLength(0))
                {
                    hitWall = true;
                }
                else if (_map[mapY, mapX] == 1)
                {
                    hitWall = true;
                }
            }

            // Distância perpendicular (sem distorção)
            float distanceToWall;
            if (side == 0)
                distanceToWall = (mapX - _playerPos.X + (1 - stepX) / 2.0f) / rayDirX;
            else
                distanceToWall = (mapY - _playerPos.Y + (1 - stepY) / 2.0f) / rayDirY;

            // Correção de Efeito Fisheye
            distanceToWall *= (float)Math.Cos(rayAngle - _playerAngle);

            if (distanceToWall <= 0.01f)
                distanceToWall = 0.01f;

            _depthBuffer[x] = distanceToWall;

            int ceiling = (int)((screenHeight / 2.0f) - screenHeight / distanceToWall);
            int floor = screenHeight - ceiling;
            int wallHeight = Math.Max(0, floor - ceiling);

            // Mapeamento de Textura (UV)
            float exactHitX =
                _playerPos.X
                + rayDirX * (distanceToWall / (float)Math.Cos(rayAngle - _playerAngle));
            float exactHitY =
                _playerPos.Y
                + rayDirY * (distanceToWall / (float)Math.Cos(rayAngle - _playerAngle));

            float textureU;
            if (side == 0)
            {
                textureU = exactHitY - (float)Math.Floor(exactHitY);
                if (rayDirX < 0)
                    textureU = 1.0f - textureU; // Inversão de textura
            }
            else
            {
                textureU = exactHitX - (float)Math.Floor(exactHitX);
                if (rayDirY > 0)
                    textureU = 1.0f - textureU; // Inverte pro outro lado
            }

            int texX = (int)(textureU * _wallTexture.Width);
            if (texX < 0)
                texX = 0;
            if (texX >= _wallTexture.Width)
                texX = _wallTexture.Width - 1;

            Rectangle sourceRect = new Rectangle(texX, 0, 1, _wallTexture.Height);

            // Shading por distância
            float shade = 1.0f - Math.Min(distanceToWall / _depth, 1.0f);
            if (side == 1)
                shade *= 0.7f; // Iluminação direcional fake
            Color wallColor = new Color((int)(255 * shade), (int)(255 * shade), (int)(255 * shade));

            _spriteBatch.Draw(
                _wallTexture,
                new Rectangle(x, ceiling, 1, wallHeight),
                sourceRect,
                wallColor
            );
        }

        // Inimigos (Billboarding)
        foreach (var monster in _monsters)
        {
            Vector2 toEnemy = monster.Position - _playerPos;
            float dist = toEnemy.Length();
            float angleToEnemy = (float)Math.Atan2(toEnemy.Y, toEnemy.X);
            float angleDiff = angleToEnemy - _playerAngle;

            while (angleDiff < -Math.PI)
                angleDiff += (float)Math.PI * 2;
            while (angleDiff > Math.PI)
                angleDiff -= (float)Math.PI * 2;

            if (Math.Abs(angleDiff) < _fov)
            {
                float correctedDist = dist * (float)Math.Cos(angleDiff);
                // Calculamos o tamanho baseado na escala real das paredes (2.0f unidades de altura projetada)
                int spriteHeight = (int)((2.0f * screenHeight) / correctedDist);
                // Reduzimos o demônio para 80% da altura da sala (0.8 unidades)
                spriteHeight = (int)(spriteHeight * 0.8f);

                // Grounding do sprite
                int floorY = (int)((screenHeight / 2.0f) + (screenHeight / correctedDist));
                int spriteScreenY = floorY - spriteHeight;

                float spriteScreenX = (0.5f * (angleDiff / (_fov / 2f)) + 0.5f) * screenWidth;

                for (int i = 0; i < spriteHeight; i++)
                {
                    int colX = (int)(spriteScreenX - spriteHeight / 2 + i);
                    if (colX >= 0 && colX < screenWidth)
                    {
                        if (_depthBuffer[colX] > correctedDist)
                        {
                            _spriteBatch.Draw(
                                monster.Sprite, // Desenha sprite da instância
                                new Rectangle(colX, spriteScreenY, 1, spriteHeight),
                                new Rectangle(
                                    (int)((i / (float)spriteHeight) * monster.Sprite.Width),
                                    0,
                                    1,
                                    monster.Sprite.Height
                                ),
                                Color.White
                            );
                        }
                    }
                }
            }
        }

        // Viewmodel da Arma
        int weaponHeight = (int)(screenHeight * 0.7f);
        int weaponWidth = (int)(
            _weaponTexture.Width * ((float)weaponHeight / _weaponTexture.Height)
        );

        // Recuo parabólico (Recoil)
        int recoilY =
            _recoilTimer > 0f ? (int)(Math.Sin((0.5f - _recoilTimer) * Math.PI / 0.5f) * 120) : 0;

        // Balanço de caminhada (Head Bob)
        int bobX = (int)(Math.Cos(_bobTimer) * 15);
        int bobY = (int)(Math.Abs(Math.Sin(_bobTimer)) * 20); // Quicada vertical (passos)

        int weaponX = (screenWidth / 2) - (weaponWidth / 2) + bobX;
        int weaponY = screenHeight - weaponHeight + bobY + recoilY;

        // Clarão de Disparo
        if (_recoilTimer > 0.35f) // Exibição temporária
        {
            int flashWidth = (int)(weaponWidth * 0.8f);
            int flashHeight = (int)(
                flashWidth * ((float)_flashTexture.Height / _flashTexture.Width)
            );

            int flashDestX = (screenWidth / 2) + bobX; // Alinhamento com a arma
            int flashDestY = weaponY + (int)(weaponHeight * 0.40f); // Ajuste no cano

            flashWidth = (int)(weaponWidth * 0.65f); // Tamanho proporcional
            flashHeight = (int)(flashWidth * ((float)_flashTexture.Height / _flashTexture.Width));

            _spriteBatch.Draw(
                _flashTexture,
                new Rectangle(flashDestX, flashDestY, flashWidth, flashHeight),
                null,
                Color.White,
                (float)Math.PI / 2f, // Rotação UP
                new Vector2(_flashTexture.Width / 2f, _flashTexture.Height / 2f), // Âncora central
                SpriteEffects.None,
                0f
            );
        }

        _spriteBatch.Draw(
            _weaponTexture,
            new Rectangle(weaponX, weaponY, weaponWidth, weaponHeight),
            Color.White
        );

        // HUD de Vida: Apenas Número + %
        int uiX = 50;
        int uiY = screenHeight - 80;
        int scale = 8; // Tamanho dos pixels do número

        void DrawPixelDigit(int dx, int dy, int n, Color color)
        {
            bool[] p = n switch
            {
                0 => new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                1 => new bool[]
                {
                    false,
                    true,
                    false,
                    true,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                    false,
                    true,
                    true,
                    true,
                },
                2 => new bool[]
                {
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                },
                3 => new bool[]
                {
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                4 => new bool[]
                {
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                },
                5 => new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                6 => new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                7 => new bool[]
                {
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                },
                8 => new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                9 => new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                },
                _ => new bool[15],
            };
            for (int i = 0; i < 15; i++)
                if (p[i])
                    _spriteBatch.Draw(
                        _pixelTexture,
                        new Rectangle(dx + (i % 3) * scale, dy + (i / 3) * scale, scale, scale),
                        color
                    );
        }

        Color healthColor = _playerHealth > 25 ? Color.Red : Color.DarkRed;

        if (_playerHealth >= 100)
        {
            DrawPixelDigit(uiX, uiY, 1, healthColor);
            DrawPixelDigit(uiX + 4 * scale, uiY, 0, healthColor);
            DrawPixelDigit(uiX + 8 * scale, uiY, 0, healthColor);
        }
        else
        {
            DrawPixelDigit(uiX, uiY, _playerHealth / 10, healthColor);
            DrawPixelDigit(uiX + 4 * scale, uiY, _playerHealth % 10, healthColor);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
