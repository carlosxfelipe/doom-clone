using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoomClone;

public partial class Game1 : Game
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
    private int _monstersKilled = 0;
    private float _damageTimer = 0f;

    private float _recoilTimer = 0f;
    private float _bobTimer = 0f;
    private float _lastBob = 0f;

    private SoundEffect _sfxShotgun;
    private SoundEffect _sfxFootstep;
    private SoundEffect _sfxNpcAttack;
    private SoundEffect _sfxNpcDeath;
    private SoundEffect _sfxNpcPain;
    private SoundEffect _sfxPlayerPain;
    private SoundEffect _sfxTheme;
    private SoundEffectInstance _themeInstance;

    private Texture2D _demonTexture;
    private Texture2D _skeletonTexture;
    private Texture2D _skeletonAttackTexture;
    private Texture2D _fireballTexture;

    private class Monster
    {
        public Vector2 Position;
        public bool Alive = true;
        public Texture2D Sprite;
        public float AttackTimer = 2.0f;
        public bool IsMelee = false;
        public float StateTimer = 0f;
        public int Health = 2;
    }

    private class Projectile
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed = 5.0f;
        public float HeightOffset = 3.5f;
        public bool Alive = true;
    }

    private List<Monster> _monsters = new List<Monster>();
    private List<Projectile> _projectiles = new List<Projectile>();
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

        _skeletonTexture = Texture2D.FromFile(GraphicsDevice, "Content/skeleton.png");
        Color[] skeletonData = new Color[_skeletonTexture.Width * _skeletonTexture.Height];
        _skeletonTexture.GetData(skeletonData);
        for (int i = 0; i < skeletonData.Length; i++)
        {
            int r = skeletonData[i].R;
            int g = skeletonData[i].G;
            int b = skeletonData[i].B;
            // Chroma Key Magenta (#FF00FF)
            if (r > 200 && b > 200 && g < 150)
                skeletonData[i] = Color.Transparent;
        }
        _skeletonTexture.SetData(skeletonData);

        _skeletonAttackTexture = Texture2D.FromFile(GraphicsDevice, "Content/skeleton_attack.png");
        Color[] skeletonAttackData = new Color[
            _skeletonAttackTexture.Width * _skeletonAttackTexture.Height
        ];
        _skeletonAttackTexture.GetData(skeletonAttackData);
        for (int i = 0; i < skeletonAttackData.Length; i++)
        {
            int r = skeletonAttackData[i].R;
            int g = skeletonAttackData[i].G;
            int b = skeletonAttackData[i].B;
            if (r > 200 && b > 200 && g < 150)
                skeletonAttackData[i] = Color.Transparent;
        }
        _skeletonAttackTexture.SetData(skeletonAttackData);

        _fireballTexture = Texture2D.FromFile(GraphicsDevice, "Content/fireball.png");
        Color[] fireballData = new Color[_fireballTexture.Width * _fireballTexture.Height];
        _fireballTexture.GetData(fireballData);
        for (int i = 0; i < fireballData.Length; i++)
        {
            if (fireballData[i].R > 200 && fireballData[i].G < 100 && fireballData[i].B > 200) // Chroma Key Magenta
                fireballData[i] = Color.Transparent;
        }
        _fireballTexture.SetData(fireballData);

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

        // Carregando os novos sons
        _sfxShotgun = SoundEffect.FromFile("Content/shotgun.wav");
        _sfxFootstep = SoundEffect.FromFile("Content/footstep.wav");
        _sfxNpcAttack = SoundEffect.FromFile("Content/npc_attack.wav");
        _sfxNpcDeath = SoundEffect.FromFile("Content/npc_death.wav");
        _sfxNpcPain = SoundEffect.FromFile("Content/npc_pain.wav");
        _sfxPlayerPain = SoundEffect.FromFile("Content/player_pain.wav");

        // Carregando e iniciando a música (em WAV para compatibilidade total no Mac)
        _sfxTheme = SoundEffect.FromFile("Content/theme.wav");
        _themeInstance = _sfxTheme.CreateInstance();
        _themeInstance.IsLooped = true;
        _themeInstance.Volume = 0.5f;
        _themeInstance.Play();
    }
}
