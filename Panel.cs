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
    /// パネル
    /// </summary>
    public class Panel
    {
        public Texture2D im;//画像
        public bool[] ma;//移動方向判定
        public int dr;//方向、*90度、描画用
        //原本の生成
        public Panel(Texture2D image, bool[] mvable)
        {
            im = image;
            ma = mvable;
            dr = 0;
        }
        //回転物の生成
        public Panel(Panel origin, int dire/*回転回数*/)
        {
            im = origin.im;
            ma = new bool[4];
            if (dire == -1)
            {
                dr = origin.dr;
                for (int i = 0; i < ma.GetLength(0); i++)
                    ma[i] = origin.ma[i];
            }
            else
            {
                dr = dire;
                for (int i = 0; i < ma.GetLength(0); i++)
                    ma[(i + dire) % 4] = origin.ma[i];
            }
        }
    }
}
