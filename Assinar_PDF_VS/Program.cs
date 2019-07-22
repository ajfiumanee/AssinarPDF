using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assinar_PDF_VS
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// Parametro
        /// [0] = Pasta Usuario
        /// [1] = Certificado
        /// [2] = PDF a assinar
        /// </summary>
        [STAThread]
        static void Main(string[] Parametros)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Parametros.Length > 0)
            {
                Application.Run(new Assinar_PDF(Parametros));
            }
            else
            {
                Application.Exit();
            }
        }
    }
}
