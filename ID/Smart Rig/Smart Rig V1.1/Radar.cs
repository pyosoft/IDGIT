﻿using pyosoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WITS_CORE;

namespace Smart_Rig_V1._1
{
    public partial class Radar : MetroFramework.Forms.MetroForm
    {

        int width = 400, height = 400, hand = 193;
        double u; // in degree
        int cx, cy; //centro del circulo
        int x, y; //coordenadas
        int tx, ty, lim = 20;
        Bitmap bmp;
        Pen pGnL, pHMSE, pINFLUX, pNPT, p5, p6, pMano;

        Graphics g;
        //Timer t = new Timer();
        //private System.Threading.Timer Lector;
        Thread tr, trNPT;
        float valorActual = 1;
        string dataConexion = "C:\\Pyosoft\\Data.txt";

        string archivoConfigurador = "C:\\Pyosoft";
        string nombreConfiguradorRadar1 = "\\RadarConfiguracion1.txt";
        string nombreConfiguradorRadar2 = "\\RadarConfiguracion2.txt";

        string cadenaConexion = string.Empty;
        static string cadenaConexionMySql = "Server=pyosoftprb.cfsh1lwjn4sj.us-west-2.rds.amazonaws.com;Port=3306;Database=";
        string userNpass = "user=pyosoft;password=PY050FTDB;";
        string MysqlConexion = string.Empty;


        //TODO: PASAR DE VARIABLES CONSTANTES A ORIENTADO A OBJETOS
        double diametro;
        double diametroLasBoquillas;
        int numerodeBoquillas;
        double anguloSimetriaAxial;
        double longitudPotencialdelNucleo;
        double distanciadeBoquillasaldonfodelhueco;
        double k = 0.122;
        double densidadLodo = 11;
        double velocidadSalidaBoquillas = 211;
        double torqueMax;
        double limitemaximopresiondiferencialMotor;
        double caidadePresionsobrelaBroca;
        double velocidadRotacionMotor;
        double areadelasBoquillas;
        double torqueAplicadoEnLaBroca;
        double variableN;
        double ROPmaxLimpieza;

        //wits de logica a utilizar

        double wits0130;
        double wits0117;
        double wits0120;
        double wits0113;
        double wits0119;
        double wits0171;
        double wits0112;
        double wits0115;


        double ropa1;
        double costa1;
        double tripa1, tripa2;
        double limiteInteriorHMSE = 0, limiteExteriorHMSE = 0;
        double ecuacionAV;
        double ecuacionM;
        double ecuacionN;

        double limiteinteriorNPT;
        double limitesuperiorNPT;
        // VALORES NPT
        double tiempoNPT = 0;// Valor del tiempo de demora operacion no planeada
        int estadoNPT = 1;
        int tiempoEjecucionPerforacion1 = 0;
        double wits0112Anterior;

        //valores influx
        private double Wits0126Anterior = 0; //Canal 0127 anterior
        private double Wits0128Anterior = 0; //Canal 0127 anterior
        private double Wits0121Anterior = 0;//Canal 0121 anterior
        private double Wits0130Anterior = 0;//Canal 0130 anterior
        private double tasaPerdidaGanancia = 0;// variable para determinar tasa de perdida o ganacia
        double diferenciaWits0126Anterior = 0;
        int tiempoINFLUX;
        int tiempoAlarmaINFLUX2;
        int tiempoAlarmaINFLUX3;
        int tiempoAlarmaINFLUX4;
        int tiempoAlarmaAnteriorINFLUX1;
        int tiempoAlarmaAnteriorINFLUX2;
        int tiempoAlarmaAnteriorINFLUX3;
        int tiempoAlarmaAnteriorINFLUX4;
        int tiempoLOSS;
        double wits0121;
        double wits0126;
        double wits0128;
        double limiteInteriorINFLUX = 0;
        double limiteExteriorINFLUX = 0;

        //valores loss
        int tiempoAlarmaLOSS;
        int tiempoAlarmaLOSS2;
        int tiempoAlarmaAnteriorLOSS1;
        int tiempoAlarmaAnteriorLOSS2;
        double limiteInteriorLOSS = 0;
        double limiteExteriorLOSS = 0;



        //pipe move

        private double wits0110 = 0;//Canal correspondiente al 0110
        //private double Wits0113 = 0;//Canal correspondiente al 0113
        //private double Wits0117 = 0;//Canal correspondiente al 0113
        private double wits0108 = 0;//Canal correspondiente al 0108
        private double wits4781 = 0;//Canal correspondiente al Trip Speed
        private double wits4782 = 0;//Canal correspondiente al Overpull
        private double VelocidadLimiteBajando = 0;//asignado al textbox del configurador
        private double VelocidadLimiteSubiendo = 0;//asignado al textbox del configurador
        private double viaje = 0;
        private double wits0108Anterior = 0;




        public Radar()
        {
            InitializeComponent();
        }

        private void Radar_Load(object sender, EventArgs e)
        {
            if (File.Exists(dataConexion))
            {
                SpComunicacion.PortName = File.ReadLines(dataConexion).Skip(2).Take(1).First();
                SpComunicacion.BaudRate = int.Parse(File.ReadLines(dataConexion).Skip(3).Take(1).First());
                seteoDeVariables();

                //Crear Bitmap
                bmp = new Bitmap(width + 1, height + 1);

                //centro
                cx = width / 2;
                cy = height / 2;

                //backgroundWorker1.RunWorkerAsync();

                tr = new Thread(Timer_Tick);
                tr.Start();
                //Lector = new System.Threading.Timer(Timer_Tick, null, 0, 1000);
                trNPT = new Thread(timerGeneral_TickNPT);
                trNPT.Start();
                //INFLUX
                trNPT = new Thread(timerGeneral_TickINFLUX);
                trNPT.Start();
                //LOSS
                trNPT = new Thread(timerGeneral_TickLOSS);
                trNPT.Start();
                //pipe move
                trNPT = new Thread(timerGeneral_TickLOSS);
                trNPT.Start();
                

            }
            else
            {
                MessageBox.Show("El puerto de comunicacion no se encuentra configurado");
            }


        }

        public void seteoDeVariables()
        {
            string archivoALeer1 = archivoConfigurador + nombreConfiguradorRadar1;

            if (File.Exists(archivoALeer1))
            {
                string[] items = File.ReadAllLines(archivoALeer1);
                foreach (string itemAseleccionar in items)
                {
                    if (itemAseleccionar.Contains("DiametroBroca"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        diametro = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("DiametroBoquillas"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        diametroLasBoquillas = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("NumeroBoquillas"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        numerodeBoquillas = int.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("AnguloChorro"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        anguloSimetriaAxial = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("LongitudNucleo"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        longitudPotencialdelNucleo = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("DistanciaFondo"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        distanciadeBoquillasaldonfodelhueco = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("TorqueMotor"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        torqueMax = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("PresionMotor"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        limitemaximopresiondiferencialMotor = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("CaidaPresion"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        caidadePresionsobrelaBroca = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("VelocidadMotor"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        velocidadRotacionMotor = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("AreaBoquillas"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        areadelasBoquillas = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("TorqueBroca"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        torqueAplicadoEnLaBroca = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("VariableN"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        variableN = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("VelocidadBajando"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        VelocidadLimiteBajando = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("VelocidadSubiendo"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        VelocidadLimiteSubiendo = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("ROPmaxLimpieza"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        ROPmaxLimpieza = double.Parse(valorItem[1]);
                    }

                }
            }

            string archiALeer2 = archivoConfigurador + nombreConfiguradorRadar2;

            if (File.Exists(archiALeer2))
            {
                string[] items = File.ReadAllLines(archiALeer2);
                foreach (string itemAseleccionar in items)
                {
                    if (itemAseleccionar.Contains("ropa1"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        ropa1 = double.Parse(valorItem[1]);
                    }

                    if (itemAseleccionar.Contains("costa1"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        costa1 = double.Parse(valorItem[1]);
                    }
                    if (itemAseleccionar.Contains("tripa1"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        tripa1 = double.Parse(valorItem[1]);
                    }
                    if (itemAseleccionar.Contains("tripa2"))
                    {
                        String[] valorItem = itemAseleccionar.Split(new char[] { '_' });
                        tripa2 = double.Parse(valorItem[1]);
                    }

                }
            }
            //limites HMSE
            double Porcentaje = ropa1 * 0.3;
            
            limiteInteriorHMSE = ropa1 + Porcentaje;
            limiteExteriorHMSE = ropa1 - Porcentaje;

            //limites NPT
            double tiempoNPT = tripa1 * 0.3;
            limiteinteriorNPT = tripa1 + tiempoNPT;
            limitesuperiorNPT = tripa1;

            ecuacionAV = new ecuacionesRadar().ecuacionAV(diametro, diametroLasBoquillas, numerodeBoquillas);

            ecuacionM = new ecuacionesRadar().ecuacionM(anguloSimetriaAxial, longitudPotencialdelNucleo, diametroLasBoquillas, distanciadeBoquillasaldonfodelhueco);
            if (variableN.ToString() != "")
                ecuacionN = variableN;
            else
                ecuacionN = new ecuacionesRadar().ecuacionN(ecuacionAV, k, ecuacionM);

            //seccion de lectura de wits

            if (File.Exists(dataConexion))
            {
                string dataBaseName = File.ReadLines(dataConexion).Skip(1).Take(1).First();
                // lblBaseDatos.Text = dataBaseName;
                cadenaConexion = File.ReadLines(dataConexion).First();
                MysqlConexion = cadenaConexionMySql + dataBaseName + ";" + userNpass;
            }
        }

        private void EjecutarEcuaciones()
        {

            try
            {
                if (!SpComunicacion.IsOpen)
                    SpComunicacion.Open();
                string[] separadorItems = { "&&", "!!" };
                string[] stringSeparators = new string[] { "\r\n" };

                int contador = 0;
                Thread.Sleep(100);
                string datosSerial = "";
                //mensajeMostrar = true;
                //Thread.Sleep(250);
                datosSerial = SpComunicacion.ReadExisting();

                string[] valor1 = datosSerial.Split(separadorItems, StringSplitOptions.None);
                foreach (string l in valor1)
                {
                    if (l != "" && l.Length > 5)
                    {
                        Tuple<List<protocoloWits>, int> resultado = new AD_protocoloWits().FormateaWits(cadenaConexion, MysqlConexion, l);
                        //pintarElemento(resultado.Item1);

                        if (resultado.Item1 != null)
                        {
                            foreach (protocoloWits item in resultado.Item1)
                            {
                                if (item.WITitem == "0130")
                                {
                                    wits0130 = double.Parse(item.WITvalor);
                                }

                                if (item.WITitem == "0117")
                                {
                                    wits0117 = double.Parse(item.WITvalor);
                                }

                                if (item.WITitem == "0113")
                                {
                                    wits0113 = double.Parse(item.WITvalor);
                                }

                                if (item.WITitem == "0119")
                                {
                                    wits0119 = double.Parse(item.WITvalor);
                                }

                                if (item.WITitem == "0120")
                                {
                                    wits0120 = double.Parse(item.WITvalor);
                                }

                                if (item.WITitem == "0171")
                                {
                                    wits0171 = double.Parse(item.WITvalor);
                                }

                                if(item.WITitem == "0126")
                                {
                                    wits0126 = double.Parse(item.WITvalor);
                                }
                                if (item.WITitem == "0108")
                                {
                                    wits0108 = double.Parse(item.WITvalor);
                                }
                                if (item.WITitem == "0110")
                                {
                                    wits0110 = double.Parse(item.WITvalor);
                                }
                                if (item.WITitem == "0112")
                                {
                                    wits0112 = double.Parse(item.WITvalor);
                                }
                            }
                        }

                    }

                    Application.DoEvents();
                }

                if (!String.IsNullOrEmpty(wits0113.ToString()) && !String.IsNullOrEmpty(wits0117.ToString()) && !String.IsNullOrEmpty(wits0119.ToString()) && !String.IsNullOrEmpty(wits0120.ToString()) && !String.IsNullOrEmpty(wits0130.ToString()) && !String.IsNullOrEmpty(wits0171.ToString()))
                {
                    double ecuacionFJ = new ecuacionesRadar().ecuacionFJ(densidadLodo, wits0130, velocidadSalidaBoquillas);

                    double ecuacionWOBe = new ecuacionesRadar().ecuacionWOBe(wits0117, ecuacionFJ, ecuacionN);

                    double ecuacionHME = new ecuacionesRadar().ecuacionMSE(torqueMax, limitemaximopresiondiferencialMotor, wits0171, wits0119, velocidadRotacionMotor, wits0130, wits0120, diametro, wits0113, wits0117);

                    double ecuacionHMSE = new ecuacionesRadar().ecuacionHMSE(torqueMax, limitemaximopresiondiferencialMotor, ecuacionN, caidadePresionsobrelaBroca,
                                                                   wits0130, velocidadRotacionMotor, wits0120, areadelasBoquillas, wits0113, ecuacionWOBe, torqueAplicadoEnLaBroca,
                                                                   wits0119, wits0171);

                    BeginInvoke(new Action(() => txtResultadosEcuaciones.Text = ecuacionFJ.ToString() + "_" + ecuacionWOBe.ToString()), null);

                    BeginInvoke(new Action(() => txtMSE.Text = ecuacionHME.ToString()), null);

                    BeginInvoke(new Action(() => txtHMSE.Text = ecuacionHMSE.ToString()), null);

                    BeginInvoke(new Action(() => txtProfundidadActual.Text = wits0110.ToString()), null);

                    if (wits0108 == wits0110)
                    {
                        BeginInvoke(new Action(() => pbAnA.Image = Image.FromFile("C:\\Users\\Andre\\Desktop\\ID\\Smart Rig\\Smart Rig V1.1\\img\\yes.png")), null);
                    }
                    else
                    {
                        BeginInvoke(new Action(() => pbAnA.Image = Image.FromFile("C:\\Users\\Andre\\Desktop\\ID\\Smart Rig\\Smart Rig V1.1\\img\\no.png")), null);
                    }
                }

                contador += 1;
                if (contador == 2)
                {
                    SpComunicacion.DiscardInBuffer();
                    contador = 0;
                }

                datosSerial = "";

            }
            catch (Exception)
            {

                throw;
            }

        }

        private float[] pintaHMSE()
        {
            float valorX = 0;
            float valorY = 0;
            //el limite interior es el costoso, 30%+ exterior 30%-
            //ejemplo interior 260, exterior 140
            
            double CostoActual = 0;

            if (!String.IsNullOrEmpty(costa1.ToString()) && !String.IsNullOrEmpty(ropa1.ToString()) && !String.IsNullOrEmpty(wits0113.ToString()))
            {
                if (wits0113.ToString() != "0")
                {
                    CostoActual = (costa1 * ropa1) / wits0113;
                }
                double R1 = limiteInteriorHMSE - limiteExteriorHMSE;
                double R2 = wits0113 - limiteExteriorHMSE;

                double porcentaje1 = 100 - ((R2 * 100)/R1);

                if(porcentaje1 < 0) { porcentaje1 = 0; }
                if (porcentaje1 > 100) { porcentaje1 = 100; }

                double porcentajex = (porcentaje1 * 160) / 100;
                double porcentajey = (porcentaje1 * -101) / 100;

                valorX =  float.Parse(porcentajex.ToString());
                valorY =  float.Parse(porcentajey.ToString()); ;

                
            }
            BeginInvoke(new Action(() => txtCOST.Text = CostoActual.ToString()), null);
            BeginInvoke(new Action(() => txtROP.Text = wits0113.ToString()), null);

            float[] Resultado = { valorX, valorY };

            return Resultado;

        }

        private float pintarLOSS()
        {
            float valorY = 0;

            if (tiempoLOSS > 1 && tasaPerdidaGanancia < 0)
            {
                double R1 = limiteExteriorLOSS - limiteInteriorLOSS;
                double R2 = tasaPerdidaGanancia;

                double porcentaje1 = ((R2 * 100) / -1);

               // double porcentajex = (porcentaje1 * 150) / 100;
                double porcentajey = (porcentaje1 * -75) / 100;

                //valorX = float.Parse(porcentajex.ToString());
                valorY = float.Parse(porcentajey.ToString()); ;
            }

            float resultado = valorY;
            return resultado;
        }

        private float[] pintarINFLUX()
        {
            float valorX = 0;
            float valorY = 0;

            if (tiempoINFLUX > 1 && tasaPerdidaGanancia > 0)
            {
                double R1 = limiteExteriorINFLUX - limiteInteriorINFLUX;
                double R2 = tasaPerdidaGanancia;

                double porcentaje1 = ((R2 * 100) / 1);

                double porcentajex = (porcentaje1 * 62) / 100;
                double porcentajey = (porcentaje1 * 35) / 100;

                valorX = float.Parse(porcentajex.ToString());
                valorY = float.Parse(porcentajey.ToString()); ;
            }

            float[] resultado = { valorX, valorY };
            return resultado;
        }

        private float[] pintarNPT()
        {
            float valorX = 0;
            float valorY = 0;

            if (tiempoNPT != 0)
            {
                double R1 = limiteinteriorNPT - limitesuperiorNPT;
                double R2 = double.Parse(txtNPT.Text);

                double porcentaje1 = ((R2 * 100) / R1);

                if (porcentaje1 > 100) { porcentaje1 = 100; }

                double porcentajex = (porcentaje1 * 168) / 100;
                double porcentajey = (porcentaje1 * -96) / 100;

                valorX = float.Parse(porcentajex.ToString());
                valorY = float.Parse(porcentajey.ToString()); ;

            }

            float[] resultado = { valorX, valorY };
            return resultado;
        }

        private float pintarTIGHT()
        {
            float valorY = 0;

            if (tiempoLOSS > 1 && tasaPerdidaGanancia < 0)
            {
                //double R1 = limiteExteriorHMSE - limiteInteriorHMSE;
                double R2 = tasaPerdidaGanancia;

                double porcentaje1 = ((R2 * 100) / -1);

                // double porcentajex = (porcentaje1 * 150) / 100;
                double porcentajey = (porcentaje1 * 75) / 100;

                //valorX = float.Parse(porcentajex.ToString());
                valorY = float.Parse(porcentajey.ToString()); ;
            }

            float resultado = valorY;
            return resultado;
        }

        private float[] pintarPIPEMOVE()
        {
            float valorX = 0;
            float valorY = 0;

            if (!String.IsNullOrEmpty(VelocidadLimiteBajando.ToString()) && !String.IsNullOrEmpty(VelocidadLimiteBajando.ToString()) && !String.IsNullOrEmpty(wits4781.ToString()))
            {
                double R1 = (VelocidadLimiteBajando*8)/100 - VelocidadLimiteBajando;
                double R2 = wits4781 - VelocidadLimiteBajando;

                double porcentaje1 = 100 - ((R2 * 100) / R1);

                double porcentajex = (porcentaje1 * -62) / 100;
                double porcentajey = (porcentaje1 * -40) / 100;

                valorX = float.Parse(porcentajex.ToString());
                valorY = float.Parse(porcentajey.ToString()); ;


            }

            BeginInvoke(new Action(() => txtSpeed.Text = wits4781.ToString()), null);

            float[] Resultado = { valorX, valorY };

            return Resultado;

        }

        private void timerGeneral_TickNPT(object sender)
        {

            double tiempoEstimadoPerforacion1 = tripa1; //Valor ingresado por textbox NPT AFE
            double tiempoEjecucion = 0; // Valor real de ejecucion
            double tiempoConexionNPT = 0;
            int tiempoEjecucionMove1 = 0;
            int tiempoEstimadoMove1 = 0; //  de donde sale??
            int tiempoNPTMove1 = 0;
            int estadoFrague = 0;
            int tiempoEjecucionMove2 = 0;
            double tiempoEstimadoMove2 = tripa2; // de donde sale??
            int tiempoNPTMove2 = 0;
            int tiempoEjecucionPerforacion2 = 0;
            double tiempoEstimadoPerforacion2 = 0; //  de donde sale??
            int tiempoNPTPerforacion2 = 0;
            double tiempoConexionNPT2 = 0;
            int tiempoEjecucionMove3 = 0;
            int tiempoEstimadoMove3 = 0; //de donde sale??
            int tiempoNPTMove3 = 0;
            int tiempoEjecucionPerforacion3 = 0;
            int tiempoEstimadoPerforacion3 = 0; //de donde sale?
            int tiempoNPTPerforacion3 = 0;
            int tiempoConexionNPT3 = 0;
            int tiempoEjecucionTYC = 0;
            int tiempoEstimadoTYC = 0; //de donde sale??
            int tiempoNPTTYC = 0;

            while (true)
            {
                //drilling y total es el valor de tiempoNPT

                //tiempoEjecucion = tiempoEjecucion + 1; //Cada evento, cada minuto el tiempo de ejecucion aumenta
                //if (tiempoEjecucion > tiempoEstimado)// Se valida si el tiempo esta dentro de lo acordado
                //{
                //    tiempoNPT = tiempoNPT+1;//Se aumenta el tiempo no planeado

                //}
                //BeginInvoke(new Action(() => txtNPT.Text = tiempoNPT.ToString()), null);
                //BeginInvoke(new Action(() => txtDrilling.Text = tiempoNPT.ToString()), null);

                if (estadoNPT == 1)
                {
                    //Inicio Sección 1
                    if (wits0108 == wits0110)
                    {
                        tiempoEjecucionPerforacion1 = tiempoEjecucionPerforacion1 + 1;

                        if (tiempoEjecucionPerforacion1 > tiempoEstimadoPerforacion1)
                        {
                            tiempoNPT = tiempoNPT + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0)
                    {
                        tiempoConexionNPT = tiempoConexionNPT + 1;

                        if (tiempoConexionNPT <= 10)
                        {
                            tiempoEjecucionPerforacion1 = tiempoEjecucionPerforacion1 + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0) // Viaje Subiendo
                    {
                        if (tiempoConexionNPT > 10)
                        {
                            tiempoEjecucionMove1 = tiempoEjecucionMove1 + 1;
                            if (tiempoEjecucionMove1 > tiempoEstimadoMove1)
                            {
                                tiempoNPTMove1 = tiempoNPTMove1 + 1;
                            }
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0) // Viaje Bajando
                    {
                        if (tiempoConexionNPT > 10)
                        {
                            tiempoEjecucionMove1 = tiempoEjecucionMove1 + 1;
                            if (tiempoEjecucionMove1 > tiempoEstimadoMove1)
                            {
                                tiempoNPTMove1 = tiempoNPTMove1 + 1;
                            }
                            if (estadoNPT == 1)
                            {

                            }
                        }
                    }
                    if (wits0110 != wits0108 && wits0121 == 0 && wits0112 == wits0112Anterior && wits0115 < 60000)
                    {
                        tiempoEjecucionMove1 = tiempoEjecucionMove1 + 1;
                        if (tiempoEjecucionMove1 > tiempoEstimadoMove1)
                        {
                            tiempoNPTMove1 = tiempoNPTMove1 + 1;
                            estadoFrague = 2;
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0 && estadoFrague == 2) // Viaje Bajando luego del frague NPT cuenta para seccion 2
                    {
                        if (tiempoConexionNPT == 10)
                        {
                            tiempoEjecucionMove2 = tiempoEjecucionMove2 + 1;
                            if (tiempoEjecucionMove2 > tiempoEstimadoMove2)
                            {
                                tiempoNPTMove2 = tiempoNPTMove2 + 1;
                                estadoNPT = 2;
                                tiempoConexionNPT = 0;
                            }

                        }
                    }
                    wits0112Anterior = wits0112;
                }
                if (estadoNPT == 2)
                {
                    //Inicio Sección 2
                    if (wits0108 == wits0110 && estadoNPT == 2)
                    {
                        tiempoEjecucionPerforacion2 = tiempoEjecucionPerforacion2 + 1;

                        if (tiempoEjecucionPerforacion2 > tiempoEstimadoPerforacion2)
                        {
                            tiempoNPTPerforacion2 = tiempoNPTPerforacion2 + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0)
                    {
                        tiempoConexionNPT2 = tiempoConexionNPT2 + 1;

                        if (tiempoConexionNPT2 <= 10)
                        {
                            tiempoEjecucionPerforacion2 = tiempoEjecucionPerforacion2 + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0) // Viaje Subiendo
                    {
                        if (tiempoConexionNPT2 > 10)
                        {
                            tiempoEjecucionMove2 = tiempoEjecucionMove2 + 1;
                            if (tiempoEjecucionMove2 > tiempoEstimadoMove2)
                            {
                                tiempoNPTMove2 = tiempoNPTMove2 + 1;
                            }
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0) // Viaje Bajando
                    {
                        if (tiempoConexionNPT2 > 10)
                        {
                            tiempoEjecucionMove2 = tiempoEjecucionMove2 + 1;
                            if (tiempoEjecucionMove2 > tiempoEstimadoMove2)
                            {
                                tiempoNPTMove2 = tiempoNPTMove2 + 1;
                            }

                        }
                    }
                    if (wits0110 != wits0108 && wits0121 == 0 && wits0112 == wits0112Anterior && wits0115 < 60000)
                    {
                        tiempoEjecucionMove2 = tiempoEjecucionMove2 + 1;
                        if (tiempoEjecucionMove2 > tiempoEstimadoMove2)
                        {
                            tiempoNPTMove2 = tiempoNPTMove2 + 1;
                            estadoFrague = 3;
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0 && estadoFrague == 3) // Viaje Bajando luego del frague
                    {
                        if (tiempoConexionNPT2 == 10)
                        {
                            tiempoEjecucionMove3 = tiempoEjecucionMove3 + 1;
                            if (tiempoEjecucionMove3 > tiempoEstimadoMove3)
                            {
                                tiempoNPTMove3 = tiempoNPTMove3 + 1;
                                estadoNPT = 3;
                            }

                        }
                    }

                    wits0112Anterior = wits0112;
                }
                if (estadoNPT == 3)
                {
                    //Inicio Sección 3
                    if (wits0108 == wits0110 && estadoNPT == 3)
                    {
                        tiempoEjecucionPerforacion3 = tiempoEjecucionPerforacion3 + 1;

                        if (tiempoEjecucionPerforacion3 > tiempoEstimadoPerforacion3)
                        {
                            tiempoNPTPerforacion3 = tiempoNPTPerforacion3 + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0)
                    {
                        tiempoConexionNPT3 = tiempoConexionNPT3 + 1;

                        if (tiempoConexionNPT3 <= 10)
                        {
                            tiempoEjecucionPerforacion3 = tiempoEjecucionPerforacion3 + 1;
                        }
                    }
                    if (wits0108 < wits0110 && wits0113 == 0) // Viaje Subiendo
                    {
                        if (tiempoConexionNPT3 > 10)
                        {
                            tiempoEjecucionMove3 = tiempoEjecucionMove3 + 1;
                            if (tiempoEjecucionMove3 > tiempoEstimadoMove3)
                            {
                                tiempoNPTMove3 = tiempoNPTMove3 + 1;
                            }
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0) // Viaje Bajando
                    {
                        if (tiempoConexionNPT3 > 10)
                        {
                            tiempoEjecucionMove3 = tiempoEjecucionMove3 + 1;
                            if (tiempoEjecucionMove3 > tiempoEstimadoMove3)
                            {
                                tiempoNPTMove3 = tiempoNPTMove3 + 1;
                            }

                        }
                    }
                    if (wits0110 != wits0108 && wits0121 == 0 && wits0112 == wits0112Anterior && wits0115 < 60000)
                    {
                        tiempoEjecucionMove3 = tiempoEjecucionMove3 + 1;
                        if (tiempoEjecucionMove3 > tiempoEstimadoMove3)
                        {
                            tiempoNPTMove3 = tiempoNPTMove3 + 1;
                            estadoFrague = 4;
                        }
                    }
                    if (wits0108 > wits0110 && wits0113 == 0 && estadoFrague == 4) // Viaje Bajando luego del frague
                    {
                        if (tiempoConexionNPT2 == 10)
                        {
                            tiempoEjecucionMove3 = tiempoEjecucionMove3 + 1;
                            if (tiempoEjecucionMove3 > tiempoEstimadoMove3)
                            {
                                tiempoNPTMove3 = tiempoNPTMove3 + 1;

                            }

                        }
                    }
                    if (wits0108 == 0 && wits0110 == 0 && wits0113 == 0 && wits0115 == 0 && wits0112 == 0 && estadoFrague == 3) // Viaje Bajando luego del frague esto va en terminación y completamiento
                    {
                        estadoNPT = 4;
                    }

                    wits0112Anterior = wits0112;

                }
                if (estadoNPT == 4)
                {
                    tiempoEjecucionTYC = tiempoEjecucionTYC + 1;
                    if (tiempoEjecucionTYC > tiempoEstimadoTYC)
                    {
                        tiempoNPTTYC = tiempoNPTTYC + 1;
                    }
                }

                Thread.Sleep(1200);
            }
        }

        private void timerGeneral_TickINFLUX(object sender)
        {
            tiempoINFLUX = tiempoINFLUX + 1; // cada vez que el intervalo configurado en el timer desde el form aumenta un minuto cuando pasan 60000 ms
            if (diferenciaWits0126Anterior < wits0126)
            {
                double diferenciaWits0126 = Wits0126Anterior - wits0126;
                tasaPerdidaGanancia = ((diferenciaWits0126 + diferenciaWits0126Anterior) / tiempoINFLUX); // formula para determinar la tasa de perdida o ganancia
                double limitetasaGanancia = (tasaPerdidaGanancia + (tasaPerdidaGanancia * 0.05));
                // Primera situación disminución de presión.
                if (Wits0121Anterior > wits0121 && wits0130 == Wits0130Anterior)
                {
                    tiempoINFLUX = tiempoINFLUX + 1;
                    lblAlerta.Text = " SPP "; //Direccionado a textbox Alarma
                    lblAlarma.Text = "SPP " + tiempoINFLUX.ToString() + "minutos"; // Direccionado a textbox Alerta

                }
                if (Wits0126Anterior < wits0126 && Wits0128Anterior < wits0128 && wits0130 == Wits0130Anterior)
                {
                    if (Wits0121Anterior > wits0121 && wits0130 == Wits0130Anterior)
                    {
                        tiempoAlarmaINFLUX2 = tiempoAlarmaINFLUX2 + 1;
                        lblAlerta.Text = "INF"; //Direccionado a textbox Alarma
                        lblAlarma.Text = "Influx " + tiempoAlarmaINFLUX2.ToString() + "minutos"; // Direccionado a textbox Alerta
                    }
                    else
                    {
                        tiempoAlarmaINFLUX3 = tiempoAlarmaINFLUX3 + 1;
                        lblAlerta.Text = "AUM"; //Direccionado a textbox Alarma
                        lblAlarma.Text = "Aumento " + tiempoAlarmaINFLUX3.ToString() + "minutos"; // Direccionado a textbox Alerta
                    }
                }
                if (limitetasaGanancia > tasaPerdidaGanancia)
                {
                    tiempoAlarmaINFLUX4 = tiempoAlarmaINFLUX4 + 1;
                    lblAlerta.Text = "TVA"; //Direccionado a textbox Alarma
                    lblAlarma.Text = "TVA " + tiempoAlarmaINFLUX4.ToString() + "minutos"; // Direccionado a textbox Alerta
                }
                if (tiempoAlarmaAnteriorINFLUX1 == tiempoINFLUX)
                {
                    tiempoINFLUX = 0;
                }
                if (tiempoAlarmaAnteriorINFLUX2 == tiempoAlarmaINFLUX2)
                {
                    tiempoAlarmaINFLUX2 = 0;
                }

                if (tiempoAlarmaAnteriorINFLUX3 == tiempoAlarmaINFLUX3)
                {
                    tiempoAlarmaINFLUX3 = 0;
                }

                if (tiempoAlarmaAnteriorINFLUX4 == tiempoAlarmaINFLUX4)
                {
                    tiempoAlarmaINFLUX4 = 0;
                }


                diferenciaWits0126 = Wits0126Anterior - wits0126;
                diferenciaWits0126Anterior = diferenciaWits0126;
                tiempoAlarmaAnteriorINFLUX1 = tiempoINFLUX;
                tiempoAlarmaAnteriorINFLUX2 = tiempoAlarmaINFLUX2;
                tiempoAlarmaAnteriorINFLUX3 = tiempoAlarmaINFLUX3;
                tiempoAlarmaAnteriorINFLUX4 = tiempoAlarmaINFLUX4;
                Wits0121Anterior = wits0121;
                Wits0126Anterior = wits0126;
                Wits0128Anterior = wits0128;
                Wits0130Anterior = wits0130;
            }

            limiteInteriorINFLUX = (tasaPerdidaGanancia + (tasaPerdidaGanancia * 0.05));//Limite interior Sextante influx
            limiteExteriorINFLUX = (tasaPerdidaGanancia + (tasaPerdidaGanancia * 0.001));//Limite exterior Sextante influx
        }

        private void timerGeneral_TickLOSS(object sender)
        {

            tiempoLOSS = tiempoLOSS + 1; // cada vez que el intervalo configurado en el timer desde el form aumenta un minuto cuando pasan 60000 ms
            if (Wits0126Anterior > wits0126)
            {
                double diferenciaWits0126 = Wits0126Anterior - wits0126;
                tasaPerdidaGanancia = ((diferenciaWits0126 + diferenciaWits0126Anterior) / tiempoLOSS); // formula para determinar la tasa de perdida o ganancia
                double limitetasaPerdida = (tasaPerdidaGanancia - (tasaPerdidaGanancia * -0.05));


                if (limitetasaPerdida > tasaPerdidaGanancia)
                {
                    tiempoAlarmaLOSS = tiempoAlarmaLOSS + 1;
                    lblAlerta.Text = "TVA"; //Direccionado a textbox Alarma
                    lblAlarma.Text = "TVA " + tiempoAlarmaLOSS.ToString() + "minutos"; // Direccionado a textbox Alerta
                }
                if (ROPmaxLimpieza > wits0113)
                {
                    tiempoAlarmaLOSS2 = tiempoAlarmaLOSS2 + 1;
                    lblAlerta.Text = "Max. Hole Cleaning ROP"; //Direccionado a textbox Alarma
                    lblAlarma.Text = "Max. Hole Cleaning ROP" + tiempoAlarmaLOSS2.ToString() + "minutos"; // Direccionado a textbox Alerta
                }
                if (tiempoAlarmaAnteriorLOSS1 == tiempoAlarmaLOSS)
                {
                    tiempoAlarmaLOSS = 0;
                }
                if (tiempoAlarmaAnteriorLOSS2 == tiempoAlarmaLOSS)
                {
                    tiempoAlarmaLOSS2 = 0;
                }

                diferenciaWits0126 = Wits0126Anterior - wits0126;
                diferenciaWits0126Anterior = diferenciaWits0126;
                tiempoAlarmaAnteriorLOSS1 = tiempoAlarmaLOSS;
                tiempoAlarmaAnteriorLOSS2 = tiempoAlarmaLOSS2;
                limiteInteriorLOSS = (tasaPerdidaGanancia - (tasaPerdidaGanancia * -0.05));//Limite interior Sextante LOSS
                limiteExteriorLOSS = (tasaPerdidaGanancia - (tasaPerdidaGanancia * -0.001));//Limite exterior Sextante LOSS
                Wits0126Anterior = wits0126;
            }


        }

        private void TimerGeneral_TickPIPE_MOVE(object sender)
        {   //Comparadores para identificar que el viaje se encuentra bajando
            if (wits0110 > wits0108)
            {
                if (wits0113 == 0)
                {
                    if (wits0108 > wits0108Anterior)
                    {
                        viaje = 1;
                    }
                    else
                    {
                        viaje = 2;
                    }
                }
            }

            if (viaje == 1)
            {
                if (wits4781 > VelocidadLimiteSubiendo)
                {
                    //textBox1.Text = "Supera limite del viaje subiendo";
                }
                if (wits0117 > 1)
                {
                    //textBox1.Text = "Punto apretado";
                }
            }
            if (viaje == 2)
            {
                if (wits4781 > VelocidadLimiteBajando)
                {
                    //textBox1.Text = "Supera limite del viaje bajando";
                }
            }
            wits0108Anterior = wits0108;

        }

       


        private void Timer_Tick(object sender)
        {
            while (true)
            {
                //float valorX = 5.2f;
                //float valorY = 2.8f;
                string lectura = string.Empty;

                //Crear Lapiz Gain & loss
                pGnL = new Pen(Color.Aqua, 6f);
                //Lapiz HMSE
                //pHMSE = new Pen(Color.LawnGreen, 6f);
                //Lapiz 3
                pINFLUX = new Pen(Color.Yellow, 6f);
                //Lapiz 4
                pNPT = new Pen(Color.LightSalmon, 6f);
                //Lapiz 5
                p5 = new Pen(Color.Black, 6f);
                //Lapiz 6
                p6 = new Pen(Color.Purple, 6f);

                //lapiz
                pMano = new Pen(Color.DarkSlateBlue, 1f);

                lectura = "";// LeerPuertoSerial();

                // Asignar grafica a bitmap
                g = Graphics.FromImage(bmp);

                float[] ValoresGnL = { 0, 0, 0 };
                if (!string.IsNullOrEmpty(lectura))
                {
                    ValoresGnL = valoresPGnL(lectura);
                }
                // lectura = ValoresGnL[2].ToString();

                EjecutarEcuaciones();

                //asignar valor de HMSE

                float[] valorPuntoHMSE = pintaHMSE();
                float valorPuntoLOSS = pintarLOSS();
                float[] valorPuntosINFLUX = pintarINFLUX();

                float[] valorPuntosNPT = pintarNPT();
                float[] valorPuntosPIPE = pintarPIPEMOVE();

                //int NuevoHMSE = Convert.ToInt32((180 * valorPuntoHMSE) + 20);
                //int NuevoNPT = Convert.ToInt32(375-(375 * valorNPT));                

                //if (NuevoNPT < 200)
                //    NuevoNPT = 200;

                //BeginInvoke(new Action(() => txtResultadoDibujo.Text = valorPuntoHMSE.ToString() + "_" + NuevoHMSE.ToString()), null);

                //if (NuevoHMSE > 200)
                //    NuevoHMSE = 200;

                //mano

                //calcula x y y y las coordenadas
                double tu = (u - lim) % 360;
                if (u >= 0 && u <= 180)
                {
                    //mitad derecha
                    //u es convertido en radianes

                    x = cx + (int)(hand * Math.Sin(Math.PI * u / 180));
                    y = cy - (int)(hand * Math.Cos(Math.PI * u / 180));
                }
                else
                {
                    x = cx - (int)(hand * -Math.Sin(Math.PI * u / 180));
                    y = cy - (int)(hand * Math.Cos(Math.PI * u / 180));
                }

                if (tu >= 0 && tu <= 180)
                {
                    //mitad derecha
                    //u es convertido en radianes

                    tx = cx + (int)(hand * Math.Sin(Math.PI * tu / 180));
                    ty = cy - (int)(hand * Math.Cos(Math.PI * tu / 180));
                }
                else
                {
                    tx = cx - (int)(hand * -Math.Sin(Math.PI * tu / 180));
                    ty = cy - (int)(hand * Math.Cos(Math.PI * tu / 180));
                }


                //Crear Circulo
                g.Clear(Color.Transparent);

                //HMSE 
                if (!String.IsNullOrEmpty(wits0113.ToString()) && wits0113.ToString() != "0")
                {
                    if (valorPuntoHMSE[0] == 0 && valorPuntoHMSE[1] == 0)
                    {                        
                        g.DrawEllipse(new Pen(Color.LawnGreen, 6f), 360 - valorPuntoHMSE[0], 99 - valorPuntoHMSE[1], 6, 6);
                    }
                    else
                    {
                        g.DrawEllipse(new Pen(Color.IndianRed, 6f), 360 - valorPuntoHMSE[0], 99 - valorPuntoHMSE[1], 6, 6);
                    }
                }
                //LOSS
                g.DrawEllipse(pGnL, 198 , 5 + valorPuntoLOSS, 6, 6);

                //INFLUX
                g.DrawEllipse(pINFLUX, 30 + valorPuntosINFLUX[0], 99 + valorPuntosINFLUX[1], 6, 6);

                //NPT
                g.DrawEllipse(pNPT, 32 + valorPuntosNPT[0], 296 + valorPuntosNPT[1], 6, 6);

                //TIGHT
                g.DrawEllipse(p5, 200, 390, 6, 6);//375

                //PIPE MOVE
                g.DrawEllipse(p6, 360 + valorPuntosPIPE[0], 290 + valorPuntosPIPE[1], 6, 6);

                //// Get the color of a background pixel.
                //Color backColor = bmp.GetPixel(1, 1);

                //// Make backColor transparent for myBitmap.
                //bmp.MakeTransparent(backColor);


                //mano
                g.DrawLine(new Pen(Color.DarkSlateBlue, 1f), new Point(cx, cy), new Point(tx, ty));
                g.DrawLine(pMano, new Point(cx, cy), new Point(x, y));

                //actualizar
                u = u + 0.5;
                if (u == 360)
                {
                    u = 0;
                }


                //antiguos valores
                //g.DrawEllipse(pGnL, 350 + ValoresGnL[0], 108 + ValoresGnL[1], 6, 6);
                //g.DrawEllipse(pHMSE, 200, NuevoHMSE, 6, 6);
                //g.DrawEllipse(p3, 41, 108, 6, 6);
                //g.DrawEllipse(p4, 41, 280, 6, 6);
                //g.DrawEllipse(p5, 200, NuevoNPT, 6, 6);//375
                //g.DrawEllipse(p6, 350, 280, 6, 6);

                // Asignar bitmap a Picture Box
                pbRadar.Image = bmp;

                g.Dispose();
                pGnL.Dispose(); pHMSE.Dispose(); pINFLUX.Dispose();

                if (!string.IsNullOrEmpty(lectura))
                {
                    valorActual = float.Parse(lectura) / 10;
                    //BeginInvoke(new Action(() => txtAlarma.Text = valorActual.ToString()), null);
                    //BeginInvoke(new Action(() => lblGnlActualValor.Text = valorActual.ToString()), null);

                    //if (valorActual < float.Parse(lblGnlMinValor.Text))
                    //{
                    //    BeginInvoke(new Action(() => lblGnlActualValor.Text = valorActual.ToString()), null);
                    //}
                    //if (valorActual > float.Parse(lblGnlMaxValor.Text))
                    //{
                    //    BeginInvoke(new Action(() => lblGnlMaxValor.Text = valorActual.ToString()), null);
                    //}

                }

                Application.DoEvents();
            }


        }

        private string LeerPuertoSerial()
        {
            string informacion = string.Empty;
            try
            {
                if (!SpComunicacion.IsOpen)
                    SpComunicacion.Open();

                informacion = SpComunicacion.ReadLine();
                SpComunicacion.Close();
            }
            catch (Exception)
            {

                throw;
            }
            return informacion;
        }

        public float[] valoresPGnL(string lectura)
        {
            float valorX = 5.25f;
            float valorY = 2.2f;
            float valorActual = 1;

            if (!string.IsNullOrEmpty(lectura) && lectura != "")
            {
                String[] resultado = lectura.Split(new char[] { '\r' });

                lectura = resultado[0];

                if (float.Parse(lectura) < valorActual)
                {
                    valorY = valorY * -1;
                    if (valorX < 0)
                    {
                        valorX = valorX * -1;
                    }
                }
                else if (float.Parse(lectura) > valorActual)
                {
                    valorX = valorX * -1;
                    if (valorY < 0)
                    {
                        valorY = valorY * -1;
                    }
                }

                float valorAbsoluto = (float.Parse(lectura) * 10) / 10;
                valorX = valorX * valorAbsoluto;
                valorY = valorY * valorAbsoluto;

            }
            else
            {
                valorX = 0;
                valorY = 0;
            }
            float[] resultadoFunction = { valorX, valorY, float.Parse(lectura) };

            return resultadoFunction;
        }
    }
}