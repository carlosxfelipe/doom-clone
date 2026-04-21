using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public partial class Game1
{
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
}
