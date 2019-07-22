using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assinar_PDF_VS
{
    public partial class Assinar_PDF : Form
    {
        string PDF;
        string PDF_Assinado;
        string Str_Usuario;
        string Arquivo;
        string Serial_Number;
        string Senha;

        public Assinar_PDF(string[] Parametros)
        {
            InitializeComponent();

            InitializeComponent();

            for (int i = 0; i <= (Parametros.Length - 1); i++)
            {
                if (i == 0)
                {
                    Str_Usuario = "Temp_User\\" + Parametros[i];
                }
                else if (i == 1)
                {
                    Arquivo = "\\CERTIFICADOS\\" + Parametros[i];
                }
            }
            Arquivo = Application.StartupPath + '\\' + Str_Usuario + Arquivo;

            if (!File.Exists(Arquivo))
            {
                MessageBox.Show("Não possivel encontrar o arquivo de configuração!, Favor informe o Administrador!", "Queops Assinar PDF", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }

            int counter = 0;
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(Arquivo);
            while ((line = file.ReadLine()) != null)
            {
                if (counter == 0)
                {
                    PDF = "\\PDF\\" + line;
                    PDF_Assinado = "\\PDF\\" + line.Replace(".", "_Assinado.");
                }
                else if (counter == 1)
                {
                    Serial_Number = line;
                }
                else if (counter == 2)
                {
                    Senha = line;
                }
                counter++;
            }

            file.Close();
            if (File.Exists(Arquivo))
            {
                File.Delete(Arquivo);
            }

            PDF = Application.StartupPath + '\\' + Str_Usuario + PDF;
            PDF_Assinado = Application.StartupPath + '\\' + Str_Usuario + PDF_Assinado;
        }

        public void verificarCertificado(X509Certificate2 certificado)
        {
            if (certificado == null)
            {
                MessageBox.Show("Certiicado não encontrado, favor instalar certificado!", "Queops Assinar", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
                Close();
            }
        }
        private SecureString GetSecurePin(string PinCode)
        {
            SecureString pwd = new SecureString();
            foreach (var c in PinCode.ToCharArray()) pwd.AppendChar(c);
            return pwd;
        }

        public static RSACryptoServiceProvider LerDispositivo(RSACryptoServiceProvider key, string PIN)
        {
            CspParameters csp = new CspParameters(key.CspKeyContainerInfo.ProviderType, key.CspKeyContainerInfo.ProviderName);
            SecureString ss = new SecureString();
            foreach (char a in PIN)
            {
                ss.AppendChar(a);
            }
            csp.ProviderName = key.CspKeyContainerInfo.ProviderName;
            csp.ProviderType = key.CspKeyContainerInfo.ProviderType;
            csp.KeyNumber = key.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2;
            csp.KeyContainerName = key.CspKeyContainerInfo.KeyContainerName;
            csp.KeyPassword = ss;
            csp.Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseDefaultKeyContainer;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);
            return rsa;
        }

        public static X509Certificate2 EscolherCertificado(string Serial_Number)
        {
            var store = new X509Store("My", StoreLocation.CurrentUser);
            var Key = new RSACryptoServiceProvider();
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = store.Certificates;
            X509Certificate2Collection fcollection = collection.Find(X509FindType.FindBySerialNumber, Serial_Number, false);
            if (fcollection.Count == 1)
            {
                return fcollection[0];
            }
            else
            {
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (Serial_Number == "")
                {
                    MessageBox.Show("Numero do serial nao encontrado!", "Queops Assinar PDF", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Application.Exit();
                }

                var cspParams = new CspParameters(24) { KeyContainerName = "XML_DISG_RSA_KEY" };
                var Key = new RSACryptoServiceProvider(cspParams);

                if (!File.Exists(PDF))
                {
                    MessageBox.Show("Não foi encontrado arquivo para assianar!", "Queops Assinar PDF", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Application.Exit();
                }
                if (File.Exists(PDF_Assinado))
                {
                    File.Delete(PDF_Assinado);
                }

                try
                {
                    SignWithThisCert(EscolherCertificado(Serial_Number), PDF, PDF_Assinado);
                }
                catch (Exception i)
                {
                    MessageBox.Show(i.Message, "Queops Assinar", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Close();

                }

                Application.Exit();
            }
            catch (CryptographicException i)
            {
                MessageBox.Show(i.Message, "Queops Assinar PDF", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
        }

        private void SignWithThisCert(X509Certificate2 cert, string SourcePdfFileName, string DestPdfFileName)
        {
            // string SourcePdfFileName = @"C:\Users\ajfiu\Documents\Visual Studio 2017\Projects\Temp\Embargos.PDF";
            // string DestPdfFileName = @"C:\Users\ajfiu\Documents\Visual Studio 2017\Projects\Temp\Embargos_Assinado.PDF";

            verificarCertificado(cert);

            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            PdfReader pdfReader = new PdfReader(SourcePdfFileName);
            FileStream signedPdf = new FileStream(DestPdfFileName, FileMode.Create);  //the output pdf file
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0');
            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
            //here set signatureAppearance at your will
            //signatureAppearance.Reason = cert.FriendlyName.ToString();
            signatureAppearance.Layer2Text = "Assinador por:" + cert.FriendlyName + " Validade: " + cert.GetExpirationDateString() + " em " + DateTime.Now.ToShortDateString();
            signatureAppearance.Contact = cert.FriendlyName;
            signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;

            RSACryptoServiceProvider Key = new RSACryptoServiceProvider();
            Key = (System.Security.Cryptography.RSACryptoServiceProvider)cert.PrivateKey;
            LerDispositivo(Key, Senha);
            MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
            //MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CADES);
        }
    }
}
