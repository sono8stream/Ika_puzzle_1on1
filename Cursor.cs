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
    public class Cursor
    {
        #region プロパティ
        string nm;
        public string Name
        {
            get { return nm; }
        }
        Texture2D im;
        Texture2D ar_im;
        Texture2D wk;
        Texture2D scoreImage;
        Texture2D nameImage;
        public Texture2D NmaeImage
        {
            get { return nameImage; }
        }
        Vector2 pos;
        Vector2 vel;//移動変化量
        int glx;//ゴールになるx座標
        int number;//カーソルの番号
        const int DEFAULT_SPEED = 8;
        public int DefaultSpeed
        {
            get { return DEFAULT_SPEED; }
        }
        int sp;//移動スピード
        public int Speed
        {
            get { return sp; }
            set { sp = value; }
        }
        int spSub;
        public int SpeedSub
        {
            get { return spSub; }
            set { spSub = value; }
        }
        int mv_count;//移動ウェイト
        public int moveCount
        {
            get { return mv_count; }
            set { mv_count = value; }
        }
        int inter;//選択間隔
        int se_count;//選択ウェイト
        Vector2 dire;//移動方向
        Panel[,] p_data;//操作するパネルのデータ
        bool[,] vsflag;//踏破したかどうか、移動の都度生き物に送る
        float[,] vsanm;//黒く塗りつぶすアニメ
        Vector2 pn_pos;//パネルセットの描画始点、左上基準
        public Vector2 Panel_Pos
        {
            get { return pn_pos; }
        }
        Keys[] k_set;
        bool slc;//選択中かどうか
        Panel pn_sub;//選択したパネルを一時保存
        bool[] mable;//移動可能性:上、右、下、左
        int cr_dr_f;//生き物の進行方向
        Vector2 cr_pos_f;//はじめの座標
        public List<Creature> cr_data;
        bool prc;//パズル完成？

        Texture2D[] cr_im;//生き物画像
        bool[] cr_mable;//移動可能方向
        int sc;//スコア
        public int Score
        {
            get { return sc; }
            set { sc = value; }
        }
        float rt;//コンボ倍率
        float fd;//フェード用処理
        bool gnr;//パネル生成中？
        SoundEffect mv_se;//効果音
        SoundEffect ch_se;
        SoundEffect it_se;
        SoundEffect fd_se;
        Vector2 nm_mv1, nm_mv2;//入れ替えるパネル番号確保
        int ch_sp;//パネル入れ替え速度
        int ch_cn;//カウント
        int[] t;//ターゲットの方向単位
        bool it_process;//アイテムを起動させたかどうか
        public bool Item_process
        {
            get { return it_process; }
            set { it_process = value; }
        }
        public bool isSpUp, isSpDown;
        #endregion
        public Cursor(string name, Texture2D image,Texture2D nameImage,Texture2D scoreImage, Texture2D waku_image, Texture2D arrow_image, Vector2 position, Panel[,] panel_data, Vector2 panel_pos, Keys[] key_set,
            Texture2D[] creature_image, int creature_direction, SoundEffect move_se, SoundEffect change_se, SoundEffect item_se, SoundEffect fade_se)
        {
            nm = name;
            wk = waku_image;
            im = image;
            this.nameImage = nameImage;
            this.scoreImage = scoreImage;
            ar_im = arrow_image;
            pos = position;
            vel = Vector2.Zero;
            sp = DEFAULT_SPEED;
            spSub = sp;
            mv_count = sp + 1;
            inter = 10;
            se_count = 0;
            p_data = panel_data;
            vsflag = new bool[p_data.GetLength(0), p_data.GetLength(1)];
            vsanm = new float[p_data.GetLength(0), p_data.GetLength(1)];
            for (int i = 0; i < vsflag.GetLength(0) * vsflag.GetLength(1); i++)
            {
                vsflag[i % vsflag.GetLength(0), i / vsflag.GetLength(0)] = false;
                vsanm[i % vsflag.GetLength(0), i / vsflag.GetLength(0)] = 1.00f;
            }
            pn_pos = panel_pos;
            k_set = key_set;
            dire = Vector2.Zero;
            slc = false;
            mable = new bool[4] { false, true, true, false };
            cr_im = creature_image;
            cr_mable = new bool[4] { false, false, false, false };
            cr_dr_f = creature_direction;
            glx = 0;
            int vcrx = 0;
            switch (cr_dr_f)
            {
                case 1:
                    vcrx = -Game1.pn_size;
                    glx = Game1.pn_size * panel_data.GetLength(0);
                    number = 0;
                    break;
                case 3:
                    vcrx = Game1.pn_size * panel_data.GetLength(0);
                    glx = -Game1.pn_size;
                    number = 1;
                    break;
            }
            cr_pos_f = new Vector2(vcrx, Game1.rnd.Next(0, panel_data.GetLength(1)) * Game1.pn_size);
            prc = false;
            mv_se = move_se;
            ch_se = change_se;
            it_se = item_se;
            fd_se = fade_se;
            cr_data = new List<Creature>();
            cr_data.Add(new Creature(cr_im, cr_pos_f, pn_pos, glx, cr_dr_f, spSub, 2, p_data, it_se, fd_se, isSpUp, isSpDown));
            sc = 0;
            rt = 1f;
            fd = 1f;
            gnr = true;
            ch_sp = 8;
            ch_cn = -1;
            t = new int[2] { 0, 0 };
            it_process = true;
        }

        public void onUpdate()
        {
            if (prc)
            {
                #region 実行
                for (int i = 0; i < cr_data.Count; i++)
                    switch (cr_data[i].onUpdate(ref vsflag, ref sc, ref rt))
                    {
                        case -1://消滅
                            cr_data.RemoveAt(i);
                            i--;
                            break;
                        case 3://増殖
                            cr_mable = cr_data[i].Movable;

                            for (int j = 0; j < 4; j++)
                                if (cr_mable[j])
                                    cr_data.Add(new Creature(cr_im, cr_data[i].Position, pn_pos, glx, j, spSub, 1, p_data, it_se, fd_se,
                                        isSpUp, isSpDown, true, 1f));
                            cr_data.RemoveAt(i);
                            i--;
                            break;
                    }
                for (int i = 0; i < vsanm.GetLength(0) * vsanm.GetLength(1); i++)
                    if (vsflag[i % vsflag.GetLength(0), i / vsflag.GetLength(0)] && vsanm[i % vsflag.GetLength(0), i / vsflag.GetLength(0)] > 0)
                    {
                        vsanm[i % vsflag.GetLength(0), i / vsflag.GetLength(0)] -= 0.02f;
                    }
                if (cr_data.Count == 0)//パズル初期化
                {
                    initialize();
                }
                #endregion
            }
            else
            {
                #region パズル操作
                if (mv_count == sp + 1)
                {
                    /*if (!it_process)
                    {*/
                        Item_Handle(true, 0);
                        Item_Handle(false, 0);
                    //}
                    Item_Handle(false, 2);
                    if (!slc)
                    {
                        if (se_count == inter)
                        {
                            //移動
                            dire = Vector2.Zero;
                            vel = Vector2.Zero;
                            if (Keyboard.GetState().IsKeyDown(k_set[0]) && mable[0])
                                dire = new Vector2(0, -Game1.pn_size);
                            if (Keyboard.GetState().IsKeyDown(k_set[1]) && mable[1])
                                dire = new Vector2(Game1.pn_size, 0);
                            if (Keyboard.GetState().IsKeyDown(k_set[2]) && mable[2])
                                dire = new Vector2(0, Game1.pn_size);
                            if (Keyboard.GetState().IsKeyDown(k_set[3]) && mable[3])
                                dire = new Vector2(-Game1.pn_size, 0);
                            if (dire != Vector2.Zero)
                            {
                                mv_count--;
                                mv_se.Play();
                            }
                            //選択
                            if (Keyboard.GetState().IsKeyDown(k_set[4]))
                            {
                                slc = true;
                                se_count = 0;
                                pn_sub = new Panel(p_data[(int)pos.X / Game1.pn_size, (int)pos.Y / Game1.pn_size], -1);
                            }
                            if (Keyboard.GetState().IsKeyDown(k_set[5]))
                            {
                                se_count = 0;
                                for (int i = 0; i < cr_data.Count; i++)
                                    cr_data[i].Mv = true;
                                prc = true;
                                Item_Handle(true, 1);//イベント有効化
                                //cr_data[0].Move_speed = sp;
                                cr_data[0].Move_speed = spSub;
                            }
                        }
                    }
                    else//入れ替え
                    {
                        if (ch_cn == -1)
                        {
                            nm_mv1 = pos;
                            if (Keyboard.GetState().IsKeyDown(k_set[0]) && mable[0] && slc)
                                t[1] = -1;
                            if (Keyboard.GetState().IsKeyDown(k_set[1]) && mable[1] && slc)
                                t[0] = 1;
                            if (Keyboard.GetState().IsKeyDown(k_set[2]) && mable[2] && slc)
                                t[1] = 1;
                            if (Keyboard.GetState().IsKeyDown(k_set[3]) && mable[3] && slc)
                                t[0] = -1;
                            if (Keyboard.GetState().IsKeyDown(k_set[4]) && se_count == inter)
                            {
                                slc = false;
                                se_count = 0;
                                return;
                            }
                            if (t[0] != 0 || t[1] != 0)
                            {
                                nm_mv2 = nm_mv1 + new Vector2(t[0], t[1]) * Game1.pn_size;
                                ch_cn = 0;
                            }
                        }
                        else if (change(ref nm_mv1, ref nm_mv2, ref ch_cn, /*ch_*/sp, t))
                        {
                            ch_cn = -1;
                            slc = false;
                            t[0] = 0;
                            t[1] = 0;
                        }
                    }
                    if (se_count < inter)
                        se_count++;
                }
                else
                {
                    mv_count--;
                    vel = dire * (sp - mv_count) / sp;
                    if (mv_count == 0)
                    {
                        pos += vel;
                        vel = Vector2.Zero;
                        mv_count = sp + 1;
                        move_judge();
                    }
                }
                #endregion
                for (int i = 0; i < cr_data.Count; i++)
                    cr_data[i].onUpdate(ref vsflag, ref sc, ref rt);
            }
        }

        public void onDraw()
        {
            int s;
            if (cr_dr_f == 1)
                s = 11;
            else
                s = 2;
            Game1.spriteBatch.Draw(wk,
                new Rectangle((int)pn_pos.X - s, (int)pn_pos.Y - 10, Game1.pn_size * p_data.GetLength(0) + 12, Game1.pn_size * p_data.GetLength(1) + 20), Color.White);
            Game1.spriteBatch.Draw(Game1.p_list[4].im, new Rectangle((int)pn_pos.X, (int)pn_pos.Y, Game1.pn_size * p_data.GetLength(0), Game1.pn_size * p_data.GetLength(1)), null,
               Color.White * fd);
            Rectangle re = new Rectangle(0, 0, Game1.pn_size, Game1.pn_size);
            for (int i = 0; i < p_data.GetLength(0); i++)//パネル描画
                for (int j = 0; j < p_data.GetLength(1); j++)
                {
                    if (i == (int)(nm_mv1.X / Game1.pn_size) && j == (int)(nm_mv1.Y / Game1.pn_size)&&ch_cn!=-1)
                    {
                        re.X = (int)(pn_pos.X + nm_mv1.X) + Game1.pn_size / 2;
                        re.Y = (int)(pn_pos.Y + nm_mv1.Y) + Game1.pn_size / 2;
                    }
                    else if (i == (int)(nm_mv2.X / Game1.pn_size) && j == (int)(nm_mv2.Y / Game1.pn_size) && ch_cn != -1)
                    {
                        re.X = (int)(pn_pos.X + nm_mv2.X) + Game1.pn_size / 2;
                        re.Y = (int)(pn_pos.Y + nm_mv2.Y) + Game1.pn_size / 2;
                    }
                    else
                    {
                        re.X = (int)pn_pos.X + Game1.pn_size / 2 + i * Game1.pn_size;
                        re.Y = (int)pn_pos.Y + Game1.pn_size / 2 + j * Game1.pn_size;
                    }
                    Game1.spriteBatch.Draw(p_data[i, j].im,
                        re, null, new Color(vsanm[i, j], vsanm[i, j], vsanm[i, j], 255 * fd),
                        (float)(Math.PI * p_data[i, j].dr / 2), new Vector2(p_data[i, j].im.Width / 2, p_data[i, j].im.Height / 2), SpriteEffects.None, 0f);
                }
            Game1.spriteBatch.Draw(im,
                new Rectangle((int)(pos.X + vel.X + pn_pos.X) - 5, (int)(pos.Y + vel.Y + pn_pos.Y) - 5, Game1.pn_size + 10, Game1.pn_size + 10), Color.White);//カーソル描画
            if (slc && ch_cn == -1)
                for (int i = 0; i < 4; i++)
                    if (mable[i])
                        Game1.spriteBatch.Draw(ar_im, new Rectangle((int)(pos.X + pn_pos.X) + Game1.pn_size / 2, (int)(pos.Y + pn_pos.Y) + Game1.pn_size / 2, Game1.pn_size, Game1.pn_size),
                            null, Color.White, (float)(Math.PI * i / 2), new Vector2(ar_im.Width / 2, ar_im.Height * 1.5f), SpriteEffects.None, 0f);
            for (int i = 0; i < cr_data.Count; i++)
                cr_data[i].onDraw();
            Game1.spriteBatch.Draw(scoreImage, new Rectangle((int)pn_pos.X + 20, (int)pn_pos.Y - 100, 120, 40), Color.White);
            Game1.spriteBatch.DrawString(Game1.spriteFont1,sc.ToString(), pn_pos + new Vector2(250, -100), Color.SkyBlue);
            Game1.spriteBatch.DrawString(Game1.spriteFont1, t[0].ToString(), pn_pos + new Vector2(0, -150), Color.SkyBlue);
            Game1.spriteBatch.DrawString(Game1.spriteFont1, t[1].ToString(), pn_pos + new Vector2(50, -150), Color.SkyBlue);
            Game1.spriteBatch.Draw(nameImage, new Rectangle((int)pn_pos.X + 20, (int)pn_pos.Y - 50, 180, 40), Color.White);
        }

        void move_judge()
        {
            for (int i = 0; i < 4; i++)
                mable[i] = true;
            if (pos.Y == 0)
                mable[0] = false;
            if (pos.X == (p_data.GetLength(0) - 1) * Game1.pn_size)
                mable[1] = false;
            if (pos.Y == (p_data.GetLength(1) - 1) * Game1.pn_size)
                mable[2] = false;
            if (pos.X == 0)
                mable[3] = false;
        }

        /// <summary>
        /// パネル入れ替え
        /// </summary>
        /// <param name="tg"></param>
        bool change(ref Vector2 m1, ref Vector2 m2, ref int count, int speed, params int[] tg)
        {
            m1 += new Vector2(tg[0], tg[1]) * Game1.pn_size / speed;
            pos = m1;
            m2 -= new Vector2(tg[0], tg[1]) * Game1.pn_size / speed;
            count++;
            if (count == speed)
            {
                m1 = Adjust(m1);
                m2 = Adjust(m2);
                ch_se.Play();
                count = -1;
                p_data[(int)pos.X / Game1.pn_size - tg[0], (int)pos.Y / Game1.pn_size - tg[1]] = new Panel(p_data[(int)pos.X / Game1.pn_size, (int)pos.Y / Game1.pn_size], -1);
                p_data[(int)pos.X / Game1.pn_size, (int)pos.Y / Game1.pn_size] = new Panel(pn_sub, -1);
                move_judge();
                return true;
            }
            return false;
        }

        Vector2 Adjust(Vector2 position)
            {
            position -= Vector2.One;
            position /= Game1.pn_size;
            position += Vector2.One;
            position *= Game1.pn_size;
            return position;
        }

        public void initialize(bool k = true)
        {
            if (k)
            {
                if (gnr)
                {
                    fd -= 0.1f;
                    if (fd <= 0f)
                        gnr = false;
                }
                else
                {
                    if (fd <= 0f)
                    {
                        p_data = new Panel[3, 4];
                        for (int i = 0; i < p_data.GetLength(0); i++)
                            for (int j = 0; j < p_data.GetLength(1); j++)
                            {
                                p_data[i, j] = new Panel(Game1.p_list[Game1.rnd.Next(0, 4)], Game1.rnd.Next(0, 4));
                                vsflag[i, j] = false;
                                vsanm[i, j] = 1.00f;
                            }
                        Item_Handle(true, 2);
                        Item_Handle(false, 2);
                        rt = 1f;
                        fd = 0.1f;
                    }
                    else
                    {
                        fd += 0.1f;
                        if (fd >= 1f)
                        {
                            prc = false;
                            gnr = true;
                            cr_pos_f.Y = Game1.rnd.Next(0, p_data.GetLength(1)) * Game1.pn_size;
                            cr_data.Add(new Creature(cr_im, cr_pos_f, pn_pos, glx, cr_dr_f, spSub, 2, p_data, it_se, fd_se, isSpUp, isSpDown));
                        }
                    }
                }
            }
            else
            {
                p_data = new Panel[3, 4];
                for (int i = 0; i < p_data.GetLength(0); i++)
                    for (int j = 0; j < p_data.GetLength(1); j++)
                    {
                        p_data[i, j] = new Panel(Game1.p_list[Game1.rnd.Next(0, 4)], Game1.rnd.Next(0, 4));
                        vsflag[i, j] = false;
                        vsanm[i, j] = 1.00f;
                    }
                cr_pos_f.Y = Game1.rnd.Next(0, p_data.GetLength(1)) * Game1.pn_size;
                cr_data.Add(new Creature(cr_im, cr_pos_f, pn_pos, glx, cr_dr_f, spSub, 2, p_data, it_se, fd_se, isSpUp, isSpDown));
                prc = false;
                gnr = true;
            }
        }

        /// <summary>
        /// 番号一致のものに条件を与える
        /// 実際のアイテム起動処理はItemクラスのActuateで
        /// </summary>
        /// <param name="key"></param>
        public void Item_Handle(bool enable, int key)
        {
            if (enable)//有効化？
            {
                for (int i = 0; i < Game1.it_list.Count; i++)
                {
                    if (Game1.it_list[i].Cursor_number == number)
                    {
                        Game1.it_list[i].Actuate(key);
                    }
                }
            }
            else//破棄？
            {
                for (int i = 0; i < Game1.it_list.Count; i++)
                {
                    if (Game1.it_list[i].Cursor_number == number && Game1.it_list[i].Effect_Count >= Game1.it_list[i].Effect_Span)
                    {
                        Game1.it_list[i].End = true;
                        Game1.it_list[i].Run();
                        if (Game1.it_list[i].Cursor_number == 0)
                        {
                            Game1.p1_it_count--;
                        }
                        Game1.it_list.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// パネル書換え
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="p"></param>
        public void Panel_Change(int x,int y,Panel p)
        {
            p_data[x, y] = p;
        }

        public Texture2D CheckPanelImage(int x, int y)
        {
            return p_data[x, y].im;
        }
    }
}
