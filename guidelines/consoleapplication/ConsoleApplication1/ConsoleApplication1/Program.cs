using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace ConsoleApplication1
{
    class Program
    {
        static void WriStr(FileStream Out, string s)
        {
            Out.Write(System.Text.Encoding.ASCII.GetBytes(s), 0, s.Length);
        }
        static void Main(string[] args)
        {

            string InJpg = @"InFile.JPG";
            string OutPdf = @"OutFile.pdf";

            byte[] buffer = new byte[8192];
            var stream = File.OpenRead(InJpg); // The easiest way to get the metadata is to temporaryly load it as a BMP
            //Bitmap bmp = (Bitmap)Bitmap.FromStream(stream);
            int w = 500;
            string wf = "5000"; //= bmp.Width; String wf = (w * 72 / bmp.HorizontalResolution).ToString().Replace(",", ".");
            string hf = "5000";
                int h = 500;//= bmp.Height; ; string hf = (h * 72 / bmp.VerticalResolution).ToString().Replace(",", ".");
            stream.Close();

            FileStream Out = File.Create(OutPdf);

            var lens = new List<long>();

            WriStr(Out, "%PDF-1.5\r\n");

            lens.Add(Out.Position);
            WriStr(Out, lens.Count.ToString() + " 0 obj " + "<</Type /Catalog\r\n/Pages 2 0 R>>\r\nendobj\r\n");

            lens.Add(Out.Position);
            WriStr(Out, lens.Count.ToString() + " 0 obj " + "<</Count 1/Kids [ <<\r\n" +
                        "/Type /Page\r\n" +
                        "/Parent 2 0 R\r\n" +
                        "/MediaBox [0 0 " + wf + " " + hf + "]\r\n" +
                        "/Resources<<  /ProcSet [/PDF /ImageC]\r\n /XObject <</Im1 4 0 R >>  >>\r\n" +
                        "/Contents 3 0 R\r\n" +
                        ">>\r\n ]\r\n" +
                        ">>\r\nendobj\r\n");

            string X = "\r\n" +
                "q\r\n" +
                "" + wf + " 0 0 " + hf + " 0 0 cm\r\n" +
                "/Im1 Do\r\n" +
                "Q\r\n";
            lens.Add(Out.Position);
            WriStr(Out, lens.Count.ToString() + " 0 obj " + "<</Length " + X.Length.ToString() + ">>" +
                        "stream" + X + "endstream\r\n" +
                        "endobj\r\n");
            lens.Add(Out.Position);
            WriStr(Out, lens.Count.ToString() + " 0 obj " + "<</Name /Im1" +
                        "/Type /XObject\r\n" +
                        "/Subtype /Image\r\n" +
                        "/Width " + w.ToString() +
                        "/Height " + h.ToString() +
                        "/Length 5 0 R\r\n" +
                        "/Filter /DCTDecode\r\n" +
                        "/ColorSpace /DeviceRGB\r\n" +
                        "/BitsPerComponent 8\r\n" +
                        ">> stream\r\n");
            long Siz = Out.Position;
            var in1 = File.OpenRead(InJpg);
            while (true)
            {
                var len = in1.Read(buffer, 0, buffer.Length);
                if (len != 0) Out.Write(buffer, 0, len); else break;
            }
            in1.Close();
            Siz = Out.Position - Siz;
            WriStr(Out, "\r\nendstream\r\n" +
                        "endobj\r\n");

            lens.Add(Out.Position);
            WriStr(Out, lens.Count.ToString() + " 0 obj " + Siz.ToString() + " endobj\r\n");

            long startxref = Out.Position;

            WriStr(Out, "xref\r\n" +
                        "0 " + (lens.Count + 1).ToString() + "\r\n" +
                        "0000000000 65535 f\r\n");
            foreach (var L in lens)
                WriStr(Out, (10000000000 + L).ToString().Substring(1) + " 00000 n\r\n");
            WriStr(Out, "trailer\r\n" +
                        "<<\r\n" +
                        "  /Size " + (lens.Count + 1).ToString() + "\r\n" +
                        "  /Root 1 0 R\r\n" +
                        ">>\r\n" +
                        "startxref\r\n" +
                        startxref.ToString() + "\r\n%%EOF");
            Out.Close();
        }
    }
}