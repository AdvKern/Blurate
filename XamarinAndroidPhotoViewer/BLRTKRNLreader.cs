using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography;

namespace Blurate
{
    class BlrtkrnlRdWr
    {
        public static string ImportBlrtkrnl_fromByteArray(byte[] src)
        {
            using (MemoryStream stream = new MemoryStream(src))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {

                    return ImportBlrtkrnl_fromReader(reader);

                }
            }
        }

        public static string ImportBlrtkrnl(string file)
        {
            BinaryReader reader = null;
            try
            {
                reader = new BinaryReader(new FileStream(file, FileMode.Open));
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.Message, "Access Error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
            return ImportBlrtkrnl_fromReader(reader);
        }

        static string ImportBlrtkrnl_fromReader(BinaryReader reader)
        { 

            string KernStrng = null;
            string NameStrng = null;

            try
            {
                if (((byte)reader.ReadChar() != 0x01) || // b
                    ((byte)reader.ReadChar() != 0x0b) || // l
                    ((byte)reader.ReadChar() != 0x02) || // u (dec)
                    ((byte)reader.ReadChar() != 0x00) || // 
                    ((byte)reader.ReadChar() != 0x01) || // r (hex)
                    ((byte)reader.ReadChar() != 0x01) || //
                    ((byte)reader.ReadChar() != 0x0a) || // a
                    ((byte)reader.ReadChar() != 0x01) || // t (hex)
                    ((byte)reader.ReadChar() != 0x09) || // 
                    ((byte)reader.ReadChar() != 0x0e))   // e
                    return null;

                List<char> NameList = new List<char>();
                char nextChar = reader.ReadChar();
                while ((byte)nextChar != 0x0a)
                {
                    NameList.Add(nextChar); //Eat newline
                    nextChar = reader.ReadChar();
                }

                List<char> ExtensionsList = new List<char>();
                nextChar = reader.ReadChar();
                while ((byte)nextChar != 0x0a)
                {
                    ExtensionsList.Add(nextChar); //Eat newline
                    nextChar = reader.ReadChar();
                }

                List<char> KernelHashList = new List<char>();
                nextChar = reader.ReadChar();
                while ((byte)nextChar != 0x0a)
                {
                    KernelHashList.Add(nextChar); //Eat newline
                    nextChar = reader.ReadChar();
                }

                List<char> KernelList = new List<char>();
                nextChar = reader.ReadChar();
                try
                {
                    while ((byte)nextChar != null)
                    {
                        KernelList.Add(nextChar); //Eat newline
                        nextChar = reader.ReadChar();
                    }
                }
                catch { }

                for (int i=0; i<KernelList.Count; i++)
                {
                    KernelList[i] += ' ';
                }

                KernStrng = new string(KernelList.ToArray());
                NameStrng = new string(NameList.ToArray());
                string KernelHashString = new string(KernelHashList.ToArray());

                string HashCheck = KernStrng + "#" + NameStrng;

                string newHash = GetMd5Sum(HashCheck);
                if (!GetMd5Sum(HashCheck).Equals(KernelHashString))
                {
                    //System.Windows.Forms.MessageBox.Show("Plugin format error!");
                    return null;
                }
            }
            catch{
                //System.Windows.Forms.MessageBox.Show("Error reading plugin!");
                return null; 
            }
            finally
            {
                reader.Close();
            }

            return NameStrng + "=" + KernStrng;
        }

        // Create an md5 sum string of this string
        static public string GetMd5Sum(string str)
        {
            // First we need to convert the string into bytes, which
            // means using a text encoder.
            Encoder enc = System.Text.Encoding.Unicode.GetEncoder();

            // Create a buffer large enough to hold the string
            char[] unicodeText = new char[str.Length];
            //enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

            byte[] raw_input = Encoding.UTF8.GetBytes(str);

            // Now that we have a byte array we can ask the CSP to hash it
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(raw_input);

            // Build the final string by converting each byte
            // into hex and appending it to a StringBuilder
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }

            // And return it
            return sb.ToString();
        }

        public static void WriteKernelToBlrtkrnl(string file, string kernel, string Name, string extensions=null)
        {
            //Use a streamwriter to write the text part of the encoding
            BinaryWriter writerB;
            try
            {
                writerB = new BinaryWriter(new FileStream(file, FileMode.Create));
            }
            catch(Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.Message, "Access Error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            try
            {
                //writerB.Write(("Blurate").ToCharArray());
                writerB.Write((byte)(0x01)); // b
                writerB.Write((byte)(0x0b)); // l
                writerB.Write((byte)(0x02)); // u (dec)
                writerB.Write((byte)(0x00));
                writerB.Write((byte)(0x01)); // r (hex)
                writerB.Write((byte)(0x01));
                writerB.Write((byte)(0x0a)); // a
                writerB.Write((byte)(0x01)); // t (hex)
                writerB.Write((byte)(0x09));
                writerB.Write((byte)(0x0e)); // e

                writerB.Write((Name).ToCharArray());
                writerB.Write((byte)(0x0a)); // end

                if (extensions != null)
                {
                    writerB.Write((extensions).ToCharArray());
                }
                writerB.Write((byte)(0x0a)); // end

                string kernHash = GetMd5Sum(kernel + "#" + Name);
                writerB.Write((kernHash).ToCharArray());
                writerB.Write((byte)(0x0a)); // end

                char[] kern = (kernel).ToCharArray();
                for (int i = 0; i < kernel.Length; i++)
                    kern[i] -= ' ';
                writerB.Write(kern);
                writerB.Close();
            }
            catch
            {
               // System.Windows.Forms.MessageBox.Show("Error writing to file!"); 
            }
            finally
            {
                writerB.Close();
            }
        }
    }
}
