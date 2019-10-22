using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace イカP_1on1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static SpriteFont spriteFont1;

        #region 画像
        Texture2D st, br, tt, cr, nl;//ルート画像
        public static Texture2D it_ef;//アイテムのエフェクト
        public static List<Texture2D> it;//アイテム画像
        enum itemList
        {
            十字パネル増加 = 0, パネル消去, パネルシャッフル, スピードアップ, スピードダウン
        }
        public static List<int> its_number;//中央に並ぶアイテムの画像
        Texture2D tl;//タイトル画像
        Texture2D bk;//背景画像
        Texture2D cs;//カーソル画像
        Texture2D ar;//矢印画像
        Texture2D[] sq, oc;//イカ、タコ画像
        Texture2D[] rs;//リザルト画像
        Texture2D nm;//数字
        Texture2D wk;//枠
        Texture2D[] playerImage;
        Texture2D startMessage;
        Texture2D scoreImage;
        Texture2D timeImage;
        Texture2D readyGoMessage;
        Texture2D finishMessage;
        Texture2D endMessage;
        public static Texture2D spUpEffect, spDownEffect;
        int winner;
        #endregion
        #region サウンド
        Song tl_bm;
        Song pl_bm;
        Song rs_bm;
        Song hr_bm;//終了間際のbgm
        SoundEffect te_se;//ゲームスタート時の効果音
        SoundEffect finishSe;
        SoundEffect cr_se1;//パズル組み換え時の効果音
        SoundEffect cr_se2;//パズル組み換え時の効果音
        SoundEffect pz_se;//パズル組み換え時の効果音
        SoundEffect it_se;//アイテム取得時の効果音
        SoundEffect fd_se;//生き物消滅時の効果音
        #endregion
        #region パネル
        public static List<Panel> p_list;
        Panel[,] panel_data1, panel_data2;
        public static List<Item> it_list;//アイテムリスト
        public static int p1_it_count;//プレイヤー1のアイテム数
        public static int pn_size = 120;//パネルひとつあたりのサイズ
        public static int h_margin = 200;//余白-横 min130
        public static int v_margin = 150;//余白-縦 min150
        #endregion
        #region 操作
        public static List<Cursor> c_list;
        bool start;//始まったか
        static float tm;//時間経過
        const float TIME_LIMIT = 60;//制限時間
        float tm_up = TIME_LIMIT;
        const int MINUTE = 60;
        bool end;//ゲーム終了
        int sc_m = 0;//最大スコア
        bool clm;//ゲーム終了間際？
        const int CLIMAX_TIME = 30;
        Color co_nm;
        #endregion
        int seed;
        public static Random rnd;
        int wn_mode;//ウィンドウモード
        int inter;//コマンドウェイト
        const int FREEZEWAIT = 70;//処理停止時間
        int freezeCount;
        const int INTERVAL = 10;
        int window_width;
        int window_height;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            wn_mode = 1;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            window_width = graphics.PreferredBackBufferWidth;
            window_height = graphics.PreferredBackBufferHeight;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont1 = Content.Load<SpriteFont>("SpriteFont1");
            // TODO: use this.Content to load your game content here
            #region 画像初期化
            st = Content.Load<Texture2D>("直線");
            br = Content.Load<Texture2D>("折線");
            tt = Content.Load<Texture2D>("T線");
            cr = Content.Load<Texture2D>("十字線");
            nl = Content.Load<Texture2D>("無し");
            it_ef = Content.Load<Texture2D>("pipo-mapeffect014");
            it = new List<Texture2D>();
            it.Add(Content.Load<Texture2D>("item_01_1"));
            it.Add(Content.Load<Texture2D>("item_01_2"));
            it.Add(Content.Load<Texture2D>("item_01_3"));
            it.Add(Content.Load<Texture2D>("item_02"));
            it.Add(Content.Load<Texture2D>("item_03"));
            tl = Content.Load<Texture2D>("logo_02");
            bk = Content.Load<Texture2D>("背景");
            cs = Content.Load<Texture2D>("カーソル");
            ar = Content.Load<Texture2D>("アロー");
            sq = new Texture2D[2] { Content.Load<Texture2D>("ika_01"), Content.Load<Texture2D>("ika_02") };
            oc = new Texture2D[2] { Content.Load<Texture2D>("tako_01"), Content.Load<Texture2D>("tako_02") };
            rs = new Texture2D[3] { Content.Load<Texture2D>("result_tako"),
                Content.Load<Texture2D>("result_ika"),Content.Load<Texture2D>("result_draw") };
            nm = Content.Load<Texture2D>("num");
            wk = Content.Load<Texture2D>("frame");
            playerImage = new Texture2D[2] { Content.Load<Texture2D>("player1"),
                Content.Load<Texture2D>("player2") };
            scoreImage = Content.Load<Texture2D>("score");
            startMessage = Content.Load<Texture2D>("Press Enter Key");
            timeImage = Content.Load<Texture2D>("time");
            readyGoMessage = Content.Load<Texture2D>("Ready Go!");
            finishMessage = Content.Load<Texture2D>("FInish!");
            endMessage = Content.Load<Texture2D>("End Game Enter Key");
            spUpEffect= Content.Load<Texture2D>("pipo-btleffect066");
            spDownEffect = Content.Load<Texture2D>("pipo-btleffect067");
            #endregion
            #region サウンド初期化
            tl_bm = Content.Load<Song>("agj_XNA_BGM_start sound");
            //te_se = Content.Load<SoundEffectInstance>("agj_XNA_BGM_start push sound");
            pl_bm = Content.Load<Song>("agj_XNA_BGM_play sound");
            rs_bm = Content.Load<Song>("agj_XNA_BGM_result sound");
            hr_bm = Content.Load<Song>("agj_XNA_BGM_play sound hurry");
            te_se = Content.Load<SoundEffect>("agj_XNA_BGM_start push sound");
            finishSe = Content.Load<SoundEffect>("XNA＿BGM＿finish");
            cr_se1 = Content.Load<SoundEffect>("agj_XNA_BGM_puzzle 1p-01");
            cr_se2 = Content.Load<SoundEffect>("agj_XNA_BGM_puzzle 2p");
            pz_se = Content.Load<SoundEffect>("agj_XNA_BGM_puzzle complete");
            it_se = Content.Load<SoundEffect>("agj_XNA_BGM_ item get");
            fd_se = Content.Load<SoundEffect>("agj_XNA_BGM_creature fade out");
            
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.Play(tl_bm);
            #endregion
            //基礎パネル初期化
            p_list = new List<Panel>();
            p_list.Add(new Panel(st, new bool[4] { true, false, true, false }));//直
            p_list.Add(new Panel(br, new bool[4] { true, true,false,false }));//折
            p_list.Add(new Panel(tt, new bool[4] { true, true, true,false }));//T
            p_list.Add(new Panel(cr, new bool[4] { true, true, true, true }));//十字
            p_list.Add(new Panel(nl, new bool[4] { false, false, false, false }));//無

            InitializeGame();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // TODO: Add your update logic here
            if (start)
            {
                if (freezeCount >0)
                {
                    freezeCount--;
                    if(freezeCount==0)
                    {
                        if(end)//ゲーム終了時
                        {
                            MediaPlayer.Play(rs_bm);
                        }
                        else//ゲーム開始時
                        {
                            MediaPlayer.Play(pl_bm);
                        }
                    }
                }
                else
                {
                    if (!end)
                    {
                        for (int i = 0; i < c_list.Count; i++)
                        {
                            c_list[i].onUpdate();
                        }
                        for (int i = 0; i < it_list.Count; i++)
                        {
                            it_list[i].Run();
                        }
                        tm += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (TIME_LIMIT - tm <= CLIMAX_TIME && !clm)//終了間際か判定
                        {
                            clm = true;
                            try
                            {
                                MediaPlayer.Stop();
                                MediaPlayer.Play(hr_bm);
                            }
                            catch { }
                            co_nm.G = 100;
                            co_nm.B = 100;
                        }
                        if (TIME_LIMIT <= tm)
                        {
                            tm = 0f;
                            end = true;
                            freezeCount = FREEZEWAIT;
                            MediaPlayer.Stop();
                            //MediaPlayer.Play(rs_bm);
                            finishSe.Play();
                            winner = -1;
                            sc_m = 0;
                            for (int i = 0; i < c_list.Count; i++)
                            {
                                if (c_list[i].Score > sc_m)
                                {
                                    sc_m = c_list[i].Score;
                                    winner = i;
                                }
                                else if (c_list[i].Score == sc_m)
                                {
                                    winner = -1;
                                }
                            }
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Enter) && inter == INTERVAL)
                    {
                        /*start = false;
                        end = false;
                        freezeCount = FREEZEWAIT;
                        for (int i = 0; i < c_list.Count; i++)
                        {
                            c_list[i].initialize(false);
                            c_list[i].Score = 0;
                        }
                        it_list.RemoveRange(0, it_list.Count);
                        co_nm = Color.White;
                        inter = 0;*/
                        InitializeGame();
                        MediaPlayer.Stop();
                        MediaPlayer.Play(tl_bm);
                    }
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Enter) && inter == INTERVAL)
            {
                start = true;
                tm = 0f;
                inter = 0;
                MediaPlayer.Stop();
                //MediaPlayer.Play(pl_bm);
                te_se.Play();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && inter == INTERVAL)
            {
                graphics.ToggleFullScreen();
                inter = 0;
            }
            if (inter < INTERVAL)
                inter++;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            int window_width = graphics.PreferredBackBufferWidth;
            int window_height = graphics.PreferredBackBufferHeight;
            int window_center_x = window_width / 2;
            int v_margin_center = v_margin / 2;
            int nm_split_width = nm.Width / 10;
            int time_font_width = 40;
            int time_font_height = 57;
            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (start)
            {
                spriteBatch.Draw(bk, new Rectangle(0, 0, window_width, window_height), Color.White);
                if (end && freezeCount == 0)
                {
                    try
                    {
                        spriteBatch.Draw(rs[winner], new Rectangle(0, 0, window_width, window_height), Color.White);
                    }
                    catch
                    {
                        spriteBatch.Draw(rs[2], new Rectangle(0, 0, window_width, window_height), Color.White);
                    }
                    spriteBatch.Draw(endMessage, new Rectangle(window_center_x - 300, 10, 600, 50), Color.White);
                    Color c = Color.White;
                    for (int i = 0; i < c_list.Count; i++)
                    {
                        if (i == winner)
                        {
                            c = Color.Blue;
                        }
                        else
                        {
                            c = Color.White;
                        }
                        spriteBatch.Draw(playerImage[i], new Rectangle(50, v_margin + 350 + 50 * i, 250, 35), c);
                        spriteBatch.DrawString(spriteFont1, c_list[i].Score.ToString(),
                            new Vector2(350, v_margin + 340 + 50 * i), c);
                    }
                }
                else
                {
                    for (int i = 0; i < its_number.Count; i++)//アイテムゾーン描画
                    {
                        spriteBatch.Draw(it[its_number[i]],
                            new Rectangle(window_center_x - pn_size / 2, v_margin + pn_size * i, pn_size, pn_size), Color.White);
                    }
                    for (int i = 0; i < it_list.Count; i++)//ストックアイテム描画
                    {
                        it_list[i].onDraw();
                    }
                    for (int i = 0; i < c_list.Count; i++)//カーソル描画
                    {
                        c_list[i].onDraw();
                    }
                    spriteBatch.Draw(timeImage, new Rectangle(window_center_x - 50, v_margin_center - 50, 100, 40), Color.White);
                    spriteBatch.Draw(nm, new Rectangle(window_center_x - 70, v_margin_center, time_font_width, time_font_height),
                    new Rectangle((int)((tm_up - tm) / MINUTE) * nm_split_width, 0, nm_split_width, nm.Height), co_nm);
                    spriteBatch.DrawString(spriteFont1, ":",
                        new Vector2(window_center_x - 30, v_margin_center), Color.White);
                    spriteBatch.Draw(nm, new Rectangle(window_center_x - 10, v_margin_center, time_font_width, time_font_height),
                    new Rectangle((int)((tm_up - tm) % MINUTE) / 10 * nm_split_width, 0, nm_split_width, nm.Height), co_nm);
                    spriteBatch.Draw(nm, new Rectangle(window_center_x + 25, v_margin_center, time_font_width, time_font_height),
                    new Rectangle((int)((tm_up - tm) % 10) * nm_split_width, 0, nm_split_width, nm.Height), co_nm);
                    if (freezeCount > 0)
                    {
                        if (end)
                        {
                            spriteBatch.Draw(finishMessage,
                                new Rectangle(window_center_x - 300, window_height / 2 - 40, 600, 80), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(readyGoMessage,
                                new Rectangle(window_center_x - 300, window_height / 2 - 40, 600, 80), Color.White);
                        }
                    }
                }
            }
            else
            {
                spriteBatch.Draw(tl, new Rectangle(0, 0, window_width, window_height), Color.White);
                spriteBatch.Draw(startMessage, new Rectangle(window_center_x - 150, window_height / 2 + 200, 300, 50), Color.White);
            }
            base.Draw(gameTime);
            spriteBatch.End();
        }

        void InitializeGame()
        {
            seed = Environment.TickCount;
            rnd = new Random(seed);
            rnd = new Random(seed + rnd.Next(0, 100));
            it_list = new List<Item>();
            its_number = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                its_number.Add(ItemReset());
            }
            p1_it_count = 0;
            //パネルセット初期化
            panel_data1 = new Panel[3, 4];
            panel_data2 = new Panel[3, 4];
            for (int i = 0; i < panel_data1.GetLength(0); i++)
                for (int j = 0; j < panel_data1.GetLength(1); j++)
                    panel_data1[i, j] = new Panel(p_list[rnd.Next(0, 4)], rnd.Next(0, 4));
            for (int i = 0; i < panel_data2.GetLength(0); i++)
                for (int j = 0; j < panel_data2.GetLength(1); j++)
                    panel_data2[i, j] = new Panel(p_list[rnd.Next(0, 4)], rnd.Next(0, 4));
            c_list = new List<Cursor>();
            c_list.Add(new Cursor("Player1", cs, playerImage[0], scoreImage, wk, ar, Vector2.Zero, panel_data1, new Vector2(h_margin, v_margin),
                new Keys[6] { Keys.W, Keys.D, Keys.S, Keys.A, Keys.Q, Keys.E }, oc, 1, cr_se1, pz_se, it_se, fd_se));
            c_list.Add(new Cursor("Player2", cs, playerImage[1], scoreImage, wk, ar, Vector2.Zero, panel_data2,
                new Vector2(window_width - h_margin - pn_size * panel_data2.GetLength(0), v_margin),
                new Keys[6] { Keys.NumPad8, Keys.NumPad6, Keys.NumPad5, Keys.NumPad4, Keys.NumPad7, Keys.NumPad9 }, sq, 3, cr_se2, pz_se, it_se, fd_se));
            start = false;
            end = false;
            inter = 0;
            MediaPlayer.IsRepeating = true;
            clm = false;
            co_nm = Color.White;
            freezeCount = FREEZEWAIT;
        }

        /// <summary>
        /// 中央アイテム欄のアイテムを配置しなおす
        /// ゲームが進むほどダイナミックなアイテムが出るように
        /// </summary>
        /// <returns></returns>
        public static int ItemReset()
        {
            int itemNo = 0;
            double dynamicBase = 0.2;
            double dynamiRate = 0.5;
            if (rnd.NextDouble()< dynamicBase + ((TIME_LIMIT - tm) / TIME_LIMIT) * dynamiRate)
            {
                if(rnd.NextDouble()<0.6)
                {
                    itemNo = (int)itemList.スピードダウン;
                }
                else
                {
                    itemNo = (int)itemList.スピードアップ;
                }
            }
            else
            {
                if (rnd.NextDouble() < 0.3)
                {
                    itemNo = (int)itemList.十字パネル増加;
                }
                else if(rnd.NextDouble()<0.5)
                {
                    itemNo = (int)itemList.パネル消去;
                }
                else
                {
                    itemNo = (int)itemList.パネルシャッフル;
                }
            }
            return itemNo;
        }
    }
}
