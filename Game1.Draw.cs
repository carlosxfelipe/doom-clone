using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public partial class Game1
{
    protected override void Draw(GameTime gameTime)
    {
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        GraphicsDevice.Clear(Color.Black);

        if (_currentState == GameState.Playing)
        {
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
                    if (
                        mapX < 0
                        || mapX >= _map.GetLength(1)
                        || mapY < 0
                        || mapY >= _map.GetLength(0)
                    )
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
                Color wallColor = new Color(
                    (int)(255 * shade),
                    (int)(255 * shade),
                    (int)(255 * shade)
                );

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

                    float time = (float)gameTime.TotalGameTime.TotalSeconds;
                    float bob = (float)Math.Sin(time * 3.0f + monster.Position.X * 5f) * 0.02f;

                    // Calculamos o tamanho baseado na escala real das paredes (2.0f unidades de altura projetada)
                    int spriteHeight = (int)((2.0f * screenHeight) / correctedDist);
                    // Reduzimos o demônio para 80% da altura da sala (0.8 unidades)
                    spriteHeight = (int)(spriteHeight * (0.8f + bob * 0.3f));

                    // Grounding do sprite com o efeito de bob suavizado
                    int floorY = (int)((screenHeight / 2.0f) + (screenHeight / correctedDist));
                    int spriteScreenY = floorY - spriteHeight + (int)(bob * screenHeight * 0.05f);

                    float spriteScreenX = (0.5f * (angleDiff / (_fov / 2f)) + 0.5f) * screenWidth;

                    for (int i = 0; i < spriteHeight; i++)
                    {
                        int colX = (int)(spriteScreenX - spriteHeight / 2 + i);
                        if (colX >= 0 && colX < screenWidth)
                        {
                            if (_depthBuffer[colX] > correctedDist)
                            {
                                _spriteBatch.Draw(
                                    monster.Sprite,
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

            // Projéteis (Bolas de Fogo)
            foreach (var p in _projectiles)
            {
                Vector2 toProj = p.Position - _playerPos;
                float dist = toProj.Length();
                float angleToProj = (float)Math.Atan2(toProj.Y, toProj.X);
                float angleDiff = angleToProj - _playerAngle;

                while (angleDiff < -Math.PI)
                    angleDiff += (float)Math.PI * 2;
                while (angleDiff > Math.PI)
                    angleDiff -= (float)Math.PI * 2;

                if (Math.Abs(angleDiff) < _fov)
                {
                    float correctedDist = dist * (float)Math.Cos(angleDiff);
                    int projHeight = (int)((0.4f * screenHeight) / correctedDist);

                    int floorY = (int)((screenHeight / 2.0f) + (screenHeight / correctedDist));
                    int projScreenY = floorY - (int)(projHeight * p.HeightOffset);

                    float projScreenX = (0.5f * (angleDiff / (_fov / 2f)) + 0.5f) * screenWidth;

                    int projWidth = projHeight;
                    for (int i = 0; i < projWidth; i++)
                    {
                        int colX = (int)(projScreenX - projWidth / 2 + i);
                        if (colX >= 0 && colX < screenWidth)
                        {
                            if (_depthBuffer[colX] > correctedDist)
                            {
                                _spriteBatch.Draw(
                                    _fireballTexture,
                                    new Rectangle(colX, projScreenY, 1, projHeight),
                                    new Rectangle(
                                        (int)((i / (float)projWidth) * _fireballTexture.Width),
                                        0,
                                        1,
                                        _fireballTexture.Height
                                    ),
                                    Color.White
                                );
                            }
                        }
                    }
                }
            }

            // Renderização de Medkits (Billboarding)
            foreach (var kit in _medkits)
            {
                Vector2 toKit = kit.Position - _playerPos;
                float dist = toKit.Length();
                float angleToKit = (float)Math.Atan2(toKit.Y, toKit.X);
                float angleDiff = angleToKit - _playerAngle;

                while (angleDiff < -Math.PI)
                    angleDiff += (float)Math.PI * 2;
                while (angleDiff > Math.PI)
                    angleDiff -= (float)Math.PI * 2;

                if (Math.Abs(angleDiff) < _fov)
                {
                    float correctedDist = dist * (float)Math.Cos(angleDiff);
                    // Medkits são menores (0.3 unidades de altura)
                    int spriteHeight = (int)((0.3f * screenHeight) / correctedDist);

                    int floorY = (int)((screenHeight / 2.0f) + (screenHeight / correctedDist));
                    int spriteScreenY = floorY - spriteHeight;

                    float spriteScreenX = (0.5f * (angleDiff / (_fov / 2f)) + 0.5f) * screenWidth;

                    int spriteWidth = spriteHeight;
                    for (int i = 0; i < spriteWidth; i++)
                    {
                        int colX = (int)(spriteScreenX - spriteWidth / 2 + i);
                        if (colX >= 0 && colX < screenWidth)
                        {
                            if (_depthBuffer[colX] > correctedDist)
                            {
                                _spriteBatch.Draw(
                                    _medkitTexture,
                                    new Rectangle(colX, spriteScreenY, 1, spriteHeight),
                                    new Rectangle(
                                        (int)((i / (float)spriteWidth) * _medkitTexture.Width),
                                        0,
                                        1,
                                        _medkitTexture.Height
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
                _recoilTimer > 0f
                    ? (int)(Math.Sin((0.5f - _recoilTimer) * Math.PI / 0.5f) * 120)
                    : 0;

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
                flashHeight = (int)(
                    flashWidth * ((float)_flashTexture.Height / _flashTexture.Width)
                );

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
            int scale = 8;
            Color healthColor = _playerHealth > 25 ? Color.Red : Color.DarkRed;

            if (_playerHealth >= 100)
            {
                DrawPixelDigit(uiX, uiY, 1, scale, healthColor);
                DrawPixelDigit(uiX + 4 * scale, uiY, 0, scale, healthColor);
                DrawPixelDigit(uiX + 8 * scale, uiY, 0, scale, healthColor);
            }
            else
            {
                DrawPixelDigit(uiX, uiY, _playerHealth / 10, scale, healthColor);
                DrawPixelDigit(uiX + 4 * scale, uiY, _playerHealth % 10, scale, healthColor);
            }

            // Placar de Monstros (Canto Superior Direito)
            int scoreX = screenWidth - 100;
            int scoreY = 30;
            Color scoreColor = Color.Yellow;

            DrawPixelDigit(scoreX, scoreY, (_monstersKilled / 100) % 10, scale, scoreColor);
            DrawPixelDigit(
                scoreX + 4 * scale,
                scoreY,
                (_monstersKilled / 10) % 10,
                scale,
                scoreColor
            );
            DrawPixelDigit(scoreX + 8 * scale, scoreY, _monstersKilled % 10, scale, scoreColor);

            _spriteBatch.End();
        }
        else if (_currentState == GameState.Menu)
        {
            DrawMenu(screenWidth, screenHeight);
        }
        else if (_currentState == GameState.Credits)
        {
            DrawCredits(screenWidth, screenHeight);
        }

        base.Draw(gameTime);
    }
}
