using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Round_Robin
{
    public partial class Form1 : Form
    {
        struct Proceso
        {
            public int _tiempo_total, tiempo_restante, tiempo_respuesta;

            public Proceso(int tiempo_total)
            {
                _tiempo_total = tiempo_restante =  tiempo_total;
                tiempo_respuesta = -1;
            }
        }
        int cant_procesos;
        List<Proceso> procesos = new List<Proceso>();
        Random random = new Random(65481981);
        int tiempo = 0;
        bool terminado = true, nuevo_proceso = false;
        Thread hilo;
        public Form1()
        {
            InitializeComponent();
            chart1.Palette = ChartColorPalette.None;
            chart1.Titles.Add("Procesos");
            chart1.Series["Procesos"].Color = Color.Aqua;
            chart1.Series["Procesos"].BorderColor = Color.Black;
        }

        int BuscarSJF()
        {
            int pos_menor = 0;
            for (int i = 1; i < procesos.Count; ++i)
                if (procesos[pos_menor].tiempo_restante > procesos[i].tiempo_restante) pos_menor = i;
            return pos_menor;
        }
        void simulacion()
        {
            _tiempo.Text = "0";
            int min = 100, max = 0, suma = 0, prom, proc_visitados = 0, pos, procesos_terminados = 0;

            while (procesos_terminados != procesos.Count)
            {
                
                pos = BuscarSJF();
                Proceso aux = procesos[pos];    //devuelve la posicion del proceso con menor tiempo restante
               
                if (aux.tiempo_respuesta < 0) //si es primera vez en ejecucion se toman sus tiempos para los calculos
                {
                    if (tiempo <= min)
                    {
                        min = tiempo;
                    }
                    if (tiempo > max)
                    {
                        max = tiempo;
                    }
                    suma += tiempo;
                    aux.tiempo_respuesta = tiempo;
                    proc_visitados++;
                }
                chart1.Series["Procesos"].Points[pos].Color = Color.Red;
                while(aux.tiempo_restante > 0)
                {
                    Thread.Sleep(200);
                    aux.tiempo_restante--;
                    _tiempo.Text = "" + ++tiempo;
                    chart1.Series["Procesos"].Points[pos].YValues[0] = aux.tiempo_restante;
                    chart1.Series["Procesos"].Points[pos].Color = Color.Red;
                    chart1.Series["Procesos"].Points[pos].BorderColor = Color.Black;
                    _ocupado.Text = "" + (aux._tiempo_total-aux.tiempo_restante);
                }
                if (nuevo_proceso)          //si la ejecucion del proceso termino por un nuevo proceso, el proceso act se pone en listos y actualizo sus datos
                {
                    chart1.Series["Procesos"].Points[pos].YValues[0] = aux.tiempo_restante;
                    chart1.Series["Procesos"].Points[pos].Color = Color.Aqua;
                    chart1.Series["Procesos"].Points[pos].BorderColor = Color.Black;
                    nuevo_proceso = false;
                }
                else                       //en caso contrario el proceso debio terminar, por lo que se actualizan sus datos
                {
                    procesos_terminados++;
                    //el tiempo restante se pone en 1000000 para no volver a ser elegido por la funcion BuscarSJF
                    aux.tiempo_restante = 1000000;
                }
                procesos[pos] = aux;

                //calcular los datos de tiempo de respuesta
                prom = suma / 7;
                min_tr.Text = "" + min;
                max_tr.Text = "" + max;
                med_tr.Text = "" + prom;
                desves_tr.Text = "En espera de finalizar procesos";

                //cuando los se tienen los tiempos de respuesta de todos los procesos en la lista
                //se calculan los resultados
                if (proc_visitados == procesos.Count)
                {
                    prom = suma / 7;
                    min_tr.Text = "" + min;
                    max_tr.Text = "" + max;
                    med_tr.Text = "" + prom;
                    prom = suma / 7;
                    desves_tr.Text = "0";
                    int temp = 0;
                    foreach(Proceso p in procesos)
                    {
                        temp += (int)(Math.Pow(p.tiempo_respuesta - prom, 2));
                    }
                    min_tr.Text = "" + min;
                    max_tr.Text = "" + max;
                    med_tr.Text = "" + prom;
                    desves_tr.Text = (Math.Sqrt(temp / 7)).ToString();
                }
            }
            terminado = true;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (!terminado)
            {
                MessageBox.Show("Para comenzar una nueva simulacion, primero debe presionar el boton terminar", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else LimpiarTodo();
            cant_procesos = (int)numericUpDown1.Value;
            if (cant_procesos <= 0)
            {
                MessageBox.Show("El numero de procesos para comenzar debe ser mayor a 0", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int min=100, max=0, suma=0;
           
            //dar valores random a cada proceso y calcular el apartado turn around
            for(int i= 0; i<cant_procesos; ++i) {
                int x = random.Next(1, 32);
                procesos.Add(new Proceso(x));
                chart1.Series["Procesos"].Points.AddXY(i+1, x);
                
                if (procesos[i]._tiempo_total<= min)
                {
                    min = procesos[i]._tiempo_total;
                }
                if (procesos[i]._tiempo_total > max)
                {
                    max = procesos[i]._tiempo_total;
                }
                suma += procesos[i]._tiempo_total;

            }
            int prom = suma/8;
            int aux=0;
            for(int i=0; i< procesos.Count(); ++i)
            {
                aux += (int)(Math.Pow(procesos[i]._tiempo_total - prom, 2));
            }
            //**********************************************************************
            d3.Text = (Math.Sqrt(aux / 7)).ToString();
            a3.Text = min.ToString();
            b3.Text = (suma / 8).ToString();
            c3.Text = max.ToString();

            _idle.Text = (procesos.Count()-1).ToString();
            if (terminado)
            {
                hilo = new Thread(simulacion);
                CheckForIllegalCrossThreadCalls = false;
                hilo.Start();
                terminado = false;
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!terminado)
            {
                hilo.Abort();
                terminado = true;
                MessageBox.Show("Simulacion terminada", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        void LimpiarTodo()
        {
            procesos.Clear();
            chart1.Series.Clear();
            terminado = true;

            chart1.Palette = ChartColorPalette.None;
            chart1.Series.Add("Procesos");

            min_tr.Text = "";
            max_tr.Text = "";
            med_tr.Text = "";
            desves_tr.Text = "";

            a3.Text = "";
            b3.Text = "";
            c3.Text = "";
            d3.Text = "";

            _tiempo.Text = "";
            _ocupado.Text = "";
            _idle.Text = "";
            tiempo = 0;

            chart1.Series["Procesos"].Color = Color.Aqua;
            chart1.Series["Procesos"].BorderColor = Color.Black;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!terminado)
                hilo.Abort();
            return;
        }

        private void btn_agregar_proc_Click(object sender, EventArgs e)
        {
            if (!terminado)
            {
                //crear un proceso nuevo y agregarlo a la lista
                int temp = random.Next(1, 32);
                procesos.Add(new Proceso(temp));
                //agregar el proceso al chart
                chart1.Series["Procesos"].Points.AddXY(procesos.Count, temp);
                nuevo_proceso = true;
            }
            else MessageBox.Show("Para agregar nuevos procesos, la simulacion debe estar activa", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
