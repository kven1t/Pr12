using System.Windows.Forms;

namespace WinFormsApp8
{
    public partial class Form1 : Form
    {
        private int numShar = 100; // текущее количество шаров
        private static double radius = 12.0; // радиус шара
        private double dia2n2 = 4.0 * radius * radius; // квадрат диаметра (для определения соприкосновения)
        private double damp = 0.00008; // затухание (коэффициент трения)
        private double maxSpeed = 8.0;
        private double polex, poley; // ширина, высота поля
        private List<TBall> balls = new List<TBall>();
        public Form1()
        {
            InitializeComponent();
            numericUpDown1.Value = numShar;
            numericUpDown2.Value = (int)radius;
            polex = pictureBox1.Width - 2;
            poley = pictureBox1.Height - 2;
            InitialBalls(ref balls);
        }
        private struct PointD
        {
            public double X;
            public double Y;
            public PointD(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
        private struct PointV
        {
            public double ugol;
            public double velo;
            public PointV(double ugol, double velo)
            {
                this.ugol = ugol;
                this.velo = velo;
            }
        }
        private class TBall
        {
            public PointD pos;
            public PointD spd;
            public PointV vct;
            public Color color;
            public void SetSpeed()
            {
                PointD p = spd;
                p.X = Math.Sin(vct.ugol) * vct.velo;
                p.Y = Math.Cos(vct.ugol) * vct.velo;
                spd = p;
            }
            public void SetVector()
            {
                PointV p = vct;
                p.ugol = Atn2(spd);
                p.velo = Hptn(spd);
                vct = p;
            }
            public void SetPosition()
            {
                PointD p = pos;
                p.X += spd.X;
                p.Y += spd.Y;
                pos = p;
            }
        }
        private static double Hptn(PointD p) => Math.Sqrt(p.X * p.X + p.Y * p.Y); // гипотенуза

        private static double Atn2(PointD p)
        {
            if (p.Y == 0.0)
            {
                if (p.X > 0.0) return 0.0;
                return Math.PI;
            }
            return Math.Atan2(p.X, p.Y);
        }
        private void Collision(ref List<TBall> mas)
        {
            double c_x, c_y; //компоненты вектора, соединяющего центры шаров
            double d1, d2; // квадраты расстояний
            double AC_scalar, BC_scalar; // Скалярные произведения векторов
            double Ap_v_x, Ap_v_y, At_v_x, At_v_y; // нормальные и тангенсальные скорости шаров
            double Bp_v_x, Bp_v_y, Bt_v_x, Bt_v_y;
            PointD spd = new PointD(); // для записи в массив
            // столкновения между шарами
            for (int i = 0; i < mas.Count - 1; i++)
            {
                for (int j = i + 1; j < mas.Count; j++)
                {
                    // расстояние на следующем шаге
                    c_x = (mas[j].pos.X + mas[j].spd.X) - (mas[i].pos.X + mas[i].spd.X);
                    c_y = (mas[j].pos.Y + mas[j].spd.Y) - (mas[i].pos.Y + mas[i].spd.Y);
                    d2 = c_x * c_x + c_y * c_y;
                    // расстояние на текущий момент
                    c_x = mas[j].pos.X - mas[i].pos.X;
                    c_y = mas[j].pos.Y - mas[i].pos.Y;
                    d1 = c_x * c_x + c_y * c_y; // квадрат расстояния между центрами шаров
                    // если шары сближаются и расстояние между центрами меньше диаметра
                    if (d2 < d1 && d1 < dia2n2)
                    {
                        AC_scalar = mas[i].spd.X * c_x + mas[i].spd.Y * c_y;
                        BC_scalar = mas[j].spd.X * c_x + mas[j].spd.Y * c_y;
                        // разложение скорости шара 1 на нормальную и тангенсальную
                        Ap_v_x = c_x * AC_scalar / d1;
                        Ap_v_y = c_y * AC_scalar / d1;
                        At_v_x = mas[i].spd.X - Ap_v_x;
                        At_v_y = mas[i].spd.Y - Ap_v_y;
                        // разложение скорости шара 2 на нормальную и тангенсальную
                        Bp_v_x = c_x * BC_scalar / d1;
                        Bp_v_y = c_y * BC_scalar / d1;
                        Bt_v_x = mas[j].spd.X - Bp_v_x;
                        Bt_v_y = mas[j].spd.Y - Bp_v_y;
                        // шары обмениваются нормальными скоростями, тангенсальные остаются прежними
                        spd.X = Bp_v_x + At_v_x;
                        spd.Y = Bp_v_y + At_v_y;
                        mas[i].spd = spd;
                        spd.X = Ap_v_x + Bt_v_x;
                        spd.Y = Ap_v_y + Bt_v_y;
                        mas[j].spd = spd;
                    }
                }
            }
            // столкновения со стенками
            for (int i = 0; i < mas.Count; i++)
            {
                spd = mas[i].spd;
                if (mas[i].pos.X < radius && spd.X < 0) spd.X = -spd.X; // левый борт
                if (mas[i].pos.Y < radius && spd.Y < 0) spd.Y = -spd.Y; // верхний борт
                if (mas[i].pos.X > polex - radius && spd.X > 0) spd.X = -spd.X; // правый борт
                if (mas[i].pos.Y > poley - radius && spd.Y > 0) spd.Y = -spd.Y; // нижний борт
                mas[i].spd = spd;
                // пересчитаем угол и скорость
                mas[i].SetVector();
            }
        }
        private void Shag(ref List<TBall> mas)
        {
            PointV vct;
            foreach (TBall ball in mas)
            {
                // вычислим замедление скорости
                vct = ball.vct;
                vct.velo -= vct.velo * damp + damp;
                if (vct.velo < damp) vct.velo = 0.0;
                ball.vct = vct;
                // пересчитаем новые скорости по осям
                ball.SetSpeed();
                // вычислим новое положение
                ball.SetPosition();
            }
        }
        private void InitialBalls(ref List<TBall> mas)
        {
            Random rnd = new Random();
            for (int i = 0; i < numShar; i++)
            {
                TBall ball = new TBall
                {
                    pos = new PointD(polex * rnd.NextDouble(), poley * rnd.NextDouble()), // случайное положение
                    vct = new PointV(Math.PI * 2.0 * rnd.NextDouble(), maxSpeed * rnd.NextDouble()), // случаное направление, скорость
                    color = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)) // случайный цвет
                };
                ball.SetSpeed();
                mas.Add(ball);
            }
        }


        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            polex = pictureBox1.Width - 2;
            poley = pictureBox1.Height - 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Shag(ref balls);
            Collision(ref balls);
            pictureBox1.Image = DrawBalls(balls);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private Image DrawBalls(List<TBall> mas)
        {
            Bitmap bmp = new Bitmap((int)polex, (int)poley);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Teal);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                foreach (TBall ball in mas)
                {
                    g.FillEllipse(new SolidBrush(ball.color), (float)(ball.pos.X - radius), (float)(ball.pos.Y - radius), (float)(2 * radius), (float)(2 * radius));
                }
            }
            return bmp;
        }
    }
}
    
