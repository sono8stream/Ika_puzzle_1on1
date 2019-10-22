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
    /// いきもの
    /// </summary>
    public class Creature
    {
        #region プロパティ
        Texture2D[] im;
        int size;
        Vector2 pos;
        Vector2 pn_pos;
        int gl_x;
        SoundEffect fd_se;//消えるときの音
        public Vector2 Position
        {
            get { return pos; }
        }
        int am_count;//アニメ用
        int am_sp;//アニメ間隔
        float fd;//フェード用
        bool fd_jud;//t:出現,f:消滅
        bool mv;//動いているかどうか
        public bool Mv
        {
            get { return mv; }
            set { mv = value; }
        }
        int dr;//移動方向
        Vector2 vel;//移動方向と大きさ、2次元
        int mv_sp;//移動スピード
        public int Move_speed
        {
            get { return mv_sp; }
            set { mv_sp = value*5/2; }
        }
        int mv_count;
        bool[] mable;//動けるか判定
        public bool[] Movable
        {
            get { return mable; }
        }
        Panel[,] pnl_data;
        int counter;
        Panel p1, p2;
        SoundEffect it_se;
        public bool isSpeedUp;//スピードアップエフェクトの実行判定
        public bool isSpeedDown;//スピードダウン
        int animCount = 0;
        #endregion
        public Creature(Texture2D[] image, Vector2 position, Vector2 panel_pos, int goal_x, int direction,int move_speed,
            int mode, Panel[,] panel_data, SoundEffect item_se,SoundEffect fadeout_se,bool isSpeedUp,bool isSpeedDown, bool moving = false, float al = 0f)
        {
            im = image;
            size = 100;
            pos = position;
            pn_pos = panel_pos;
            gl_x = goal_x;
            am_count = 0;
            am_sp = 20;
            mv = moving;
            dr = direction;
            mv_sp = move_speed*5/2;
            pnl_data = panel_data;
            vel = Vector2.Zero;
            mv_count = 0;
            fd = al;
            fd_jud = true;
            mable = new bool[] { true, true, true, true };
            counter = mode;//どんな移動か、-1:消滅、0:消滅中、1:移動実行中、2:移動先調査、3:増殖
            it_se = item_se;
            fd_se = fadeout_se;
            this.isSpeedUp = isSpeedUp;
            this.isSpeedDown = isSpeedDown;
        }
        public int onUpdate(ref bool[,] visit_flag, ref int score, ref float ration)
        {
            if (am_count < am_sp * 2 - 1)
                am_count++;
            else
                am_count = 0;
            if (fd_jud && fd < 1.0f)
                fd += 0.1f;
            else
                fd_jud = false;
            //移動処理
            if (mv)//移動命令が下ったら
                switch (counter)
                {
                    case 0://消滅中
                        fd -= 0.1f;
                        if (fd <= 0)
                            counter = -1;
                        break;
                    case 1://移動
                        if (mv_count == 0)
                        {
                            vel = Vector2.Zero;
                            try
                            {
                                if (!visit_flag[(int)(pos.X + DtoV(dr, 1).X * mv_sp) / Game1.pn_size, (int)(pos.Y + DtoV(dr, 1).Y * mv_sp) / Game1.pn_size])
                                {
                                    visit_flag[(int)(pos.X + DtoV(dr, 1).X * mv_sp) / Game1.pn_size, (int)(pos.Y + DtoV(dr, 1).Y * mv_sp) / Game1.pn_size] = true;
                                    mv_count++;
                                }
                                else
                                {
                                    counter = 0;
                                }
                            }
                            catch
                            {
                                mv_count++;
                            }
                        }
                        else
                        {
                            mv_count++;
                            vel = DtoV(dr, mv_count);
                            if (mv_count == mv_sp)
                            {
                                pos += vel;
                                vel = Vector2.Zero;
                                mv_count = 0;
                                counter = 2;
                                if (pos.X == gl_x)
                                {
                                    counter = 0;
                                    score += (int)(100 * ration);
                                    ration += 0.2f;
                                    it_se.Play();
                                    int it_number = Game1.ItemReset();//中央アイテム欄のセット
                                    int pl = 0;
                                    if (dr == 1)
                                    {
                                        pl = 0;
                                    }
                                    else if (dr == 3)
                                    {
                                        pl = 1;
                                    }
                                    Game1.it_list.Add(new Item(Game1.it[Game1.its_number[(int)(pos.Y / Game1.pn_size)]],
                                        Game1.its_number[(int)(pos.Y / Game1.pn_size)], pl));
                                    Game1.its_number[(int)(pos.Y / Game1.pn_size)] = it_number;
                                }
                            }
                        }
                        break;
                    case 2://移動判定
                        counter = 0;
                        bool vl;//移動判定するかどうか
                        int dr_s = dr;//方向格納
                        try
                        {
                            p1 = pnl_data[(int)(pos.X / Game1.pn_size), (int)(pos.Y / Game1.pn_size)];
                        }
                        catch
                        {
                            p1 = new Panel(im[0], new bool[4] { true, true, true, true });
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (i != (dr + 2) % 4)
                            {
                                vl = false;
                                Vector2 direction = DtoV(i, mv_sp);
                                try
                                {
                                    p2 = pnl_data[(int)(pos.X + direction.X ) / Game1.pn_size, (int)(pos.Y + direction.Y) / Game1.pn_size];
                                    vl = visit_flag[(int)(pos.X + direction.X) / Game1.pn_size, (int)(pos.Y + direction.Y) / Game1.pn_size];
                                }
                                catch//前方にパネルがない
                                {
                                    if (pos.X + direction.X == gl_x)
                                    {
                                        p2 = new Panel(im[0], new bool[4] { true, true, true, true });
                                    }
                                    else
                                    {
                                        p2 = new Panel(im[0], new bool[4] { false, false, false, false });
                                        //vl = true;
                                    }
                                }
                                if (!vl)
                                {
                                    mable[i] = p1.ma[i] & p2.ma[(i + 2) % 4];
                                    if (mable[i])
                                    {
                                        counter++;
                                        dr_s = i;
                                        am_sp = 10;
                                        am_count = 0;
                                    }
                                }
                            }
                        }
                        if (counter >= 2)
                        {
                            counter = 3;
                        }
                        if (counter == 1)
                        {
                            dr = dr_s;
                        }
                        if (counter == 0)
                        {
                            fd_se.Play();
                        }
                        break;
                }
            return counter;
        }

        public void onDraw()
        {
            Game1.spriteBatch.Draw(im[am_count / am_sp],
                new Rectangle((int)(pos.X +vel.X+ pn_pos.X) + (Game1.pn_size - size) / 2, (int)(pos.Y +vel.Y+ pn_pos.Y) + (Game1.pn_size - size) / 2, size, size), Color.White * fd);

            /*spriteBatch.DrawString(spriteFont1, counter.ToString(), new Vector2(0, -100) + pn_pos, Color.White);
            spriteBatch.DrawString(spriteFont1, dr.ToString(), new Vector2(0, -200) + pn_pos, Color.White);
            spriteBatch.DrawString(spriteFont1, new Vector2((int)(pos.X + DtoV(dr).X * mv_sp) / Game1.pn_size, (int)(pos.Y + DtoV(dr).Y * mv_sp) / Game1.pn_size).ToString(), new Vector2(0, -250) + pn_pos, Color.White);
            spriteBatch.DrawString(spriteFont1, pos.ToString(), new Vector2(0, -300) + pn_pos, Color.White);*/
            /*for (int i = 0; i < 4; i++)
            {
                try
                {
                    Game1.spriteBatch.DrawString(Game1.spriteFont1, mable[i].ToString(), new Vector2(pn_pos.X + 300, 30 * i), Color.White);
                    Game1.spriteBatch.DrawString(Game1.spriteFont1,
                        new Vector2((int)(pos.X + DtoV(i, mv_sp).X) / Game1.pn_size, (int)(pos.Y + DtoV(i, mv_sp).Y) / Game1.pn_size).ToString(), new Vector2(pn_pos.X + 50, 900 + 30 * i), Color.White);
                    Game1.spriteBatch.DrawString(Game1.spriteFont1, p1.ma[i].ToString(), new Vector2(pn_pos.X+100, 30 * i), Color.White);
                    Game1.spriteBatch.DrawString(Game1.spriteFont1, p2.ma[i].ToString(), new Vector2(pn_pos.X+200, 30 * i), Color.White);
                }
                catch
                {
                }
            }*/
            if (isSpeedUp || isSpeedDown)
            {
                int animPattern = 10;
                int animCountLimit = 50;
                Texture2D t;
                if(isSpeedUp)
                {
                    t = Game1.spUpEffect;
                }
                else
                {
                    t = Game1.spDownEffect;
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                Game1.spriteBatch.Draw(t,
                    new Rectangle((int)(pos.X + vel.X + pn_pos.X) + (Game1.pn_size - size)/ 2, (int)(pos.Y + vel.Y + pn_pos.Y) + (Game1.pn_size - size) / 2, size, size),
                    new Rectangle((animCount * animPattern / animCountLimit)*(t.Width / animPattern ), 0, t.Width / animPattern, t.Height), Color.White);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin();
                animCount++;
                if(animCount==animCountLimit)
                {
                    animCount = 0;
                    //isSpeedUp = false;
                    //isSpeedDown = false;
                }
            }
        }
        /// <summary>
        /// 移動方向判定
        /// </summary>
        /// <param name="dire"></param>
        /// <returns></returns>
        public Vector2 DtoV(int dire,int count)
        {
            Vector2 v = Vector2.Zero;
            switch (dire)
            {
                case 0:
                    v.Y = -Game1.pn_size * count / mv_sp;
                    break;
                case 1:
                    v.X = Game1.pn_size * count / mv_sp;
                    break;
                case 2:
                    v.Y = Game1.pn_size * count / mv_sp;
                    break;
                case 3:
                    v.X = -Game1.pn_size * count / mv_sp;
                    break;
            }
            return v;
        }

    }
}
