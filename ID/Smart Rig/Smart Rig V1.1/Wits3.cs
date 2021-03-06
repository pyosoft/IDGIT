﻿using pyosoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smart_Rig_V1._1
{
    public partial class Wits3 : MetroFramework.Forms.MetroForm
    {
        public Wits3()
        {
            InitializeComponent();
        }

        bool seleccionado = false;
        string archivoConfigurador = "C:\\Pyosoft";
        string nombreConfigurador = "\\WitsConfiguracion3.txt";

        private void Wits3_Load(object sender, EventArgs e)
        {
            UsoControles("pnlWits2", false);
            UsoControles("pnlWits3", false);

            string archivoALeer = archivoConfigurador + nombreConfigurador;
            if (File.Exists(archivoALeer))
            {
                string[] items = File.ReadAllLines(archivoALeer);

                foreach (string itemAseleccionar in items)
                {
                    CheckBox check = (CheckBox)this.Controls.Find(itemAseleccionar, true).FirstOrDefault();

                    if (check != null)
                    {
                        foreach (Control item in pnlWits1.Controls)
                        {
                            if (item is CheckBox)
                            {
                                if (item.Name == check.Name)
                                {
                                    ((CheckBox)item).Checked = true;
                                }
                            }
                        }

                        foreach (Control item in pnlWits2.Controls)
                        {
                            if (item.Name == check.Name)
                            {
                                ((CheckBox)item).Checked = true;
                            }
                        }

                        foreach (Control item in pnlWits3.Controls)
                        {
                            if (item.Name == check.Name)
                            {
                                ((CheckBox)item).Checked = true;
                            }
                        }
                    }
                }
            }
        }

        private void UsoControles(string nombreControl, bool habilita)
        {
            var panel = (Panel)this.Controls.Find(nombreControl, true).FirstOrDefault();

            foreach (Control item in panel.Controls)
            {
                item.Enabled = habilita;
            }
        }

        private void SeleccionarControles(string nombreBoton, string nombrePanel)
        {
            Button boton = (Button)this.Controls.Find(nombreBoton, true).FirstOrDefault();

            if (seleccionado)
            {
                boton.Text = "Seleccionar todo";
                seleccionado = false;
            }
            else
            {
                boton.Text = "Deseleccionar todo";
                seleccionado = true;
            }
            Panel panel = (Panel)this.Controls.Find(nombrePanel, true).FirstOrDefault();

            foreach (Control item in panel.Controls)
            {
                if (item is CheckBox)
                {
                    ((CheckBox)item).Checked = seleccionado;
                }
            }
        }

        private void btnAtras1_Click(object sender, EventArgs e)
        {
            Wits2 ventanaWits2 = new Wits2();
            ventanaWits2.Show();
            this.Hide();
        }

        private void btnTodo1_Click(object sender, EventArgs e)
        {
            SeleccionarControles("btnTodo1", "pnlWits1");
        }

        private void btnTodo2_Click(object sender, EventArgs e)
        {
            SeleccionarControles("btnTodo1", "pnlWits2");
        }

        private void btnTodo3_Click(object sender, EventArgs e)
        {
            SeleccionarControles("btnTodo1", "pnlWits3");
        }

        private void btnAtras2_Click(object sender, EventArgs e)
        {
            UsoControles("pnlWits1", true);
            UsoControles("pnlWits2", false);
            UsoControles("pnlWits3", false);
        }

        private void btnAtras3_Click(object sender, EventArgs e)
        {
            UsoControles("pnlWits1", false);
            UsoControles("pnlWits2", true);
            UsoControles("pnlWits3", false);
        }

        private void btnSiguiente1_Click(object sender, EventArgs e)
        {
            UsoControles("pnlWits1", false);
            UsoControles("pnlWits2", true);
            UsoControles("pnlWits3", false);
        }

        private void btnSiguiente2_Click(object sender, EventArgs e)
        {

            UsoControles("pnlWits1", false);
            UsoControles("pnlWits2", false);
            UsoControles("pnlWits3", true);
        }

        private void btnSiguiente3_Click(object sender, EventArgs e)
        {
            List<string> valoresAguardar = new List<string>();

            foreach (Control item in pnlWits1.Controls)
            {
                if (item is CheckBox)
                {
                    if (((CheckBox)item).Checked)
                    {
                        valoresAguardar.Add(item.Name);
                    }
                }
            }
            foreach (Control item in pnlWits2.Controls)
            {
                if (item is CheckBox)
                {
                    if (((CheckBox)item).Checked)
                    {
                        valoresAguardar.Add(item.Name);
                    }
                }
            }
            foreach (Control item in pnlWits3.Controls)
            {
                if (item is CheckBox)
                {
                    if (((CheckBox)item).Checked)
                    {
                        valoresAguardar.Add(item.Name);
                    }
                }
            }

            int guardar = new AD_protocoloWits().guardarConfiguracionWits(nombreConfigurador, valoresAguardar);

            Wits4 ventanaWits4 = new Wits4();
            ventanaWits4.Show();
            this.Hide();
        }
    }
}
