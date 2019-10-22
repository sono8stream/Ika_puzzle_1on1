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
    public class Item
    {
        Texture2D im;
        const int PATTERN = 30;
        int ani_count;
        Vector2 pos;
        int pr_key;//効果実行タイミング識別値
        public enum Process_Key
        {
            常時実行 = 0, パズル完成時, パズル生成時
        }
        bool valid;//効果実行中？
        int ef_sp;//効果時間
        public int Effect_Span
        {
            get { return ef_sp; }
        }
        int ef_cn;//効果時間カウント
        public int Effect_Count
        {
            get { return ef_cn; }
        }
        bool en;//効果終了？
        public bool End
        {
            get { return en; }
            set { en = value; }
        }
        int cr;//効果対象カーソル番号
        public int Cursor_number
        {
            get { return cr; }
        }
        /// <summary>
        /// 効果発動処理
        /// </summary>
        public delegate void Item_Effect();
        public Item_Effect Effect;
        bool enable;
        public Item(Texture2D image, int effect_number/*効果番号*/, int player_number)
        {
            im = image;
            ani_count = 0;
            ef_cn = -1;
            en = false;
            valid = false;
            /*Effect = delegate ()
            {
                if (!valid)
                {
                    return;
                }
                else
                {
                    ani_count++;
                    if (ani_count >= PATTERN)
                    {
                        ani_count = 0;
                    }
                }
            };
            Effect += delegate ()
            {
                /*if (ef_cn >= ef_sp)
                {
                    en = true;
                    ef_cn = 0;
                }
                if (ef_cn < ef_sp)
                {
                    ef_cn++;
                }
            };*/
            Event_judge(effect_number, player_number);
        }
        public void onDraw()
        {
            Game1.spriteBatch.Draw(im, new Rectangle((int)pos.X, (int)pos.Y, Game1.pn_size-50, Game1.pn_size-50), Color.White);
            //Game1.spriteBatch.DrawString(Game1.spriteFont1,ef_cn.ToString(), pos, Color.White);
            if (valid)
            {
                int w = Game1.it_ef.Width / 3;
                int h = Game1.it_ef.Height / 10;
                Game1.spriteBatch.Draw(Game1.it_ef, new Rectangle((int)pos.X - 80, (int)pos.Y - 60, Game1.pn_size + 100, Game1.pn_size + 100),
                    new Rectangle(w * (ani_count % 3), h * (ani_count / 3), w, h), Color.White*0.7f);
                //Game1.spriteBatch.DrawString(Game1.spriteFont1, ef_cn.ToString(), pos, Color.White);
            }
        }

        public void Run()
        {
            if (!valid)
            {
                return;
            }
            else
            {
                ani_count++;
                if (ani_count >= PATTERN)
                {
                    ani_count = 0;
                }
                if (ef_cn < ef_sp)
                {
                    ef_cn++;
                }
                Effect();
            }
        }

        /// <summary>
        /// 付加するイベントの判定
        /// 対象カーソル、効果時間のセットも
        /// </summary>
        void Event_judge(int ef_number, int p_number)
        {
            int t_number = 0;
            switch (ef_number)
            {
                case 0://パネル強化
                    pr_key = (int)Process_Key.パズル生成時;
                    Effect = new Item_Effect(Super_Panel);
                    ef_sp = 0;
                    t_number = p_number;
                    break;
                case 1://パネル弱化
                    pr_key = (int)Process_Key.パズル生成時;
                    Effect = new Item_Effect(Lose_Panel);
                    ef_sp = 0;
                    t_number = Math.Abs(p_number - 1);
                    break;
                case 2://パネル初期化
                    pr_key = (int)Process_Key.常時実行;
                    Effect = new Item_Effect(Shuffle_Panel);
                    ef_sp = 0;
                    t_number = Math.Abs(p_number - 1);
                    break;
                case 3://移動速度増加
                    pr_key = (int)Process_Key.常時実行;
                    Effect = new Item_Effect(Speed_Up);
                    ef_sp = 600;
                    t_number = p_number;
                    Game1.c_list[t_number].Item_process = false;
                    break;
                case 4://移動速度減少
                    pr_key = (int)Process_Key.常時実行;
                    Effect = new Item_Effect(Speed_Down);
                    ef_sp = 600;
                    t_number = Math.Abs(p_number - 1);
                    Game1.c_list[t_number].Item_process = false;
                    break;
            }
            cr = t_number;
            pos = new Vector2(0, Game1.v_margin + Game1.pn_size * 4);
            if (t_number == 0)
            {
                pos.X = Game1.c_list[cr].Panel_Pos.X + (Game1.pn_size / 2) * Game1.p1_it_count;
                Game1.p1_it_count++;
            }
            else /*if (t_number==-1)*/
            {
                pos.X = Game1.c_list[cr].Panel_Pos.X + (Game1.pn_size / 2) * (Game1.it_list.Count - Game1.p1_it_count);
            }
        }
        /// <summary>
        /// 効果開始処理
        /// </summary>
        /// <param name="key"></param>
        public void Actuate(int key)
        {
            if (key == pr_key && !valid)
            {
                valid = true;
            }
        }

        void Speed_Up()
        {
            if (ef_cn == 0)
            {
                if (Game1.c_list[cr].SpeedSub > 3)
                {
                    Game1.c_list[cr].SpeedSub -= 3;
                    Game1.c_list[cr].isSpUp = true;
                    for (int i=0;i<Game1.c_list[cr].cr_data.Count;i++)
                    {
                        Game1.c_list[cr].cr_data[i].isSpeedUp = true;
                    }
                    enable = true;
                }
                else
                {
                    enable = false;
                }
            }
            if (en && enable == true)
            {
                Game1.c_list[cr].SpeedSub += 3;
                if(Game1.c_list[cr].SpeedSub==Game1.c_list[cr].DefaultSpeed)
                {
                    Game1.c_list[cr].isSpUp = false;
                    for (int i = 0; i < Game1.c_list[cr].cr_data.Count; i++)
                    {
                        Game1.c_list[cr].cr_data[i].isSpeedUp = false;
                    }
                }
            }
        }
        void Speed_Down()
        {
            if (ef_cn == 0)
            {
                Game1.c_list[cr].Speed += 2;
                Game1.c_list[cr].SpeedSub += 2;
                Game1.c_list[cr].moveCount = Game1.c_list[cr].Speed;
                Game1.c_list[cr].isSpDown = true;
                for (int i = 0; i < Game1.c_list[cr].cr_data.Count; i++)
                {
                    Game1.c_list[cr].cr_data[i].isSpeedDown = true;
                }
            }
            if (en)
            {
                Game1.c_list[cr].Speed -= 2;
                Game1.c_list[cr].SpeedSub -= 2;
                Game1.c_list[cr].moveCount = Game1.c_list[cr].Speed;
                if (Game1.c_list[cr].SpeedSub == Game1.c_list[cr].DefaultSpeed)
                {
                    Game1.c_list[cr].isSpDown = false;
                    for (int i = 0; i < Game1.c_list[cr].cr_data.Count; i++)
                    {
                        Game1.c_list[cr].cr_data[i].isSpeedDown = false;
                    }
                }
            }
        }
        /// <summary>
        /// 4枚のパネルをランダムに十字に
        /// </summary>
        void Super_Panel()
        {
            if (/*ef_cn == 0*/en)
            {
                /*int l;//変更パネルの列
                if (cr == 0)
                {
                    l = 2;
                }
                else
                {
                    l = 0;
                }*/
                int loopCount = 0;
                Panel p = new Panel(Game1.p_list[3], 0);
                for (int i = 0; i < 3; i++)
                {
                    while (i < 5)
                    {
                        int x = Game1.rnd.Next(0, 3);
                        int y = Game1.rnd.Next(0, 4);
                        if (p.im != Game1.c_list[cr].CheckPanelImage(x, y))
                        {
                            Game1.c_list[cr].Panel_Change(x, y, p);
                            break;
                        }
                        loopCount++;
                        if(loopCount==12)
                        {
                            loopCount = 0;
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 3列目パネルを無に
        /// </summary>
        void Lose_Panel()
        {
            if (ef_cn == 0)
            {
                /*int l;//変更パネルの列
                if (cr == 0)
                {
                    l = 2;
                }
                else
                {
                    l = 0;
                }*/
                int loopCount = 0;
                Panel p = new Panel(Game1.p_list[4], 0);
                for (int i = 0; i < 2; i++)
                {
                    while (i < 5)
                    {
                        int x = Game1.rnd.Next(0, 3);
                        int y = Game1.rnd.Next(0, 4);
                        if (p.im != Game1.c_list[cr].CheckPanelImage(x, y))
                        {
                            Game1.c_list[cr].Panel_Change(x, y, p);
                            break;
                        }
                        loopCount++;
                        if (loopCount == 12)
                        {
                            loopCount = 0;
                            break;
                        }
                    }
                }
                //Game1.c_list[cr].Panel_Change(l, i, new Panel(Game1.p_list[4], 0));
            }
        }
        /// <summary>
        /// すべてのパネルを初期化
        /// </summary>
        void Shuffle_Panel()
        {
            if (/*ef_cn == 0*/en)
            {
                for (int i = 0; i < 3 * 4; i++)
                {
                    Game1.c_list[cr].Panel_Change(i % 3, i / 3, new Panel(Game1.p_list[Game1.rnd.Next(0, 4)],
                        Game1.rnd.Next(0, 4)));
                }
            }
        }
    }
}
