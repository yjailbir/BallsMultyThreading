using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace shariki
{
    //Изначально "игра" на паузе
    //Чтобы запустить/поставить на паузу, нажми пробел
    //Чтобы заспавнить очередной шарик, кликни мышкой. Он появится в случайном месте со случайной скоростью
    //Можно сначала натыкать много шариков, а потом нажать пробел и смотреть, как они себя ведут
    public partial class Form1 : Form
    {
        //Переменная, которая отвечает за паузу/воспроизведение работы
        bool pause = true;
        List<Ball> balls = new List<Ball>(); //Массив шариков
        List<Thread> threads = new List<Thread>(); //Массив потоков (каждый i-й поток для i-го шарика)
        public class Ball 
        {
            private int x;
            private int y;
            private int width;
            private int height;
            private Color color;
            private int dx; //Скорость
            private int dy; //Скорость
            private List<Color> colors = (new Color[] { Color.White, Color.Red, Color.Green, Color.Yellow, Color.Orange, Color.Pink, Color.Purple, Color.Blue }).ToList(); //Шарики могут быть только этих цветов. .toList() - переводит массив в объект List
            public Ball()
            {
                Random random = new Random();
                x = random.Next(10, 600); //Выбираем рандомныу координаты
                y = random.Next(10, 400);
                width = 10;
                height = 10;
                color = colors[random.Next(colors.Count)]; //Выбираем рандомный цвет шарика
                dx = random.Next(-50, 50); //Выбираем рандомнную скорость
                dy = dx; //Скорости уменьшаются равномерно, т.к. по обеим осям они равны
            }
            public int X //Всё, что ниже - свойства
            {
                set { x = value; }
                get { return x; }
            }

            public int Y
            {
                set { y = value; }
                get { return y; }
            }
            public int WIDTH
            {
                set { width = value; }
                get { return width; }
            }

            public int HEIGHT
            {
                set { height = value; }
                get { return height; }
            }
            public int DX
            {
                set { dx = value; }
                get { return dx; }
            }

            public int DY
            {
                set { dy = value; }
                get { return dy; }
            }
            public Color COLOR
            {
                set { color = value; }
                get { return color; }
            }

        }
        //Ниже - логика игры
        public Form1()
        {
            InitializeComponent();
        }
        private void newBall() //Функция, создающая очередной шарик, и забрасывающая его в массив
        {
            Ball ball = new Ball();
            balls.Add(ball);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Click(object sender, EventArgs e) //При клике мы создаём новый поток выполнения newBall(), запускаем его и забрасываем в массив потоков
        {
            Thread thread = new Thread(newBall); //Создаём поток, выполняющий newBall()
            threads.Add(thread);
            thread.Start();
        }

        private void Form1_Paint(object sender, PaintEventArgs e) //При обновлении окна рисуем каждый шарик по его координатам и цвету (Рисует кадры, анимацию при каждом тике таймера)
        {
                Graphics graphics = CreateGraphics();
                graphics.Clear(Color.Black);
                foreach (Ball ball in balls) //Перебираем массив шариков, и рисуем каждый из них
                 graphics.FillEllipse(new SolidBrush(ball.COLOR), ball.X, ball.Y, ball.WIDTH, ball.HEIGHT);
        }

        private void timer_Tick(object sender, EventArgs e) //Каждый тик таймера(50 мс) считаем характеристики(координаты и скорость) каждого шарика
        {
            if(!pause)
            {
                int i;
                for (i = 0; i < balls.Count; i++)
                {
                    if (balls[i].DX == 0) //Если шарик остановился
                    {
                        threads[i].Join();
                        threads[i].Interrupt();
                        //Поток закрыт, но шар остаётся на поле. Если нужно удалить шар после остановки - расскомментируй строчку ниже
                        //balls.Remove(balls[i]); 
                        continue;
                    }
                    if (balls[i].DX > 0) //Уменьшаем скорость по иннерции
                    {
                        balls[i].DX -= 1;
                        balls[i].DY -= 1;
                    }
                    else //То же самое, но для шариков, катящихся в другую сторону
                    {
                        balls[i].DX += 1;
                        balls[i].DY += 1;
                    }
                    if (balls[i].X < 0 || balls[i].X > 800) //Следим, чтобы шарики не вылетели за поле
                        balls[i].DX *= -1;
                    if (balls[i].Y < 0 || balls[i].Y > 400)
                        balls[i].DY *= -1;
                    balls[i].X += balls[i].DX; //Присваиваем шарикам новую скорость
                    balls[i].Y += balls[i].DY;
                    bool tmp = true; //Хранит информацию о наличии движущихся шариков (Е - предикат) (Изначально допускаем, что все шарики остановились)
                    foreach (Ball ball in balls) //Проверяем, есть ли ещё в наличии движущиеся шарики
                    {
                        if(ball.DX != 0)
                        {
                            tmp = false;
                            break;
                        }
                    }
                    if (tmp) //Даем знать пользователю, есть ли такие шарики(потоки)
                        Text = "Все действующие потоки, кроме основного закрыты";
                    else
                        Text = "Есть не закрытые потоки помимо основного";
                }
            }
            Invalidate();
        }
        //Важная штука от мерцания со StackOferwlow (Двойная буферизация)
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
                return handleParam;
            }
        }
        //По нажатию пробела пауза/продолжение
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) //После точки клавиша, на которой висит функция
                pause = !pause;
        }
    }
}
