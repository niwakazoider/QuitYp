using System;
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace QuitYpApplication
{

    class QuitTokenState
    {
        public string token { get; set; }
        public string tokenSecret { get; set; }
        private AES aes;

        public QuitTokenState()
        {
            aes = new AES();
        }

        public void Save()
        {
            try
            {
                Properties.Settings.Default.Token = aes.Encrypt(token);
                Properties.Settings.Default.TokenSecret = aes.Encrypt(tokenSecret);
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {

            }
        }

        public void Load()
        {
            try
            {
                token = aes.Decrypt(Properties.Settings.Default.Token);
                tokenSecret = aes.Decrypt(Properties.Settings.Default.TokenSecret);
            }
            catch (Exception) {
                token = "";
                tokenSecret = "";
            }
        }

        /*
        * http://programmers.high-way.info/cs/aes.html
        */
        class AES
        {
            //private string AesIV = "!QAZ2WSX#EDC4RFV";
            private string AesKey = @"5TGB&YHN7UJM(IK<";

            public AES() {
                Init();
            }

            private void Init()
            {
                string mac = "FE8BC46AD0AF";
                NetworkInterface[] nicList = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nic in nicList)
                {
                    mac = nic.GetPhysicalAddress().ToString();
                    break;
                }
                AesKey = mac + "7144";

            }

            private AesCryptoServiceProvider aes128()
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                aes.BlockSize = 128;
                aes.KeySize = 128;
                //aes.IV = Encoding.UTF8.GetBytes(AesIV);
                aes.Key = Encoding.UTF8.GetBytes(AesKey);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                return aes;
            }

            public string Encrypt(string text)
            {
                byte[] data = Encoding.Unicode.GetBytes(text);

                AesCryptoServiceProvider aes = aes128();
                aes.GenerateIV();

                using (ICryptoTransform encrypt = aes.CreateEncryptor())
                {
                    byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
                    return Convert.ToBase64String(aes.IV.Concat(dest).ToArray());
                }
            }

            public string Decrypt(string text)
            {
                byte[] data = Convert.FromBase64String(text);

                AesCryptoServiceProvider aes = aes128();
                aes.IV = data.Take(16).ToArray();

                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] dest = decrypt.TransformFinalBlock(data, 16, data.Length-16);
                    return Encoding.Unicode.GetString(dest);
                }
            }

        }

    }
}
