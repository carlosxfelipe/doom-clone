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
                    monster.Health--;
                    if (monster.Health <= 0)
                    {
                        monster.Alive = false;
                        _monstersKilled++;
                        _sfxNpcDeath.Play(); // Som de morte real!
                    }
                    else
                    {
                        _sfxNpcPain.Play(); // Som de dor!
                    }
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

            float distToPlayer = Vector2.Distance(_playerPos, monster.Position);

            // IA de Perseguição para corpo-a-corpo
            if (monster.IsMelee)
            {
                Vector2 dirToPlayer = _playerPos - monster.Position;
                if (distToPlayer > 0.7f && distToPlayer < 15f)
                {
                    dirToPlayer.Normalize();
                    float chaseSpeed = 1.2f * dt;
                    Vector2 nextPos = monster.Position + dirToPlayer * chaseSpeed;
                    if (_map[(int)nextPos.Y, (int)nextPos.X] == 0)
                        monster.Position = nextPos;
                }
            }

            // IA de Ataque à distância (Demônio)
            if (!monster.IsMelee && distToPlayer < 12f)
            {
                monster.AttackTimer -= dt;
                if (monster.AttackTimer <= 0)
                {
                    Vector2 dirToPlayer = _playerPos - monster.Position;
                    dirToPlayer.Normalize();

                    _sfxNpcAttack.Play(); // Som de ataque do NPC!

                    _projectiles.Add(
                        new Projectile
                        {
                            Position = monster.Position,
                            Direction = dirToPlayer,
                            Speed = 6.0f,
                            HeightOffset = 3.5f,
                        }
                    );

                    // Intervalo randômico entre 1.5 e 3.5 segundos
                    monster.AttackTimer = 1.5f + (float)_rng.NextDouble() * 2.0f;
                }
            }

            // Lógica de Animação de Ataque (Esqueleto)
            if (monster.IsMelee)
            {
                if (monster.StateTimer > 0)
                {
                    monster.StateTimer -= dt;
                    monster.Sprite = _skeletonAttackTexture;
                }
                else
                {
                    monster.Sprite = _skeletonTexture;
                    if (distToPlayer < 1.2f)
                    {
                        monster.StateTimer = 0.5f; // Duração do "corte"
                        _sfxNpcAttack.Play(); // Som de ataque corpo-a-corpo!
                    }
                }
            }

            // Dano por contato direto (mordida ou espada)
            if (distToPlayer < 0.8f && _damageTimer <= 0)
            {
                _playerHealth -= monster.IsMelee ? 12 : 10; // Espada agora tira 12
                _damageTimer = 0.6f;
                _sfxPlayerPain.Play(); // Som de dor do jogador!
                if (_playerHealth < 0)
                    _playerHealth = 0;
            }
        }

        // Lógica das Bolas de Fogo
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            p.Position += p.Direction * p.Speed * dt;

            // Efeito de queda (Gravidade)
            p.HeightOffset -= 1.5f * dt;
            if (p.HeightOffset < 0.2f)
                p.HeightOffset = 0.2f;

            if (_map[(int)p.Position.Y, (int)p.Position.X] == 1)
            {
                p.Alive = false;
            }
            else if (Vector2.Distance(p.Position, _playerPos) < 0.5f)
            {
                _playerHealth -= 15;
                p.Alive = false;
                _sfxPlayerPain.Play(); // Som de dor do jogador ao ser atingido por projétil!
                if (_playerHealth < 0)
                    _playerHealth = 0;
            }

            if (!p.Alive)
                _projectiles.RemoveAt(i);
        }

        // Sistema de Respawn Dinâmico
        _monsters.RemoveAll(mons => !mons.Alive);
        if (_monsters.Count < 3) // Limite de 3 inimigos simultâneos
        {
            int rx = _rng.Next(1, _map.GetLength(1) - 1);
            int ry = _rng.Next(1, _map.GetLength(0) - 1);
            Vector2 spawnPos = new Vector2(rx + 0.5f, ry + 0.5f);
            float distToPlayer = Vector2.Distance(_playerPos, spawnPos);

            if (_map[ry, rx] == 0 && distToPlayer > 6.0f) // Spawn apenas longe do jogador
            {
                bool isMelee = _rng.Next(2) == 0;
                _monsters.Add(
                    new Monster
                    {
                        Position = spawnPos,
                        Sprite = isMelee ? _skeletonTexture : _demonTexture,
                        IsMelee = isMelee,
                    }
                );
            }
        }

        // Coleta de Medkits
        for (int i = _medkits.Count - 1; i >= 0; i--)
        {
            var kit = _medkits[i];
            if (Vector2.Distance(_playerPos, kit.Position) < 0.6f && _playerHealth < 100)
            {
                _playerHealth += 30;
                if (_playerHealth > 100)
                    _playerHealth = 100;
                kit.Active = false;
                _medkits.RemoveAt(i);
                // Poderia tocar um som de cura aqui se tivéssemos um
            }
        }

        // Sistema de Respawn de Medkits
        if (_medkits.Count < 2) // Mantém sempre 2 medkits no mapa
        {
            int rx = _rng.Next(1, _map.GetLength(1) - 1);
            int ry = _rng.Next(1, _map.GetLength(0) - 1);
            Vector2 spawnPos = new Vector2(rx + 0.5f, ry + 0.5f);
            if (_map[ry, rx] == 0 && Vector2.Distance(_playerPos, spawnPos) > 4.0f)
            {
                _medkits.Add(new Item { Position = spawnPos });
            }
        }

        if (_playerHealth <= 0)
        {
            _playerHealth = 100;
            _playerPos = new Vector2(2.5f, 2.5f);
            _monstersKilled = 0;
        }

        base.Update(gameTime);
    }
}
