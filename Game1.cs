using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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

    private float _recoilTimer = 0f;
    private float _bobTimer = 0f;

    private Texture2D _enemyTexture;

    private class Monster
    {
        public Vector2 Position;
        public bool Alive = true;
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

    private Vector2 _playerPos = new Vector2(2.5f, 2.5f); // Centro da célula 2,2 (caminho livre no inicio do labirinto)
    private float _playerAngle = 0f;

    private float _fov = (float)Math.PI / 3.0f; // Campo de visão de 60 graus
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

        // Criando textura em tempo de execução de 1x1 branco para chão/teto
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Carregando as texturas geradas no diretório Content
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

            // Chroma Key Robusto: Pega até as bordas misturadas e "anti-aliasing" da compressão da imagem
            // Qualquer cor que seja predominantemente vermelha e azul (roxo/magenta), independente de quão escura for
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

        _enemyTexture = Texture2D.FromFile(GraphicsDevice, "Content/enemy.png");
        Color[] enemyData = new Color[_enemyTexture.Width * _enemyTexture.Height];
        _enemyTexture.GetData(enemyData);
        for (int i = 0; i < enemyData.Length; i++)
        {
            int r = enemyData[i].R;
            int g = enemyData[i].G;
            int b = enemyData[i].B;
            if (r > g + 40 && b > g + 40) // Chroma key para magenta
                enemyData[i] = Color.Transparent;
        }
        _enemyTexture.SetData(enemyData);

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

        // Animação de caminhada (Head bob)
        if (k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S))
            _bobTimer += dt * 12f; // Velocidade do passo
        else
            _bobTimer = 0f; // Reseta arma ao parar

        // Controle de Tiro (Recoil e Animação)
        if (_recoilTimer > 0)
            _recoilTimer -= dt;

        MouseState m = Mouse.GetState();
        bool isShooting = k.IsKeyDown(Keys.Space) || m.LeftButton == ButtonState.Pressed;

        if (isShooting && _recoilTimer <= 0)
        {
            _recoilTimer = 0.5f;

            // Tenta acertar o monstro mais próximo que está vivo e na mira
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
                    break; // Um tiro, uma morte (por enquanto)
                }
            }
        }

        // Lógica de RESPRAWN (Opção 3)
        // Remove mortos e adiciona um novo se não houver monstros
        _monsters.RemoveAll(mons => !mons.Alive);
        if (_monsters.Count < 3) // Mantém sempre 3 monstros no mapa
        {
            int rx = _rng.Next(1, _map.GetLength(1) - 1);
            int ry = _rng.Next(1, _map.GetLength(0) - 1);
            if (_map[ry, rx] == 0) // Só nasce em espaço vazio
            {
                _monsters.Add(new Monster { Position = new Vector2(rx + 0.5f, ry + 0.5f) });
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        GraphicsDevice.Clear(Color.Black);

        // --- CÁLCULO DE CHÃO E TETO RAPIDAMENTE NO CPU ---
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
            // O fator foi ajustado para remover o '0.5f' e bater PERFEITAMENTE com a projeção de altura da parede
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

                // Aplicar sombra baseada em distância do chão
                pColor.R = (byte)(pColor.R * cMult / 255);
                pColor.G = (byte)(pColor.G * cMult / 255);
                pColor.B = (byte)(pColor.B * cMult / 255);

                _screenBuffer[y * screenWidth + x] = pColor;
            }
        }

        // Envia todos os pixels do chão calculados para a placa de vídeo
        _screenTexture.SetData(_screenBuffer);

        // Prepara uma Matriz de Escala para esticar os nossos 800x600 originais pro tamanho nativo e exato da tela inteira do seu Mac
        Matrix scaleMatrix = Matrix.CreateScale(
            (float)GraphicsDevice.Viewport.Width / screenWidth,
            (float)GraphicsDevice.Viewport.Height / screenHeight,
            1.0f
        );

        // Ativa o PointWrap para esticar mantendo o visual "quadradão/pixelado" intacto (Point) e permitir a foto do céu girar infinito (Wrap)
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            null,
            SamplerState.PointWrap,
            null,
            null,
            null,
            scaleMatrix
        );

        // -- DESENHA O CÉU (Skybox) --
        // Calcula um deslocamento X baseado no ângulo que o jogador está olhando.
        // Multiplicar por uma constante ajusta a velocidade da rotação do céu!
        int skyOffset = (int)(_playerAngle * 250);

        _spriteBatch.Draw(
            _skyTexture,
            new Rectangle(0, 0, screenWidth, screenHeight / 2), // Cobre só a metade de cima da tela
            new Rectangle(skyOffset, 0, screenWidth, _skyTexture.Height / 2), // Seleciona um pedaço movel da textura original com Loop Infinito!
            Color.White
        );

        // Desenha a "tela" do chão 3D desenhada a mão na CPU.
        // A parte de cima é transparente então o Céu brilha pelo buraco superior!
        _spriteBatch.Draw(_screenTexture, Vector2.Zero, Color.White);

        // 2. RAYCASTING
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
            int side = 0; // 0 para parede vertical (leste-oeste), 1 para horizontal (norte-sul)

            // Algoritmo DDA (Digital Differential Analyzer) Exato
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

            // Distância Euclidiana Exata
            float distanceToWall;
            if (side == 0)
                distanceToWall = (mapX - _playerPos.X + (1 - stepX) / 2.0f) / rayDirX;
            else
                distanceToWall = (mapY - _playerPos.Y + (1 - stepY) / 2.0f) / rayDirY;

            // --- AJUSTE VITAL: Corrige o distorcimento olho-de-peixe (fisheye)! ---
            distanceToWall *= (float)Math.Cos(rayAngle - _playerAngle);

            // Prevenção de divisão por zero ou muito perto da parede
            if (distanceToWall <= 0.01f)
                distanceToWall = 0.01f;

            _depthBuffer[x] = distanceToWall;

            int ceiling = (int)((screenHeight / 2.0f) - screenHeight / distanceToWall);
            int floor = screenHeight - ceiling;
            int wallHeight = Math.Max(0, floor - ceiling);

            // --- CÁLCULO EXATO DE TEXTURA (DDA UV Mapping) ---
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
                    textureU = 1.0f - textureU; // Inverte ao olhar pra um lado da parede
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

            // --- AJUSTE ESTÉTICO: Sombreamento por distância ---
            float shade = 1.0f - Math.Min(distanceToWall / _depth, 1.0f);
            if (side == 1)
                shade *= 0.7f; // Faz uma das faces 30% mais escura (simula iluminação global direcional)
            Color wallColor = new Color((int)(255 * shade), (int)(255 * shade), (int)(255 * shade));

            _spriteBatch.Draw(
                _wallTexture,
                new Rectangle(x, ceiling, 1, wallHeight),
                sourceRect,
                wallColor
            );
        } // Fim da renderização das paredes

        // 3. INIMIGOS (Billboarding com Múltiplas instâncias)
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
                int spriteSize = (int)(screenHeight / correctedDist);

                // AJUSTE DE ALTURA: O pé do monstro agora fica no chão
                int floorY = (screenHeight / 2) + (spriteSize / 2);
                int spriteScreenY = floorY - spriteSize;

                float spriteScreenX = (0.5f * (angleDiff / (_fov / 2f)) + 0.5f) * screenWidth;

                for (int i = 0; i < spriteSize; i++)
                {
                    int colX = (int)(spriteScreenX - spriteSize / 2 + i);
                    if (colX >= 0 && colX < screenWidth)
                    {
                        if (_depthBuffer[colX] > correctedDist)
                        {
                            _spriteBatch.Draw(
                                _enemyTexture,
                                new Rectangle(colX, spriteScreenY, 1, spriteSize),
                                new Rectangle(
                                    (int)((i / (float)spriteSize) * _enemyTexture.Width),
                                    0,
                                    1,
                                    _enemyTexture.Height
                                ),
                                Color.White
                            );
                        }
                    }
                }
            }
        }

        // 4. ARMA (com animação!)
        int weaponHeight = (int)(screenHeight * 0.7f);
        int weaponWidth = (int)(
            _weaponTexture.Width * ((float)weaponHeight / _weaponTexture.Height)
        );

        // Coice da arma atirando (Recua "para baixo" em um pequeno pulo parabólico de senoide)
        int recoilY =
            _recoilTimer > 0f ? (int)(Math.Sin((0.5f - _recoilTimer) * Math.PI / 0.5f) * 120) : 0;

        // Caminhada ("Head bob") em formato curvo e 8 deitado
        int bobX = (int)(Math.Cos(_bobTimer) * 15);
        // Valor absoluto cria quicadas curtas verticais quando você pisa ("passos")
        int bobY = (int)(Math.Abs(Math.Sin(_bobTimer)) * 20);

        int weaponX = (screenWidth / 2) - (weaponWidth / 2) + bobX;
        int weaponY = screenHeight - weaponHeight + bobY + recoilY;

        // Clarão do tiro (Muzzle Flash)
        if (_recoilTimer > 0.35f) // Desenha apenas nos primeiros milissegundos
        {
            // O novo clarão que criei (horizontal, deitado)
            int flashWidth = (int)(weaponWidth * 0.8f);
            int flashHeight = (int)(
                flashWidth * ((float)_flashTexture.Height / _flashTexture.Width)
            );

            // Centraliza o ponto de destino (centro da tela e base/topo do cano da espingarda)
            int flashDestX = (screenWidth / 2) + bobX;
            // Puxamos ela uns bons centímetros pra baixo para aterrissar sobre o metal do cano real da espingarda
            int flashDestY = weaponY + (int)(weaponHeight * 0.40f);

            // O clarão pode ser um pouco menor também (65% da espátula da arma pra não cegar o player)
            flashWidth = (int)(weaponWidth * 0.65f);
            flashHeight = (int)(flashWidth * ((float)_flashTexture.Height / _flashTexture.Width));

            _spriteBatch.Draw(
                _flashTexture,
                new Rectangle(flashDestX, flashDestY, flashWidth, flashHeight), // Local e tamanho
                null,
                Color.White,
                (float)Math.PI / 2f, // Gira a imagem +90 graus (positivo) para atirar o fogo para cima!
                new Vector2(_flashTexture.Width / 2f, _flashTexture.Height / 2f), // Roda usando o centro como âncora
                SpriteEffects.None,
                0f
            );
        }

        _spriteBatch.Draw(
            _weaponTexture,
            new Rectangle(weaponX, weaponY, weaponWidth, weaponHeight),
            Color.White
        );

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
