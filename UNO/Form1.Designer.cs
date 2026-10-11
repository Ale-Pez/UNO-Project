namespace UNO
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cartas_jugador1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cartas_jugador2 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cartas_jugador3 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblAvisoTemp = new System.Windows.Forms.Label();
            this.panel_Superior_GroupBox4 = new System.Windows.Forms.Panel();
            this.boton_reinicio = new System.Windows.Forms.Button();
            this.lblTurno = new System.Windows.Forms.Label();
            this.panel_Inferior_GroupBox4 = new System.Windows.Forms.Panel();
            this.boton_uno = new System.Windows.Forms.Button();
            this.panel_Central_GroupBox4 = new System.Windows.Forms.Panel();
            this.mazo_cartas = new System.Windows.Forms.PictureBox();
            this.pila_cartas = new System.Windows.Forms.PictureBox();
            this.icono_color_juego = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel_Superior_GroupBox4.SuspendLayout();
            this.panel_Inferior_GroupBox4.SuspendLayout();
            this.panel_Central_GroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mazo_cartas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pila_cartas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icono_color_juego)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 621);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(884, 140);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cartas_jugador1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Nirmala UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(884, 140);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Jugador 1";
            // 
            // cartas_jugador1
            // 
            this.cartas_jugador1.AutoScroll = true;
            this.cartas_jugador1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cartas_jugador1.Location = new System.Drawing.Point(3, 23);
            this.cartas_jugador1.Name = "cartas_jugador1";
            this.cartas_jugador1.Size = new System.Drawing.Size(878, 114);
            this.cartas_jugador1.TabIndex = 0;
            this.cartas_jugador1.WrapContents = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(734, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(150, 621);
            this.panel2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cartas_jugador2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Nirmala UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(150, 621);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Jugador 2";
            // 
            // cartas_jugador2
            // 
            this.cartas_jugador2.AutoScroll = true;
            this.cartas_jugador2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cartas_jugador2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.cartas_jugador2.Location = new System.Drawing.Point(3, 23);
            this.cartas_jugador2.Name = "cartas_jugador2";
            this.cartas_jugador2.Size = new System.Drawing.Size(144, 595);
            this.cartas_jugador2.TabIndex = 0;
            this.cartas_jugador2.WrapContents = false;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Controls.Add(this.groupBox3);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(150, 621);
            this.panel3.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cartas_jugador3);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Nirmala UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(150, 621);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Jugador 3";
            // 
            // cartas_jugador3
            // 
            this.cartas_jugador3.AutoScroll = true;
            this.cartas_jugador3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cartas_jugador3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.cartas_jugador3.Location = new System.Drawing.Point(3, 23);
            this.cartas_jugador3.Name = "cartas_jugador3";
            this.cartas_jugador3.Size = new System.Drawing.Size(144, 595);
            this.cartas_jugador3.TabIndex = 0;
            this.cartas_jugador3.WrapContents = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Transparent;
            this.panel4.Controls.Add(this.groupBox4);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(150, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(584, 621);
            this.panel4.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblAvisoTemp);
            this.groupBox4.Controls.Add(this.panel_Superior_GroupBox4);
            this.groupBox4.Controls.Add(this.panel_Inferior_GroupBox4);
            this.groupBox4.Controls.Add(this.panel_Central_GroupBox4);
            this.groupBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(584, 621);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            // 
            // lblAvisoTemp
            // 
            this.lblAvisoTemp.AutoSize = true;
            this.lblAvisoTemp.BackColor = System.Drawing.Color.Transparent;
            this.lblAvisoTemp.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvisoTemp.Location = new System.Drawing.Point(377, 24);
            this.lblAvisoTemp.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAvisoTemp.Name = "lblAvisoTemp";
            this.lblAvisoTemp.Size = new System.Drawing.Size(0, 21);
            this.lblAvisoTemp.TabIndex = 4;
            // 
            // panel_Superior_GroupBox4
            // 
            this.panel_Superior_GroupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel_Superior_GroupBox4.Controls.Add(this.boton_reinicio);
            this.panel_Superior_GroupBox4.Controls.Add(this.lblTurno);
            this.panel_Superior_GroupBox4.Location = new System.Drawing.Point(3, 16);
            this.panel_Superior_GroupBox4.Name = "panel_Superior_GroupBox4";
            this.panel_Superior_GroupBox4.Size = new System.Drawing.Size(578, 100);
            this.panel_Superior_GroupBox4.TabIndex = 6;
            // 
            // boton_reinicio
            // 
            this.boton_reinicio.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.boton_reinicio.BackColor = System.Drawing.Color.Yellow;
            this.boton_reinicio.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boton_reinicio.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.boton_reinicio.Location = new System.Drawing.Point(217, 8);
            this.boton_reinicio.Name = "boton_reinicio";
            this.boton_reinicio.Size = new System.Drawing.Size(157, 41);
            this.boton_reinicio.TabIndex = 5;
            this.boton_reinicio.Text = "¡Reiniciar Juego!";
            this.boton_reinicio.UseVisualStyleBackColor = false;
            this.boton_reinicio.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblTurno
            // 
            this.lblTurno.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblTurno.AutoSize = true;
            this.lblTurno.BackColor = System.Drawing.Color.Transparent;
            this.lblTurno.Font = new System.Drawing.Font("Nirmala UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTurno.Location = new System.Drawing.Point(262, 64);
            this.lblTurno.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTurno.Name = "lblTurno";
            this.lblTurno.Size = new System.Drawing.Size(75, 25);
            this.lblTurno.TabIndex = 4;
            this.lblTurno.Text = "Turno: ";
            // 
            // panel_Inferior_GroupBox4
            // 
            this.panel_Inferior_GroupBox4.Controls.Add(this.boton_uno);
            this.panel_Inferior_GroupBox4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Inferior_GroupBox4.Location = new System.Drawing.Point(3, 518);
            this.panel_Inferior_GroupBox4.Name = "panel_Inferior_GroupBox4";
            this.panel_Inferior_GroupBox4.Size = new System.Drawing.Size(578, 100);
            this.panel_Inferior_GroupBox4.TabIndex = 7;
            // 
            // boton_uno
            // 
            this.boton_uno.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.boton_uno.BackColor = System.Drawing.Color.Yellow;
            this.boton_uno.Cursor = System.Windows.Forms.Cursors.Hand;
            this.boton_uno.Font = new System.Drawing.Font("Nirmala UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boton_uno.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.boton_uno.Location = new System.Drawing.Point(238, 33);
            this.boton_uno.Name = "boton_uno";
            this.boton_uno.Size = new System.Drawing.Size(109, 47);
            this.boton_uno.TabIndex = 4;
            this.boton_uno.Text = "¡UNO!";
            this.boton_uno.UseVisualStyleBackColor = false;
            this.boton_uno.Click += new System.EventHandler(this.boton_uno_Click);
            // 
            // panel_Central_GroupBox4
            // 
            this.panel_Central_GroupBox4.Controls.Add(this.mazo_cartas);
            this.panel_Central_GroupBox4.Controls.Add(this.pila_cartas);
            this.panel_Central_GroupBox4.Controls.Add(this.icono_color_juego);
            this.panel_Central_GroupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Central_GroupBox4.Location = new System.Drawing.Point(3, 16);
            this.panel_Central_GroupBox4.Name = "panel_Central_GroupBox4";
            this.panel_Central_GroupBox4.Size = new System.Drawing.Size(578, 602);
            this.panel_Central_GroupBox4.TabIndex = 8;
            // 
            // mazo_cartas
            // 
            this.mazo_cartas.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.mazo_cartas.BackgroundImage = global::UNO.Properties.Resources.back_uno;
            this.mazo_cartas.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.mazo_cartas.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mazo_cartas.Location = new System.Drawing.Point(436, 243);
            this.mazo_cartas.Name = "mazo_cartas";
            this.mazo_cartas.Size = new System.Drawing.Size(70, 100);
            this.mazo_cartas.TabIndex = 0;
            this.mazo_cartas.TabStop = false;
            // 
            // pila_cartas
            // 
            this.pila_cartas.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pila_cartas.BackgroundImage = global::UNO.Properties.Resources.back_uno;
            this.pila_cartas.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pila_cartas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pila_cartas.Location = new System.Drawing.Point(267, 243);
            this.pila_cartas.Name = "pila_cartas";
            this.pila_cartas.Size = new System.Drawing.Size(70, 100);
            this.pila_cartas.TabIndex = 1;
            this.pila_cartas.TabStop = false;
            // 
            // icono_color_juego
            // 
            this.icono_color_juego.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.icono_color_juego.BackgroundImage = global::UNO.Properties.Resources.yellow;
            this.icono_color_juego.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.icono_color_juego.Cursor = System.Windows.Forms.Cursors.Default;
            this.icono_color_juego.Location = new System.Drawing.Point(219, 173);
            this.icono_color_juego.Name = "icono_color_juego";
            this.icono_color_juego.Size = new System.Drawing.Size(170, 239);
            this.icono_color_juego.TabIndex = 2;
            this.icono_color_juego.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::UNO.Properties.Resources.background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(884, 761);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.MinimumSize = new System.Drawing.Size(900, 798);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UNO";
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.panel_Superior_GroupBox4.ResumeLayout(false);
            this.panel_Superior_GroupBox4.PerformLayout();
            this.panel_Inferior_GroupBox4.ResumeLayout(false);
            this.panel_Central_GroupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mazo_cartas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pila_cartas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icono_color_juego)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel cartas_jugador1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FlowLayoutPanel cartas_jugador2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.FlowLayoutPanel cartas_jugador3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.PictureBox pila_cartas;
        private System.Windows.Forms.PictureBox mazo_cartas;
        private System.Windows.Forms.Label lblTurno;
        private System.Windows.Forms.Button boton_uno;
        private System.Windows.Forms.Button boton_reinicio;
        private System.Windows.Forms.Label lblAvisoTemp;
        private System.Windows.Forms.Panel panel_Superior_GroupBox4;
        private System.Windows.Forms.Panel panel_Inferior_GroupBox4;
        private System.Windows.Forms.Panel panel_Central_GroupBox4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox icono_color_juego;
    }
}

